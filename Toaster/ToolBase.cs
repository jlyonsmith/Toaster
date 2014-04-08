using System;
using ToolBelt;

namespace Toaster
{
    public abstract class ToolBase
    {
        public bool HasOutputErrors { get; protected set; }
        public bool WarningsAsErrors { get; protected set; }

        protected void WriteError(string format, params object[] args)
        {
            HasOutputErrors = true;

            ConsoleUtility.WriteMessage(MessageType.Error, format, args);
        }

        protected void WriteWarning(string format, params object[] args)
        {
            if (WarningsAsErrors)
                HasOutputErrors = true;

            ConsoleUtility.WriteMessage(MessageType.Warning, format, args);
        }

        protected void WriteMessage(string format, params object[] args)
        {
            ConsoleUtility.WriteMessage(MessageType.Normal, format, args);
        }
    }
}

