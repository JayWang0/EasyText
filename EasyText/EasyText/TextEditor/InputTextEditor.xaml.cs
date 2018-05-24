using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EasyText.Common;
using EasyText.Helpers;
using Excel;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using Newtonsoft.Json;

namespace EasyText.TextEditor
{
	/// <summary>
	///     InputTextEditor.xaml 的交互逻辑
	/// </summary>
	public partial class InputTextEditor : UserControl
	{
		public static readonly DependencyProperty EditorTitleProperty = DependencyProperty.Register(
			"EditorTitle", typeof(string), typeof(InputTextEditor), new PropertyMetadata(default(string)));

		private readonly string textTableName = "TextTable";


		private EditorMode _editorMode = EditorMode.TextMode;
		private List<ExpandoObject> cachedExpandoObject = new List<ExpandoObject>();

		private CompletionWindow completionWindow;

		public Action ContextChanged = () => { };
		private DataSet currentDataSet = new DataSet();

		private string currentFileName;

		public InputTextEditor()
		{
			InitializeComponent();

			txtEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
			txtEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

			var foldingUpdateTimer = new DispatcherTimer();
			foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
			foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
			foldingUpdateTimer.Start();

			cmbEditorModeSwitch.ItemsSource = new List<EditorMode>
			{
				EditorMode.TextMode,
				EditorMode.DataTableMode
			};

			// trigger editor mode.
			cmbEditorModeSwitch.SelectedIndex = 0;

			dataGridTable.CellEditEnding += (o, e) => { TriggerContextChanged(); };
			dataGridTable.RowEditEnding += (o, e) => { TriggerContextChanged(); };
			dataGridTable.Sorting += (o, e) => { TriggerContextChanged(); };
			dataGridTable.ColumnReordered += (o, e) => { TriggerContextChanged(); };

			txtEditor.TextChanged += (o, e) => { TriggerContextChanged(); };
			txtColumnDelimiter.TextChanged += (o, e) => { TriggerContextChanged(); };
		}

		public EditorMode EditorMode
		{
			get { return _editorMode; }
			set
			{
				_editorMode = value;

				// do switch
				if (_editorMode == EditorMode.DataTableMode)
				{
					var tableNames = currentDataSet.Tables.Cast<DataTable>().Select(dt => dt.TableName).ToList();

					dataGridTableNames.ItemsSource = tableNames;
					dataGridTableNames.SelectedIndex = 0;

					dataGrid.Visibility = Visibility.Visible;
					txtEditor.Visibility = Visibility.Collapsed;
				}
				else
				{
					txtEditor.Visibility = Visibility.Visible;
					dataGrid.Visibility = Visibility.Collapsed;
				}
			}
		}

		public string EditorTitle
		{
			get { return (string)GetValue(EditorTitleProperty); }
			set { SetValue(EditorTitleProperty, value); }
		}

		private void TriggerContextChanged()
		{
			ContextChanged();
			cachedExpandoObject.Clear();
		}

		public List<ExpandoObject> GetExpandoObjects(EditorMode editorMode)
		{
			Debug.Assert(cachedExpandoObject != null);

			if (cachedExpandoObject.Count == 0)
			{
				if (editorMode == EditorMode.TextMode)
				{
					var input = txtEditor.Text;
					var columnDelimiter = txtColumnDelimiter.Text;

					var lines = CommonHelper.GetLines(input);

					if (lines.Count > 100)
					{
					}

					// remove empty.
					lines.RemoveAll(string.IsNullOrEmpty);

					var items = new List<ExpandoObject>(lines.Count);
					foreach (var line in lines)
					{
						var expandoObject = new ExpandoObject();
						var dict = expandoObject as IDictionary<string, object>;

						var columns = line.Split(new[] { columnDelimiter }, StringSplitOptions.RemoveEmptyEntries);

						for (var i = 0; i < columns.Length; i++)
						{
							dict["$" + i] = columns[i].Trim();
						}

						items.Add(expandoObject);
					}

					cachedExpandoObject = items;
				}

				if (editorMode == EditorMode.DataTableMode)
				{
					if (currentDataSet.HasChanges())
					{
						currentDataSet.AcceptChanges();
					}

					var dataView = dataGridTable.ItemsSource as DataView;
					if (dataView != null)
					{
						var items = new List<ExpandoObject>();

						foreach (DataRowView row in dataView)
						{
							var expandoObject = new ExpandoObject();
							var dict = expandoObject as IDictionary<string, object>;

							for (var i = 0; i < dataView.Table.Columns.Count; i++)
							{
								dict["$" + i] = row[i].SafeString().Trim();
							}

							items.Add(expandoObject);
						}

						cachedExpandoObject = items;
					}
				}
			}

			return cachedExpandoObject;
		}

