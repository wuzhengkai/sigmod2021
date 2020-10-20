using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.ProgramSynthesis.Visualization;

namespace Microsoft.ProgramSynthesis.Visualization.Tests
{
    [TestFixture]
    public class Tests
    {
        private const string TestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_semantic.json";

        public static TestCase[] GetAllTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(TestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetAllTests))]
        public void Test(TestCase testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string DataTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_extended.json";

        public static TestCase_Ext[] GetAllDataTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(DataTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Ext[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetAllDataTests))]
        public void TestData(TestCase_Ext testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string TutorialTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_tutorial.json";

        public static TestCase_Paper[] GetTutorialTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(TutorialTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetTutorialTests))]
        public void TestTutorial(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string HistEasyTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_histeasy.json";

        public static TestCase_Paper[] GetHistEasyTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(HistEasyTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetHistEasyTests))]
        public void TestHistEasy(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string HistHardTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_histhard.json";

        public static TestCase_Paper[] GetHistHardTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(HistHardTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetHistHardTests))]
        public void TestHistHard(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string ScatterEasyTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_scattereasy.json";

        public static TestCase_Paper[] GetScatterEasyTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(ScatterEasyTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetScatterEasyTests))]
        public void TestScatterEasy(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string ScatterHardTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_scatterhard.json";

        public static TestCase_Paper[] GetScatterHardTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(ScatterHardTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetScatterHardTests))]
        public void TestScatterHard(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min,
                                        testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string LineEasyTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_lineasy.json";

        public static TestCase_Paper[] GetLineEasyTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(LineEasyTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetLineEasyTests))]
        public void TestLineEasy(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        private const string LineHardTestFileString =
            "Microsoft.ProgramSynthesis.Visualization.Tests.TestData.tests_linehard.json";

        public static TestCase_Paper[] GetLineHardTests()
        {
            using (Stream resourceStream =
                typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(LineHardTestFileString))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    return JsonConvert.DeserializeObject<TestCase_Paper[]>(resourceReader.ReadToEnd());
                }
            }
        }

        [TestCaseSource(nameof(GetLineHardTests))]
        public void TestLineHard(TestCase_Paper testCase)
        {
            if (testCase.IgnoredReason != null)
            {
                Assert.Ignore(testCase.IgnoredReason);
            }

            var context = new NbContext(testCase.Dataframe, testCase.Columns, testCase.Types, testCase.Min, testCase.Max);
            string res = VisualizationGrammar.Instance.Parse(testCase.Query, context);
            Console.WriteLine(res);
            Assert.AreEqual(testCase.Output, res);
        }

        [TestCase]
        public void TestSingle()
        {
            Parse parse = VisualizationGrammar.Instance.ParseSingle(
                "histogram MetaCritic bin from 5 to 20 step 5",
                new NbContext("movies", new string[] { "MetaCritic" }));

            Console.WriteLine(parse.DebuggerDisplay);
            parse.SynthesizeCall();
            Console.WriteLine(parse.Code);
            Assert.That(parse, Is.Not.Null);
        }

        public struct TestCase
        {
            public string Name;

            public string Query;

            public string Output;

            public string IgnoredReason;

            public string Dataframe;

            public string[] Columns;

            public override string ToString() => Name;
        }

        public struct TestCase_Ext
        {
            public string Name;

            public string Query;

            public string Output;

            public string IgnoredReason;

            public string Dataframe;

            public string[] Columns;

            public string[] Types;

            public string[] Min;

            public string[] Max;

            public override string ToString() => Name;
        }

        public struct TestCase_Paper
        {
            public string Name;

            public string Query;

            public string Output;

            public string IgnoredReason;

            public string ResultCategory;

            public string Level;

            public string Dataframe;

            public string[] Columns;

            public string[] Types;

            public string[] Min;

            public string[] Max;

            public override string ToString() => Name;
        }
    }
}