using System;
using System.Diagnostics;

namespace EasyText.Helpers
{
	public class PerformanceHelper : IDisposable
	{
		private Stopwatch stopwatch;

		public PerformanceHelper()
		{
			stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			stopwatch.Stop();
			stopwatch = null;
		}

		public void WriteTimeMessage(string label, bool restart = false)
		{
			Console.WriteLine($@"{label} -->{stopwatch.ElapsedMilliseconds} ms");

			if (restart)
			{
				stopwatch.Restart();
			}
		}
	}
}