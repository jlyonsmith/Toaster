using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using ToolBelt;

namespace Toaster
{
    [CommandLineTitle("ToastCommandLineTitle")]
    [CommandLineDescription("ToastCommandLineDescription")]
    [CommandLineCopyright("JohnLyonSmithCopyright")]
    [CommandLineExample("ToastCommandLineExample")]
#if DEBUG    
    [CommandLineConfiguration("CommandLineDebugConfig")]
#else
    [CommandLineConfiguration("CommandLineReleaseConfig")]
#endif
    public class ToastTool : IProcessCommandLine, ITool
    {
        #region Construction
        static ToastTool()
        {
            TestContext = new TestContext();
        }

        public ToastTool()
        {
        }

        #endregion

        #region Public Properties
        [CommandLineArgument("property", Description = "PropertyArgumentDescription", ValueHint = "PropertyArgumentHint", ShortName = "p")]
        public string Property { get; set; }

        [CommandLineArgument("help", Description = "HelpArgumentDescription", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument("debug", Description = "DebugArgumentDescription", ShortName = "d")]
        public bool AttachDebugger { get; set; }

        [CommandLineArgument("deploymentdir", Description = "DeploymentDirArgumentDescription", ShortName = "dd", Initializer = typeof(InitializeDeploymentDir))]
        public ParsedPath DeploymentDir { get; set; }

        internal class InitializeDeploymentDir
        {
            public static ParsedPath Parse(string value)
            {
                return new ParsedPath(value, PathType.Directory).MakeFullPath();
            }
        }

        [CommandLineArgument("testclass", Description = "TestClassArgumentDescription", ShortName = "tc")]
        public string TestClassName { get; set; }

        [CommandLineArgument("testmethod", Description = "TestMethodArgumentDescription", ShortName = "tm")]
        public string TestMethodName { get; set; }

        [CommandLineArgument("deploymentitem", Description = "DeploymentItemArgumentDescription", ShortName = "di", Initializer = typeof(InitializeDeploymentItem))]
        public List<DeploymentItem> DeploymentItems { get; set; }

        internal class InitializeDeploymentItem
        {
            public static DeploymentItem Parse(string value)
            {
                // NOTE: Deployment items that come from the command line cannot have $() variables 
                // because we don't have any properties at the time we do the pre- assembly load 
                // deployments.
                return new DeploymentItem(value);
            }
        }

        [CommandLineArgument("outputfile", Description = "OutputFileArgumentDescription", ShortName="o")]
        public ParsedPath OutputFile { get; set; }

        [CommandLineArgument("assemblydir", Description = "AssemblySearchDirArgumentDescription", ValueHint="AssemblySearchDirArgumentHint", ShortName = "ad", Initializer=typeof(InitializeAssemblySearchDirs))]
        public List<ParsedPath> AssemblySearchDirs { get; set; }

        internal class InitializeAssemblySearchDirs
        {
            public static ParsedPath Parse(string value)
            {
                return new ParsedPath(value, PathType.Directory).MakeFullPath();
            }
        }

        [DefaultCommandLineArgument("testfile", Description = "TestFileArgumentDescription", ShortName = "f", ValueHint = "TestFileArgumentHint", Initializer = typeof(InitializeTestFile))]
        public ParsedPath TestFile { get; set; }

        internal class InitializeTestFile
        {
            public static ParsedPath Parse(string value)
            {
                return new ParsedPath(value, PathType.File).MakeFullPath();
            }
        }
        
        public int ExitCode 
        { 
            get 
            { 
                return (Output.HasOutputErrors ? 1 : (TestContext.TestResult != null && !TestContext.TestResult.Success) ? 2 : 0); 
            } 
        }

        public static TestContext TestContext { get; private set; }

        #endregion

        #region Private Properties
        private CommandLineParser parser;

        private CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(typeof(ToastTool), typeof(TestingResources));

                return parser;
            }
        }

        #endregion

