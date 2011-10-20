using System;
using System.IO;
#if USE_TOASTER
using Toaster;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using ToolBelt;

namespace ToolBelt.UnitTests
{
	[TestClass] 
	public class PathExTests
	{
		[TestMethod] public void TestFindFileInPaths()
		{
			// TODO: Test FindFileInPaths
		}
	}
}