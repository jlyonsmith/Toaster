﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ToolBelt;

namespace Toaster
{
    [CommandLineTitle("ButterCommandLineTitle")]
    [CommandLineDescription("ButterCommandLineDescription")]
    [CommandLineCopyright("JohnLyonSmithCopyright")]
    [CommandLineExample("ButterCommandLineExample")]
#if DEBUG
    [CommandLineConfiguration("CommandLineDebugConfig")]
#else
    [CommandLineConfiguration("CommandLineReleaseConfig")]
#endif
    public class ButterTool : ToolBase, IProcessCommandLine, IProcessEnvironment, ITool
    {
        #region Private Classes
        private class BinaryToInstrument
        {
            public BinaryToInstrument(ParsedPath file, ParsedPath deploymentDir)
            {
                File = file;
                
                // Fill out other properties using deployment directory, etc..
                SymbolFile = file.WithExtension(".mdb");
                InstrumentedFile = deploymentDir.Append(file.FileAndExtension);
                InstrumentedSymbolFile = InstrumentedFile.WithExtension(".mdb");
            }

            public bool InGlobalAssemblyCache { get; set; }
            public bool NeedsResigning { get; set; }
            public bool ForcedTo32Bit { get; set; }
            public AssemblyName AssemblyName { get; set; }
            public AssemblyName InstrumentedAssemblyName { get; set; }
            public ParsedPath File { get; set; }
            public ParsedPath SymbolFile { get; private set; }
            public ParsedPath InstrumentedFile { get; private set; }
            public ParsedPath InstrumentedSymbolFile { get; private set; }
        }
        
        #endregion
        
        #region Private Fields
        private CommandLineParser parser;
        private ParsedPath toastExe;
        private ParsedPath chgMvidExe;
        private ParsedPath crumbExe;
        private ParsedPath vsInstrExe;
        private ParsedPath vsPerfCmdExe;
        private ParsedPath snExe;
        private ParsedPath gacUtilExe;
        private ParsedPath ngenExe;
        private List<BinaryToInstrument> binariesToInstrument = new List<BinaryToInstrument>();

    	#endregion
        
        #region Construction
        public ButterTool()
        {
        }

        #endregion

        #region Public Properties
        [CommandLineArgument("help", Description = "HelpArgumentDescription", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument("property", Description = "PropertyArgumentDescription", ValueHint = "PropertyArgumentHint", ShortName = "p")]
        public string Property { get; set; }

        // This comes from the BUTTER_TOOLPATH environment variable
        public ParsedPathList ToolDirs { get; set; }

        // This comes from the BUTTER_KEYFILE environment variable
        public ParsedPath KeyFile { get; set; }

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

        [CommandLineArgument("coveragedeploymentitem", Description = "CoverageDeploymentItemArgumentDescription", ShortName = "cdi", Initializer = typeof(InitializeDeploymentItem))]
        public List<DeploymentItem> CoverageDeploymentItems { get; set; }

        [CommandLineArgument("deploymentitem", Description = "DeploymentItemArgumentDescription", ShortName = "di", Initializer = typeof(InitializeDeploymentItem))]
        public List<DeploymentItem> DeploymentItems { get; set; }

        internal class InitializeDeploymentItem
        {
            public static DeploymentItem Parse(string value)
            {
                return new DeploymentItem(value);
            }
        }

        [CommandLineArgument("outputfile", Description = "OutputFileArgumentDescription", ShortName = "o")]
        public ParsedPath OutputFile { get; set; }

        [CommandLineArgument("assemblydir", Description = "AssemblySearchDirArgumentDescription", ValueHint = "AssemblySearchDirArgumentHint", ShortName = "ad", Initializer = typeof(InitializeAssemblySearchDirs))]
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

        public int ExitCode { get { return (HasOutputErrors ? -1 : 0); } }
        
        #endregion

        #region Private Properties
        private CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(typeof(ButterTool), typeof(TestingResources));

                return parser;
            }
        }

        #endregion

