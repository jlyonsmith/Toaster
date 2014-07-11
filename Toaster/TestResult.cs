using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.ObjectModel;
using System.CodeDom.Compiler;

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

        private static Regex stackRegex = new Regex("^  at (?'method'.*?) in (?'file'.*?):(?'line'[0-9]+)", RegexOptions.Multiline);

        public void WriteResults(TextWriter writer)
        {
            TsonTextWriter wr = new TsonTextWriter(writer);

            wr.WriteProperty("StartDate", this.StartDate.ToShortDateString());
            wr.WriteProperty("StartTime", this.StartTime.ToString());
            wr.WriteProperty("EndDate", this.EndDate.ToShortDateString());
            wr.WriteProperty("EndTime", this.EndTime.ToString());
            wr.WriteProperty("TestCount", this.TestCount.ToString());
            wr.WriteProperty("PassedTests", this.PassedTests.ToString());
            wr.WriteProperty("FailedTests", this.FailedTests.ToString());
            wr.WriteProperty("SkippedTests", this.SkippedTests.ToString());

            if (TestAssembly != null)
            {
                wr.WriteObjectPropertyStart("TestAssembly");
                wr.WriteProperty("Name", this.TestAssembly.Name);
                wr.WriteProperty("ExecutionTime", this.TestAssembly.ExecutionTime.ToString());

                if (this.TestAssembly.ExecutionCycles != 0)
                    wr.WriteProperty("TotalCycles", this.TestAssembly.ExecutionCycles.ToString());

                WriteTestMethod(wr, "AssemblyInitializeMethod", this.TestAssembly.AssemblyInitializeMethod);

                wr.WriteArrayPropertyStart("TestClasses");

                foreach (var testClass in this.TestAssembly.TestClasses_Internal)
                {
                    wr.WriteArrayObjectStart();

                    wr.WriteProperty("Name", testClass.Name);
                    wr.WriteProperty("ExecutionTime", testClass.ExecutionTime.ToString());

                    if (testClass.ExecutionCycles != 0)
                        wr.WriteProperty("TotalCycles", testClass.ExecutionCycles.ToString());

                    wr.WriteProperty("Order", testClass.Order.ToString());

                    WriteTestMethod(wr, "ClassInitializeMethod", testClass.ClassInitializeMethod);

                    wr.WriteArrayPropertyStart("TestMethods");

                    foreach (var testMethod in testClass.TestMethods)
                    {
                        wr.WriteArrayObjectStart();

                        WriteTestMethod(wr, "TestInitializeMethod", testMethod.TestInitializeMethod);
                        WriteTestMethod(wr, "TestMethod", testMethod);
                        WriteTestMethod(wr, "TestCleanupMethod", testMethod.TestCleanupMethod);

                        wr.WriteArrayObjectEnd();
                    }

                    wr.WriteArrayPropertyEnd();  // TestMethods

                    WriteTestMethod(wr, "ClassCleanupMethod", testClass.ClassCleanupMethod);

                    wr.WriteObjectPropertyEnd();   // TestClass
                }

                wr.WriteArrayPropertyEnd(); // TestClasses

                WriteTestMethod(wr, "AssemblyCleanupMethod", this.TestAssembly.AssemblyCleanupMethod);

                wr.WriteObjectPropertyEnd(); // TestAssembly
            }

            wr.WriteLine();
        }

        private void WriteTestMethod(TsonTextWriter wr, string canonicalMethodName, TestMethod testMethod)
        {
            if (testMethod == null)
                return;

            wr.WriteObjectPropertyStart(canonicalMethodName);

            wr.WriteProperty("Name", testMethod.Name);
            wr.WriteProperty("Outcome", testMethod.Outcome.ToString());
            wr.WriteProperty("ExecutionTime", testMethod.ExecutionTime.ToString());

            if (testMethod.ExecutionCycles != 0)
                wr.WriteProperty("TotalCycles", testMethod.ExecutionCycles.ToString());

            ExplicitTestMethod explicitTestMethod = testMethod as ExplicitTestMethod;

            if (explicitTestMethod != null)
                wr.WriteProperty("Order", explicitTestMethod.Order.ToString());

            if (!String.IsNullOrEmpty(testMethod.Output))
            {
                wr.WriteProperty("Output", testMethod.Output);
            }

            if (testMethod.Exception != null)
            {
                wr.WriteObjectPropertyStart("Failure");

                wr.WriteProperty("Message", testMethod.Exception.Message);

                wr.WriteArrayPropertyStart("Stack");

                string stackTrace = testMethod.Exception.StackTrace;

                if (stackTrace != null)
                {
                    Match m = stackRegex.Match(stackTrace);

                    while (m.Success)
                    {
                        wr.WriteArrayObjectStart();
                        wr.WriteProperty("FileName", m.Groups[2].Value);
                        wr.WriteProperty("MethodName", m.Groups[1].Value);
                        wr.WriteProperty("Line", m.Groups[3].Value);
                        wr.WriteArrayObjectEnd();
                        m = m.NextMatch();
                    }
                }

                wr.WriteArrayPropertyEnd(); // Stack

                wr.WriteObjectPropertyEnd(); // Failure
            }

            wr.WriteObjectPropertyEnd(); // TestMethod
        }
    }
}
