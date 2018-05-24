using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace EasyText.Codes
{
	public abstract class BaseCodeParser : ICodeParser
	{
		public abstract string Left { get; }

		public abstract string Right { get; }

		public virtual string SplitString
		{
			get
			{
				{
					return string.Format("({0}[\\s\\S]*?{1})", Left, Right);
				}
			}
		}

		public virtual bool CanExecute(string code)
		{
			return code.StartsWith(Left) && code.EndsWith(Right);
		}

		public virtual string GetCodeText(string code)
		{
			Debug.Assert(Left != "");
			Debug.Assert(Right != "");

			var regexText = string.Format(@"{0}([\s\S]*?){1}", Left, Right);
			var regex = new Regex(regexText, RegexOptions.Multiline | RegexOptions.IgnoreCase);

			var match = regex.Match(code);
			var jsCode = match.Groups[1].Value;

			return jsCode;
		}

		public abstract string ExecuteCode(List<ExpandoObject> rows, int lineIndex, string code);
	}
}