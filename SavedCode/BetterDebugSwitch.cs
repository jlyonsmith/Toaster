/*
 * Could something like this be used to implement a better /debug switch?
 * 
 * 1. Get to just before running first method
 * 2. Do all the guff below; start VS (maybe), get DTE, attach VS to this process.  
 *    Might need to do this in an external process.  Use global atom table to communicate 
 *    method name to set breakpoint.
 * 3. Run on and hit breakpoint that the debugger set.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Security.Permissions;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Diagnostics.TestHelpers;
using System.IO;

#pragma warning disable 618

namespace Microsoft.VisualStudio.TraceLog.Instrumentation.UnitTest
{
    class InstrumentationTestHelper : IDisposable
    {
        public InstrumentationTestHelper(
            ConfigMessagePacker._CollectionPlan collectionPlan,
            string unitTestDll,
            Type targetType,
            Microsoft.VisualStudio.TraceLog.ConfigMessagePacker.TraceOptions instrumentationOptions,
            bool startDevEnv,
            bool attach,
            string debugOutput,
            bool printf,
            string solutionName)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "collectionplan.xml");
            Environment.SetEnvironmentVariable("VSLOGGER_CPLAN", path);

            collectionPlan.ToXml(path);

            InternalInit(UnitTestType.Detour, unitTestDll, targetType, instrumentationOptions, startDevEnv, attach, debugOutput, printf, solutionName);
        }

        public InstrumentationTestHelper(
            UnitTestType testType,
            string unitTestDll,
            Type targetType,
            Microsoft.VisualStudio.TraceLog.ConfigMessagePacker.TraceOptions instrumentationOptions,
            bool startDevEnv,
            bool attach,
            string debugOutput,
            bool printf,
            string solutionName)
        {
            Environment.SetEnvironmentVariable("VSTS_INSTRUMENTATION_METHOD", "1"); //2 == Detour
            Environment.SetEnvironmentVariable("VSTS_NOLOGGERNAME", "1"); // run instrumentation even if a vs logger cannot be found
            Environment.SetEnvironmentVariable("VSTS_INSTRUMENT", unitTestDll);
            Environment.SetEnvironmentVariable("VSTS_INSTRUMENTATION_OPTIONS", ((int)instrumentationOptions).ToString());

            InternalInit(testType, unitTestDll, targetType, instrumentationOptions, startDevEnv, attach, debugOutput, printf, solutionName);
        }

        private void InternalInit(
            UnitTestType testType,
            string unitTestDll,
            Type targetType,
            Microsoft.VisualStudio.TraceLog.ConfigMessagePacker.TraceOptions instrumentationOptions,
            bool startDevEnv,
            bool attach,
            string debugOutput,
            bool printf,
            string solutionName)
        {
            EnvDTE._DTE dte = null;

            if (solutionName != null)
            {
                dte = GetIDEInstances(solutionName);
            }

            Environment.SetEnvironmentVariable("VSTS_DEBUG", debugOutput);
            Environment.SetEnvironmentVariable("COR_PROFILER", "{301EC75B-AD5A-459c-A4C4-911C878FA196}");
            Environment.SetEnvironmentVariable("Cor_Enable_Profiling", "1");
            Environment.SetEnvironmentVariable("UNITTEST_LOGGER", "1");
#if NO_RAZZLE
            Environment.SetEnvironmentVariable("VSTS_DUMP_RESULT", "0");
#endif
            Environment.SetEnvironmentVariable("VSTS_FILE_LOGGER_PRINTF", printf ? "1" : "0");

            string target = "ProxyServer.exe";
            string port = "localhost:" + (9090 + (int)instrumentationOptions).ToString();
            string sink = testType.ToString() + ".rem";
            string sinkUrl = "ipc://" + port + "/" + sink;

            EventWaitHandle startEvent = new EventWaitHandle(false, EventResetMode.ManualReset, sinkUrl + "/StartEvent");

            _stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset, sinkUrl + "/StopEvent");

            string args = port + " " + sink;

            if (startDevEnv && !attach)
            {
                args = " /debugexe " + target + " " + args;
                target = TestUtility.DevEnvExe;
            }

            _server = System.Diagnostics.Process.Start(target, args);

            Assert.IsNotNull(_server, "Failed to start proxy server");

            startEvent.WaitOne();

            if (dte != null)
            {
                EnvDTE80.Debugger2 dbg2 = dte.Debugger as EnvDTE80.Debugger2;
                EnvDTE80.Transport trans = dbg2.Transports.Item("Default") as EnvDTE80.Transport;
                EnvDTE80.Engine [] engines = new EnvDTE80.Engine [] { trans.Engines.Item("Managed") as EnvDTE80.Engine, trans.Engines.Item("Native") as EnvDTE80.Engine };
                EnvDTE80.Process2 proc2 = dbg2.GetProcesses(trans, System.Environment.MachineName).Item("Server.exe") as EnvDTE80.Process2;
                proc2.Attach2(engines);
            }

            BinaryServerFormatterSinkProvider serverSinkProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientSinkProvider = new BinaryClientFormatterSinkProvider();
            IDictionary properties = new System.Collections.Hashtable();

            serverSinkProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            properties["name"] = "ipc://" + port;
            properties["priority"] = "20";
            properties["portName"] = port;
            properties["exclusiveAddressUse"] = "false";

            _channel = new IpcChannel(properties, clientSinkProvider, serverSinkProvider);
            ChannelServices.RegisterChannel(_channel, true);

            // Create a message sink.
            string objectUri;
            System.Runtime.Remoting.Messaging.IMessageSink messageSink = _channel.CreateMessageSink(sinkUrl, null, out objectUri);

            WellKnownClientTypeEntry remotetype = new WellKnownClientTypeEntry(targetType, sinkUrl);
            RemotingConfiguration.RegisterWellKnownClientType(remotetype);

            _invoker = (MethodInvoker)Activator.CreateInstance(targetType);

            Assert.IsNotNull(_invoker, "Failed to get method invoker");
        }

        public MethodInvoker Invoker
        {
            get
            {
                return _invoker;
            }
        }

        public void Dispose()
        {
            if (_stopEvent != null)
            {
                _stopEvent.Set();
                _stopEvent = null;
            }

            _server = null;
        }

        private class NativeMethods
        {
            [DllImport("ole32.dll")]
            public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

            [DllImport("ole32.dll")]
            public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
        }

        private static EnvDTE._DTE GetIDEInstances(string name)
        {
            Hashtable runningObjects = GetRunningObjectTableSnapshot();
            IDictionaryEnumerator rotEnumerator = runningObjects.GetEnumerator();

            while (rotEnumerator.MoveNext())
            {
                string candidateName = (string) rotEnumerator.Key;

                // VS DTE objects always start with this magic prefix...
                if (!candidateName.StartsWith("!VisualStudio.DTE"))
                    continue;

                EnvDTE._DTE ide = rotEnumerator.Value as EnvDTE._DTE;

                if (ide == null)
                    continue;

                try
                {
                    string solutionFile = ide.Solution.FullName;

                    if (solutionFile != String.Empty && solutionFile.IndexOf(name) >= 0)
                    {
                        return ide;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static Hashtable GetRunningObjectTableSnapshot()
        {
            Hashtable result = new Hashtable();
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            NativeMethods.GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, IntPtr.Zero) == 0)
            {
                object runningObjectVal = null;

                try
                {
                    runningObjectTable.GetObject(monikers[0], out runningObjectVal);
                }
                catch (FileNotFoundException)
                {
                    // This happens if the VS instance isn't properly initialized
                    continue;
                }

                if (runningObjectVal == null)
                    continue;

                IBindCtx ctx;

                NativeMethods.CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                result[runningObjectName] = runningObjectVal;
            }

            return result;
        }

        #region Private Fields
		private MethodInvoker _invoker;
        private EventWaitHandle _stopEvent;
        private System.Diagnostics.Process _server;
        private IpcChannel _channel;

	    #endregion
    }
}
