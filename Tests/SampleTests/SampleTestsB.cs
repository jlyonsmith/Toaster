using System;
using System.Collections.Generic;
using System.Text;
using Toaster;

//
// This class should have no implicit (setup/teardown) or explicit test defined in it
//

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
