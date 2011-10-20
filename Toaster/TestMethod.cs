using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;

namespace Toaster
{
    public enum UnitTestOutcome
    {
        Failed,
        Passed, 
        Skipped,
        Pending,
        Running,
    }

    public class TestMethod
    {
        public TestMethod(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.Outcome = UnitTestOutcome.Pending;
        }

        // This constructor is used so that each test initialize/cleanup invocation can have a separate outcome
        internal TestMethod(TestMethod other)
        {
            this.MethodInfo = other.MethodInfo;
            this.Outcome = other.Outcome;
            this.ExpectedExceptions = other.ExpectedExceptions;
        }

        public string Name { get { return MethodInfo.DeclaringType.Name + "." + MethodInfo.Name; } }
        public TimeSpan ExecutionTime { get; internal set; }
        public long ExecutionCycles { get; internal set; }
        public UnitTestOutcome Outcome { get; internal set; }
        public Exception Exception { get; internal set; }
        public Type[] ExpectedExceptions { get; internal set; }
        public DeploymentItem[] DeploymentItems { get; internal set; }
        public string Output { get; internal set; }
        public virtual int TestCount 
        {
            get
            {
                return (MethodInfo != null ? 1 : 0);
            }
        }

        public virtual int GetTestOutcomeCount(UnitTestOutcome outcome)
        {
            return (Outcome == outcome ? 1 : 0);
        }
        
        internal MethodInfo MethodInfo { get; set; }
    }

    public class ExplicitTestMethod : TestMethod
    {
        public ExplicitTestMethod(MethodInfo methodInfo) : base(methodInfo)
        {
            this.Order = short.MaxValue;
        }

        public TestMethod TestInitializeMethod { get; internal set; }
        public TestMethod TestCleanupMethod { get; internal set; }
        public short Order { get; internal set; }
        public override int TestCount
        {
            get
            {
                return base.TestCount +
                    (TestInitializeMethod != null ? 1 : 0) +
                    (TestCleanupMethod != null ? 1 : 0);
            }
        }

        public override int GetTestOutcomeCount(UnitTestOutcome outcome)
        {
            return base.GetTestOutcomeCount(outcome) +
                (TestInitializeMethod != null && TestInitializeMethod.Outcome == outcome ? 1 : 0) +
                (TestCleanupMethod != null && TestCleanupMethod.Outcome == outcome ? 1 : 0);
        }

        internal void SetTestOutcomeToSkipped()
        {
            this.Outcome = UnitTestOutcome.Skipped;

            if (TestInitializeMethod != null)
                TestInitializeMethod.Outcome = UnitTestOutcome.Skipped;

            if (TestCleanupMethod != null)
                TestCleanupMethod.Outcome = UnitTestOutcome.Skipped;
        }
    }
}
