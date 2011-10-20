using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.Diagnostics.TestHelpers
{
    public static class BuildUtility
    {
        // TODO-johnls-2008/10:  Tests that use this class have been disabled.  Perhaps put this 
        // into a Toast helper library, but make sure there are CLR v2.0 and v4.0 versions.
        public static void RunBuild(string projectFile, string logFile, string target)
        {
            RunBuild(projectFile, logFile, target, null);
        }

        public static void RunBuild(string projectFile, string logFile, string target, BuildPropertyGroup properties)
        {
            bool result = false;
            Engine engine = new Engine();

            try
            {
                FileLogger fileLogger = new FileLogger();

                fileLogger.Parameters = String.Format("logfile={0}", logFile);
                fileLogger.Verbosity = LoggerVerbosity.Diagnostic;

                TraceLogger traceLogger = new TraceLogger();

                traceLogger.Verbosity = LoggerVerbosity.Normal;

                engine.RegisterLogger(fileLogger);
                engine.RegisterLogger(traceLogger);

                if (properties == null)
                    result = engine.BuildProjectFile(projectFile, new string[] { target });
                else
                    result = engine.BuildProjectFile(projectFile, new string[] { target }, properties);
            }
            finally
            {
                engine.UnregisterAllLoggers();
            }

            Assert.IsTrue(result,
                String.Format("Build of project '{0}', target '{1}' failed.  See {2} for more details.",
                    projectFile, target, logFile));
        }
    }
}
