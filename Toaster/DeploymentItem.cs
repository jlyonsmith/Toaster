using System;
using System.Collections.Generic;
using System.Text;
using ToolBelt;

namespace Toaster
{
    public class DeploymentItem
    {
        public ParsedPath Path { get; internal set; }
        public ParsedPath OutputDirectory { get; internal set; }
        public object Tag { get; set; }
        public bool IsExecutable 
        { 
            get 
            {
                if (Path == null)
                    return false;
                else
                    return Path.Extension == ".dll" || Path.Extension == ".exe"; 
            } 
        }

        public DeploymentItem(string path)
        {
            this.Path = new ParsedPath(path, PathType.File);
            this.OutputDirectory = ParsedPath.Empty;
        }

        public DeploymentItem(ParsedPath path, ParsedPath outputDirectory)
        {
            this.Path = path;
            this.OutputDirectory = outputDirectory;
        }

        public override string ToString()
        {
            return Path.ToString();
        }
    }
}
