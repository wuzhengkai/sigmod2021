using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//using Microsoft.ProgramSynthesis.Diagnostics;
//using Microsoft.ProgramSynthesis.Utils;
//using Microsoft.ProgramSynthesis.Utils.Interactive;

namespace Microsoft.ProgramSynthesis.Visualization.Grammar {
    public static class MyExtension
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey s, TValue t)
        {
            if (!dict.ContainsKey(s)) dict.Add(s, t);
            return dict[s];
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<T[]> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }

        public static IEnumerable<Tuple<T, T>> CartesianProduct<T>(this IEnumerable<T> sequences, IEnumerable<T> seq1)
        {
            return from l in sequences
                   from r in seq1
                   select new Tuple<T,T>(l, r);
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Item1;
            value = tuple.Item2;
        }
    }

    /// <summary>
    ///     The <see cref="Grammar"/> class is used to represent the DSL, which is a Context Free Language.
    ///     It creates the <see cref="Rule"/>s in BNF form from the language specification file.
    ///     Then the <see cref="Grammar.Parse"/> applies rules and <see cref="Annotator"/>s to the sequence of tokens using a slightly modified CYK algorithm.
    /// </summary>
    public class Grammar {
        private const char RuleSeparator = ';';
        private const string SideSeparator = ":=";
        private const char RhsSeparator = '|';
        private const char SemSeparator = '{';
        private const char KeyValueSeparator = ':';
        private const int CandidatesPerInterval = 20;

        internal const string Root = "Root";
        private readonly PorterStemmer _stemmer = new PorterStemmer();

        private readonly Dictionary<string, Token> _symbols = new Dictionary<string, Token>();

        /// <summary>
        ///     Creates the rules from the language spec file and array of <see cref="Annotator"/>s.
        /// </summary>
        /// <param name="grammar"> The string representations of rules defined in the DSL spec. </param>
        /// <param name="annotators"> The annotators to be applied in the parsing. </param>
        public Grammar(string grammar, params Annotator[] annotators) {
            Rules = CreateRules(grammar);
            Annotators = annotators.ToList();
        }

        private List<Rule> Rules { get; }

        private List<Annotator> Annotators { get; }

        /// <summary>
        ///     Parses a given user query under a certain context.
        /// /// </summary>
        /// <param name="input"> The string denoting the user query. Note that if user have multiple queries in one line separated by ";", it should be split before passed into this method. </param>
        /// <param name="context"> The current context in the notebook. </param>
        /// <returns> A string containing synthesized API call(s). </returns>
        public string Parse(String input, NbContext context) {
            if (String.IsNullOrEmpty(input)) input = "overview";
            Parse[] res = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => ParseSingle(x, context)).ToArray();
            foreach (Parse s in res) {
                //Console.WriteLine(s.DebuggerDisplay);
                s.SynthesizeCall();
            }
            string result = String.Join("\n", res.Select(x => x.Code));
            //Console.WriteLine(result);
            return result;
        }

        /// <summary>
        ///     Parses a single given user query under a certain context.
        ///     Using a modified CYK algorithm. The difference is the annotator which is independent on rules.
        ///     Since we have fitScore for each parse, to avoid exponential total results, we keep parse with 20 highest fitScore in each interval.
        /// </summary>
        /// <param name="input"> The string denoting the user query. Note that if user have multiple queries in one line separated by ";", it should be split before passed into this method. </param>
        /// <param name="context"> The current context in the notebook. </param>
        /// <returns> A parse result with all the properties that needed to synthesize API call(s). </returns>
        public Parse ParseSingle(string input, NbContext context) {
            input = input.TrimEnd(new char[] { '.', '?' });
            string[] rawTokens = input.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(token => !StopWords.List.Contains(token)).ToArray();
            string[] tokens = rawTokens.Select(x => x.ToLower()).Select(_stemmer.StemWord).ToArray();

            var chart = new List<Parse>[tokens.Length + 1, tokens.Length + 1];
            for (int end = 1; end < tokens.Length + 1; end++) {
                for (int start = end - 1; start >= 0; start--) {
                    var list = new List<Parse>();
                    chart[start, end] = list;
                    Annotators.ForEach(annotator =>
                                           annotator.Annotate(this, chart, tokens, start, end, context, rawTokens));
                    Rules.ForEach(rule => rule.Apply(chart, tokens, start, end));
                    list = new List<Parse>();
                    chart[start, end].Sort((x, y) => y.FitScore.CompareTo(x.FitScore));
                    foreach (Parse s in chart[start, end]) {
                        bool flag = true;
                        foreach (Parse t in list)
                            if (t.IsSameParseResult(s)) {
                                flag = false;
                                break;
                            }

                        if (flag) list.Add(s);
                        if (list.Count >= CandidatesPerInterval) break;
                    }

                    chart[start, end] = list;
                }
            }

            var newChart = chart[0, tokens.Length].Where(parse => parse.Rule.Lhs.Name == Root).ToArray();
            Array.Sort(newChart, (x, y) => y.FitScore.CompareTo(x.FitScore));
            //for (int i= 0;i<20;++i)
            //    Console.WriteLine(chart[0, tokens.Length][i].DebuggerDisplay);
            if (newChart.Length > 0) {
                var result = newChart[0];
                result.SetContext(context);
                //Console.WriteLine(result.DebuggerDisplay);
                return result;
            } 
            else {
                var result = new Parse(null, null, new Dictionary<string, string>());
                result.SetContext(context);
                return result;
            }
        }

        private List<Rule> CreateRules(string grammar) {
            var rules = new List<Rule>();

            string[] ruleStrings = SplitAndTrim(grammar, RuleSeparator);
            for (int i = ruleStrings.Length - 1; i >= 0; i--) {
                string rule = ruleStrings[i];

                string[] split = SplitAndTrim(rule, SideSeparator);
                if (split.Length != 2) {
                    throw new ArgumentException($"The following rule is invalid: {rule}");
                }

                string lhs = split[0];
                foreach (string rhs in SplitAndTrim(split[1], RhsSeparator)) {
                    // rhs = val { key : value}
                    string[] semanticSplit = rhs.Split(SemSeparator);

                    string[] rhsElement = SplitAndTrim(semanticSplit[0], ' ');

                    var semantics = new Dictionary<string, string>();
                    if (semanticSplit.Length == 2) {
                        // Handle semantics
                        string[] keyValueSplit = SplitAndTrim(semanticSplit[1], KeyValueSeparator);
                        if (keyValueSplit.Length != 2) {
                            throw new ArgumentException($"The semantics of the following rule is invalid: {rule}");
                        }

                        string key = keyValueSplit[0];
                        string value = keyValueSplit[1].TrimEnd('}').TrimEnd();
                        semantics[key] = value;
                    }

                    rules.AddRange(Rule.Create(this, lhs, rhsElement, semantics));
                }
            }

            return rules;
        }

        internal Token GetOrCreateSymbol(string s) {
            if (string.IsNullOrWhiteSpace(s)) {
                throw new ArgumentException($"Token is white-space.");
            }

            if (s.StartsWith("$")) {
                return _symbols.GetOrAdd(s, new Token(s));
            }

            if (s.Length > 2 && s.StartsWith("\"") && s.EndsWith("\"")) {
                string sub = RemoveQuote(s);
                if (string.IsNullOrWhiteSpace(sub)) {
                    throw new ArgumentException($"Quoted string is empty");
                }

                return _symbols.GetOrAdd(s, new Constant(sub));
            }

            return _symbols.GetOrAdd(s, new Token(s));
        }

        private static string RemoveQuote(string s) {
            if (s.Length < 2 || s[0] != '"' || s[s.Length - 1] != '"') {
                throw new ArgumentException($"The string {s} is not quoted");
            }

            return s.Substring(1, s.Length - 2);
        }

        private static string[] SplitAndTrim(string s, char c) {
            return s.Split(new[] { c }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToArray();
        }

        private static string[] SplitAndTrim(string s, string sep) {
            return Regex.Split(s, sep).Select(e => e.Trim()).Where(e => e != string.Empty).ToArray();
        }
    }
}