        #region Public Methods
        public void Execute()
        {
            if (ShowHelp)
            {
                Output.Message(Parser.LogoBanner);
                Output.Message(Parser.Usage);
                return;
            }

            // Get the test results going by measuring the start time
            TestResult testResult = new TestResult();
            DateTime now = DateTime.Now;

            testResult.StartDate = now.Date;
            testResult.StartTime = now.TimeOfDay;

            Output.Message(MessageImportance.Normal,
                TestingResources.RuntimeVersion(
                    Marshal.SizeOf(typeof(IntPtr)) == 4 ? TestingResources.WordSize32 : TestingResources.WordSize64,
                    RuntimeEnvironment.GetSystemVersion().ToString(),
                    RuntimeEnvironment.GetRuntimeDirectory()));

            if (TestFile == null)
            {
                Output.Error(TestingResources.TestAssemblyNotSupplied);
                return;
            }

            TestFile = TestFile.MakeFullPath();

            if (!File.Exists(TestFile))
            {
                Output.Error(TestingResources.TestAssemblyCannotBeFound(TestFile));
                return;
            }

            if (DeploymentDir == null)
            {
                // If no deployment directory specified, then it is wherever the test assembly is
                DeploymentDir = new ParsedPath(TestFile.VolumeAndDirectory, PathType.Directory);

                Output.Message(MessageImportance.Low, TestingResources.DeploymentDirIsTestAssemblyDir);
            }

            if (DeploymentItems == null)
            {
                // Avoid problems with nulls
                DeploymentItems = new List<DeploymentItem>();
            }

            if (AssemblySearchDirs == null)
            {
                // Avoid nulls
                AssemblySearchDirs = new List<ParsedPath>();
            }

            // Set test context results
            ToastTool.TestContext.TestResult = testResult;

            // Set properties for compatability with MSTest
            ToastTool.TestContext.TestDeploymentDir = DeploymentDir;

            // Save the current working directory as we will change it later
            ToastTool.TestContext.InternalOriginalCurrentDirectory = new ParsedPath(Environment.CurrentDirectory, PathType.Directory);

            // Initialize properties from the environment and from the command line
            TestContext.Properties.AddFromEnvironment();
            TestContext.Properties.AddFromPropertyString(Property);
            TestContext.Properties.AddWellKnown(DeploymentDir);

            // Expand all paths in any command line deployment items relative to the working directory
            foreach (var deploymentItem in DeploymentItems)
            {
                deploymentItem.Path = deploymentItem.Path.MakeFullPath();
                deploymentItem.OutputDirectory = DeploymentDir;
            }

            // Copy the test assembly to the deployment directory
            if (!CopyDeploymentItem(new DeploymentItem(TestFile.MakeFullPath(), DeploymentDir)))
                return;

            // If any deployments fail it's an error because we assume we will not be 
            // able to load the test assembly, so change the outputter to upgrade any warnings we print.
            Output.WarningsAsErrors = true;

            if (!CopyDeploymentItems(this.DeploymentItems.ToArray()))
                return;

            Output.WarningsAsErrors = false;

            TestAssembly testAssembly = null;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

            try
            {
                // We use Assembly.Load so that the test assembly and subsequently loaded
                // assemblies end up in the correct load context.  If the assembly cannot be
                // found it will raise a AssemblyResolve event where we will search for the 
                // assembly.
                testAssembly = testResult.TestAssembly = new TestAssembly(Assembly.Load(TestFile.FileAndExtension));
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsCriticalException(e))
                    throw;

                Output.Error(TestingResources.UnableToLoadTestAssembly(TestFile, e.ToString()));

                // Not being able to load the test assembly pretty much shuts us down
                return;
            }

            Type[] types;

            // We won't get dependency errors until we actually try to reflect on all the types in the assembly
            try
            {
                types = testAssembly.Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                string message = TestingResources.UnableToReflectAssemblyTypes(testAssembly.Name);

                // There is one entry in the exceptions array for each null in the types array,
                // and they correspond positionally.
                foreach (Exception ex in e.LoaderExceptions)
                    message += Environment.NewLine + "   " + ex.Message;

                Output.Error(message);

                // Not being able to reflect on classes in the test assembly is a critical error
                return;
            }

