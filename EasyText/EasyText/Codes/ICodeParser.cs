using System.Collections.Generic;
using System.Dynamic;

namespace EasyText.Codes
{
	internal interface ICodeParser
	{
		string Left { get; }
		string Right { get; }
		string SplitString { get; }

		bool CanExecute(string code);

		string GetCodeText(string code);
		string ExecuteCode(List<ExpandoObject> rows, int lineIndex, string code);
	}
}