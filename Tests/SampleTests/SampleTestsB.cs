using System;
using System.Collections.Generic;
using System.Text;
using Toaster;

namespace SampleTests
{
    [TestClass]
    [Order(200)]
    [DeploymentItem(@"$(SolutionDir)SampleTests\TestFiles\TestFile1.txt", "TestFiles")]
    class SampleTestsB
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize()
        {
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
        }
    }
}
