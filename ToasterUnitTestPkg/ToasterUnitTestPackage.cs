using System;
using System.IO;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using VSLangProj2;
using VSLangProj80;
using VslangProj90;
using VslangProj100;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;
using System.Text;
using ToolBelt;

namespace Toaster
{
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the informations needed to show the this package in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidToasterUnitTestPkgString)]
	public sealed class ToastUnitTestPackage : Package, IDisposable
	{

		#region Private Classes
		private class ToasterProjectInfo
		{
			public ToasterProjectInfo(List<string> deploymentItemFiles, Project project)
			{
				DeploymentItemFiles = deploymentItemFiles;
				Project = project;
			}

			public List<string> DeploymentItemFiles { get; private set; }
			public Project Project { get; private set; }
		}

		#endregion

		#region Private Fields
		private DTE2 dte2;
		private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		private System.Diagnostics.Process testProcess;
		private const int buildTimerInterval = 200;
		private OutputHelper Output { get; set; }
		private string testResultsFile;
		private List<ToasterProjectInfo> toasterProjects;
		private int activeToasterProject = -1;

		#endregion

		#region Construction
		public ToastUnitTestPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		#endregion

		#region Package Members
		protected override void Initialize()
		{
			base.Initialize();

			this.dte2 = this.GetService(typeof(SDTE)) as DTE2;

			if (this.dte2 == null)
			{
				Debug.WriteLine("Unable to get DTE2 service");
				return;
			}

			this.Output = new OutputHelper(new PaneOutputter(dte2));

			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

			if (null == mcs)
			{
				Debug.WriteLine("Unable to get menu command service");
				return;
			}

			// Create the command for the menu item.
			CommandID menuCommandID = new CommandID(GuidList.guidToasterUnitTestCmdSet, (int)PkgCmdIDList.cmdidRunToastTests);
			MenuCommand menuItem = new MenuCommand(OnRunTests, menuCommandID);
			mcs.AddCommand(menuItem);

			// Hook up the timer event
			timer.Tick += new EventHandler(OnTimerTick);
			timer.Interval = buildTimerInterval;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (timer != null)
				{
					timer.Tick -= new EventHandler(OnTimerTick);
					timer.Dispose();
					timer = null;
				}

				if (testProcess != null)
				{
					testProcess.Dispose();
					testProcess = null;
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Event Handlers
		internal void OnTimerTick(object sender, EventArgs e)
		{
			// Check to see if the process that was launched has exited. If so, then we can 
			// stop the timer.
			if (testProcess != null && testProcess.HasExited)
			{
				timer.Stop();

				// Time to get the test results and transform them into a clickable output
				OutputTestResults();

				testProcess.Close();
				testProcess = null;

				activeToasterProject++;

				if (activeToasterProject < toasterProjects.Count)
				{
					// Start running the next test project
					this.ProcessTestProject(toasterProjects[activeToasterProject]);
				}
				else
				{
					toasterProjects = null;
					activeToasterProject = -1;
				}
			}
		}

		internal void OnOutputOrErrorDataReceived(object sendingProcess, DataReceivedEventArgs arg)
		{
			if (arg.Data != null)
			{
				// Just display the output
				Output.Message(arg.Data);
			}
		}

		internal void OnRunTests(object sender, EventArgs args)
		{
			((PaneOutputter)(Output.Outputter)).ClearOutput();

			if (dte2.Solution == null)
			{
				Output.Error("No solution open.");
				return;
			}

			Projects projects = (Projects)dte2.Solution.Projects;

			if (projects == null || projects.Count == 0)
			{
				Output.Error("Solution contains no projects");
				return;
			}

			// Create a new list of all Toaster unit test projects in the solution
			this.toasterProjects = new List<ToasterProjectInfo>();
			
			for (int i = 1; i <= projects.Count; i++)
			{
				Project project = projects.Item(i);

				if (project.Kind == PrjKind.prjKindCSharpProject ||
					project.Kind == PrjKind.prjKindVBProject)
				{
					ToasterProjectInfo toasterProjInfo = ExtractToastProjectInfo(project);

					if (toasterProjInfo != null)
						toasterProjects.Add(toasterProjInfo);
				}
				else if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
				{
					RecurseGetProjectItems(toasterProjects, project.ProjectItems);
				}
			}

			if (toasterProjects.Count == 0)
				Output.Warning("No unit test projects found");
			else
			{
				activeToasterProject = 0;

				// Process the first unit test project
				ProcessTestProject(toasterProjects[activeToasterProject]);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		#region Private Methods
		private void RecurseGetProjectItems(List<ToasterProjectInfo> toasterProjects, ProjectItems projectItems)
		{
			// Empty solution items folder
			if (projectItems == null)
				return;

			for (int i = 1; i <= projectItems.Count; i++)
			{
				ProjectItem projectItem = projectItems.Item((object)i);
				Project project = projectItem.SubProject;

				if (project == null)
					continue;

				if (project.Kind == PrjKind.prjKindCSharpProject ||
					project.Kind == PrjKind.prjKindVBProject)
				{
					ToasterProjectInfo toasterProjInfo = ExtractToastProjectInfo(project);

					if (toasterProjInfo != null)
						toasterProjects.Add(toasterProjInfo);
				}
				else if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
				{
					RecurseGetProjectItems(toasterProjects, projectItem.ProjectItems);
				}
			}
		}

		private ToasterProjectInfo ExtractToastProjectInfo(Project project)
		{
			bool isToastUnitTestProject = false;
			List<string> deploymentItemFiles = new List<string>();

			using (XmlReader reader = XmlReader.Create(project.FullName))
			{
				string toasterUnitTestProjectName = reader.NameTable.Add("ToasterUnitTestProject");
				string toasterDeploymentItemName = reader.NameTable.Add("ToasterDeploymentItem");
				string includeName = reader.NameTable.Add("Include");

				while (reader.Read())
				{
					if (object.Equals(reader.LocalName, toasterUnitTestProjectName))
					{
						isToastUnitTestProject = (String.Compare(
							reader.ReadElementContentAsString(), "true",
							StringComparison.InvariantCultureIgnoreCase) == 0);
					}
					else if (object.Equals(reader.LocalName, toasterDeploymentItemName))
					{
						reader.MoveToAttribute(includeName, String.Empty);

						while (reader.ReadAttributeValue())
						{
							deploymentItemFiles.Add(reader.Value);
						}
					}
				}
			}

			return isToastUnitTestProject ? new ToasterProjectInfo(deploymentItemFiles, project) : null;
		}

		private static IDictionary CreateProjectProperties(Configuration config)
		{
			Dictionary<string, string> properties = new Dictionary<string, string>(
				StringComparer.InvariantCultureIgnoreCase);

			properties.Add("Configuration", config.ConfigurationName);

			return properties;
		}

		private IList<DeploymentItem> GenerateDeploymentItems(
			ToasterProjectInfo toasterProjInfo, ParsedPath deploymentDir, ParsedPath projectDir, IDictionary properties)
		{
			List<DeploymentItem> deploymentItems = new List<DeploymentItem>();

			for (int i = 0; i < toasterProjInfo.DeploymentItemFiles.Count; i++)
			{
				string file = toasterProjInfo.DeploymentItemFiles[i];

				file = StringUtility.ReplaceTags(file, "$(", ")", properties, TaggedStringOptions.LeaveUnknownTags);

				deploymentItems.Add(new DeploymentItem(
					new ParsedPath(file, PathType.File).MakeFullPath(projectDir), deploymentDir));
			}

			return deploymentItems;
		}

		private void ProcessTestProject(ToasterProjectInfo toasterProjInfo)
		{
			Project project = toasterProjInfo.Project;
			Configuration config = project.ConfigurationManager.ActiveConfiguration;
			string outputPath = config.Properties.Item("OutputPath").Value.ToString();
			string assemblyName = project.Properties.Item("AssemblyName").Value.ToString();
			ParsedPath projectFile = new ParsedPath(project.FileName, PathType.File);
			ParsedPath projectDir = new ParsedPath(projectFile, PathParts.VolumeAndDirectory);
			ParsedPath testFile = new ParsedPath(projectFile, PathParts.VolumeAndDirectory).Append(outputPath, PathType.Directory).Append(assemblyName + ".dll", PathType.File);

			if (testFile != null)
			{
				Output.Message("Running unit test project '{0}'", project.Name);

				if (!File.Exists(testFile))
				{
					Output.Error("Unit test assembly '{0}' not found", testFile);
					return;
				}

				ParsedPath pkgAssemblyPath = new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File);
				ParsedPath solutionFile = new ParsedPath(dte2.Solution.FileName.ToString(), PathType.File);
				ParsedPath deploymentDir = new ParsedPath(solutionFile, PathParts.VolumeAndDirectory).Append(@"TestDeployment\", PathType.Directory);

				ToastTool tool = new ToastTool(this.Output.Outputter);

				IDictionary properties = CreateProjectProperties(config);
				IList<DeploymentItem> deploymentItems = GenerateDeploymentItems(
					toasterProjInfo, deploymentDir, projectDir, properties);

				// There may be no deployment items which means the deployment directory does not get created,
				// so force its creation now if necessary because we need it to exist 
				if (!Directory.Exists(deploymentDir))
				{
					if (!tool.CreateDeploymentDirectory(deploymentDir))
						return;
				}

				if (!tool.CopyDeploymentItems(deploymentItems))
					return;

				string toastExe;

				// Use toast from the extension directory
				toastExe = new ParsedPath(pkgAssemblyPath, PathParts.VolumeAndDirectory).Append("Toast.exe", PathType.File);

				DateTime now = DateTime.Now;
				ParsedPath outputFile = deploymentDir.Append(
					String.Format("{0}_{1:0000}{2:00}{3:00}_{4:00}{5:00}{6:00}.testresults",
					projectFile.File, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second), PathType.File);
				CommandLineParser parser = new CommandLineParser(typeof(ToastTool));

				tool.DeploymentDir = deploymentDir;
				tool.OutputFile = outputFile;
				tool.TestFile = testFile;

				parser.GetTargetArguments(tool);

				string arguments = parser.Arguments;

				arguments += " /p:Configuration=" + config.ConfigurationName.ToString();

				try
				{
					Output.Message("Running '\"{0}\"{1}{2}' in '{3}'",
						toastExe, parser.Arguments.Length > 0 ? " " : "",
						arguments, solutionFile.VolumeAndDirectoryNoSeparator);

					ProcessStartInfo info = new ProcessStartInfo(toastExe, arguments);

					info.UseShellExecute = false;
					info.CreateNoWindow = true;
					info.RedirectStandardOutput = true;
					info.RedirectStandardError = true;
					info.WorkingDirectory = solutionFile.VolumeAndDirectoryNoSeparator;

					testProcess = new System.Diagnostics.Process();

					testProcess.StartInfo = info;
					testProcess.OutputDataReceived += new DataReceivedEventHandler(OnOutputOrErrorDataReceived);
					testProcess.ErrorDataReceived += new DataReceivedEventHandler(OnOutputOrErrorDataReceived);

					bool ok = testProcess.Start();

					if (ok)
					{
						this.testResultsFile = outputFile;

						// Start a windows timer to check for the test process to finish
						timer.Start();

						testProcess.BeginOutputReadLine();
						testProcess.BeginErrorReadLine();
					}
					else
					{
						Output.Error("Program did not start");
					}
				}
				catch (Exception e)
				{
					Output.Error(e.ToString());
				}
			}
		}

		private void OutputTestResults()
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings();

			settings.ConformanceLevel = ConformanceLevel.Auto;

			XmlWriter xw = XmlTextWriter.Create(sb, settings);

			try
			{
				// Load the source document (to be transformed)
				XslCompiledTransform transform = new XslCompiledTransform();
				Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Toaster.UnitTest.xslt");
				Debug.Assert(resourceStream != null);

				transform.Load(XmlReader.Create(resourceStream));
				using (XmlReader xmlReader = XmlReader.Create(new StreamReader(testResultsFile, true)))
				{
					transform.Transform(xmlReader, xw);
				}
			}
			catch (Exception e)
			{
				Output.Error("Unable to transform test results '{0}'", testResultsFile);
				Output.Error("\n" + e);
			}
			finally
			{
				xw.Close();
			}

			Output.Message(sb.ToString());
		}

		#endregion
	}
}
