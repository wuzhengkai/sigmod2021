using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the operator used in filtering.
    /// </summary>
    public class OperatorAnnotator : Annotator {
        private static readonly Dictionary<string, string> OperatorType = new Dictionary<string, string>() {
            { "greater", "gt" },
            { "larger", "gt"},
            { "more", "gt" },
            { "smaller", "lt" },
            { "less", "lt" },
            { "equal", "eq" },
            { "not", "nt" },
            { "=", "eq" }
        };

        private static readonly Dictionary<string, string> OperatorSymbol = new Dictionary<string, string>() {
            { "lt", "<" },
            { "le", "<=" },
            { "gt", ">"},
            { "ge", ">=" },
            { "eq", "==" },
            { "ne", "!="}
        };

        private static readonly HashSet<string> RawOperatorList = new HashSet<string>() {
            "<", "<=", ">", ">=", "==", "!="
        };

        private OperatorAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="OperatorAnnotator"/>.
        /// </summary>
        public static OperatorAnnotator Instance { get; } = new OperatorAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="OperatorAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$operator";

        private string Analyze(string p, string q) {
            if (!OperatorType.ContainsKey(p) || !OperatorType.ContainsKey(q)) return "NA";
            p = OperatorType[p]; q = OperatorType[q];
            if (p == q) {
                if (p == "nt") return "NA";
                else return p;
            }
            else {
                if (p == "nt") {
                    if (q == "gt") return "le";
                    if (q == "lt") return "ge";
                    if (q == "eq") return "ne";
                    return "NA";
                }

                if (p == "gt" && q == "eq") return "ge";
                if (p == "lt" && q == "eq") return "le";
                if (q == "gt" && p == "eq") return "ge";
                if (q == "lt" && p == "eq") return "le";

                return "NA";
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token with the description text of filtering operator.
        ///     TODO: Add support for fuzzy description. 
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start > 2) return;
            string cur1 = tokens[start], cur2 = cur1;
            if (end == start + 2) cur2 = tokens[start + 1];
            string res = Analyze(cur1, cur2);

            if (RawOperatorList.Contains(cur1)) res = cur1;
            else {
                if (res == "NA") return;
                res = OperatorSymbol[res];
            }

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> {
                                                { Parse.Value, res },
                                            }));
        }
    }
}