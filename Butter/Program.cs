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
            ButterTool tool = new ButterTool();

            try
            {
                tool.ProcessEnvironment();
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
    }
}
