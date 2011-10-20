using System;
using System.Collections.Generic;
using System.Text;
using ToolBelt;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace Buckle
{
	public enum AccessModifier
	{
		Public, Internal
	}
	
	[CommandLineCopyright("Copyright (c) 2010, John Lyon-Smith")]
	[CommandLineDescription("Generates strongly typed wrappers for string .resx resources")]
	[CommandLineTitle("Buckle ResX to C# String Wrapper Class Generator")]
	public class BuckleTool : IProcessCommandLine
	{
		#region Classes
		private class ResourceItem
		{
			#region Fields
			private Type dataType;
			private string name;
			private string valueString;

			#endregion

			#region Constructors
			public ResourceItem(string name, string stringResourceValue)
				: this(name, typeof(string))
			{
				this.valueString = stringResourceValue;
			}

			public ResourceItem(string name, Type dataType)
			{
				this.name = name;
				this.dataType = dataType;
			}

			#endregion

			#region Properties
			public Type DataType
			{
				get
				{
					return this.dataType;
				}
			}

			public string Name
			{
				get
				{
					return this.name;
				}
			}

			public string ValueString
			{
				get
				{
					return this.valueString;
				}
			} 
			#endregion
		}

		#endregion

		#region Fields
		private List<ResourceItem> resources = new List<ResourceItem>();
		private StreamReader reader;
		private StreamWriter writer;
		private const string InvalidCharacters = ".$*{}|<>";
		private CommandLineParser parser;
		private bool runningFromCommandLine = false;
		
		#endregion

		#region Constructors
		public BuckleTool(IOutputter outputter)
		{
			this.Output = new OutputHelper(outputter);
		}

		#endregion		
	
		#region Properties
		public OutputHelper Output { get; private set; }

		[DefaultCommandLineArgument("default", Description = "Input resource file", ValueHint = "<resx-file>")]
		public ParsedPath ResXFileName { get; set; }
		
		[CommandLineArgument("output", ShortName = "o", Description = "Output file", ValueHint = "<output-cs>")]
		public ParsedPath OutputFileName { get; set; }

		[CommandLineArgument("namespace", ShortName = "n", Description = "Namespace to generate wrapper class in", ValueHint = "<name-space>")]
		public string Namespace { get; set; }
		
		[CommandLineArgument("wrapper", ShortName = "w", Description = "Fully qualified string wrapper class; must be either System.String or ToolBelt.Message (or equivalent) class", ValueHint = "<wrapper-class>")]
		public string WrapperClass { get; set; }
		
		[CommandLineArgument("access", ShortName = "a", Description = "Resource class access; public or internal", ValueHint = "<public-internal>")]
		public AccessModifier Modifier { get; set; }

		private CommandLineParser Parser
		{
			get
			{
				if (this.parser == null)
				{
					this.parser = new CommandLineParser(typeof(BuckleTool));
				}
				
				return parser;
			}
		}
		 
		#endregion	
		
		#region Methods
		public void Execute()
		{
			Output.Message(Parser.LogoBanner);
		
			if (!runningFromCommandLine)
			{
				Output.Message(Parser.ProcessName + Parser.Arguments);
			}
		
			if (ResXFileName == null)
			{
				Output.Message(Parser.Usage);
				return;
			}
			
			if (WrapperClass == null)
			{
				Output.Error("A string wrapper class must be specified");
				return;
			} 
			
			if (OutputFileName == null)
			{
				OutputFileName = ResXFileName.ChangeExtension(".cs");
			}
			
			using (this.reader = new StreamReader(ResXFileName))
			{
				using (this.writer = new StreamWriter(OutputFileName, false, Encoding.ASCII))
				{
					GenerateCode();
				}
			}
		}

		private void GenerateCode()
		{
			ReadResources();
			WriteNamespaceStart();
			WriteClassStart();

			int num = 0;

			foreach (ResourceItem item in resources)
			{
				if (item.DataType.IsPublic)
				{
					num++;
					this.WriteResourceMethod(item);
				}
				else
				{
					ConsoleUtility.WriteMessage(MessageType.Warning, "Resource skipped. Type {0} is not public.", item.DataType);
				}
			}
			Console.WriteLine("Generated strongly typed resource wrapper method(s) for {0} resource(s) in {1}", num, ResXFileName);
			WriteClassEnd();
			WriteNamespaceEnd();
		}

		private void WriteNamespaceStart()
		{
			DateTime now = DateTime.Now;
			writer.Write(String.Format(CultureInfo.InvariantCulture, 
@"//
// This file genenerated by the Buckle tool on {0} at {1}. 
//
// Contains strongly typed wrappers for resources in {2}
//

"
				, now.ToShortDateString(), now.ToShortTimeString(), ResXFileName.FileAndExtension));
			
			if ((Namespace != null) && (Namespace.Length != 0))
			{
				writer.Write(String.Format(CultureInfo.InvariantCulture, 
@"namespace {0} {{
"
					, Namespace));
			}
			writer.Write(
@"using System;
using System.Resources;
using System.Diagnostics;
using System.Globalization;
"
				);
		}

		private void WriteNamespaceEnd()
		{
			if ((Namespace != null) && (Namespace.Length != 0))
			{
				writer.Write(
@"}
"
					);
			}
		}

		private void WriteClassStart()
		{
			writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
/// <summary>
/// Strongly typed resource wrappers generated from {0}.
/// </summary>
{1} class {2}
{{
    internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof({2}));
"
				, ResXFileName.FileAndExtension, Modifier.ToString().ToLower(), OutputFileName.File ));
		}

		private void WriteClassEnd()
		{
			writer.Write(
@"}
");
		}

		private void WriteResourceMethod(ResourceItem item)
		{
			StringBuilder builder = new StringBuilder(item.Name);
			for (int i = 0; i < item.Name.Length; i++)
			{
				if (InvalidCharacters.IndexOf(builder[i]) != -1)
				{
					builder[i] = '_';
				}
			}
			if (item.DataType == typeof(string))
			{
				string parametersWithTypes = string.Empty;
				string parameters = string.Empty;
				int paramCount = 0;
				try
				{
					paramCount = this.GetNumberOfParametersForStringResource(item.ValueString);
				}
				catch (ApplicationException exception)
				{
					ConsoleUtility.WriteMessage(MessageType.Error, "Resource has been skipped: {0}", exception.Message);
				}
				for (int j = 0; j < paramCount; j++)
				{
					string str3 = string.Empty;
					if (j > 0)
					{
						str3 = ", ";
					}
					parametersWithTypes = parametersWithTypes + str3 + "object param" + j.ToString(CultureInfo.InvariantCulture);
					parameters = parameters + str3 + "param" + j.ToString(CultureInfo.InvariantCulture);
				}
				
				if ((item.ValueString != null) && (item.ValueString.Length != 0))
				{
					writer.Write(
@"
    /// <summary>"
						);
					foreach (string str4 in item.ValueString.Replace("\r", string.Empty).Split("\n".ToCharArray()))
					{
						writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    /// {0}"
							, str4));
					}
					writer.Write(
@"
    /// </summary>"
						);
				}
				if ((string.Equals(WrapperClass, "String", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(WrapperClass, "System.String", StringComparison.Ordinal)))
				{
					WriteStringMethodBody(item, OutputFileName.File, builder.ToString(), paramCount, parameters, parametersWithTypes);
				}
				else
				{
					WriteMessageMethodBody(item, OutputFileName.File, builder.ToString(), paramCount, parameters, parametersWithTypes);
				}
			}
			else
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    public static {0} {1}
    {{
        get
        {{
            return ({0})ResourceManager.GetObject(""{2}"");
        }}
    }}"
					, item.DataType, builder, item.Name ));
			}
		}

		private void WriteStringMethodBody(
			ResourceItem item, 
			string resourceClassName, 
			string methodName, 
			int paramCount, 
			string parameters, 
			string parametersWithTypes)
		{
			string str = "temp";
			if (parameters.Length > 0)
			{
				str = "string.Format(CultureInfo.CurrentCulture, temp, " + parameters + ")";
			}
			if (paramCount == 0)
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    public static {0} {1}
    {{
        get
        {{
            string temp = ResourceManager.GetString(""{2}"", CultureInfo.CurrentUICulture);
            return {3};
        }}
    }}
"
					, WrapperClass, methodName, item.Name, str ));
			}
			else
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    public static {0} {1}({2})
    {{
        string temp = ResourceManager.GetString(""{3}"", CultureInfo.CurrentUICulture);
        return {4};
    }}
