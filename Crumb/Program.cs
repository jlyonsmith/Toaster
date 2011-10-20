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
            CrumbTool crumb = new CrumbTool(new ConsoleOutputter(Console.Error));

            // Get all the command line arguments
            if (!((IProcessCommandLine)crumb).ProcessCommandLine(args))
            {
                return 1;
            }

            try
            {
                crumb.Execute();
            }
            catch (Exception e)
            {
                // Log any exceptions that slip through
                crumb.Output.Error(e.ToString());
            }

            return crumb.ExitCode;
        }
    }
}