            //
            // Go through all the types in the test assembly and find all the test methods.
            // An explicit test method is marked with a TestMethod attribute.  An implicit test method
            // are all ClassInitialize, TestCleanup, etc.. methods.  They can cause the test run to fail 
            // just like explicit test methods.
            // 

            try
            {
                if (!FindAllTestMethods(testResult, testAssembly, types))
                    return;
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsCriticalException(e))
                    throw;

                // If we have any problems loading the test assembly that's a critical error
                // that indicates a deployment problem and we must stop.
                Output.Error(TestingResources.ProblemProcessingTestAssembly(e.ToString()));

                return;
            }

            //
            // Start running the tests.  Go through all assemblies, classes & methods do deployments, run tests.
            //

            // Change the current directory to the deployment directory
            Environment.CurrentDirectory = DeploymentDir;

            ProcessCycleStopwatch assemblyCycleStopwatch = null;
            
            if (ProcessCycleStopwatch.IsSupported)
                assemblyCycleStopwatch = ProcessCycleStopwatch.StartNew();

            Stopwatch assemblyStopwatch = Stopwatch.StartNew();

            RunTestMethod(null, testAssembly.AssemblyInitializeMethod);

            // Now go through and run all the tests that we found
            foreach (var testClass in testAssembly.TestClasses_Internal)
            {
                // If we have been given a class name then only run tests from that class
                if (!String.IsNullOrEmpty(TestClassName) &&
                    !testClass.Name.EndsWith(TestClassName, StringComparison.CurrentCultureIgnoreCase))
                {
                    testClass.SetAllTestOutcomesToSkipped();
                    Output.Message(MessageImportance.Low, TestingResources.SkippingTestClass(testClass.Name));
                    continue;
                }

                ProcessCycleStopwatch classCycleStopwatch = null;

                if (ProcessCycleStopwatch.IsSupported)
                    classCycleStopwatch = ProcessCycleStopwatch.StartNew();

                Stopwatch classStopwatch = Stopwatch.StartNew();

                Output.Message(MessageImportance.Low, TestingResources.TestClassRunning(testClass.Name));

                CopyDeploymentItems(testClass.DeploymentItems);

                RunTestMethod(null, testClass.ClassInitializeMethod);

                object testObj = null;

                try
                {
                    testObj = Activator.CreateInstance(testClass.Type);
                }
                catch (Exception e)
                {
                    if (ExceptionUtility.IsCriticalException(e))
                        return;

                    if (e is TargetInvocationException)
                    {
                        Output.Error(TestingResources.UnableToCreateInstanceOfTestClass(testClass.Name, e.InnerException.Message));
                    }
                    else
                    {
                        Output.Error(TestingResources.UnableToCreateInstanceOfTestClass(testClass.Name, e.Message));
                    }

                    // Not being able to create the test class is a critical error and we need to exit
                    return;
                }

                if (testObj != null)
                {
                    foreach (var testMethod in testClass.TestMethods)
                    {
                        // If we have been given a test method name then only run test methods that end with that name
                        if (!String.IsNullOrEmpty(TestMethodName) &&
                            !testMethod.Name.EndsWith(TestMethodName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            testMethod.SetTestOutcomeToSkipped();
                            Output.Message(MessageImportance.Low, TestingResources.SkippingTestMethod(testMethod.Name));
                            continue;
                        }

                        if (testMethod.TestInitializeMethod != null)
                            CopyDeploymentItems(testMethod.TestInitializeMethod.DeploymentItems);

                        RunTestMethod(testObj, testMethod.TestInitializeMethod);

                        CopyDeploymentItems(testMethod.DeploymentItems);
                        RunTestMethod(testObj, testMethod);

                        if (testMethod.TestCleanupMethod != null)
                            CopyDeploymentItems(testMethod.TestCleanupMethod.DeploymentItems);

                        RunTestMethod(testObj, testMethod.TestCleanupMethod);
                    }
                }

                RunTestMethod(null, testClass.ClassCleanupMethod);

                classStopwatch.Stop();
                testClass.ExecutionTime = new TimeSpan(classStopwatch.ElapsedTicks);

                if (classCycleStopwatch != null)
                {
                    classCycleStopwatch.Stop();
                    testClass.ExecutionCycles = unchecked((long)classCycleStopwatch.ElapsedCycles);
                }
            }

            RunTestMethod(null, testAssembly.AssemblyCleanupMethod);

            assemblyStopwatch.Stop();
            testAssembly.ExecutionTime = new TimeSpan(assemblyStopwatch.ElapsedTicks);

            if (assemblyCycleStopwatch != null)
            {
                assemblyCycleStopwatch.Stop();
                testAssembly.ExecutionCycles = unchecked((long)assemblyCycleStopwatch.ElapsedCycles);
            }

            // Grab the end time
            now = DateTime.Now;

            testResult.EndDate = now.Date;
            testResult.EndTime = now.TimeOfDay;

            // Output test results in XML and use XSLT to format the results.  
            // Ensure output can be piped into msxsl or xslt.csr script
            string resultsText = testResult.ToXml();
            
            if (OutputFile != null)
            {
                try
                {
                    using (StreamWriter wr = new StreamWriter(OutputFile.ToString(), false, Encoding.Unicode))
                    {
                        wr.Write(resultsText);
                    }

                    Output.Message("Test results written to '{0}'", OutputFile.ToString());
                }
                catch (Exception e)
                {
                    if (ExceptionUtility.IsCriticalException(e))
                        throw;                    
                        
                    Output.Error(TestingResources.UnableToWriteOutputFile(OutputFile, e.Message));
                    OutputFile = null;
                }
            }

            // If no output file OR we failed to write the output file, write to console
            if (OutputFile == null)
                Console.WriteLine(resultsText);

            if (testResult.FailedTests > 0)
                Output.Warning("{0} test{1} FAILED", testResult.FailedTests, testResult.FailedTests > 1 ? "s" : "");
            else
                Output.Message(MessageImportance.High, "All tests PASSED");

            // Test may have failed, but the program HAS executed at this point
            return;
        }

        public bool CopyDeploymentItems(IList<DeploymentItem> deploymentItems)
        {
            if (deploymentItems == null || deploymentItems.Count == 0)
                return true;

            bool ok = true;

            foreach (var deploymentItem in deploymentItems)
            {
                ok &= CopyDeploymentItem(deploymentItem);
            }

            return ok;
        }

        public bool CreateDeploymentDirectory(ParsedPath path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsCriticalException(e))
                    throw;

                Output.Warning(TestingResources.UnableToCreateDeploymentDirectory(path));
                return false;
            }
        }

        #endregion

        #region Private Event Handlers
        private static object appDomainLock = new object();

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Lock for changing the assembly resolution handler
            lock (appDomainLock)
            {
                // Avoid getting re-entrancy problems for assemblies that we load here
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

                string name = args.Name.Split(',')[0];
                string fileName = (System.IO.Path.GetExtension(name) == ".dll" ? name : name + ".dll");

                try
                {
                    // Search in deployment directory, which may be the same as the source directory for 
                    // the assembly if no deployment directory was given.  Note that because this event
                    // came from a Load call, using LoadFrom here will still cause the assembly to be in 
                    // the correct load context.
                    try { return Assembly.LoadFrom(DeploymentDir.Append(fileName, PathType.File)); }
                    catch (FileNotFoundException) { }

                    // Search in specified assembly search directories
                    foreach (ParsedPath dir in AssemblySearchDirs)
                    {
                        try { return Assembly.LoadFrom(dir.Append(fileName, PathType.File)); }
                        catch (FileNotFoundException) { }
                    }

                    // Search in the GAC and probing paths with the partial name
                    try { return Assembly.Load(name); }
                    catch (FileNotFoundException) { }

                    // Search in test assembly source directory
                    try { return Assembly.LoadFrom(
                        new ParsedPath(TestFile, PathParts.VolumeAndDirectory).Append(fileName, PathType.File)); }
                    catch (FileNotFoundException) { }

                    // Return null instead of throwing an exception or resource loading breaks
                    return null;
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                }
            }
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Output.Message(MessageImportance.Low, TestingResources.LoadedAssembly(
                (args.LoadedAssembly.GlobalAssemblyCache || args.LoadedAssembly.Location == String.Empty) ? 
                    args.LoadedAssembly.FullName : args.LoadedAssembly.Location));
        }

        #endregion

        #region Private Methods

        private bool FindAllTestMethods(TestResult testResult, TestAssembly testAssembly, Type[] types)
        {
            foreach (var type in types)
            {
                object[] attrs = type.GetCustomAttributes(false);

                if (type.IsNested)
                    // Ignore nested types
                    continue;

                if (attrs.Length == 0 || FindAttribute(attrs, typeof(TestClassAttribute)) == null)
                    // No attributes or TestClass attribute, no tests
                    continue;

                OrderAttribute priorityAttr;

                Output.Message(MessageImportance.Low, TestingResources.TestClassFound(type.ToString()));

                TestClass testClass = new TestClass(type);

                testResult.TestAssembly.TestClasses_Internal.Add(testClass);

                // By default tests are run in the order that they are declared in the class.  
                // The OrderAttribute can be used to order running of methods AND classes.
                priorityAttr = (OrderAttribute)FindAttribute(attrs, typeof(OrderAttribute));

                if (priorityAttr != null)
                    testClass.Order = priorityAttr.Order;

                List<DeploymentItem> deploymentItems = new List<DeploymentItem>();

                foreach (var attr in attrs)
                {
                    if (attr is DeploymentItemAttribute)
                    {
                        DeploymentItem deploymentItem = CreateDeploymentItem((DeploymentItemAttribute)attr);

                        if (deploymentItem != null)
                            deploymentItems.Add(deploymentItem);
                    }
                }

                if (deploymentItems != null)
                    testClass.DeploymentItems = deploymentItems.ToArray();

                // Now get all the public methods in the class and search for ones marked with test attributes
                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

                List<Type> expectedExceptions = new List<Type>();

                foreach (var method in methods)
                {
                    attrs = method.GetCustomAttributes(false);

                    TestMethod testMethod = null;

                    // We determine what type of test method we have by searching for the various 
                    // explicit (TestMethod) and implicit (ClassInitialize, etc..) methods.
                    if (FindAttribute(attrs, typeof(TestMethodAttribute)) != null)
                    {
                        if (!CheckInstanceMethodVoidNoParams(method))
                            continue;

                        ExplicitTestMethod explicitTestMethod = new ExplicitTestMethod(method);

                        testMethod = explicitTestMethod;

                        priorityAttr = (OrderAttribute)FindAttribute(attrs, typeof(OrderAttribute));

                        if (priorityAttr != null)
                            explicitTestMethod.Order = priorityAttr.Order;

                        testClass.TestMethods_Internal.Add(explicitTestMethod);
                    }
                    else if (FindAttribute(attrs, typeof(AssemblyInitializeAttribute)) != null)
                    {
                        if (testAssembly.AssemblyInitializeMethod != null)
                        {
                            Output.Error(TestingResources.SecondAssemblyInitializeFound(testClass.Name));
                            return false;
                        }

                        if (!CheckStaticMethodVoidNoParams(method))
                            continue;

                        testMethod = testAssembly.AssemblyInitializeMethod = new TestMethod(method);
                    }
                    else if (FindAttribute(attrs, typeof(AssemblyCleanupAttribute)) != null)
                    {
                        if (testAssembly.AssemblyCleanupMethod != null)
                        {
                            Output.Error(TestingResources.SecondAssemblyCleanupFound(testClass.Name));
                            return false;
                        }

                        if (!CheckStaticMethodVoidNoParams(method))
                            continue;

                        testMethod = testAssembly.AssemblyCleanupMethod = new TestMethod(method);
                    }
                    else if (FindAttribute(attrs, typeof(ClassInitializeAttribute)) != null)
                    {
                        if (testClass.ClassInitializeMethod != null)
                        {
                            Output.Error(TestingResources.SecondClassInitializeFound(testClass.Name));
                            return false;
                        }

                        if (!CheckStaticMethodVoidTestContextParam(method))
                            continue;

                        testMethod = testClass.ClassInitializeMethod = new TestMethod(method);
                    }
                    else if (FindAttribute(attrs, typeof(ClassCleanupAttribute)) != null)
                    {
                        if (testClass.ClassCleanupMethod != null)
                        {
                            Output.Error(TestingResources.SecondClassCleanupFound(testClass.Name));
                            return false;
                        }

                        if (!CheckStaticMethodVoidNoParams(method))
                            continue;

                        testMethod = testClass.ClassCleanupMethod = new TestMethod(method);
                    }
                    else if (FindAttribute(attrs, typeof(TestInitializeAttribute)) != null)
                    {
                        if (testClass.TestInitializeMethod != null)
                        {
                            Output.Error(TestingResources.SecondTestInitializeFound(testClass.Name));
                            return false;
                        }

                        if (!CheckInstanceMethodVoidNoParams(method))
                            continue;

                        testMethod = testClass.TestInitializeMethod = new TestMethod(method);
                    }
                    else if (FindAttribute(attrs, typeof(TestCleanupAttribute)) != null)
                    {
                        if (testClass.TestCleanupMethod != null)
                        {
                            Output.Error(TestingResources.SecondTestCleanupFound(testClass.Name));
                            return false;
                        }

                        if (!CheckInstanceMethodVoidNoParams(method))
                            continue;

                        testMethod = testClass.TestCleanupMethod = new TestMethod(method);
                    }

                    if (testMethod != null)
                    {
                        // Check if we have any expected exception or deployment item attributes 
                        expectedExceptions.Clear();
                        deploymentItems.Clear();

                        foreach (var attr in attrs)
                        {
                            ExpectedExceptionAttribute expectedExceptionAttr = attr as ExpectedExceptionAttribute;
                            DeploymentItemAttribute deploymentItemAttr = attr as DeploymentItemAttribute;

                            if (expectedExceptionAttr != null)
                            {
                                Type exceptionType = expectedExceptionAttr.ExceptionType;

                                if (!exceptionType.IsSubclassOf(typeof(Exception)))
                                    Output.Warning(TestingResources.ExpectedExceptionMustBeException(testMethod.Name));
                                else
                                    expectedExceptions.Add(exceptionType);
                            }
                            else if (deploymentItemAttr != null)
                            {
                                DeploymentItem deploymentItem = CreateDeploymentItem(deploymentItemAttr);
                                
                                if (deploymentItem != null)
                                    deploymentItems.Add(deploymentItem);
                            }
                        }

                        testMethod.ExpectedExceptions = expectedExceptions.ToArray();
                        testMethod.DeploymentItems = deploymentItems.ToArray();
                    }
                }

                // Go through each explicit test method and set the initialize and cleanup calls if they exist in the class
                foreach (var testMethod in testClass.TestMethods_Internal)
                {
                    if (testClass.TestInitializeMethod != null)
                    {
                        testMethod.TestInitializeMethod = new TestMethod(testClass.TestInitializeMethod);
                    }

                    if (testClass.TestCleanupMethod != null)
                    {
                        testMethod.TestCleanupMethod = new TestMethod(testClass.TestCleanupMethod);
                    }
                }

                if (testClass.TestCount == 0)
                    Output.Warning(TestingResources.TestClassHasNoTestMethods(type.Name));
            }

            // Sort classes in ascending order
            testAssembly.TestClasses_Internal.Sort((testClass1, testClass2) => testClass1.Order - testClass2.Order);

            // Sort tests in ascending order
            foreach (var testClass in testAssembly.TestClasses_Internal)
            {
                testClass.TestMethods_Internal.Sort((testMethod1, testMethod2) => testMethod1.Order - testMethod2.Order);
            }

            return true;
        }

        private static void SafeFileCopy(ParsedPath from, ParsedPath to)
        {
            // Unset any read-only flag on the target file if it exists
            if (File.Exists(to))
            {
                File.SetAttributes(to, File.GetAttributes(to) & ~FileAttributes.ReadOnly);
            }

            // Copy in the deployed file and overwrite existing if necessary
            File.Copy(from, to, true);
        }

        private bool CopyDeploymentItem(DeploymentItem deploymentItem)
        {
            Debug.Assert(!deploymentItem.Path.IsRelativePath);
            Debug.Assert(!deploymentItem.OutputDirectory.IsRelativePath);

            if (!Directory.Exists(deploymentItem.OutputDirectory))
            {
                if (!CreateDeploymentDirectory(deploymentItem.OutputDirectory))
                    return false;
            }

            ParsedPath newDeploymentFile = deploymentItem.OutputDirectory.Append(deploymentItem.Path.FileAndExtension);

            // Don't try and copy a file over itself
            if (newDeploymentFile == deploymentItem.Path)
                return true;

            try
            {
                SafeFileCopy(deploymentItem.Path, newDeploymentFile);

                // Special processing for executables.  Check if a .pdb file exists and deploy that too.
                if (deploymentItem.IsExecutable)
                {
                    ParsedPath deploymentFilePdb = deploymentItem.Path.ChangeExtension(".pdb");

                    if (File.Exists(deploymentFilePdb))
                    {
                        SafeFileCopy(deploymentFilePdb, newDeploymentFile.ChangeExtension(".pdb"));
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsCriticalException(e))
                    throw;

                Output.Warning(TestingResources.UnableToCopyFile(deploymentItem.Path, newDeploymentFile, e.Message));
                return false;
            }
        }

        private void RunTestMethod(object testObj, TestMethod testMethod)
        {
            if (testMethod == null)
                return;

            StringBuilder output = new StringBuilder();
            StringWriter outputWriter = new StringWriter(output);
            ProcessCycleStopwatch methodCycleStopwatch = null;
            Stopwatch methodStopwatch = new Stopwatch();

            testMethod.Outcome = UnitTestOutcome.Running;
            TestContext.ResetOutput();
            TestContext.ActiveTestMethod = testMethod;

            Output.Message(MessageImportance.Low, TestingResources.TestMethodRunning(testMethod.Name));

            try
            {
                if (ProcessCycleStopwatch.IsSupported)
                    methodCycleStopwatch = ProcessCycleStopwatch.StartNew();

                methodStopwatch.Start();

                ParameterInfo[] paramInfo = testMethod.MethodInfo.GetParameters();

                if (AttachDebugger)
                {
                    // Only attach to the first test that runs
                    AttachDebugger = false;

                    // Indicate to the user to attach the debugger, or Windows will
                    // put up the annoying problem reporting dialog.
                    Output.Message(MessageImportance.High, "[Attach debugger now or press a key to continue]");
                    
                    while (true)
                    {
                        if (Debugger.IsAttached)
                        {
                            Output.Message("Debugger is attached");
                            Debugger.Break();
                            break;
                        }
                        else if (!Console.KeyAvailable)
                        {
                            Thread.Sleep(250);
                            continue;
                        }
                        else
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            break;
                        }
                    }
                }

                if (testMethod.MethodInfo.IsStatic)
                {
                    if (paramInfo.Length == 0)
                        testMethod.MethodInfo.Invoke(null, null);
                    else if (paramInfo.Length == 1 && paramInfo[0].ParameterType == typeof(TestContext))
                        testMethod.MethodInfo.Invoke(null, new object[] { ToastTool.TestContext });
                    else
                        Debug.Fail("Unsupported test method type");
                }
                else
                {
                    if (paramInfo.Length == 0)
                        testMethod.MethodInfo.Invoke(testObj, null);
                    else
                        Debug.Fail("Unsupported test method type");
                }
            }
            catch (Exception e)
            {
                // Because we are using reflection the test exception will be inside a reflection invocation exception
                if (e.InnerException != null)
                    e = e.InnerException;

                // Check if one of the expected exceptions
                if (testMethod.ExpectedExceptions.Length > 0)
                {
                    if (Array.IndexOf<Type>(testMethod.ExpectedExceptions, e.GetType()) != -1)
                        // Exception was expected, ignore it
                        e = null;
                }

                if (e != null)
                {
                    testMethod.Exception = e;
                    testMethod.Outcome = UnitTestOutcome.Failed;
                }
            }
            finally
            {
                outputWriter.Close();

                methodStopwatch.Stop();
                testMethod.ExecutionTime = new TimeSpan(methodStopwatch.ElapsedTicks);

                if (methodCycleStopwatch != null)
                {
                    methodCycleStopwatch.Stop();
                    testMethod.ExecutionCycles = unchecked((long)methodCycleStopwatch.ElapsedCycles);
                }
            }

            if (testMethod.Outcome == UnitTestOutcome.Running)
                testMethod.Outcome = UnitTestOutcome.Passed;

            testMethod.Output = TestContext.GetOutput();
            TestContext.ActiveTestMethod = null;

            Output.Message(MessageImportance.High, TestingResources.PassFailMessage(
                (testMethod.Exception != null) ? TestingResources.FailedAllCaps : TestingResources.PassedAllCaps, testMethod.Name));

            if (testMethod.Exception != null)
            {
                Output.Error(testMethod.Exception.ToString());
            }
        }

        private bool CheckInstanceMethodVoidNoParams(MethodInfo method)
        {
            return CheckInstanceMethod(method) && CheckMethodVoidNoParams(method);
        }

        private bool CheckStaticMethodVoidNoParams(MethodInfo method)
        {
            return CheckStaticMethod(method) && CheckMethodVoidNoParams(method);
        }

        private bool CheckStaticMethod(MethodInfo method)
        {
            if (!method.IsStatic)
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseNotStatic(method.Name));
                return false;
            }

            return true;
        }

        private bool CheckInstanceMethod(MethodInfo method)
        {
            if (method.IsStatic)
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseInstance(method.Name));
                return false;
            }

            return true;
        }

        private bool CheckStaticMethodVoidTestContextParam(MethodInfo method)
        {
            return CheckStaticMethod(method) && CheckMethodVoidTestContextParam(method);
        }

        private bool CheckMethodVoidNoParams(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseNotVoidReturn(method.Name));
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 0)
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseHasParameters(method.Name));
                return false;
            }

            return true;
        }

        private bool CheckMethodVoidTestContextParam(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseNotVoidReturn(method.Name));
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 1 && parameters[0].GetType() != typeof(TestContext))
            {
                Output.Warning(TestingResources.MethodIgnoredBecauseNoTestContext(method.Name));
                return false;
            }

            return true;
        }

        private object FindAttribute(object[] attrs, Type attrType)
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i].GetType() == attrType)
                {
                    return attrs[i];
                }
            }

            return null;
        }

        private DeploymentItem CreateDeploymentItem(DeploymentItemAttribute deploymentItemAttr)
        {
            PropertyCollection properties = ToastTool.TestContext.Properties;
            DeploymentItem deploymentItem = null;
            string path = deploymentItemAttr.Path;
            string outputDirectory = deploymentItemAttr.OutputDirectory;

            try
            {
                if (properties != null)
                {
                    path = StringUtility.ReplaceTags(path, "$(", ")", properties);
                    outputDirectory = StringUtility.ReplaceTags(outputDirectory, "$(", ")", properties);
                }

                deploymentItem = new DeploymentItem(
                    new ParsedPath(path, PathType.File).MakeFullPath(ToastTool.TestContext.InternalOriginalCurrentDirectory),
                    deploymentItemAttr.OutputDirectory == ParsedPath.Empty ?
                        DeploymentDir : new ParsedPath(outputDirectory, PathType.Directory).MakeFullPath(DeploymentDir));
            }
            catch (ArgumentException e)
            {
                Output.Error(TestingResources.UnableToProcessDeploymentItem(
                    path, outputDirectory, e.Message));
            }

            return deploymentItem;
        }

        #endregion
        
        #region IProcessCommandLine Members

        public bool ProcessCommandLine(string[] args)
        {
            try
            {
                Parser.ParseAndSetTarget(args, this);
            }
            catch (CommandLineArgumentException e)
            {
                Output.Error(e.Message);
                return false;
            }

            return true;
        }

        #endregion
    }
}
