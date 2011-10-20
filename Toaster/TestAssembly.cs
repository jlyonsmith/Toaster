using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Toaster
{
    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly)
        {
            this.Assembly = assembly;
        }

        public Assembly Assembly { get; private set; }
        public string Name { get { return this.Assembly.FullName; } }
        public TestMethod AssemblyInitializeMethod { get; internal set; }
        public TestMethod AssemblyCleanupMethod { get; internal set; }
        public TimeSpan ExecutionTime { get; internal set; }
        public long ExecutionCycles { get; internal set; }
        public int TestCount
        {
            get
            {
                int count = 0;

                foreach (var testClass in testClasses_Internal)
                {
                    count += testClass.TestCount;
                }

                return count +
                    (AssemblyInitializeMethod != null ? 1 : 0) +
                    (AssemblyCleanupMethod != null ? 1 : 0);
            }
        }

        public int TestOutcomeCount(UnitTestOutcome outcome)
        {
            int count = 0;

            foreach (var testClass in testClasses_Internal)
            {
                count += testClass.GetTestOutcomeCount(outcome);
            }

            return count +
                (AssemblyInitializeMethod != null && AssemblyInitializeMethod.Outcome == outcome ? 1 : 0) +
                (AssemblyCleanupMethod != null && AssemblyCleanupMethod.Outcome == outcome ? 1 : 0);
        }

        private List<TestClass> testClasses_Internal;
        private ReadOnlyCollection<TestClass> testClasses;

        internal List<TestClass> TestClasses_Internal
        {
            get
            {
                if (testClasses_Internal == null)
                    testClasses_Internal = new List<TestClass>();

                return testClasses_Internal;
            }
        }

        public ReadOnlyCollection<TestClass> TestClasses
        {
            get
            {
                if (testClasses != null)
                    testClasses = new ReadOnlyCollection<TestClass>(TestClasses_Internal);

                return testClasses;
            }
        }
    }
}
