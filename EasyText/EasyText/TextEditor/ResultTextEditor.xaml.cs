using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EasyText.Common;
using EasyText.Helpers;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Indentation.CSharp;

namespace EasyText.TextEditor
{
	/// <summary>
	///     ResultTextEditor.xaml 的交互逻辑
	/// </summary>
	public partial class ResultTextEditor : UserControl
	{
		public static readonly DependencyProperty EditorTitleProperty = DependencyProperty.Register(
			"EditorTitle", typeof (string), typeof (ResultTextEditor), new PropertyMetadata(default(string)));

		private CompletionWindow completionWindow;

		private string currentFileName;

		public ResultTextEditor()
		{
			InitializeComponent();

			textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
			textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

			var foldingUpdateTimer = new DispatcherTimer();
			foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
			foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
			foldingUpdateTimer.Start();


			//DataTable dataTable = new DataTable();

			//dataTable.Columns.Add("$1");
			//dataTable.Columns.Add("$2");
			//dataTable.Columns.Add("$3");
			//dataTable.Columns.Add("$4");

			//dataTable.Rows.Add(new object[] { 1, 2, 3, 4 });
			//dataTable.Rows.Add(new object[] { 1, 3, 3, 4 });
			//dataTable.Rows.Add(new object[] { 1, 4, 3, 4 });


			//dataGrid.ItemsSource = dataTable.DefaultView;
		}

		public string EditorTitle
		{
			get { return (string) GetValue(EditorTitleProperty); }
			set { SetValue(EditorTitleProperty, value); }
		}

		public bool AppendNewLine
		{
			get { return CheckBoxAppendNewLine.IsChecked == true; }
		}

		public bool AutoCalculate
		{
			get { return CheckBoxAutoCalculate.IsChecked == true; }
		}

		public void AddMessage(string type, string message)
		{
			infoPanelName.Content = type;
			infoPanelValue.Text = message;

			infoPanel.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;

			if (type == "Error")
			{
				infoPanelName.Foreground = Brushes.Red;
				infoPanelValue.Foreground = Brushes.Red;
			}
			else if (type == "Info")
			{
				infoPanelName.Foreground = Brushes.Blue;
				infoPanelValue.Foreground = Brushes.Blue;
			}
			else
			{
				infoPanelName.Foreground = Brushes.Black;
				infoPanelValue.Foreground = Brushes.Black;
			}
		}

		private void openFileClick(object sender, RoutedEventArgs e)
		{
			var dlg = CommonHelper.GetOpenFileDialog("Results");
			if (dlg.ShowDialog() ?? false)
			{
				currentFileName = dlg.FileName;
				textEditor.Load(currentFileName);
				textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(
					Path.GetExtension(currentFileName));
			}
		}

		private void saveFileClick(object sender, EventArgs e)
		{
			if (currentFileName == null)
			{
				var dlg = CommonHelper.GetSaveFileDialog("Results");
				if (dlg.ShowDialog() ?? false)
				{
					currentFileName = dlg.FileName;
				}
				else
				{
					return;
				}
			}
			textEditor.Save(currentFileName);
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

		private void alignClick(object sender, RoutedEventArgs e)
		{
		}

		#region Folding

		private FoldingManager foldingManager;
		private object foldingStrategy;

		private void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (textEditor.SyntaxHighlighting == null)
			{
				foldingStrategy = null;
			}
			else
			{
				switch (textEditor.SyntaxHighlighting.Name)
				{
					case "XML":
						foldingStrategy = new XmlFoldingStrategy();
						textEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
						break;
					case "C#":
					case "C++":
					case "PHP":
					case "Java":
						textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(textEditor.Options);
						foldingStrategy = new BraceFoldingStrategy();
						break;
					default:
						textEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
						foldingStrategy = null;
						break;
				}
			}
			if (foldingStrategy != null)
			{
				if (foldingManager == null)
					foldingManager = FoldingManager.Install(textEditor.TextArea);
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
				((BraceFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
			}
			if (foldingStrategy is XmlFoldingStrategy)
			{
				((XmlFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
			}
		}

		#endregion
	}
}