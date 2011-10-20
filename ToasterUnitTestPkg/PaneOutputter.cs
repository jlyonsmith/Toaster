using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using VSLangProj;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Collections;
using Toaster;
using System.Windows.Forms;
using System.Collections.Generic;
using ToolBelt;

namespace Toaster
{
    public sealed class PaneOutputter : IOutputter
    {
        #region Private Fields
        private OutputWindowPane outputWindowPane;
        private DTE2 dte2;

        #endregion

        #region Construction
        public PaneOutputter(DTE2 dte)
        {
            this.dte2 = dte;
        }

        #endregion

        #region IOutputter Implementation

        void IOutputter.OutputCustomEvent(OutputCustomEventArgs e)
        {
        }

        void IOutputter.OutputErrorEvent(OutputErrorEventArgs e)
        {
            Write("error: {0}", e.Code);
            WriteLine(e.Message);
        }

        void IOutputter.OutputWarningEvent(OutputWarningEventArgs e)
        {
            Write("warning: {0}", e.Code);
            WriteLine(e.Message);
        }

        void IOutputter.OutputMessageEvent(OutputMessageEventArgs e)
        {
            WriteLine(e.Message);
        }

        #endregion

        #region Public Methods
        public void ClearOutput()
        {
            OutputPane.Clear();
            OutputPane.Activate();
        }

        #endregion

        #region Private Methods
        private EnvDTE.OutputWindowPane OutputPane
        {
            get
            {
                if (outputWindowPane == null)
                {
                    string name = "Toaster Unit Tests";

                    // Retrieve handle to the VS.NET output window collection
                    OutputWindow outputWindow = (OutputWindow)dte2.Windows.Item(Constants.vsWindowKindOutput).Object;

                    foreach (OutputWindowPane pane in outputWindow.OutputWindowPanes)
                    {
                        if (pane.Name == name)
                        {
                            outputWindowPane = pane;
                            break;
                        }
                    }

                    if (outputWindowPane == null)
                        outputWindowPane = outputWindow.OutputWindowPanes.Add(name);
                }

                return outputWindowPane;
            }
        }

        private void Write(string format, params object[] objs)
        {
            OutputPane.OutputString(String.Format(format, objs));
        }

        private void WriteLine(string format, params object[] objs)
        {
            OutputPane.OutputString(String.Format(format + Environment.NewLine, objs));
        }

        #endregion
    }
}
