using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates a numerical value from given token.
    /// </summary>
    public class NumberAnnotator : Annotator {
        private NumberAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="NumberAnnotator"/>.
        /// </summary>
        public static NumberAnnotator Instance { get; } = new NumberAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="NumberAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$number";

        /// <inheritdoc />
        /// <summary>
        ///     Uses <see cref="double.TryParse"/> to turn the token into a numerical value.
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start != 1) return;
            if (!double.TryParse(tokens[start], out double number)) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end]
                .Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                               new Dictionary<string, string> { { Parse.Value, number.ToString() } }));
        }
    }
}