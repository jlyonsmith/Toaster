//
// This file genenerated by the Buckle tool on 7/11/2014 at 5:40 PM. 
//
// Contains strongly typed wrappers for resources in TestingResources.strings
//

namespace Toaster {
using System;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Globalization;


/// <summary>
/// Strongly typed resource wrappers generated from TestingResources.strings.
/// </summary>
public class TestingResources
{
    internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof(TestingResources));

    /// <summary>
    /// The expected collection contains {1} occurrence(s) of <{2}>. The actual collection contains {3} occurrence(s). {0}
    /// </summary>
    public static ToolBelt.Message ActualHasMismatchedElements(object param0, object param1, object param2, object param3)
    {
        Object[] o = { param0, param1, param2, param3 };
        return new ToolBelt.Message("ActualHasMismatchedElements", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Expected:<{1} ({2})>. Actual:<{3} ({4})>. {0}
    /// </summary>
    public static ToolBelt.Message AreEqualDifferentTypesFailMsg(object param0, object param1, object param2, object param3, object param4)
    {
        Object[] o = { param0, param1, param2, param3, param4 };
        return new ToolBelt.Message("AreEqualDifferentTypesFailMsg", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Expected:<{1}>. Actual:<{2}>. {0}
    /// </summary>
    public static ToolBelt.Message AreEqualFailMsg(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("AreEqualFailMsg", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Expected any value except:<{1}>. Actual:<{2}>. {0}
    /// </summary>
    public static ToolBelt.Message AreNotEqualFailMsg(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("AreNotEqualFailMsg", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Program arguments
    /// </summary>
    public static ToolBelt.Message ArgumentArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("ArgumentArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <arguments>
    /// </summary>
    public static ToolBelt.Message ArgumentArgumentHint
    {
        get
        {
            return new ToolBelt.Message("ArgumentArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Name of the assembly file for which the display name is desired.
    /// </summary>
    public static ToolBelt.Message AssemblyFileArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("AssemblyFileArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <assembly-file>
    /// </summary>
    public static ToolBelt.Message AssemblyFileArgumentHint
    {
        get
        {
            return new ToolBelt.Message("AssemblyFileArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Assembly '{0}' has a strong name and no keyfile was provided.  Assuming verification skipping is enabled.
    /// </summary>
    public static ToolBelt.Message AssemblyHasAStrongNameAndNoKeyFile(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("AssemblyHasAStrongNameAndNoKeyFile", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Additional directory to search for missing assemblies during assembly loads.  Can be specified multiple times.
    /// </summary>
    public static ToolBelt.Message AssemblySearchDirArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("AssemblySearchDirArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <assembly-dir>
    /// </summary>
    public static ToolBelt.Message AssemblySearchDirArgumentHint
    {
        get
        {
            return new ToolBelt.Message("AssemblySearchDirArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// {0} failed. {1}
    /// </summary>
    public static ToolBelt.Message AssertionFailed(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("AssertionFailed", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Both collection contain same elements.
    /// </summary>
    public static ToolBelt.Message BothCollectionsSameElements
    {
        get
        {
            return new ToolBelt.Message("BothCollectionsSameElements", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Both collection references point to the same collection object. {0}
    /// </summary>
    public static ToolBelt.Message BothCollectionsSameReference(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("BothCollectionsSameReference", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Loads and runs unit test assemblies and collects coverage data using VSTS coverage tools.  The environment variable BUTTER_TOOLPATH can be set to a semi-colon separated list of directories that will be searched in order for the tools needed during coverage instrumentation.  These tools are vsinstr.exe, vsperfcmd.exe, sn.exe, crumb.exe, chgmvid.exe, gacutil.exe, ngen.exe and toast.exe.  BUTTER_KEYFILE can be set to the name of the keyfile to use for resigning if the assemblies are fully signed.  If assemblies are partially signed or not strongly named this environment variable does not need to be set.  Butter's command line is intentionally identical to toast.exe.
    /// </summary>
    public static ToolBelt.Message ButterCommandLineDescription
    {
        get
        {
            return new ToolBelt.Message("ButterCommandLineDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// butter -deploymentdir:TestResults MyUnitTests.dll
    /// </summary>
    public static ToolBelt.Message ButterCommandLineExample
    {
        get
        {
            return new ToolBelt.Message("ButterCommandLineExample", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Buttered Toast Unit Test Tool for .NET
    /// </summary>
    public static ToolBelt.Message ButterCommandLineTitle
    {
        get
        {
            return new ToolBelt.Message("ButterCommandLineTitle", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Cannot find '{0}' in any PATH directory
    /// </summary>
    public static ToolBelt.Message CannotFindExeInPathDirs(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("CannotFindExeInPathDirs", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Cannot find '{0}' in any given tool directory
    /// </summary>
    public static ToolBelt.Message CannotFindExeInToolDirs(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("CannotFindExeInToolDirs", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// {0}({1})
    /// </summary>
    public static ToolBelt.Message CollectionEqualReason(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("CollectionEqualReason", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Debug
    /// </summary>
    public static ToolBelt.Message CommandLineDebugConfig
    {
        get
        {
            return new ToolBelt.Message("CommandLineDebugConfig", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Release
    /// </summary>
    public static ToolBelt.Message CommandLineReleaseConfig
    {
        get
        {
            return new ToolBelt.Message("CommandLineReleaseConfig", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// (null)
    /// </summary>
    public static ToolBelt.Message Common_NullInMessages
    {
        get
        {
            return new ToolBelt.Message("Common_NullInMessages", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// (object)
    /// </summary>
    public static ToolBelt.Message Common_ObjectString
    {
        get
        {
            return new ToolBelt.Message("Common_ObjectString", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// File which needs to be copied to the same directory as the test assembly before loading the assembly to run tests AND instrumented for coverage.
    /// </summary>
    public static ToolBelt.Message CoverageDeploymentItemArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("CoverageDeploymentItemArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Coverage deployment item '{0}' must be an executable file
    /// </summary>
    public static ToolBelt.Message CoverageDeploymentItemMustBeExecutable(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("CoverageDeploymentItemMustBeExecutable", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Creating temporary directories
    /// </summary>
    public static ToolBelt.Message CreatingTemporaryDirectories
    {
        get
        {
            return new ToolBelt.Message("CreatingTemporaryDirectories", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Determines the display name (also known as the strong name or full name) of a .NET assembly.
    /// </summary>
    public static ToolBelt.Message CrumbCommandLineDescription
    {
        get
        {
            return new ToolBelt.Message("CrumbCommandLineDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Attach the just-in-time debugger to the test process before running the first test.  You must still set a breakpoint in the test in order to stop the debugger in it.
    /// </summary>
    public static ToolBelt.Message DebugArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("DebugArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Deleting temporary directories
    /// </summary>
    public static ToolBelt.Message DeletingTemporaryDirectories
    {
        get
        {
            return new ToolBelt.Message("DeletingTemporaryDirectories", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Directory to deploy the unit test assembly and any assemblies that it requires in order to load.
    /// </summary>
    public static ToolBelt.Message DeploymentDirArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("DeploymentDirArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// A deployment directory must be specified.
    /// </summary>
    public static ToolBelt.Message DeploymentDirectoryMustBeSpecified
    {
        get
        {
            return new ToolBelt.Message("DeploymentDirectoryMustBeSpecified", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// No deployment directory given, using test assembly directory.
    /// </summary>
    public static ToolBelt.Message DeploymentDirIsTestAssemblyDir
    {
        get
        {
            return new ToolBelt.Message("DeploymentDirIsTestAssemblyDir", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// File which needs to be copied to the same directory as the test assembly before loading the assembly to run tests.
    /// </summary>
    public static ToolBelt.Message DeploymentItemArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("DeploymentItemArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Deployment item '{0}' does not exist
    /// </summary>
    public static ToolBelt.Message DeploymentItemDoesNotExist(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("DeploymentItemDoesNotExist", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Deployment item symbols '{0}' do not exist
    /// </summary>
    public static ToolBelt.Message DeploymentItemSymbolsDoNotExist(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("DeploymentItemSymbolsDoNotExist", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// The number of elements in the collections do not match. Expected:<{1}>. Actual:<{2}>.{0}
    /// </summary>
    public static ToolBelt.Message ElementNumbersDontMatch(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("ElementNumbersDontMatch", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Element at index {0} do not match.
    /// </summary>
    public static ToolBelt.Message ElementsAtIndexDontMatch(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("ElementsAtIndexDontMatch", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// An error occurred running a tool as follows: {0} {1}
    /// </summary>
    public static ToolBelt.Message ErrorRunningTool(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("ErrorRunningTool", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Expected exception on method '{0}' must be derived from Exception
    /// </summary>
    public static ToolBelt.Message ExpectedExceptionMustBeException(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("ExpectedExceptionMustBeException", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// FAILED
    /// </summary>
    public static ToolBelt.Message FailedAllCaps
    {
        get
        {
            return new ToolBelt.Message("FailedAllCaps", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Failure running '{0}'.n{1}
    /// </summary>
    public static ToolBelt.Message FailureRunning(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("FailureRunning", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Failure running '{0}'.
    /// </summary>
    public static ToolBelt.Message FailureRunningNoOutput(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("FailureRunningNoOutput", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Shows command line help.
    /// </summary>
    public static ToolBelt.Message HelpArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("HelpArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Additional binary to instrument for coverage. Can be specified multiple times.
    /// </summary>
    public static ToolBelt.Message InstrumentArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("InstrumentArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <assembly>
    /// </summary>
    public static ToolBelt.Message InstrumentArgumentHint
    {
        get
        {
            return new ToolBelt.Message("InstrumentArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Instrument Binaries for Coverage
    /// </summary>
    public static ToolBelt.Message InstrumentBinaries
    {
        get
        {
            return new ToolBelt.Message("InstrumentBinaries", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// {0}Expected type:<{1}>. Actual type:<{2}>.
    /// </summary>
    public static ToolBelt.Message IsInstanceOfFailMsg(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("IsInstanceOfFailMsg", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Copyright (c) 2014 John Lyon-Smith.  All rights reserved.
    /// </summary>
    public static ToolBelt.Message JohnLyonSmithCopyright
    {
        get
        {
            return new ToolBelt.Message("JohnLyonSmithCopyright", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Public/private key file to use for resigning strongly named assembly after coverage instrumentation.
    /// </summary>
    public static ToolBelt.Message KeyFileArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("KeyFileArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <key-file>
    /// </summary>
    public static ToolBelt.Message KeyFileArgumentHint
    {
        get
        {
            return new ToolBelt.Message("KeyFileArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Loaded assembly '{0}'
    /// </summary>
    public static ToolBelt.Message LoadedAssembly(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("LoadedAssembly", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Loaded assembly '{0}' from {1} '{2}'
    /// </summary>
    public static ToolBelt.Message LoadedAssemblyFrom(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("LoadedAssemblyFrom", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Method '{0}' ignored because it does not have zero parameters
    /// </summary>
    public static ToolBelt.Message MethodIgnoredBecauseHasParameters(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("MethodIgnoredBecauseHasParameters", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Method '{0}' ignored because it is not an instance method
    /// </summary>
    public static ToolBelt.Message MethodIgnoredBecauseInstance(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("MethodIgnoredBecauseInstance", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Method '{0}' ignored because it does not have a single TestContext parameter
    /// </summary>
    public static ToolBelt.Message MethodIgnoredBecauseNoTestContext(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("MethodIgnoredBecauseNoTestContext", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Method '{0}' ignored because it is not a static method
    /// </summary>
    public static ToolBelt.Message MethodIgnoredBecauseNotStatic(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("MethodIgnoredBecauseNotStatic", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Method '{0}' ignored because it does not have 'void' return type
    /// </summary>
    public static ToolBelt.Message MethodIgnoredBecauseNotVoidReturn(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("MethodIgnoredBecauseNotVoidReturn", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// No deployment items specified to instrument for coverage.
    /// </summary>
    public static ToolBelt.Message NoCoverageDeploymentItemsSpecified
    {
        get
        {
            return new ToolBelt.Message("NoCoverageDeploymentItemsSpecified", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// The parameter '{0}' is invalid. The value cannot be null. {1}.
    /// </summary>
    public static ToolBelt.Message NullParameterToAssert(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("NullParameterToAssert", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Different number of elements.
    /// </summary>
    public static ToolBelt.Message NumberOfElementsDiff
    {
        get
        {
            return new ToolBelt.Message("NumberOfElementsDiff", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// The name of a file to write the test results to in TSON format. If dash (-) is given as the name the output will be written to the standard error stream.
    /// </summary>
    public static ToolBelt.Message OutputFileArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("OutputFileArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// PASSED
    /// </summary>
    public static ToolBelt.Message PassedAllCaps
    {
        get
        {
            return new ToolBelt.Message("PassedAllCaps", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// {0} : '{1}'
    /// </summary>
    public static ToolBelt.Message PassFailMessage(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("PassFailMessage", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Problem processing test assemblies looking for test methods :  {0}
    /// </summary>
    public static ToolBelt.Message ProblemProcessingTestAssembly(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("ProblemProcessingTestAssembly", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Program to run and collect coverage for
    /// </summary>
    public static ToolBelt.Message ProgramArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("ProgramArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <program>
    /// </summary>
    public static ToolBelt.Message ProgramArgumentHint
    {
        get
        {
            return new ToolBelt.Message("ProgramArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Properties to pass to the test context. Use a semicolon to separate multiple properties.  TestDir and TestDeploymentDir are always set and contain the deployment directory.  A SolutionDir property is automatically set if there is a .sln file in a parent directory of the test.
    /// </summary>
    public static ToolBelt.Message PropertyArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("PropertyArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <name>=<value>
    /// </summary>
    public static ToolBelt.Message PropertyArgumentHint
    {
        get
        {
            return new ToolBelt.Message("PropertyArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Process running {0} CLR {1} from '{2}'.
    /// </summary>
    public static ToolBelt.Message RuntimeVersion(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("RuntimeVersion", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Run Unit Tests
    /// </summary>
    public static ToolBelt.Message RunUnitTests
    {
        get
        {
            return new ToolBelt.Message("RunUnitTests", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Saving original binaries
    /// </summary>
    public static ToolBelt.Message SavingOriginalBinaries
    {
        get
        {
            return new ToolBelt.Message("SavingOriginalBinaries", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Second [AssemblyCleanup] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondAssemblyCleanupFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondAssemblyCleanupFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Second [AssemblyInitialize] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondAssemblyInitializeFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondAssemblyInitializeFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Second [ClassCleanup] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondClassCleanupFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondClassCleanupFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Second [ClassInitialize] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondClassInitializeFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondClassInitializeFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Second [TestCleanup] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondTestCleanupFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondTestCleanupFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Second [TestInitialize] method found in class {0}
    /// </summary>
    public static ToolBelt.Message SecondTestInitializeFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SecondTestInitializeFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Skipping test class '{0}' because of command line
    /// </summary>
    public static ToolBelt.Message SkippingTestClass(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SkippingTestClass", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Skipping test method '{0}' because of command line
    /// </summary>
    public static ToolBelt.Message SkippingTestMethod(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("SkippingTestMethod", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Start Coverage Monitor
    /// </summary>
    public static ToolBelt.Message StartCoverageMonitor
    {
        get
        {
            return new ToolBelt.Message("StartCoverageMonitor", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Stop Coverage Monitor
    /// </summary>
    public static ToolBelt.Message StopCoverageMonitor
    {
        get
        {
            return new ToolBelt.Message("StopCoverageMonitor", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Test assembly '{0}' cannot be found
    /// </summary>
    public static ToolBelt.Message TestAssemblyCannotBeFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestAssemblyCannotBeFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Loaded test assembly '{0}'
    /// </summary>
    public static ToolBelt.Message TestAssemblyLoaded(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestAssemblyLoaded", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Test assembly not supplied
    /// </summary>
    public static ToolBelt.Message TestAssemblyNotSupplied
    {
        get
        {
            return new ToolBelt.Message("TestAssemblyNotSupplied", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Specifies the full name (including the namespace) of the test class to run.  Only the test methods in this class will be run unless further filtered by the -testname argument.
    /// </summary>
    public static ToolBelt.Message TestClassArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("TestClassArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Found test class '{0}'
    /// </summary>
    public static ToolBelt.Message TestClassFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestClassFound", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Test class '{0}' has no explicit or implicit test methods defined in it.
    /// </summary>
    public static ToolBelt.Message TestClassHasNoTestMethods(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestClassHasNoTestMethods", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Running test class '{0}'
    /// </summary>
    public static ToolBelt.Message TestClassRunning(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestClassRunning", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Assembly containing tests
    /// </summary>
    public static ToolBelt.Message TestFileArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("TestFileArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <test-assembly>
    /// </summary>
    public static ToolBelt.Message TestFileArgumentHint
    {
        get
        {
            return new ToolBelt.Message("TestFileArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Specifies the name (not including namespace and class) of a test method to run.  All this test methods matching this name will run.  Item deployment will be done for all classes unless the -testclass argument is given.  Only explicit test methods (those marked as [TestMethod] can be filtered this way.  
    /// </summary>
    public static ToolBelt.Message TestMethodArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("TestMethodArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Running test method '{0}'
    /// </summary>
    public static ToolBelt.Message TestMethodRunning(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("TestMethodRunning", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Loads and runs unit test assemblies. 
    /// </summary>
    public static ToolBelt.Message ToastCommandLineDescription
    {
        get
        {
            return new ToolBelt.Message("ToastCommandLineDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// toast -deploymentdir:TestResults MyUnitTests.dll
    /// </summary>
    public static ToolBelt.Message ToastCommandLineExample
    {
        get
        {
            return new ToolBelt.Message("ToastCommandLineExample", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Toast Unit Test Tool for .NET
    /// </summary>
    public static ToolBelt.Message ToastCommandLineTitle
    {
        get
        {
            return new ToolBelt.Message("ToastCommandLineTitle", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Directory containing location of tools to use.  The tools required are vsinstr.exe, vsperfmon.exe, sn.exe, gacutil.exe and ngen.exe.  Multiple tool directories can be specified by using the argument multiple times.  Directories will be searched in the order specified.
    /// </summary>
    public static ToolBelt.Message ToolDirArgumentDescription
    {
        get
        {
            return new ToolBelt.Message("ToolDirArgumentDescription", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// <tool-dir>
    /// </summary>
    public static ToolBelt.Message ToolDirArgumentHint
    {
        get
        {
            return new ToolBelt.Message("ToolDirArgumentHint", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Unable to copy file '{0}' to '{1}'. {2}
    /// </summary>
    public static ToolBelt.Message UnableToCopyFile(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("UnableToCopyFile", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to create test deployment directory '{0}'
    /// </summary>
    public static ToolBelt.Message UnableToCreateDeploymentDirectory(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("UnableToCreateDeploymentDirectory", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to create instance of test class '{0}'. {1}
    /// </summary>
    public static ToolBelt.Message UnableToCreateInstanceOfTestClass(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("UnableToCreateInstanceOfTestClass", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to load test assembly '{0}'. {1}
    /// </summary>
    public static ToolBelt.Message UnableToLoadTestAssembly(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("UnableToLoadTestAssembly", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to process DeploymentItem with path '{0}' and output directory '{1}'.  {2}.
    /// </summary>
    public static ToolBelt.Message UnableToProcessDeploymentItem(object param0, object param1, object param2)
    {
        Object[] o = { param0, param1, param2 };
        return new ToolBelt.Message("UnableToProcessDeploymentItem", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to reflect on types in assembly '{0}'
    /// </summary>
    public static ToolBelt.Message UnableToReflectAssemblyTypes(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("UnableToReflectAssemblyTypes", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unable to write test result output to file '{0}'. {1}
    /// </summary>
    public static ToolBelt.Message UnableToWriteOutputFile(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("UnableToWriteOutputFile", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// Unit tests failed to run.
    /// </summary>
    public static ToolBelt.Message UnitTestsFailedToRun
    {
        get
        {
            return new ToolBelt.Message("UnitTestsFailedToRun", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Update Global Assembly Cache
    /// </summary>
    public static ToolBelt.Message UpdateGlobalAssemblyCache
    {
        get
        {
            return new ToolBelt.Message("UpdateGlobalAssemblyCache", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Update Native Image Cache
    /// </summary>
    public static ToolBelt.Message UpdateNativeImageCache
    {
        get
        {
            return new ToolBelt.Message("UpdateNativeImageCache", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// Using '{0}' from directory '{1}'
    /// </summary>
    public static ToolBelt.Message UsingExeFrom(object param0, object param1)
    {
        Object[] o = { param0, param1 };
        return new ToolBelt.Message("UsingExeFrom", typeof(TestingResources), ResourceManager, o);
    }

    /// <summary>
    /// 32-bit
    /// </summary>
    public static ToolBelt.Message WordSize32
    {
        get
        {
            return new ToolBelt.Message("WordSize32", typeof(TestingResources), ResourceManager, null);
        }
    }

    /// <summary>
    /// 64-bit
    /// </summary>
    public static ToolBelt.Message WordSize64
    {
        get
        {
            return new ToolBelt.Message("WordSize64", typeof(TestingResources), ResourceManager, null);
        }
    }
}
}