		private void openFileClick(object sender, RoutedEventArgs e)
		{
			var dlg = CommonHelper.GetOpenFileDialog("Inputs");

			if (dlg.ShowDialog() ?? false)
			{
				currentFileName = dlg.FileName;

				if (currentFileName.EndsWith(".xls"))
				{
					using (var stream = File.Open(currentFileName, FileMode.Open, FileAccess.Read))
					{
						//1. Reading from a binary Excel file ('97-2003 format; *.xls)
						var excelReader = ExcelReaderFactory.CreateBinaryReader(stream);

						currentDataSet = excelReader.AsDataSet();
						EditorMode = EditorMode.DataTableMode;
					}
				}
				else if (currentFileName.EndsWith(".xlsx"))
				{
					using (var stream = File.Open(currentFileName, FileMode.Open, FileAccess.Read))
					{
						//2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
						var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

						currentDataSet = excelReader.AsDataSet();
						EditorMode = EditorMode.DataTableMode;
					}
				}
				else
				{
					currentDataSet.Clear();

					txtEditor.Load(currentFileName);
					txtEditor.SyntaxHighlighting =
						HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));

					// Can't use Property, because It will trigger changed event.
					EditorMode = EditorMode.TextMode;
				}
			}
		}

		private void saveFileClick(object sender, EventArgs e)
		{
			if (currentFileName == null)
			{
				var dlg = CommonHelper.GetSaveFileDialog("Inputs");

				if (dlg.ShowDialog() ?? false)
				{
					currentFileName = dlg.FileName;
				}
				else
				{
					return;
				}
			}
			txtEditor.Save(currentFileName);
		}

		private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{

		}

		private void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (e.Text.Length > 0 && completionWindow != null)
			{
				if (!char.IsLetterOrDigit(e.Text[0]))
				{
					// Whenever a non-letter is typed while the completion window is open,
					// insert the currently selected element.
					completionWindow.CompletionList.RequestInsertion(e);
				}
			}
			// do not set e.Handled=true - we still want to insert the character that was typed
		}

		private void align_Click(object sender, RoutedEventArgs e)
		{
			if (EditorMode == EditorMode.TextMode)
			{
				var expandoObjects = GetExpandoObjects(EditorMode.TextMode);

				txtEditor.Text = GetTextFromDataTable(expandoObjects.ToDataTable());
			}
			else
			{
				CommonHelper.ShowConfirmMessage("Only work for text mode");
			}
		}

		private string GetTextFromDataTable(DataTable dataTable)
		{
			// DataTable is easy to foreach.

			var maxLengthList = new List<int>();
			foreach (DataColumn column in dataTable.Columns)
			{
				var maxLength = 0;
				foreach (DataRow row in dataTable.Rows)
				{
					maxLength = Math.Max(maxLength, row[column].SafeString().Length);
				}

				// add extra 2 space.
				maxLength += 2;

				maxLengthList.Add(maxLength);
			}

			Debug.Assert(maxLengthList.Count == dataTable.Columns.Count);

			var lines = Enumerable.Range(0, dataTable.Rows.Count).Select(i => "").ToList();


			for (var i = 0; i < lines.Count; i++)
			{
				var row = dataTable.Rows[i];

				for (var j = 0; j < maxLengthList.Count; j++)
				{
					var column = dataTable.Columns[j];

					if (j == maxLengthList.Count - 1)
					{
						lines[i] += row[column].SafeString().PadRight(maxLengthList[j] - 2);
					}
					else
					{
						lines[i] += row[column].SafeString().PadRight(maxLengthList[j]) + txtColumnDelimiter.Text;
					}
				}
			}

			return string.Join(Environment.NewLine, lines);
		}

		private void addRow_Click(object sender, RoutedEventArgs e)
		{
			var promptMessage = CommonHelper.GetPromptInput("Please enter a number");
			if (promptMessage == null)
			{
				return;
			}

			var total = 0;
			if (int.TryParse(promptMessage, out total))
			{
				var input = txtEditor.Text;

				var lines = CommonHelper.GetLines(input);

				var builder = new StringBuilder();

				for (var i = 0; i < Math.Max(lines.Count, total); i++)
				{
					if (i < lines.Count)
					{
						builder.AppendLine(i + "  " + txtColumnDelimiter.Text + lines[i]);
					}
					else
					{
						if (i < total)
						{
							builder.AppendLine(i + "  " + txtColumnDelimiter.Text);
						}
					}
				}

				txtEditor.Text = builder.ToString();
			}
			else
			{
				CommonHelper.ShowErrorMessage("Please enter a number");
			}
		}

		private void dataGridTableNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (dataGridTableNames.SelectedItem != null
				&& currentDataSet.Tables[dataGridTableNames.SelectedItem.ToString()] != null)
			{
				dataGridTable.ItemsSource = currentDataSet.Tables[dataGridTableNames.SelectedItem.ToString()].DefaultView;
			}
			else
			{
				dataGridTable.ItemsSource = null;
			}
		}

		private void cmbEditorModeSwitch_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var editorMode = (EditorMode)cmbEditorModeSwitch.SelectedItem;

			if (editorMode == EditorMode.DataTableMode)
			{
				// add datatable to dataSet as table 0, and name it to TextTable.
				DataTable dtTextTable = null;

				var expandos = GetExpandoObjects(EditorMode.TextMode);
				if (expandos.Count > 0)
				{
					dtTextTable = expandos.ToDataTable();
					dtTextTable.TableName = textTableName;
				}

				// if TextTable 0 not exist, add it, otherwise change it, accept changes.
				if (dtTextTable != null)
				{
					if (currentDataSet.Tables.Count > 0)
					{
						if (currentDataSet.Tables[0].TableName == textTableName)
						{
							currentDataSet.Tables[0].ReplaceByDataTable(dtTextTable);
						}
						else
						{
							var tmpDataSet = new DataSet();
							tmpDataSet.Tables.Add(dtTextTable);
							tmpDataSet.Merge(currentDataSet);

							currentDataSet = tmpDataSet;
						}
					}
					else
					{
						currentDataSet.Tables.Add(dtTextTable);
					}
				}
			}
			else if (editorMode == EditorMode.TextMode)
			{
				// if TextTable 0 exist, Get Text from this table.
				var expandoObjects = GetExpandoObjects(EditorMode.DataTableMode);
				txtEditor.Text = GetTextFromDataTable(expandoObjects.ToDataTable());

				dataGridTableNames.SelectedIndex = -1;
			}
			else
			{
				Debug.Fail("error");
			}

			EditorMode = editorMode;

			// clear cache.
			cachedExpandoObject = new List<ExpandoObject>();
		}

		private void parseJson_Click(object sender, RoutedEventArgs e)
		{
			var jsonText = txtEditor.Text;

			if (jsonText.TrimStart().StartsWith("["))
			{
				// this is a list.
			}
			else if (jsonText.TrimStart().StartsWith("{"))
			{
				var expando = (ExpandoObject)JsonConvert.DeserializeObject(jsonText, typeof(ExpandoObject));
			}
			else
			{
				CommonHelper.ShowErrorMessage("Invalid Json");
			}
		}

		#region Folding

		private FoldingManager foldingManager;
		private object foldingStrategy;

		private void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (txtEditor.SyntaxHighlighting == null)
			{
				foldingStrategy = null;
			}
			else
			{
				switch (txtEditor.SyntaxHighlighting.Name)
				{
					case "XML":
						foldingStrategy = new XmlFoldingStrategy();
						txtEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
						break;
					case "C#":
					case "C++":
					case "PHP":
					case "Java":
						txtEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(txtEditor.Options);
						foldingStrategy = new BraceFoldingStrategy();
						break;
					default:
						txtEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
						foldingStrategy = null;
						break;
				}
			}
			if (foldingStrategy != null)
			{
				if (foldingManager == null)
					foldingManager = FoldingManager.Install(txtEditor.TextArea);
				UpdateFoldings();
			}
			else
			{
				if (foldingManager != null)
				{
					FoldingManager.Uninstall(foldingManager);
					foldingManager = null;
				}
			}
		}

		private void UpdateFoldings()
		{
			if (foldingStrategy is BraceFoldingStrategy)
			{
				((BraceFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, txtEditor.Document);
			}
			if (foldingStrategy is XmlFoldingStrategy)
			{
				((XmlFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, txtEditor.Document);
			}
		}

		#endregion
	}

	public enum EditorMode
	{
		TextMode,
		DataTableMode
	}
}