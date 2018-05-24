using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using EasyText.Helpers;
using EasyText.Windows;

namespace EasyText
{
	/// <summary>
	///     MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			InputTextEditor.ContextChanged += () => { ParseTemplate(); };

			TextBoxWhere.TextChanged += (o, e) => { ParseTemplate(); };
			TextBoxWhere.LostFocus +=
				(o, e) =>
				{
					LicenseHelper.Instance.DisplayReminderWindow();
				};

			TemplateTextEditor.textEditor.TextChanged += (o, e) => { ParseTemplate(188); };

			ResultTextEditor.CheckBoxAppendNewLine.Click += (o, e) => { ParseTemplate(); };
			ResultTextEditor.btnCalculate.Click += (o, e) => { ParseTemplate(100, true); };
		}

		private void ParseTemplate(int delayMillSeconds = 100, bool forceExecute = false)
		{
			TaskHelper.Delay("MainWindow+ParseTemplate", () =>
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					LicenseHelper.Instance.Validation();

					if (ResultTextEditor.AutoCalculate || forceExecute)
					{
						var whereText = TextBoxWhere.Text;
						var template = TemplateTextEditor.textEditor.Text;

						var appendNewLine = ResultTextEditor.AppendNewLine;
						var expandos = InputTextEditor.GetExpandoObjects(InputTextEditor.EditorMode);

						ResultTextEditor.AddMessage("Info", "Processing...");

						// run a task to execute code.
						Task.Factory.StartNew(() =>
						{
							var codeRunner = new CodeRunner();
							return codeRunner.ApplyTemplate(expandos, whereText, template, appendNewLine);
						}).ContinueWith(tt =>
						{
							var result = tt.Result;

							Dispatcher.BeginInvoke(new Action(() =>
							{
								ResultTextEditor.textEditor.Text = result.Message;
								ResultTextEditor.AddMessage("Error", result.Error);
							}));
						});
					}
				}));
			}, delayMillSeconds);
		}

		private void Exit_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Application.Current.MainWindow.Close();
		}

		private void OnlineHelp_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/EasyHelper/EasyText/wiki");
		}

		private void ViewHelpDocument_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
		}

		private void About_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			new About { Owner = this }.ShowDialog();
		}

		private void Register_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			LicenseHelper.Instance.OpenPurchaseUrl();
		}

		private void Purchase_MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			LicenseHelper.Instance.OpenPurchaseUrl();
		}
	}
}