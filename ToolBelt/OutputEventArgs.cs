using System;
using System.Threading;

namespace ToolBelt
{
    [Serializable]
    public abstract class OutputEventArgs : EventArgs
    {
        private string message;
        private int threadId;
        private DateTime timestamp;

        protected OutputEventArgs() { }
        protected OutputEventArgs(string message)
        {
            this.timestamp = DateTime.Now;
            // A sneaky way to get the current thread ID
            this.threadId = Thread.CurrentThread.GetHashCode();
            this.message = message;
        }

        public string Message { get { return message; } }
        public int ThreadId { get { return threadId; } }
        public DateTime Timestamp { get { return timestamp; } }
    }

    [Serializable]
    public class OutputErrorEventArgs : OutputEventArgs
    {
        private string code;

        public OutputErrorEventArgs() { }
        public OutputErrorEventArgs(string message) : base(message) { }
        public OutputErrorEventArgs(string message, string code)
            : base(message)
        {
            this.code = code;
        }

        public string Code { get { return code; } }
    }

    [Serializable]
    public class OutputWarningEventArgs : OutputEventArgs
    {
        private string code;

        public OutputWarningEventArgs() { }
        public OutputWarningEventArgs(string message) : base(message) { }
        public OutputWarningEventArgs(string message, string code)
            : base(message)
        {
            this.code = code;
        }

        public string Code { get { return code; } }
    }

    [Serializable]
    public enum MessageImportance
    {
        High = 0,
        Normal = 1,
        Low = 2
    }

    [Serializable]
    public class OutputMessageEventArgs : OutputEventArgs
    {
        private MessageImportance importance;

        public OutputMessageEventArgs() { }
        public OutputMessageEventArgs(string message, MessageImportance importance)
            : base(message)
        {
            this.importance = importance;
        }

        public MessageImportance Importance { get { return importance; } }
    }

    [Serializable]
    public class OutputCustomEventArgs : EventArgs
    {
        public OutputCustomEventArgs() : base() { }
    }
}

