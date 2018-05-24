using System.Collections.Generic;
using System.Dynamic;
using EasyText.Helpers;

namespace EasyText.Codes
{
	public class TextCodeParser : BaseCodeParser
	{
		public override string Left
		{
			get { return ""; }
		}

		public override string Right
		{
			get { return ""; }
		}

		public override string SplitString
		{
			get { return ""; }
		}

		public override bool CanExecute(string code)
		{
			return true;
		}

		public override string ExecuteCode(List<ExpandoObject> rows, int lineIndex, string code)
		{
			var row = rows[lineIndex];
			var columns = row.GetExpandoObjectOrderProperties();

			// this is normal replace code.
			for (var i = 0; i < columns.Count; i++)
			{
				code = code.Replace("$" + i, columns[i].Trim());
			}

			return code;
		}
	}
}