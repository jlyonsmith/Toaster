using System;
using System.Collections.Generic;
using System.Text;
using ToolBelt;

namespace Toaster
{
    class Program
    {
        static int Main(string[] args)
        {
            ToastTool tool = new ToastTool();

            // Add a handler for any exceptions that occur on different threads.  
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            try
            {
                tool.ProcessCommandLine(args);
                tool.Execute();
                
                return tool.ExitCode;
            }
            catch (Exception exception)
            {
                while (exception != null)
                {
                    ConsoleUtility.WriteMessage(MessageType.Error, "{0}", exception.Message);
                    exception = exception.InnerException;
                }
                return 1;
            }
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // We just want to spit out the exception and return a non-zero exit code.
            ConsoleUtility.WriteMessage(MessageType.Error, e.ExceptionObject.ToString());
            Environment.Exit(1);
        }
    }
}
