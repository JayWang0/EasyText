using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using EasyText.Windows;
using Microsoft.Win32;

//1using System.Runtime.Remoting.Messaging;

namespace EasyText.Helpers
{
	public static class CommonHelper
	{
		public static MessageBoxResult ShowConfirmMessage(string message)
		{
			return MessageBox.Show(message, "Confirm", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static MessageBoxResult ShowErrorMessage(string message)
		{
			return MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void InvokeDispatcherAction(this Action action)
		{
			if (Application.Current.CheckAccess())
			{
				action();
			}
			else
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public static DataTable ToDataTable(this List<ExpandoObject> items)
		{
			var data = items.ToArray();
			if (!data.Any())
			{
				return new DataTable();
			}
			var dt = new DataTable();
			var maxKeyItem = items.OrderByDescending(item => ((IDictionary<string, object>) item).Keys.Count)
				.First();

			foreach (var key in ((IDictionary<string, object>) maxKeyItem).Keys)
			{
				dt.Columns.Add(key);
			}
			foreach (var d in data)
			{
				dt.Rows.Add(((IDictionary<string, object>) d).Values.ToArray());
			}
			return dt;
		}

		public static void ReplaceByDataTable(this DataTable oldData, DataTable newData)
		{
			oldData.Clear();
			oldData.Columns.Clear();
			oldData.Rows.Clear();
			oldData.AcceptChanges();

			foreach (DataColumn column in newData.Columns)
			{
				oldData.Columns.Add(column.ColumnName);
			}
			oldData.AcceptChanges();

			foreach (DataRow row in newData.Rows)
			{
				var newRow = oldData.NewRow();

				foreach (DataColumn column in newData.Columns)
				{
					var str = row[column.ColumnName].SafeString();
					newRow[column.ColumnName] = str;
				}

				oldData.Rows.Add(newRow);
			}
			oldData.AcceptChanges();
		}

		// var view = contentGrid.ItemsSource as DataView;
		// var pivotTable = Pivot(view.Table);
		// contentGrid.ItemsSource = pivotTable.DefaultView;
		public static DataTable Pivot(this DataTable tbl)
		{
			var tblPivot = new DataTable();

			tblPivot.Columns.Add(tbl.Columns[0].ColumnName);
			for (var i = 0; i < tbl.Rows.Count; i++)
			{
				tblPivot.Columns.Add("$" + Convert.ToString(i));
			}

			for (var col = 0; col < tbl.Columns.Count; col++)
			{
				var r = tblPivot.NewRow();
				r[0] = tbl.Columns[col].ToString();
				for (var j = 0; j < tbl.Rows.Count; j++)
				{
					r[j] = tbl.Rows[j][col];
				}

				tblPivot.Rows.Add(r);
			}
			return tblPivot;
		}

		public static List<string> GetExpandoObjectOrderProperties(this ExpandoObject expando)
		{
			var dict = expando as IDictionary<string, object>;
			var result = new List<string>();

			for (var i = 0; i < 1000000; i++)
			{
				var key = "$" + i;
				if (dict.ContainsKey(key))
				{
					result.Add(dict[key].SafeString());
				}
				else
				{
					break;
				}
			}


			return result;
		}

		public static string SafeString(this object obj)
		{
			if (obj == null)
			{
				return "";
			}

			return obj.ToString();
		}


		public static string MakeJsonString(string s)
		{
			return string.Format(@"""{0}""", s.Replace(@"""", @"\"""));
		}


		public static string ProcessQuoteAndWindowsNewLine(string s)
		{
			if (s == null)
				return null;

			return s.Replace(@"'", @"\'").
				Replace(@"""", @"\""").
				Replace("\r", @"\r").
				Replace("\n", @"\n");
		}

		public static OpenFileDialog GetOpenFileDialog(string folderName)
		{
			var dlg = new OpenFileDialog();

			var initialDirectory = Environment.CurrentDirectory + "\\" + folderName;
			if (!Directory.Exists(initialDirectory))
			{
				Directory.CreateDirectory(initialDirectory);
			}

			dlg.InitialDirectory = initialDirectory;
			dlg.CheckFileExists = true;

			return dlg;
		}

		public static SaveFileDialog GetSaveFileDialog(string folderName)
		{
			var dlg = new SaveFileDialog();
			var initialDirectory = Environment.CurrentDirectory + "\\" + folderName;

			if (!Directory.Exists(initialDirectory))
			{
				Directory.CreateDirectory(initialDirectory);
			}

			dlg.InitialDirectory = initialDirectory;
			dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

			return dlg;
		}

		public static string GetPromptInput(string title, string info = "", int width = 400, int height = 250)
		{
			var prompt = new Prompt
			{
				Width = width,
				Height = height,
				Title = title,
				Owner = Application.Current.MainWindow
			};

			prompt.Title = info;

			prompt.ShowDialog();

			return prompt.DialogResult == true ? prompt.Message : null;
		}


		public static List<string> GetLines(string input)
		{
			var lines = new List<string>();

			using (var reader = new StringReader(input))
			{
				//string line = "";
				//do
				//{
				//	line = reader.ReadLine();
				//	lines.Add(line);
				//} while (line != null);

				var line = "";
				while (line != null)
				{
					line = reader.ReadLine();
					if (line != null)
					{
						lines.Add(line);
					}
				}
			}
			return lines;
		}
	}
}