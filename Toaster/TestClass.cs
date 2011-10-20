using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Toaster
{
    public class TestClass
    {
        public TestClass(Type type)
        {
            this.Type = type;
            this.Order = short.MaxValue;
        }

        public Type Type { get; private set; }
        public string Name { get { return this.Type.FullName; } }
        public short Order { get; internal set; }
        public TimeSpan ExecutionTime { get; internal set; }
        public long ExecutionCycles { get; internal set; }
        public TestMethod TestInitializeMethod { get; internal set; }
        public TestMethod TestCleanupMethod { get; internal set; }
        public TestMethod ClassInitializeMethod { get; internal set; }
        public TestMethod ClassCleanupMethod { get; internal set; }
        public DeploymentItem[] DeploymentItems { get; internal set; }
        public int TestCount
        {
            get
            {
                int count = 0;

                foreach (var testMethod in testMethods_Internal)
                {
                    count += testMethod.TestCount;
                }

                return count +
                    (ClassInitializeMethod != null ? 1 : 0) +
                    (ClassCleanupMethod != null ? 1 : 0);
            }
        }

        public int GetTestOutcomeCount(UnitTestOutcome outcome)
        {
            int count = 0;

            foreach (var testMethod in testMethods_Internal)
            {
                count += testMethod.GetTestOutcomeCount(outcome);
            }

            return count +
                (ClassInitializeMethod != null && ClassInitializeMethod.Outcome == outcome ? 1 : 0) +
                (ClassCleanupMethod != null && ClassCleanupMethod.Outcome == outcome ? 1 : 0);
        }

        private List<ExplicitTestMethod> testMethods_Internal;
        private ReadOnlyCollection<ExplicitTestMethod> testMethods;

        internal List<ExplicitTestMethod> TestMethods_Internal
        {
            get
            {
                if (testMethods_Internal == null)
                    testMethods_Internal = new List<ExplicitTestMethod>();

                return testMethods_Internal;
            }
        }

        public ReadOnlyCollection<ExplicitTestMethod> TestMethods
        {
            get
            {
                if (testMethods == null)
                    testMethods = new ReadOnlyCollection<ExplicitTestMethod>(TestMethods_Internal);

                return testMethods;
            }
        }

        internal void SetAllTestOutcomesToSkipped()
        {
            foreach (var testMethod in testMethods_Internal)
            {
                testMethod.SetTestOutcomeToSkipped();
            }

            if (ClassInitializeMethod != null)
                ClassInitializeMethod.Outcome = UnitTestOutcome.Skipped;
                
            if (ClassCleanupMethod != null)
                ClassCleanupMethod.Outcome = UnitTestOutcome.Skipped;
        }
    }
}
