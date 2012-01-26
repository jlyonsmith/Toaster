using System;
using System.Text;
using System.Collections.Generic;
using Toaster;
using ToolBelt;

namespace Toaster.UnitTests
{
	[TestClass]
	public class ToastTests
	{
		// In these tests we need to run a new Toast process so that we pick up the test version
		// of the toast binaries instead of the installed versions.

		[TestMethod]
		[DeploymentItem(@"$(SolutionDir)Tests\SampleTests\bin\$(Configuration)\SampleTests.dll")]
		[DeploymentItem(@"$(SolutionDir)Toaster\bin\$(Configuration)\Toaster.dll")]
		[DeploymentItem(@"$(SolutionDir)Toast\bin\$(Configuration)\Toast.exe")]
		[DeploymentItem(@"$(SolutionDir)Toolbelt\bin\$(Configuration)\ToolBelt.dll")]
		public void TestRunningSampleTests()
		{
			string output;

			Command.Run("toast SampleTests.dll", out output);

			// TODO-johnls: Need to assert that something good actually happened here...
		}
	}
}
