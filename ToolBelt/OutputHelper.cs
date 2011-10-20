using System;
using System.Resources;
using System.Text.RegularExpressions;

namespace ToolBelt
{
    public class OutputHelper
    {
        #region Private Fields
        private IOutputter outputter;
        private bool hasOutputErrors;
        private ResourceManager resourceManager;
        private bool warningsAsErrors;
        private static Regex prefixRegex = new Regex(@"^\s*([A-Za-z]+\d+):\s*(.*)");

        #endregion

        #region Constructors
        public OutputHelper(IOutputter outputter)
        {
            this.outputter = outputter;
            this.hasOutputErrors = false;
        }

        public OutputHelper(IOutputter outputter, ResourceManager resourceManager)
        {
            this.outputter = outputter;
            this.resourceManager = resourceManager;
            this.hasOutputErrors = false;
            this.warningsAsErrors = false;
        }

        #endregion

        #region Private Methods
        public static string ExtractMessageCode(string messageIn, out string messageOut)
        {
            Match match = prefixRegex.Match(messageIn);

            if (match.Success)
            {
                messageOut = match.Groups[2].Value;
                return match.Groups[1].Value;
            }
            else
            {
                messageOut = messageIn;
                return null;
            }
        }

        #endregion

        #region Public Properties
        public IOutputter Outputter 
        {
            get { return this.outputter; }
            set { this.outputter = value; } 
        }

        public bool WarningsAsErrors
        {
            get { return warningsAsErrors; }
            set { warningsAsErrors = value; }
        }

        #endregion
        
        #region Public Methods
		public void Message()
        {
            Message(MessageImportance.Normal, String.Empty);
        }

        public void Message(string message, params object[] messageArgs)
        {
            Message(MessageImportance.Normal, message, messageArgs);
        }

        public void Message(MessageImportance importance, string message, params object[] messageArgs)
        {
            OutputMessageEventArgs e = new OutputMessageEventArgs(StringUtility.CultureFormat(message, messageArgs), importance);

            this.outputter.OutputMessageEvent(e);
        }

        public void Error(string message, params object[] messageArgs)
        {
            string code = ExtractMessageCode(message, out message);
            OutputErrorEventArgs e = new OutputErrorEventArgs(StringUtility.CultureFormat(message, messageArgs), code);

            this.hasOutputErrors = true;

            this.outputter.OutputErrorEvent(e);
        }

        public void Warning(string message, params object[] messageArgs)
        {
            if (warningsAsErrors)
            {
                Error(message, messageArgs);
                return;
            }

            string code = ExtractMessageCode(message, out message);
            OutputWarningEventArgs e = new OutputWarningEventArgs(StringUtility.CultureFormat(message, messageArgs), code);

            this.outputter.OutputWarningEvent(e);
        }

        public bool HasOutputErrors { get { return this.hasOutputErrors; } }

	    #endregion    
	}
}
