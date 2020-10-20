using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the string that defines the format of line style.
    /// </summary>
    public class LineFormatAnnotator : Annotator {
        /// <summary>
        ///     Predefined set of line style in Matplotlib.
        ///     See https://matplotlib.org/api/_as_gen/matplotlib.pyplot.plot.html#matplotlib.pyplot.plot for reference.
        /// </summary>
        private static readonly Dictionary<string, string> LineType = new Dictionary<string, string>() {
            { "solid", "-" }, { "dashed", "--" }, { "dash-dot", "-." }, { "dotted", ":" }, { "dot-dash", "-."}
        };

        private LineFormatAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="LineFormatAnnotator"/>.
        /// </summary>
        public static LineFormatAnnotator Instance { get; } = new LineFormatAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="LineFormatAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$linefmt";

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token with the description text of line format.
        ///     TODO: Add support for fuzzy description. 
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start != 1) return;
            if (!LineType.ContainsKey(rawTokens[start])) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> {
                                                { Parse.Value, LineType[rawTokens[start]] },
                                                { "line", LineType[rawTokens[start]] }
                                            }));
        }
    }
}