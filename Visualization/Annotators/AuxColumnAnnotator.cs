using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates auxiliary column names.
    ///     Similar to <see cref="ColumnAnnotator"/>, annotates a column name by matching with data column titles.
    ///     Annotates column names that are not to be plotted but used as other parameters like <c>s</c> in
    ///     <c>plt.scatter(x, y, s = MarkerSizeArray)</c>.
    /// </summary>
    public class AuxColumnAnnotator : ColumnAnnotator {
        private AuxColumnAnnotator() { }

        /// <summary>
        ///     Instance of <see cref="AuxColumnAnnotator"/>.
        /// </summary>
        public static new AuxColumnAnnotator Instance { get; } = new AuxColumnAnnotator();

        /// <inheritdoc />
        /// <summary>
        ///     The symbol used in grammar to represent <see cref="AuxColumnAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$auxcolumn";

        /// <inheritdoc />
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
                                            new Dictionary<string, string> { { "Value", colName } }, fitScore));
        }
    }
}