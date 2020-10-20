using System.Collections.Generic;

namespace Microsoft.ProgramSynthesis.Visualization.Grammar {
    /// <summary>
    ///     Abstract class of Annotator.
    ///     Must have a representation in grammar and how to create a parse from a range of tokens.
    /// </summary>
    public abstract class Annotator {
        /// <summary>
        ///     A string that represents the annotator in grammar.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Annotates the desired parse result from a range of tokens.
        /// </summary>
        /// <param name="g"> The grammar. </param>
        /// <param name="chart"> The chart to store all parsing results. </param>
        /// <param name="tokens"> Tokens in the query. </param>
        /// <param name="start"> The starting position of the range (included). </param>
        /// <param name="end"> The ending position of the range (excluded). </param>
        /// <param name="context"> The current context in the notebook. </param>
        /// <param name="rawTokens"> The unstemmed token sequence. </param>
        public abstract void Annotate(Grammar g, List<Parse>[,] chart, IReadOnlyList<string> tokens, int start,
                                      int end, NbContext context, IReadOnlyList<string> rawTokens);
    }
}