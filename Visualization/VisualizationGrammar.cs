using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.ProgramSynthesis.Visualization.Annotators;
using Microsoft.ProgramSynthesis.Visualization.Grammar;
using System;

namespace Microsoft.ProgramSynthesis.Visualization {
    /// <summary>
    ///     The wrap up of grammar used in visualization scenario.
    /// </summary>
    public class VisualizationGrammar {
        private VisualizationGrammar() {
            var annotators = new Annotator[] {
                NumberAnnotator.Instance, NumberListAnnotator.Instance, ColumnAnnotator.Instance,
                AuxColumnAnnotator.Instance, PrecolorAnnotator.Instance, MarkerFormatAnnotator.Instance,
                LineFormatAnnotator.Instance, QuotedStringAnnotator.Instance, OperatorAnnotator.Instance,
                AggregatedOperatorAnnotator.Instance
            };

            string grammarpath = "D:\\msr\\visualize\\Visualization\\Visualization.grammar";

            if (!File.Exists(grammarpath))
            {
                Console.Out.WriteLine("No file");
                return;
            }

            string grammarText = File.ReadAllText(grammarpath);

            Grammar = new Grammar.Grammar(grammarText, annotators);
        }

        /// <summary>
        ///     Instance for <see cref="VisualizationGrammar"/>.
        /// </summary>
        public static VisualizationGrammar Instance { get; } = new VisualizationGrammar();

        private Grammar.Grammar Grammar { get; }

        /// <summary>
        ///     Parses the given input under context.
        /// </summary>
        /// <param name="input"> Input query </param>
        /// <param name="context"> Context </param>
        /// <returns> A parsing result. </returns>
        public string Parse(string input, NbContext context) => Grammar.Parse(input, context);

        public Parse ParseSingle(string input, NbContext context) => Grammar.ParseSingle(input, context);
    }
}