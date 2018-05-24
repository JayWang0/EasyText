using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EasyText.Codes;
using EasyText.Helpers;

namespace EasyText
{
	public class CodeRunner
	{
		private readonly List<ICodeParser> _codeParsers;
		private Dictionary<string, string> _errorCodes = new Dictionary<string, string>();

		public CodeRunner()
		{
			_codeParsers = new List<ICodeParser> { new TextCodeParser() };
		}

		public Regex GetSplitRegex()
		{
			var regexText = string.Join("|",
				_codeParsers.Where(cp => !string.IsNullOrWhiteSpace(cp.SplitString)).Select(cp => cp.SplitString));

			if (string.IsNullOrEmpty(regexText))
			{
				return null;
			}
			else
			{
				return new Regex(regexText, RegexOptions.Multiline);
			}
		}

		private CodeRunnerResult ApplyTemplateCore(List<ExpandoObject> items, string where, string template,
			bool appendNewLine)
		{
			_errorCodes = new Dictionary<string, string>();

			var builder = new StringBuilder();

			for (var index = 0; index < items.Count; index++)
			{
				if (CanExecuteCode(items, index, where))
				{
					var result = new StringBuilder();
					var regex = GetSplitRegex();

					var codes = regex == null ? new[] { template } : regex.Split(template);

					foreach (var code in codes)
					{
						result.Append(ExecuteCode(items, index, code));
					}

					if (appendNewLine)
					{
						result.AppendLine();
					}

					builder.Append(result);
				}
			}

			var errorContent = string.Join(Environment.NewLine,
				_errorCodes.Select(errorCode => $"{errorCode.Key} , {errorCode.Value}").ToList());

			return new CodeRunnerResult { Message = builder.ToString(), Error = errorContent };
		}

		public CodeRunnerResult ApplyTemplate(List<ExpandoObject> items, string where, string template, bool appendNewLine)
		{
			var splitLines =
				CommonHelper.GetLines(template)
					.Where(line => line.Contains("**********") && line.Replace("*", "").Length == 0).ToList();
			splitLines.Add("#NEW#");

			var splitTemplates = template.Split(splitLines.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

			var result = new CodeRunnerResult();

			foreach (var splitTemplate in splitTemplates)
			{
				var splitTemplateResult = ApplyTemplateCore(items, where, splitTemplate, appendNewLine);
				result.Message += splitTemplateResult.Message;
				result.Error += splitTemplateResult.Error;
			}

			return result;
		}

		public bool CanExecuteCode(List<ExpandoObject> rows, int lineIndex, string where)
		{
			return true;
		}

		public string ExecuteCode(List<ExpandoObject> rows, int lineIndex, string code)
		{
			foreach (var codeParser in _codeParsers)
			{
				if (codeParser.CanExecute(code))
				{
					try
					{
						if (_errorCodes.ContainsKey(code))
						{
							return code;
						}
						return codeParser.ExecuteCode(rows, lineIndex, code);
					}
					catch (Exception ex)
					{
						_errorCodes.Add(code, ex.Message);
						return code;
					}
				}
			}

			return code;
		}
	}

	public class CodeRunnerResult
	{
		public string Message { get; set; }
		public string Error { get; set; }
	}
}