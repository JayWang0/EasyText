// EasyLogViewer - an easy log viewer tool.
// Copyright (c) 2013-2015 QiWang(343875957@qq.com, wangqi1131@163.com, wangqi1131@gmail.com).


using System.Windows;
using System.Windows.Navigation;
using EasyText.Helpers;

namespace EasyText.Windows
{
	/// <summary>
	///     Interaction logic for About.xaml
	/// </summary>
	public partial class LicenseReminder
	{
		public LicenseReminder()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			e.Handled = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			LicenseHelper.Instance.OpenPurchaseUrl();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void BtnFreeTrial_Click(object sender, RoutedEventArgs e)
		{
			LicenseHelper.Instance.Validation(msg => { CommonHelper.ShowConfirmMessage(msg); });
			Close();
		}
	}
}