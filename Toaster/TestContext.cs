using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ToolBelt;

namespace Toaster
{
    public class TestContext
    {
        public TestContext()
        {
            Properties = new PropertyCollection();
            OutputWriter = new StringWriter(new StringBuilder());
        }

        // For compatibility with MSTest
        public string TestDeploymentDir { get; internal set; }
        public string TestDir { get { return TestDeploymentDir; } }

        // Directory where toast was originally run
        public string OriginalCurrentDirectory { get { return InternalOriginalCurrentDirectory; } }

        // Property collection for use by tests
        public PropertyCollection Properties { get; private set; }
        
        // Test results.  Updated as tests run.
        public TestResult TestResult { get; internal set; }

        // The active test method
        public TestMethod ActiveTestMethod { get; internal set; }

        // A writer for ouputting to the test results file
        private StringWriter OutputWriter { get; set; }

        internal void ResetOutput()
        {
            OutputWriter.GetStringBuilder().Length = 0;
        }

        internal string GetOutput()
        {
            OutputWriter.Flush();
            return OutputWriter.GetStringBuilder().ToString();
        }

        public void WriteLine(string format, params object[] args)
        {
            OutputWriter.WriteLine(format, args);
        }

        internal ParsedPath InternalOriginalCurrentDirectory { get; set; }
    }
}