"
					, WrapperClass, methodName, parametersWithTypes, item.Name, str ));
			}
		}

		private void WriteMessageMethodBody(
			ResourceItem item, 
			string resourceClassName, 
			string methodName, 
			int paramCount, 
			string parameters, 
			string parametersWithTypes)
		{
			if (paramCount == 0)
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    public static {0} {1}
    {{
        get
        {{
            return new {0}(""{2}"", typeof({3}), ResourceManager, null);
        }}
    }}
"
					, WrapperClass, methodName, item.Name, resourceClassName ));
			}
			else
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, 
@"
    public static {0} {1}({2})
    {{
        Object[] o = {{ {4} }};
        return new {0}(""{3}"", typeof({5}), ResourceManager, o);
    }}
"
					, WrapperClass, methodName, parametersWithTypes, item.Name, parameters, resourceClassName ));
			}
		}

		public int GetNumberOfParametersForStringResource(string resourceValue)
		{
			string input = resourceValue.Replace("{{", "").Replace("}}", "");
			string pattern = "{(?<value>[^{}]*)}";
			int num = -1;
			for (Match match = Regex.Match(input, pattern); match.Success; match = match.NextMatch())
			{
				string str3 = null;
				try
				{
					str3 = match.Groups["value"].Value;
				}
				catch
				{
				}
				if ((str3 == null) || (str3.Length == 0))
				{
					throw new ApplicationException(
						string.Format(CultureInfo.InvariantCulture, "\"{0}\": Empty format string at position {1}", new object[] { input, match.Index }));
				}
				string s = str3;
				int length = str3.IndexOfAny(",:".ToCharArray());
				if (length > 0)
				{
					s = str3.Substring(0, length);
				}
				int num3 = -1;
				try
				{
					num3 = (int)uint.Parse(s, CultureInfo.InvariantCulture);
				}
				catch (Exception exception)
				{
					throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "\"{0}\": {1}: {{{2}}}", new object[] { input, exception.Message, str3 }), exception);
				}
				if (num3 > num)
				{
					num = num3;
				}
			}
			return (num + 1);
		}

		private void ReadResources()
		{
			this.resources.Clear();
			XmlDocument document = new XmlDocument();
			document.Load(this.ResXFileName);
			foreach (XmlElement element in document.DocumentElement.SelectNodes("data"))
			{
				string attribute = element.GetAttribute("name");
				if ((attribute == null) || (attribute.Length == 0))
				{
					ConsoleUtility.WriteMessage(MessageType.Warning, "Resource skipped. Empty name attribute: {0}", element.OuterXml);
					continue;
				}
				Type dataType = null;
				string typeName = element.GetAttribute("type");
				if ((typeName != null) && (typeName.Length != 0))
				{
					try
					{
						dataType = Type.GetType(typeName, true);
					}
					catch (Exception exception)
					{
						ConsoleUtility.WriteMessage(MessageType.Warning, "Resource skipped. Could not load type {0}: {1}", typeName, exception.Message);
						continue;
					}
				}
				ResourceItem item = null;
				if ((dataType == null) || (dataType == typeof(string)))
				{
					string stringResourceValue = null;
					XmlNode node = element.SelectSingleNode("value");
					if (node != null)
					{
						stringResourceValue = node.InnerXml;
					}
					if (stringResourceValue == null)
					{
						ConsoleUtility.WriteMessage(MessageType.Warning, "Resource skipped.  Empty value attribute: {0}", element.OuterXml);
						continue;
					}
					item = new ResourceItem(attribute, stringResourceValue);
				}
				else
				{
					item = new ResourceItem(attribute, dataType);
				}
				this.resources.Add(item);
			}
		}

		#endregion

		#region IProcessCommandLine Members

		public bool ProcessCommandLine(string[] args)
		{
			this.runningFromCommandLine = true;

			try
			{
				Parser.ParseAndSetTarget(args, this);
			}
			catch (CommandLineArgumentException e)
			{
				Output.Error(e.Message);
				return false;
			}

			// All the semantic checking of arguments is done when the task is executed

			return true;
		}

		#endregion
	}
}
