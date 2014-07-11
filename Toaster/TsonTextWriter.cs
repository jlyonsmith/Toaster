using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Toaster
{
    public class TsonTextWriter
    {
        IndentedTextWriter wr;
        bool[] needComma;
        bool haveWritten;

        public TsonTextWriter(TextWriter baseWriter)
        {
            this.wr = new IndentedTextWriter(baseWriter, "  ");
            this.wr.Indent = 0;
            this.needComma = new bool[16];
            this.haveWritten = false;
        }

        string TsonSafe(string s)
        {
            if (s.IndexOfAny(new char[] { '{', '}', '[', ']', ':', ',', '#', '"', '\n' }) != -1)
                return "\"" + s.Replace("\"", "\\\"") + "\"";
            else
                return s;
        }

        public void WriteProperty(string name, string value)
        {
            if (needComma[wr.Indent])
                wr.WriteLine(",");
            else if (haveWritten)
                WriteLine();

            wr.Write(name);
            wr.Write(": ");
            wr.Write(TsonSafe(value));

            needComma[wr.Indent] = true;
        }

        public void WriteObjectPropertyStart(string name)
        {
            if (needComma[wr.Indent])
                wr.WriteLine(",");
            else if (haveWritten)
                WriteLine();

            wr.Write(name);
            wr.WriteLine(":");
            wr.Write("{");

            needComma[wr.Indent] = true;
            wr.Indent++;
        }

        public void WriteObjectPropertyEnd()
        {
            wr.WriteLine();
            needComma[wr.Indent] = false;
            wr.Indent--;
            wr.Write("}");
        }

        public void WriteArrayPropertyStart(string name)
        {
            if (needComma[wr.Indent])
                wr.WriteLine(",");
            else if (haveWritten)
                WriteLine();

            wr.Write(name);
            wr.WriteLine(":");
            wr.Write("[");

            needComma[wr.Indent] = true;
            wr.Indent++;
        }

        public void WriteArrayPropertyEnd()
        {
            needComma[wr.Indent] = false;
            wr.WriteLine();
            wr.Indent--;
            wr.Write("]");
        }

        public void WriteArrayObjectStart()
        {
            if (needComma[wr.Indent])
                wr.WriteLine(",");
            else
                WriteLine();

            needComma[wr.Indent] = true;

            wr.Write("{");

            wr.Indent++;
            needComma[wr.Indent] = false;
        }

        public void WriteArrayObjectEnd()
        {
            needComma[wr.Indent] = false;
            wr.WriteLine();

            wr.Indent--;
            needComma[wr.Indent] = true;

            wr.Write("}");
        }

        public void WriteLine()
        {
            wr.WriteLine();
            haveWritten = true;
        }
    }
}