        #region Public Methods
        public void Execute()
        {
            if (ShowHelp)
            {
                WriteMessage(Parser.LogoBanner);
                WriteMessage(Parser.Usage);
                return;
            }

            WriteMessage(
                TestingResources.RuntimeVersion(
                    Marshal.SizeOf(typeof(IntPtr)) == 4 ? TestingResources.WordSize32 : TestingResources.WordSize64,
                    RuntimeEnvironment.GetSystemVersion().ToString(),
                    RuntimeEnvironment.GetRuntimeDirectory()));

            if (TestFile == null)
            {
                WriteError(TestingResources.TestAssemblyNotSupplied);
                return;
            }

            if (DeploymentDir == null)
            {
                WriteError(TestingResources.DeploymentDirectoryMustBeSpecified);
                return;
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

            // First, find the tools we'll need.
            if (!FindTool("toast.exe", out toastExe))
                return;

            if (!FindTool("chgmvid.exe", out chgMvidExe))
                return;

            if (!FindTool("crumb.exe", out crumbExe))
                return;

            if (!FindTool("vsinstr.exe", out vsInstrExe))
                return;

            if (!FindTool("vsperfcmd.exe", out vsPerfCmdExe))
                return;

            if (!FindTool("sn.exe", out snExe))
                return;

            if (!FindTool("gacutil.exe", out gacUtilExe))
                return;

            if (!FindTool("ngen.exe", out ngenExe))
                return;

            WriteMessage(TestingResources.UsingExeFrom(toastExe.FileAndExtension, toastExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(crumbExe.FileAndExtension, crumbExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(vsInstrExe.FileAndExtension, vsInstrExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(vsPerfCmdExe.FileAndExtension, vsPerfCmdExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(snExe.FileAndExtension, snExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(gacUtilExe.FileAndExtension, gacUtilExe.VolumeAndDirectory));
            WriteMessage(TestingResources.UsingExeFrom(ngenExe.FileAndExtension, ngenExe.VolumeAndDirectory));

            // Expand all paths in any command line deployment items relative to the working directory
            foreach (var deploymentItem in DeploymentItems)
            {
                deploymentItem.Path = deploymentItem.Path.MakeFullPath();
                deploymentItem.OutputDirectory = DeploymentDir;
            }

            // Check for coverage deployment items
            if (CoverageDeploymentItems.Count == 0)
            {
                WriteError(TestingResources.NoCoverageDeploymentItemsSpecified);
                return;
            }

            // Process the coverage deployment items, expanding paths, checking if executable and that they exist
            foreach (var deploymentItem in CoverageDeploymentItems)
            {
                deploymentItem.Path = deploymentItem.Path.MakeFullPath();
                deploymentItem.OutputDirectory = DeploymentDir;

                if (!deploymentItem.IsExecutable)
                {
                    WriteError(TestingResources.CoverageDeploymentItemMustBeExecutable(deploymentItem.Path.ToString()));
                    return;
                }

                if (!File.Exists(deploymentItem.Path))
                {
                    WriteError(TestingResources.DeploymentItemDoesNotExist(deploymentItem.Path.ToString()));
                    return;
                }

                BinaryToInstrument binaryToInstrument = new BinaryToInstrument(deploymentItem.Path, DeploymentDir);
                
                binariesToInstrument.Add(binaryToInstrument);
            }

            // Create the deployment directory
            if (!Directory.Exists(DeploymentDir))
                Directory.CreateDirectory(DeploymentDir);

            // Get each assemblies full name
            WriteMessage("Getting assembly full names");

            foreach (var binaryToInstrument in binariesToInstrument)
            {
                // TODO-johnls-1/6/2009: Deal with unmanaged modules - they won't have an assembly name

                GetAssemblyFullName(binaryToInstrument);

                if (HasOutputErrors)
                    return;

                if (binaryToInstrument.AssemblyName != null)
                {
                    CheckForBinaryInGac(binaryToInstrument);

                    if (HasOutputErrors)
                        return;
                }
            }

            // Check assembly GAC status
            WriteMessage("Getting assembly GAC status");

            foreach (var binaryToInstrument in binariesToInstrument)
            {
                if (binaryToInstrument.AssemblyName != null)
                {
                    CheckForBinaryInGac(binaryToInstrument);

                    if (HasOutputErrors)
                        return;
                }
            }

            // Instrument binaries
            WriteMessage(TestingResources.InstrumentBinaries);

            foreach (var binaryToInstrument in binariesToInstrument)
            {
                InstrumentBinaryForCoverage(binaryToInstrument);

                if (HasOutputErrors)
                    return;

                ChangeMvid(binaryToInstrument);
            }

            // Resign assemblies 
            WriteMessage("Resign Instrumented Assemblies");

            foreach (var binaryToInstrument in binariesToInstrument)
            {
                if (binaryToInstrument.NeedsResigning)
                {
                    if (KeyFile == null)
                    {
                        WriteWarning(TestingResources.AssemblyHasAStrongNameAndNoKeyFile(binaryToInstrument.File));
                        break;
                    }

                    ResignAssembly(binaryToInstrument, KeyFile);

                    if (HasOutputErrors)
                        return;
                }
            }

            try
            {
                // Check for strongly named assemblies in the GAC and replace with instrumented ones
                WriteMessage(TestingResources.UpdateGlobalAssemblyCache);

                foreach (var binaryToInstrument in binariesToInstrument)
                {
                    if (binaryToInstrument.InGlobalAssemblyCache)
                    {
                        if (binaryToInstrument.InstrumentedAssemblyName != null)
                        {
                            // The processor architecture changed, we must attempt to uninstall/install the assembly
                            // This might fail if the assembly was installed by an MSI.
                            UninstallBinaryFromGac(binaryToInstrument);

                            if (HasOutputErrors)
                                break;

                            InstallInstrumentedBinaryToGac(binaryToInstrument);

                            if (HasOutputErrors)
                                break;
                        }
                        else
                        {
                            UpdateBinaryInGacWithInstrumented(binaryToInstrument);

                            if (HasOutputErrors)
                                break;
                        }
                    }
                }

                // If any GAC updates failed, bail out now attempting to fix things in the finalizer
                if (HasOutputErrors)
                    return;

                DateTime now = DateTime.Now;
                ParsedPath coverageFile = this.DeploymentDir.Append(
                    String.Format("{0}_{1:0000}{2:00}{3:00}_{4:00}{5:00}{6:00}.coverage", 
                    TestFile.File, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second), 
                    PathType.File);
                
                // Start the coverage monitor
                WriteMessage(TestingResources.StartCoverageMonitor);

                StartCoverageMonitor(coverageFile);

                if (HasOutputErrors)
                    return;

                try
                {
                    // Run the unit tests
                    WriteMessage(TestingResources.RunUnitTests);

                    CommandLineParser parser = new CommandLineParser(typeof(ToastTool));
                    ToastTool toast = new ToastTool();

                    toast.TestClassName = this.TestClassName;
                    toast.TestMethodName = this.TestMethodName;
                    toast.AssemblySearchDirs = this.AssemblySearchDirs;
                    toast.OutputFile = this.OutputFile;
                    toast.TestFile = this.TestFile;
                    toast.Property = this.Property;
                    toast.DeploymentDir = this.DeploymentDir;
                    toast.DeploymentItems = this.DeploymentItems;

                    parser.GetTargetArguments(toast);

                    int exitCode = Command.Run(String.Format("\"{0}\" {1}", toastExe, parser.Arguments));

                    if (exitCode != 0)
                        WriteError(TestingResources.UnitTestsFailedToRun);
                }
                finally
                {
                    // Shutdown the coverage monitor
                    WriteMessage(TestingResources.StopCoverageMonitor);

                    StopCoverageMonitor();
                }
            }
            finally
            {
                WriteMessage(TestingResources.UpdateGlobalAssemblyCache);

                // Restore the original binaries in the GAC and NGEN caches
                foreach (var binaryToInstrument in binariesToInstrument)
                {
                    if (binaryToInstrument.InGlobalAssemblyCache)
                    {
                        if (binaryToInstrument.InstrumentedAssemblyName != null)
                        {
                            UninstallInstrumentedBinaryFromGac(binaryToInstrument);
                            InstallBinaryToGac(binaryToInstrument);
                        }
                        else
                        {
                            UpdateBinaryInGacWithOriginal(binaryToInstrument);
                        }
                    }
                }
            }

            return;
        }

        public static string GetExtendedFullName(AssemblyName assemblyName)
        {
            if (assemblyName.ProcessorArchitecture == ProcessorArchitecture.None)
                return String.Format("{0}",
                    assemblyName.FullName);
            else
                return String.Format("{0}, processorArchitecture={1}",
                    assemblyName.FullName, assemblyName.ProcessorArchitecture.ToString().ToLower());
        }

        #endregion

        #region Private Methods

        private bool FindTool(string toolExe, out ParsedPath toolExePath)
        {
            toolExePath = null;
            
            IList<ParsedPath> paths;
            ParsedPath toolPath = new ParsedPath(toolExe, PathType.File);

            // NOTE: We specifically DO NOT look in the current directory.  To do that simply add '.' to the PATH
            // or the list of ToolDirs.

            // If there are no tool directories specified, look in the PATH
            if (ToolDirs == null || ToolDirs.Count == 0)
            {
                paths = PathUtility.FindFileInPaths(
                    new ParsedPathList(Environment.GetEnvironmentVariable("PATH"), PathType.Directory), toolPath);

                if (paths.Count == 0)
                {
                    WriteError(TestingResources.CannotFindExeInPathDirs(toolExe));
                    return false;
                }
            }
            else
            {
                paths = PathUtility.FindFileInPaths(new ParsedPathList(ToolDirs, PathType.Directory), toolPath);

                if (paths.Count == 0)
                {
                    WriteError(TestingResources.CannotFindExeInToolDirs(toolExe));
                    return false;
                }
            }

            // Take the first tool.exe that we found
            toolExePath = new ParsedPath(paths[0], PathType.File);

            return true;
        }

        private void RunTool(string command, Action<string> successAction)
        {
            RunTool(command, successAction, true);
        }

        private void RunTool(string command, Action<string> successAction, bool captureOutput)
        {
            string output;
            int exitCode;

            WriteMessage(command);

            if (captureOutput)
            {
                exitCode = Command.Run(command, out output);
            }
            else
            {
                exitCode = Command.Run(command, null, null, debugMode:false);
                output = String.Empty;
            }

            if (exitCode == 0)
            {
                successAction(output);
            }
            else
            {
                WriteError(String.Format(TestingResources.FailureRunning(command, output)));
            }
        }

        private void InstrumentBinaryForCoverage(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" /coverage \"/outputpath:{1}\" \"{2}\"",
                vsInstrExe,
                binaryToInstrument.InstrumentedFile.VolumeAndDirectoryNoSeparator,
                binaryToInstrument.File), 
                output =>
                {
                    binaryToInstrument.NeedsResigning = (output.IndexOf("VSP2001") != -1);
                    binaryToInstrument.ForcedTo32Bit = (output.IndexOf("VSP2013") != -1);

                    if (binaryToInstrument.ForcedTo32Bit)
                    {
                        AssemblyName assemblyName = new AssemblyName();

                        assemblyName.Name = binaryToInstrument.AssemblyName.Name;
                        assemblyName.Version = binaryToInstrument.AssemblyName.Version;
                        assemblyName.CultureInfo = binaryToInstrument.AssemblyName.CultureInfo;
                        assemblyName.SetPublicKeyToken(binaryToInstrument.AssemblyName.GetPublicKeyToken());
                        assemblyName.ProcessorArchitecture = ProcessorArchitecture.X86;

                        binaryToInstrument.InstrumentedAssemblyName = assemblyName;
                    }

                    WriteMessage("Instrumented '{0}' to '{1}", binaryToInstrument.File, binaryToInstrument.InstrumentedFile);
                });
        }

        private void ChangeMvid(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" \"{1}\"", chgMvidExe, binaryToInstrument.InstrumentedFile),
                output =>
                {
                    WriteMessage("Changed MVID on instrumented assembly '{0}'", binaryToInstrument.InstrumentedFile);
                });
        }

        private void GetAssemblyFullName(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" \"{1}\"", crumbExe, binaryToInstrument.File),
                output =>
                {
                    binaryToInstrument.AssemblyName = new AssemblyName(output.Trim());

                    WriteMessage("Assembly '{0}' has extended full name '{1}'",
                        binaryToInstrument.File, GetExtendedFullName(binaryToInstrument.AssemblyName));
                });
        }

