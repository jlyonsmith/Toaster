using System;
using System.Collections.Generic;
using System.Text;
using ToolBelt;

namespace Toaster
{
    class Program
    {
        private static IOutputter consoleOutputter = new ConsoleOutputter(Console.Error);
        
        static int Main(string[] args)
        {
            ToastTool tool = new ToastTool(consoleOutputter);

            // Get all the command line arguments
            if (!((IProcessCommandLine)tool).ProcessCommandLine(args))
            {
                return 1;
            }

            // Add a handler for any exceptions that occur on different threads.  
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            try
            {
                tool.Execute();
            }
            catch (Exception e)
            {
                // Log any exceptions that slip through.
                tool.Output.Error(e.ToString());
            }

            return tool.ExitCode;
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // We just want to spit out the exception and return a non-zero exit code.
            consoleOutputter.OutputErrorEvent(new OutputErrorEventArgs(e.ExceptionObject.ToString()));
            Environment.Exit(1);
        }
    }
}
