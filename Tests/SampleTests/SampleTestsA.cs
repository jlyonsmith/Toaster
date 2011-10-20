using System;
using System.Collections.Generic;
using Toaster;

namespace SampleTests
{
    [Order(100)]
    [TestClass]    
    public class SampleTestsA
    {
        private static TestContext testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // You can save this if you want to, but this information is always available in Toast.TestContext
            SampleTestsA.testContext = testContext;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }
    
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        [DeploymentItem(@"$(SolutionDir)SampleTests\TestFiles\TestFile2.txt")]
        public void TestDeployment()
        {
        }

        [TestMethod]
        [Order(200)]
        public void AssertTrueTest()
        {
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        [Order(100)]
        public void AssertAreEqualTest()
        {
            Assert.AreEqual<int>(10, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void ThrowsException()
        {
            int a = 1;
            int b = 0;
            int c = a / b;
        }
    }
}
