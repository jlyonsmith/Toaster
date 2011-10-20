using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.ObjectModel;

namespace Toaster
{
    public class TestResult
    {
        public DateTime StartDate { get; internal set; }
        public TimeSpan StartTime { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public TimeSpan EndTime { get; internal set; }
        public int FailedTests { get { return TestAssembly == null ? 0 : TestAssembly.TestOutcomeCount(UnitTestOutcome.Failed); } }
        public int PassedTests { get { return TestAssembly == null ? 0 : TestAssembly.TestOutcomeCount(UnitTestOutcome.Passed); } }
        public int SkippedTests { get { return TestAssembly == null ? 0 : TestAssembly.TestOutcomeCount(UnitTestOutcome.Skipped); } }
        public bool Success
        {
            get
            {
                if (TestAssembly == null)
                    return true;

                int total = TestAssembly.TestCount;
                int passed = TestAssembly.TestOutcomeCount(UnitTestOutcome.Passed);
                int skipped = TestAssembly.TestOutcomeCount(UnitTestOutcome.Skipped);

                return (total == passed + skipped);
            }
        }
        public TestAssembly TestAssembly { get; internal set; }
        public int TestCount 
        {
            get 
            {
                return (TestAssembly != null ? TestAssembly.TestCount : 0);
            }
        }

        private static Regex stackRegex = new Regex("^   at (?'method'.*?) in (?'file'.*?):line (?'line'[0-9]+)", RegexOptions.Multiline);

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();

            using (XmlTextWriter wr = new XmlTextWriter(new StringWriter(sb)))
            {
                wr.Formatting = Formatting.Indented;
                wr.IndentChar = ' ';
                wr.Indentation = 2;

                this.ToXml(wr);

                wr.Flush();
            }

            return sb.ToString();
        }

        public void ToXml(XmlTextWriter wr)
        {
            wr.WriteStartDocument();

            wr.WriteStartElement("TestResult");
            wr.WriteAttributeString("StartDate", this.StartDate.ToShortDateString());
            wr.WriteAttributeString("StartTime", this.StartTime.ToString());
            wr.WriteAttributeString("EndDate", this.EndDate.ToShortDateString());
            wr.WriteAttributeString("EndTime", this.EndTime.ToString());
            wr.WriteAttributeString("TestCount", this.TestCount.ToString());
            wr.WriteAttributeString("PassedTests", this.PassedTests.ToString());
            wr.WriteAttributeString("FailedTests", this.FailedTests.ToString());
            wr.WriteAttributeString("SkippedTests", this.SkippedTests.ToString());

            if (TestAssembly != null)
            {
                wr.WriteStartElement("TestAssembly");
                wr.WriteAttributeString("Name", this.TestAssembly.Name);
                wr.WriteAttributeString("ExecutionTime", this.TestAssembly.ExecutionTime.ToString());

                if (this.TestAssembly.ExecutionCycles != 0)
                    wr.WriteAttributeString("TotalCycles", this.TestAssembly.ExecutionCycles.ToString());

                TestMethodToXml(wr, this.TestAssembly.AssemblyInitializeMethod);

                foreach (var testClass in this.TestAssembly.TestClasses_Internal)
                {
                    wr.WriteStartElement("TestClass");
                    wr.WriteAttributeString("Name", testClass.Name);
                    wr.WriteAttributeString("ExecutionTime", testClass.ExecutionTime.ToString());

                    if (testClass.ExecutionCycles != 0)
                        wr.WriteAttributeString("TotalCycles", testClass.ExecutionCycles.ToString());

                    wr.WriteAttributeString("Order", testClass.Order.ToString());

                    TestMethodToXml(wr, testClass.ClassInitializeMethod);

                    foreach (var testMethod in testClass.TestMethods)
                    {
                        TestMethodToXml(wr, testMethod.TestInitializeMethod);

                        TestMethodToXml(wr, testMethod);

                        TestMethodToXml(wr, testMethod.TestCleanupMethod);
                    }

                    TestMethodToXml(wr, testClass.ClassCleanupMethod);

                    wr.WriteEndElement();   // TestClass
                }

                TestMethodToXml(wr, this.TestAssembly.AssemblyCleanupMethod);

                wr.WriteEndElement(); // TestAssembly
            }

            wr.WriteEndElement(); // TestResult
        }

        private void TestMethodToXml(XmlTextWriter wr, TestMethod testMethod)
        {
            if (testMethod == null)
                return;

            wr.WriteStartElement("TestMethod");
            wr.WriteAttributeString("Name", testMethod.Name);
            wr.WriteAttributeString("Outcome", testMethod.Outcome.ToString());
            wr.WriteAttributeString("ExecutionTime", testMethod.ExecutionTime.ToString());

            if (testMethod.ExecutionCycles != 0)
                wr.WriteAttributeString("TotalCycles", testMethod.ExecutionCycles.ToString());

            ExplicitTestMethod explicitTestMethod = testMethod as ExplicitTestMethod;

            if (explicitTestMethod != null)
                wr.WriteAttributeString("Order", explicitTestMethod.Order.ToString());

            if (!String.IsNullOrEmpty(testMethod.Output))
            {
                wr.WriteStartElement("Output");
                wr.WriteString(testMethod.Output);
                wr.WriteEndElement();
            }

            if (testMethod.Exception != null)
            {
                wr.WriteStartElement("Failure");
                wr.WriteStartElement("Message");
                wr.WriteString(testMethod.Exception.Message);
                wr.WriteEndElement(); // Message

                wr.WriteStartElement("Stack");
                string stackTrace = testMethod.Exception.StackTrace;

                if (stackTrace != null)
                {
                    Match m = stackRegex.Match(stackTrace);

                    while (m.Success)
                    {
                        wr.WriteStartElement("StackFrame");
                        wr.WriteAttributeString("FileName", m.Groups[2].Value);
                        wr.WriteAttributeString("MethodName", m.Groups[1].Value);
                        wr.WriteAttributeString("Line", m.Groups[3].Value);
                        wr.WriteEndElement();

                        m = m.NextMatch();
                    }
                }

                wr.WriteEndElement(); // Stack

                wr.WriteEndElement(); // Failure
            }

            wr.WriteEndElement(); // TestMethod
        }
    }
}
