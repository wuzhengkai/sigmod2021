using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the aggregated operators used in group by.
    /// </summary>
    public class AggregatedOperatorAnnotator : Annotator {
        private static readonly Dictionary<string, string> OperatorType = new Dictionary<string, string>() {
            { "mean", "mean" },
            { "sum", "sum"},
            { "averag", "mean" },
            { "avg", "mean" },
            { "count", "count" },
            { "standard deviat", "std" },
            { "standard error", "std" },
            { "std", "std" },
            { "minimum", "min" },
            { "min", "min" },
            { "maximum", "maximum" },
            { "max", "max" }
        };

        private AggregatedOperatorAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="AggregatedOperatorAnnotator"/>.
        /// </summary>
        public static AggregatedOperatorAnnotator Instance { get; } = new AggregatedOperatorAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="AggregatedOperatorAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$aggreop";

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token with the description text of aggregated function names.
        ///     TODO: Add support for fuzzy description. 
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start > 2) return;
            string cur = tokens[start];
            if (end == start + 2) cur += " " + tokens[start + 1];
            if (!OperatorType.ContainsKey(cur)) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> {
                                                { Parse.Value, OperatorType[cur] },
                                            }));
        }
    }
}