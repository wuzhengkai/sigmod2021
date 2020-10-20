using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates the a constant quoted string value.
    /// </summary>
    public class QuotedStringAnnotator : Annotator {
        private QuotedStringAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="QuotedStringAnnotator"/>.
        /// </summary>
        public static QuotedStringAnnotator Instance { get; } = new QuotedStringAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="QuotedStringAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$quotedstring";

        /// <inheritdoc />
        /// <summary>
        ///     Matches the token in the format "s".
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start != 1) return;
            string cur = tokens[start];
            if (!cur.StartsWith("\"") || !cur.EndsWith("\"")) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> {
                                                { Parse.Value, cur },
                                            }));
        }
    }
}