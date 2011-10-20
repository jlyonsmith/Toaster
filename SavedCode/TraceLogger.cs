using System;
using System.IO;
using System.Security;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Microsoft.VisualStudio.Diagnostics.TestHelpers
{
    public class TraceLogger : Logger
    {
        public override void Initialize(IEventSource eventSource)
        {
            if (Parameters != null)
            {
                throw new LoggerException("This logger does not have any parameters.");
            }

            eventSource.ProjectStarted += new ProjectStartedEventHandler(OnProjectStarted);
            eventSource.MessageRaised += new BuildMessageEventHandler(OnMessageRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(OnWarningRaised);
            eventSource.ErrorRaised += new BuildErrorEventHandler(OnErrorRaised);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(OnProjectFinished);
            eventSource.BuildStarted += new BuildStartedEventHandler(OnBuildStarted);
            eventSource.TargetStarted += new TargetStartedEventHandler(OnTargetStarted);
            eventSource.TargetFinished += new TargetFinishedEventHandler(OnTargetFinished);
        }

        void OnTargetFinished(object sender, TargetFinishedEventArgs e)
        {
            indent--;
            WriteLineIndented(String.Format("Done building target \"{0}\" in project \"{1}\" -- {2}",
                e.TargetName, Path.GetFileName(e.ProjectFile), e.Succeeded ? "SUCCEEDED" : "FAILED"));
        }

        void OnTargetStarted(object sender, TargetStartedEventArgs e)
        {
            WriteLineIndented(String.Format("Target {0}:", e.TargetName));
            indent++;
        }

        void OnErrorRaised(object sender, BuildErrorEventArgs e)
        {
            WriteLineIndented(FormatErrorEvent(e));
        }

        void OnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            WriteLineIndented(FormatWarningEvent(e));
        }

        void OnMessageRaised(object sender, BuildMessageEventArgs e)
        {
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
                )
            {
                WriteLineIndented(e.Message);
            }
        }

        void OnBuildStarted(object sender, BuildStartedEventArgs e)
        {
            WriteLineIndented(String.Format("Build started {0}.", e.Timestamp.ToString()));
        }

        void OnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // Just the regular message string is good enough here, so just display that.
            WriteLineIndented("__________________________________________________");
            WriteLineIndented(String.Format("Project \"{0}\" (Build target(s)):", e.ProjectFile));
            WriteLineIndented(String.Empty);
        }

        void OnProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            // The regular message string is good enough here too.
            WriteLineIndented(String.Format(String.Empty, e));
        }

        private void WriteLineIndented(string s)
        {
            // Loop through each line in the message and indent it
            StringReader reader = new StringReader(s);
            string tabs = new String('\t', indent);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                Trace.Write(tabs);
                Trace.WriteLine(line);
            }
        }

        private int indent;
    }
}