        private void CheckForBinaryInGac(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" -nologo -l \"{1}\"", gacUtilExe, GetExtendedFullName(binaryToInstrument.AssemblyName)),
                output =>
                {
                    binaryToInstrument.InGlobalAssemblyCache = !(output.IndexOf("Number of items = 0") != -1);

                    if (binaryToInstrument.InGlobalAssemblyCache)
                    {
                        WriteMessage("Assembly '{0}' found in GAC",
                            binaryToInstrument.File);
                    }
                    else
                    {
                        WriteMessage("Assembly '{0}' not found in GAC",
                            binaryToInstrument.File);
                    }
                });
        }

        private void UninstallBinaryFromGac(BinaryToInstrument binaryToInstrument)
        {
            UninstallBinaryFromGac(binaryToInstrument.AssemblyName);
        }

        private void UninstallInstrumentedBinaryFromGac(BinaryToInstrument binaryToInstrument)
        {
            UninstallBinaryFromGac(binaryToInstrument.InstrumentedAssemblyName);
        }
        
        private void UninstallBinaryFromGac(AssemblyName assemblyName)
        {
            string fullName = GetExtendedFullName(assemblyName);

            RunTool(String.Format("\"{0}\" -nologo -u \"{1}\"", gacUtilExe, fullName),
                output =>
                {
                    WriteMessage("Uninstalled assembly '{0}' from GAC", fullName);
                });
        }

        private void InstallBinaryToGac(BinaryToInstrument binaryToInstrument)
        {
            InstallBinaryToGac(binaryToInstrument.File);
        }

        private void InstallInstrumentedBinaryToGac(BinaryToInstrument binaryToInstrument)
        {
            InstallBinaryToGac(binaryToInstrument.InstrumentedFile);
        }

        private void InstallBinaryToGac(ParsedPath fileName)
        {
            RunTool(String.Format("\"{0}\" -nologo -i \"{1}\"", gacUtilExe, fileName),
                output =>
                {
                    WriteMessage("Installed assembly '{0}' to GAC", fileName);
                });
        }

        private void UpdateBinaryInGacWithInstrumented(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" -nologo -f -i \"{1}\"", gacUtilExe, binaryToInstrument.InstrumentedFile),
                output =>
                {
                    WriteMessage("Installed '{0}' to GAC", binaryToInstrument.InstrumentedFile);
                });
        }

        private void UpdateBinaryInGacWithOriginal(BinaryToInstrument binaryToInstrument)
        {
            RunTool(String.Format("\"{0}\" -nologo -f -i \"{1}\"", gacUtilExe, binaryToInstrument.File), 
                output => 
                {
                    WriteMessage("Updated assembly '{0}' in GAC", GetExtendedFullName(binaryToInstrument.AssemblyName));
                });
        }

        private void ResignAssembly(BinaryToInstrument binaryToInstrument, ParsedPath keyFile)
        {
            RunTool(String.Format("\"{0}\" -Ra \"{1}\" \"{2}\"", snExe, binaryToInstrument.InstrumentedFile, keyFile),
                output =>
                {
                    WriteMessage("Resigned '{0}'", binaryToInstrument.InstrumentedFile);
                });
        }

        private void StartCoverageMonitor(ParsedPath coverageFile)
        {
            // Note that we cannot do any redirection on this command as it passes it's output handles to a spawned child process.
            // The Process class waits until the processes output streams are closed before considering it exited, which causes us to hang
            // if we try and redirect any output.
            RunTool(String.Format("\"{0}\" /start:coverage \"/output:{1}\"", vsPerfCmdExe, coverageFile),
                output =>            
                {
                    WriteMessage("Monitor started");
                }, 
                false);
        }

        private void StopCoverageMonitor()
        {   
            RunTool(String.Format("\"{0}\" /shutdown", vsPerfCmdExe),
                output =>
                {
                    WriteMessage("Monitor stopped");
                });
        }

        #endregion

        #region IProcessCommandLine Members

        public void ProcessCommandLine(string[] args)
        {
            Parser.ParseAndSetTarget(args, this);
        }

        #endregion

        #region IProcessEnvironment Members

        public void ProcessEnvironment()
        {
            string toolPath = Environment.GetEnvironmentVariable("BUTTER_TOOLPATH");

            if (!String.IsNullOrEmpty(toolPath))
            {
                string[] toolDirs = toolPath.Split(Path.PathSeparator);

                ToolDirs = new ParsedPathList();

                foreach (var dir in toolDirs)
                {
                    ToolDirs.Add(new ParsedPath(dir, PathType.Directory).MakeFullPath());
                }
            }
            
            string keyFile = Environment.GetEnvironmentVariable("BUTTER_KEYFILE");

            if (!String.IsNullOrEmpty(keyFile))
            {
                KeyFile = new ParsedPath(keyFile, PathType.File).MakeFullPath();
            }
        }

        #endregion
    }
}
