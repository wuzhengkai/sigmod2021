using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using Microsoft.ProgramSynthesis.Utils;

namespace Microsoft.ProgramSynthesis.Visualization.Grammar {
    /// <summary>
    ///     Abstract class used to represent each production rule in the grammar.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class Rule {
        /// <summary>
        ///     The properties that should be nonrepeated.
        /// </summary>
        protected static readonly HashSet<string> NonRepeated = new HashSet<string>(new string[] {
            "histodens", "histobins", "histostack", "histolog", "scattermarker", "scattercolor"
        });

        /// <summary>
        ///     The penalty to apply when there is an "error" token in the rule.
        /// </summary>
        private static readonly double ErrorPenalty = 0.95;

        protected Rule(Grammar g, Token lhs, IReadOnlyList<Token> rhs, IDictionary<string, string> semantics) {
            Grammar = g;
            Lhs = lhs;
            Rhs = rhs;
            Semantics = semantics;
        }

        /// <summary>
        ///     Left hand side token of the rule.
        /// </summary>
        public Token Lhs { get; }

        /// <summary>
        ///     Right hand side tokens of the rule.
        /// </summary>
        public IReadOnlyList<Token> Rhs { get; }

        /// <summary>
        ///     Semantics of the rule, which is essentially the properties we can get from the rule..
        /// </summary>
        public IDictionary<string, string> Semantics { get; }

        /// <summary>
        ///     Reference to the grammar.
        /// </summary>
        public Grammar Grammar { get; }

        private string DebuggerDisplay => $"Rule({Lhs}=>{string.Join(" ", Rhs)}";

        /// <summary>
        ///     Calculates the new fitScore considering the errors.
        /// </summary>
        /// <param name="fitScore"> Original fitScore </param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="errors"> Number of error tokens in the application of the rule. </param>
        /// <returns> New fitScore </returns>
        protected static double CalcFitScoreUnderErrors(double fitScore, int start, int end, int errors, int errorlen) {
            double depreciation = (double) (end - start - errors) / (end - start);
            return fitScore * depreciation * Math.Pow(ErrorPenalty, errors + errorlen);
        }

        /// <summary>
        ///     Applies the rule to the tokens in given range.
        /// </summary>
        /// <param name="chart"> The chart to store all parsing results. </param>
        /// <param name="tokens"> Tokens in the query. </param>
        /// <param name="start"> The starting position of the range (included). </param>
        /// <param name="end"> The ending position of the range (excluded). </param>
        public abstract void Apply(List<Parse>[,] chart, IReadOnlyList<string> tokens, int start, int end);

        private static Rule Create(Grammar g, Token lhs, IReadOnlyList<Token> rhs,
                                   IDictionary<string, string> semantics) {
            if (rhs.All(e => e is Constant)) {
                return new LexicalRule(g, lhs, rhs.Cast<Constant>().ToList(), semantics);
            }

            switch (rhs.Count) {
                case 1:
                    return new UnaryRule(g, lhs, rhs[0], semantics);
                case 2:
                    return new BinaryRule(g, lhs, rhs[0], rhs[1], semantics);
                default: throw new ArgumentException();
            }
        }

