// EasyLogViewer - an easy log viewer tool.
// Copyright (c) 2013-2015 QiWang(343875957@qq.com, wangqi1131@163.com, wangqi1131@gmail.com).


using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Navigation;

namespace EasyText.Windows
{
	/// <summary>
	///     Interaction logic for About.xaml
	/// </summary>
	public partial class About
	{
		public About()
		{
			InitializeComponent();

			var assembly = Assembly.GetExecutingAssembly();

			var copyright =
				(AssemblyCopyrightAttribute)
					Attribute.GetCustomAttribute(assembly, typeof (AssemblyCopyrightAttribute));

			var fileVersion =
				(AssemblyFileVersionAttribute)
					Attribute.GetCustomAttribute(assembly, typeof (AssemblyFileVersionAttribute));

			buildVersion.Text = fileVersion.Version;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}