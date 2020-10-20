using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization.Annotators {
    /// <summary>
    ///     Annotates a list of numerical value from given tokens.
    /// </summary>
    public class NumberListAnnotator : Annotator {
        private NumberListAnnotator() { }

        /// <summary>
        ///     The instance of <see cref="NumberListAnnotator"/>.
        /// </summary>
        public static NumberListAnnotator Instance { get; } = new NumberListAnnotator();

        /// <summary>
        ///     The symbol used in grammar to represent <see cref="NumberListAnnotator"/>
        /// </summary>
        public override string Name { get; } = "$number_list";

        /// <inheritdoc />
        /// <summary>
        ///     Annotates a list of numerical values.
        ///     Requirement: 1. No less than 3 numbers in the range. 2. The start and the end must be number. 3. No more than 1 non-numerical value.
        ///     TODO: Support cases like 10,20,30,...,60.
        /// </summary>
        public override void Annotate(Grammar.Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens) {
            if (end - start < 3) return;
            if (!double.TryParse(tokens[start], out double head)) return;
            if (!double.TryParse(tokens[end - 1], out double tail)) return;

            string res = head.ToString();
            int cnt = 0;
            for (int i = start + 1; i < end; ++i) {
                if (!double.TryParse(tokens[i], out double temp)) cnt++;
                else res += "," + temp;
            }

            if (cnt > 1) return;
            if (end - start - cnt < 3) return;

            Token symbol = g.GetOrCreateSymbol(Name);
            chart[start, end].Add(new Parse(new AnnotatorRule(g, symbol), new Parse[0],
                                            new Dictionary<string, string> { { Parse.Value, res } },
                                            (double) (end - start - cnt) / (end - start)));
        }
    }
}