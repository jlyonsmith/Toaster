﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Runtime.InteropServices;
using ToolBelt;

namespace Toaster
{
    [CommandLineDescription("CrumbCommandLineDescription")]
    public class CrumbTool : ToolBase, IProcessCommandLine, ITool
    {
        #region Construction
        static CrumbTool()
        {
        }

        public CrumbTool()
        {
        }

        #endregion

        #region Public Properties
        [CommandLineArgument("help", Description = "HelpArgumentDescription", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [DefaultCommandLineArgument("assemblyfile", Description = "AssemblyFileArgumentDescription", ShortName = "f", ValueHint = "AssemblyFileArgumentHint", Initializer = typeof(InitializeAssemblyFile))]
        public ParsedPath AssemblyFile { get; set; }

        internal class InitializeAssemblyFile
        {
            public static ParsedPath Parse(string value)
            {
                return new ParsedPath(value, PathType.File).MakeFullPath();
            }
        }

        public int ExitCode { get { return (HasOutputErrors ? -1 : 0); } }

        public static TestContext TestContext { get; private set; }

        #endregion

        #region Private Properties
        private CommandLineParser parser;

        private CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(typeof(CrumbTool), typeof(TestingResources));

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

            if (AssemblyFile == null)
            {
                WriteError("Assembly file not supplied");
                return;
            }

            Assembly assembly = null;

            // TODO-johnls-1/6/2009: Deal with native images.  Error or what?

            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(AssemblyFile);
            }
            catch (Exception e)
            {
                if (e is FileLoadException)
                {
                    WriteError("Unable to load assembly '{0}'", AssemblyFile);
                    return;
                }

                if (e is FileNotFoundException)
                {
                    WriteError("Assembly cannot be found '{0}'", AssemblyFile);
                    return;
                }

                if (e is BadImageFormatException)
                {
                    WriteError("'{0}' is not a valid managed assembly", AssemblyFile);
                    return;
                }

                throw;
            }

            // This is all we came for
            WriteMessage(ButterTool.GetExtendedFullName(assembly.GetName()));

            return;
        }

        #endregion

        #region IProcessCommandLine Members

        public void ProcessCommandLine(string[] args)
        {
            Parser.ParseAndSetTarget(args, this);
        }

        #endregion
    }
}
