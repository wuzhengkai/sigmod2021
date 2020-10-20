using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the string that defines the format of marker type.
    /// </summary>
    public class MarkerFormatAnnotator : Annotator {
        /// <summary>
        ///     Predefined set of marker type in Matplotlib.
        ///     See https://matplotlib.org/api/_as_gen/matplotlib.pyplot.plot.html#matplotlib.pyplot.plot for reference.
        /// </summary>
        private static readonly Dictionary<string, string> MarkerType = new Dictionary<string, string>() {
            { "point", "." },
            { "dot", "." },
            { "pixel", "," },
            { "circle", "o" },
            { "triangle_down", "v" },
            { "triangle_up", "^" },
            { "triangle_left", "<" },
            { "triangle_right", ">" },
            { "tri_down", "1" },
            { "tri_up", "2" },
            { "tri_left", "3" },
            { "tri_right", "4" },
            { "square", "s" },
            { "pentagon", "p" },
            { "star", "*" },
            { "hexagon1", "h" },
            { "hexagon2", "H" },
            { "plus", "+" },
            { "x", "x" },
            { "diamond", "D" },
            { "thin_diamond", "d" },
            { "vline", "|" },
            { "hline", "_" }
        };

        private MarkerFormatAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="MarkerFormatAnnotator"/>.
        /// </summary>
        public static MarkerFormatAnnotator Instance { get; } = new MarkerFormatAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="MarkerFormatAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$markerfmt";

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token with the description text of marker type.
        ///     TODO: Add support for fuzzy description. 
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start != 1) return;
            if (!MarkerType.ContainsKey(rawTokens[start])) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> {
                                                { Parse.Value, MarkerType[rawTokens[start]] },
                                                { "marker", MarkerType[rawTokens[start]] }
                                            }));
        }
    }
}