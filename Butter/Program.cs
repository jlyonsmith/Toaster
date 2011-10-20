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
            ButterTool butter = new ButterTool(new ConsoleOutputter(Console.Error));

            // Get any settings from the environment
            if (!((IProcessEnvironment)butter).ProcessEnvironment())
            {
                return 1;
            }

            // Get all the command line arguments
            if (!((IProcessCommandLine)butter).ProcessCommandLine(args))
            {
                return 1;
            }

            try
            {
                butter.Execute();
            }
            catch (Exception e)
            {
                // Log any exceptions that slip through
                butter.Output.Error(e.ToString());
            }

            return butter.ExitCode;
        }
    }
}
