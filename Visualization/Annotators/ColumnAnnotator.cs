using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates column names.
    ///     Matches consecutive tokens with the column name get from <c>NbContext</c>.
    ///     Uses editing distance to give <c>fitScore</c> to show the confidence in a certain matching.
    /// </summary>
    public class ColumnAnnotator : Annotator {
        /// <summary>
        ///     Any matching with a similarity lower than the threshold would be discarded.
        /// </summary>
        protected static readonly double ColumnNameThreshold = 0.5;
        protected ColumnAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="ColumnAnnotator"/>.
        /// </summary>
        public static ColumnAnnotator Instance { get; } = new ColumnAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="ColumnAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$column";

        /// <summary>
        ///     Calculates the editing distance between string s and t.
        ///     Note that the cost of changing one character is 2, while adding or removing character is 1.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns> An integer denoting the editing distance between string s and t. The result is non-negative. </returns>
        protected static int ComputeEditingDistance(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            s = s.ToLower();
            t = t.ToLower();

            // Step 1
            if (n == 0) {
                return m;
            }

            if (m == 0) {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) { }

            for (int j = 0; j <= m; d[0, j] = j++) { }

            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = t[j - 1] == s[i - 1] ? 0 : 2;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        /// <summary>
        ///     Calculates the similarity score between string a and t.
        ///     Similarity is defined by editing distance divided by the length of string t.
        /// </summary>
        /// <param name="s"> The string of the matched part in the query. </param>
        /// <param name="t"> The string of column name. </param>
        /// <returns> A double denoting the similarity score. The result falls in range [0,1]. </returns>
        protected static double Similarity(string s, string t) {
            double res = ComputeEditingDistance(s, t);
            return 1.0 - res / t.Length;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Annotates the consecutive tokens by trying to match with column names.
        ///     Chooses the column name that has the highest similarity. Throw the results if highest similarity is under 0.5
        ///     FitScore of the resulting rule is set to be the similarity.
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            string s = string.Join(" ", rawTokens.ToArray(), start, end - start);

            double fitScore = 0;
            string colName = "";

            foreach (string t in context.RawColumnNames.Concat(context.RawSingleFrameNames)) {
                if (t.Length * 2 < s.Length) continue;
                double cur = Similarity(s, t);
                if (cur > fitScore) {
                    fitScore = cur;
                    colName = t;
                }
            }

            // Avoid add too much column annotations.
            if (fitScore < ColumnNameThreshold) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> { { "ColName", colName } }, fitScore));
        }
    }
}