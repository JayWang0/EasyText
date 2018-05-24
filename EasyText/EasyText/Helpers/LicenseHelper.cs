using System;
using System.Diagnostics;
using System.Windows;
using EasyText.Windows;

namespace EasyText.Helpers
{
	public class LicenseHelper
	{
		private static LicenseHelper _instance;

		private static LicenseReminder reminderWindow;

		private readonly string computerKey = "";


		private bool isValidLicense = false;

		/// <summary>
		///     ctor
		/// </summary>
		public LicenseHelper()
		{
			ReNewReminderWindow();
		}

		public static LicenseHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new LicenseHelper();
				}

				return _instance;
			}
		}

		private void ReNewReminderWindow()
		{
			new Action(
				() => { reminderWindow = new LicenseReminder {Owner = Application.Current.MainWindow, ShowActivated = true}; })
				.InvokeDispatcherAction();
		}

		public bool Validation(Action<string> successFunc = null)
		{
			return true;
		}

		public void DisplayReminderWindow()
		{
			new Action(() =>
			{
				ReNewReminderWindow();
				reminderWindow.ShowDialog();
			}).InvokeDispatcherAction();
		}

		public void OpenPurchaseUrl()
		{
			Process.Start(new ProcessStartInfo("https://github.com/EasyHelper/EasyText#donation"));
		}

		public string GetComputerKey()
		{
			return computerKey;
		}
	}
}