using System.Windows;

namespace EasyText.Windows
{
	/// <summary>
	///     Prompt.xaml 的交互逻辑
	/// </summary>
	public partial class Prompt
	{
		public Prompt()
		{
			InitializeComponent();
		}

		public string Message
		{
			get { return textEditor.Text; }
			set { textEditor.Text = value; }
		}

		public string Title
		{
			get { return txtTitle.Text; }
			set
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					txtTitle.Text = value;
					txtTitle.Visibility = Visibility.Visible;
				}
				else
				{
					txtTitle.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}