        /// <summary>
        ///     Creates the (list of) rules by splitting right hand side rules and resolving new semantic rules.
        /// </summary>
        /// <param name="grammar"></param>
        /// <param name="lhs"> Left hand side of the grammar </param>
        /// <param name="rhss"> List of right hand side rules of the grammar </param>
        /// <param name="semantics"> Semantic rules before resolving $ reference. </param>
        /// <returns> The list of rules obtained. </returns>
        public static IEnumerable<Rule> Create(Grammar grammar, string lhs, IReadOnlyList<string> rhss,
                                               IDictionary<string, string> semantics) {
            Token lhsSymbol = grammar.GetOrCreateSymbol(lhs);
            IEnumerable<IEnumerable<string>> rhsCombinations =
                rhss.Select(rhs => rhs.EndsWith("?")
                                ? new[] { null, rhs.Substring(0, rhs.Length - 1) }
                                : new[] { rhs })
                    .CartesianProduct();
            foreach (IEnumerable<string> combination in rhsCombinations) {
                var rhs = combination.ToList();
                var newSemantics = new Dictionary<string, string>();
                foreach ((string key, string value) in semantics) {
                    if (value.StartsWith("$")) {
                        // Update indices of semantic references for optional RHS
                        var split = value.Split('.');
                        int index = int.Parse(split[0].Substring(1));
                        index = index - rhs.Take(index).Count(r => r == null);
                        Debug.Assert(index >= 0);
                        newSemantics[key] = $"${index}.{split[1]}";
                    }
                    else {
                        newSemantics[key] = value;
                    }
                }

                yield return Create(grammar, lhsSymbol,
                                    rhs.Where(r => r != null).Select(grammar.GetOrCreateSymbol).ToList(),
                                    newSemantics);
            }
        }
    }

    /// <summary>
    ///     The rule used for annotator, should not be really used.
    /// </summary>
    public class AnnotatorRule : Rule {
        /// <summary>
        ///     Create an annotator rule.
        /// </summary>
        /// <param name="g"> Grammar </param>
        /// <param name="lhs"> Left hand token </param>
        public AnnotatorRule(Grammar g, Token lhs) :
            base(g, lhs, new Token[0], new Dictionary<string, string>()) { }

        /// <inheritdoc />
        /// <summary>
        ///     The apply method of annotator rule should not be used, should use <see cref="Annotator.Apply"/> instead.
        /// </summary>
        public override void Apply(List<Parse>[,] chart, IReadOnlyList<string> tokens, int start, int length) =>
            throw new NotImplementedException();
    }

    /// <summary>
    ///     The rule that obtain lexical values (raw string value quoted).
    /// </summary>
    public class LexicalRule : Rule {
        /// <summary>
        ///     Creates a lexical rule.
        /// </summary>
        /// <param name="g"> Grammar </param>
        /// <param name="lhs"> Left hand token </param>
        /// <param name="rhs"> Right hand tokens </param>
        /// <param name="semantics"> Semantics </param>
        internal LexicalRule(Grammar g, Token lhs, IReadOnlyList<Constant> rhs, IDictionary<string, string> semantics)
            : base(g, lhs, rhs, semantics) { }

        /// <inheritdoc />
        /// <summary>
        ///     Only apply to string constants.
        /// </summary>
        public override void Apply(List<Parse>[,] chart, IReadOnlyList<string> tokens, int start, int end) {
            List<string> constants = tokens.Skip(start).Take(end - start).ToList();
            if (Rhs.Select(e => e.Name).SequenceEqual(constants)) {
                string value = string.Join(" ", constants);
                chart[start, end]
                    .Add(new Parse(this, new Parse[0], new Dictionary<string, string> { { Parse.Value, value } }));
            }
        }
    }

    /// <summary>
    ///     The rule used for unary rules (only one token in right hand side).
    /// </summary>
    public class UnaryRule : Rule {
        /// <summary>
        ///     Creates a unary rule.
        /// </summary>
        /// <param name="g"> Grammar </param>
        /// <param name="lhs"> Left hand token </param>
        /// <param name="rhs"> Right hand token </param>
        /// <param name="semantics"> Semantics </param>
        internal UnaryRule(Grammar g, Token lhs, Token rhs, IDictionary<string, string> semantics)
            : base(g, lhs, new[] { rhs }, semantics) { }

        /// <inheritdoc />
        /// <summary>
        ///     Applies the unary rule. Note that we allow at most 2 error tokens before or after the right hand side token
        ///     and in total at most 3 error tokens.
        /// </summary>
        public override void Apply(List<Parse>[,] chart, IReadOnlyList<string> tokens, int start, int end) {
            var newEntry = new List<Parse>();
            for (int lefterrors = 0; lefterrors <= 2; ++lefterrors)
            for (int righterrors = 0; righterrors <= 3 - lefterrors; ++righterrors) {
                if (start + lefterrors >= end - righterrors) continue;
                int nstart = start + lefterrors, nend = end - righterrors;
                int errorlen = 0;
                for (int i = start; i < nstart; ++i) errorlen += tokens[i].Length;
                for (int i = nend; i < end; ++i) errorlen += tokens[i].Length;

                foreach (Parse parse in chart[nstart, nend]) {
                    if (parse.Rule.Lhs == Rhs[0]) {
                        IDictionary<string, string> newSemantics;
                        if (Semantics.Count == 0) {
                            // No explicit semantics, just delegate the parse semantics
                            newSemantics = parse.Semantics;
                        }
                        else {
                            // Calculate the semantics using the children parses
                            newSemantics = new Dictionary<string, string>(parse.Semantics);
                            foreach ((string key, string value) in Semantics) {
                                if (value.StartsWith("$0.")) {
                                    string accessKey = value.Substring(3);
                                    newSemantics[key] = newSemantics[accessKey];
                                }
                                else {
                                    newSemantics[key] = value;
                                }
                            }
                        }

                        newEntry.Add(new Parse(this, new List<Parse> { parse }, newSemantics,
                                               CalcFitScoreUnderErrors(
                                                   parse.FitScore, start, end, lefterrors + righterrors, errorlen)));
                    }
                }
            }

            chart[start, end].AddRange(newEntry);
        }
    }

    /// <summary>
    ///     Rule used for binary rules.
    /// </summary>
    public class BinaryRule : Rule {
        /// <summary>
        ///     Creates a binary rule.
        /// </summary>
        /// <param name="g"> Grammar </param>
        /// <param name="lhs"> Left hand token </param>
        /// <param name="rhs1"> Right hand token 1 </param>
        /// <param name="rhs2"> Right hand token 2 </param>
        /// <param name="semantics"> Semantics </param>
        internal BinaryRule(Grammar g, Token lhs, Token rhs1, Token rhs2, IDictionary<string, string> semantics)
            : base(g, lhs, new[] { rhs1, rhs2 }, semantics) { }

        private string UnionProperty(string a, string b) {
            return string.Join(";", a.Split(';').Union(b.Split(';')));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Applies the binary rule. Note that when applying two rhs tokens, we need to join their semantics.
        ///     For all properties other than ColName, we will override rhs1's semantic.
        ///     We allow at most 3 error tokens in total at left, right and middle of the binary rule. 
        /// </summary>
        public override void Apply(List<Parse>[,] chart, IReadOnlyList<string> tokens, int start, int end) {
            for (int lefterrors = 0; lefterrors <= 3; ++lefterrors) {
                for (int miderrors = 0; miderrors <= 3 - lefterrors; ++miderrors) {
                    for (int righterrors = 0; righterrors <= 3 - lefterrors - miderrors; ++righterrors) {
                        if (start + lefterrors + righterrors + miderrors >= end) {
                            continue;
                        }

                        int nstart = start + lefterrors, nend = end - righterrors;
                        for (int mid = nstart + 1; mid + miderrors < nend; mid++) {
                            int errorlen = 0;
                            for (int i = start; i < nstart; ++i) errorlen += tokens[i].Length;
                            for (int i = nend; i < end; ++i) errorlen += tokens[i].Length;
                            for (int i = mid; i < mid + miderrors; ++i) errorlen += tokens[i].Length;

                            var parseCombinations = chart[nstart, mid].CartesianProduct(chart[mid + miderrors, nend]);

                            foreach ((Parse parse1, Parse parse2) in parseCombinations) {
                                if (parse1.Rule.Lhs != Rhs[0] || parse2.Rule.Lhs != Rhs[1]) {
                                    continue;
                                }

                                var semantics = new Dictionary<string, string>(parse1.Semantics);

                                // Override parse1 semantics if necessary
                                double dupPenalty = 1;
                                foreach ((string key, string value) in parse2.Semantics) {
                                    // Penalty for repeated properties.
                                    if (key != Parse.Value && NonRepeated.Contains(key) &&
                                        semantics.ContainsKey(key) && key != "Value")
                                        dupPenalty *= 0.8;
                                    // Check if the property is Column Name.
                                    // TODO: Need to add an aggregated type for properties like ColName.
                                    if ((key == "ColName" && semantics.ContainsKey("ColName")) ||
                                        (key == "group_col") && semantics.ContainsKey("group_col")) {
                                        semantics[key] = UnionProperty(semantics[key], value);
                                    }
                                    else {
                                        if ((key == "filter_v" && semantics.ContainsKey("filter_v")) ||
                                            (key == "filter_op" && semantics.ContainsKey("filter_op")))
                                            semantics[key] = semantics[key] +";" + value;
                                        else semantics[key] = value;
                                    }
                                }

                                // Calculate the rule semantics
                                foreach ((string key, string value) in Semantics) {
                                    if (value.StartsWith("$0.")) {
                                        string accessKey = value.Substring(3);
                                        semantics[key] = parse1.Semantics[accessKey];
                                    }
                                    else if (value.StartsWith("$1.")) {
                                        string accessKey = value.Substring(3);
                                        semantics[key] = parse2.Semantics[accessKey];
                                    }
                                    else {
                                        semantics[key] = value;
                                    }
                                }

                                chart[start, end].Add(
                                    new Parse(this, new[] { parse1, parse2 }, semantics,
                                              dupPenalty *
                                              CalcFitScoreUnderErrors(
                                                  parse1.FitScore * parse2.FitScore, start, end,
                                                  lefterrors + miderrors + righterrors, errorlen)));
                            }
                        }
                    }
                }
            }
        }
    }
}