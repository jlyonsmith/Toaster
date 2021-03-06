﻿using System;
using System.Collections.Generic;
using Toaster;

namespace BadDependencyTests
{
    [Order(100)]
    [TestClass]
    public class BadTests
    {
        // This type is intended to cause a reflection exception for this test class.  For this to work
        // ensure that the SampleTests.dll is not copied to the bin\ directory.
        #pragma warning disable 414
        SampleTests.BogusType bogus = new SampleTests.BogusType();
        #pragma warning restore 414

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }
    }
}

