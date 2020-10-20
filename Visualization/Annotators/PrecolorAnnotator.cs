using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the string that defines the format of predefined color.
    /// </summary>
    public class PrecolorAnnotator : Annotator {
        /// <summary>
        ///     Predefined set of colors in Matplotlib.
        ///     See https://matplotlib.org/api/_as_gen/matplotlib.pyplot.plot.html#matplotlib.pyplot.plot for reference.
        /// </summary>
        private static readonly HashSet<string> Precolorlist =
            new HashSet<string>(new string[] { "white", "cyan", "black", "blue", "green", "red", "yellow", "magenta" });

        private PrecolorAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="PrecolorAnnotator"/>
        /// </summary>
        public static PrecolorAnnotator Instance { get; } = new PrecolorAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="PrecolorAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$precolor";

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token with the description text of predefined colors.
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start != 1) return;
            if (!Precolorlist.Contains(tokens[start])) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end]
                .Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                               new Dictionary<string, string> {
                                   { Parse.Value, tokens[start].Substring(0, 1) },
                                   { "color", tokens[start].Substring(0, 1) }
                               }));
        }
    }
}