using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace ToolBelt
{
    public class MSBuildOutputter : IOutputter
    {
        #region Fields
        private IBuildEngine buildEngine;
        private string taskName;
        
        #endregion

        #region Construction
        public MSBuildOutputter(IBuildEngine buildEngine, string taskName)
        {
            this.buildEngine = buildEngine;
            this.taskName = taskName;
        }

        #endregion

        #region IOutputter Members

        public void OutputCustomEvent(OutputCustomEventArgs e)
        {
        }

        public void OutputErrorEvent(OutputErrorEventArgs e)
        {
            BuildErrorEventArgs args = new BuildErrorEventArgs(
                "", e.Code, "", 0, 0, 0, 0, e.Message, "", taskName, e.Timestamp, null);
            buildEngine.LogErrorEvent(args);
        }

        public void OutputWarningEvent(OutputWarningEventArgs e)
        {
            BuildWarningEventArgs args = new BuildWarningEventArgs(
                "", e.Code, "", 0, 0, 0, 0, e.Message, "", taskName, e.Timestamp, null); 
            buildEngine.LogWarningEvent(args);
        }

        public void OutputMessageEvent(OutputMessageEventArgs e)
        {
            BuildMessageEventArgs args = new BuildMessageEventArgs(
                e.Message, "", taskName, (Microsoft.Build.Framework.MessageImportance)(int)e.Importance);
            buildEngine.LogMessageEvent(args);
        }

        #endregion
    }
}
