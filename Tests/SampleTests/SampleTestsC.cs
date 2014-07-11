using System;
using System.Collections.Generic;
using System.Text;
using Toaster;
using ToolBelt;

namespace SampleTests
{
    [Order(300)]
    [TestClass]
    public class SampleTestsC
    {
        [ClassInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            // Checking that accessing non-existent property returns null not exception
            Assert.IsNull(testContext.Properties["NON-EXISTENT-PROPERTY"]);
        }

        [TestMethod]
        public void TestWriteOutput()
        {
            for (int i = 0; i < 5; i++)
                ToastTool.TestContext.WriteLine("Hello");
        }

        [TestMethod]
        [Order(10)]
        public void TestAssertThrowsFail1()
        {
            Assert.Throws(delegate { throw new ArgumentException(); }, typeof(NullReferenceException), 
                "Expected different exception.  Test OK.");
        }

        [TestMethod]
        [Order(20)]
        public void TestAssertThrowsFail2()
        {
            Assert.Throws(delegate { }, typeof(NullReferenceException), "Expected an exception.  Test OK.");
        }

        [TestMethod]
        [Order(30)]
        public void TestAssertThrowsSucceed()
        {
            Assert.Throws(delegate { throw new NullReferenceException(); }, typeof(NullReferenceException), 
                "Expected exception.  Test OK.");
        }
    }
}
