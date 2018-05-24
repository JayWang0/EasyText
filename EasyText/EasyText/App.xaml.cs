using System;
using System.Windows;

namespace EasyText
{
	/// <summary>
	///     App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		[STAThread]
		public static void Main()
		{
			var application = new App();

			Initialize();

			application.InitializeComponent();
			application.Run();
		}

		private static void Initialize()
		{
		}
	}
}