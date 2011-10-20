using System;
using System.Threading;

namespace ToolBelt
{
    [Serializable]
    public abstract class OutputEventArgs : EventArgs
    {
        private string _message;
        private int _threadId;
        private DateTime _timestamp;

        protected OutputEventArgs() { }
        protected OutputEventArgs(string message)
        {
            _timestamp = DateTime.Now;
            // A sneaky way to get the current thread ID
            _threadId = Thread.CurrentThread.GetHashCode();
            _message = message;
        }

        public string Message { get { return _message; } }
        public int ThreadId { get { return _threadId; } }
        public DateTime Timestamp { get { return _timestamp; } }
    }

    [Serializable]
    public class OutputErrorEventArgs : OutputEventArgs
    {
        private string _code;

        public OutputErrorEventArgs() { }
        public OutputErrorEventArgs(string message) : base(message) { }
        public OutputErrorEventArgs(string message, string code)
            : base(message)
        {
            _code = code;
        }

        public string Code { get { return _code; } }
    }

    [Serializable]
    public class OutputWarningEventArgs : OutputEventArgs
    {
        private string _code;

        public OutputWarningEventArgs() { }
        public OutputWarningEventArgs(string message) : base(message) { }
        public OutputWarningEventArgs(string message, string code)
            : base(message)
        {
            _code = code;
        }

        public string Code { get { return _code; } }
    }

    [Serializable]
    public enum MessageImportance
    {
        High,
        Normal,
        Low
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

