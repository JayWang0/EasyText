using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasyText.Helpers
{
	public static class TaskHelper
	{
		private static readonly ConcurrentDictionary<string, CancellationTokenSource> taskDictionary =
			new ConcurrentDictionary<string, CancellationTokenSource>();

		public static void Delay(string id, Action action, int delayMilliseconds)
		{
			try
			{
				if (taskDictionary.ContainsKey(id))
				{
					CancellationTokenSource source = null;
					taskDictionary.TryGetValue(id, out source);
					if (source != null)
					{
						source.Cancel();
					}
					taskDictionary.TryRemove(id, out source);
				}

				var cancelSource = new CancellationTokenSource();

				TaskEx.Delay(delayMilliseconds, cancelSource.Token).ContinueWith(_ => action(), cancelSource.Token).ContinueWith(
					_ =>
					{
						CancellationTokenSource c2 = null;
						taskDictionary.TryRemove(id, out c2);
					}, cancelSource.Token);

				taskDictionary.TryAdd(id, cancelSource);
			}
			catch (Exception ex)
			{
				// eat it.
				Debug.Fail(ex.ToString());
			}
		}
	}

	public static class TaskEx
	{
		private static readonly Task _sPreCompletedTask = GetCompletedTask();
		private static readonly Task _sPreCanceledTask = GetPreCanceledTask();

		public static Task Delay(int dueTimeMs, CancellationToken cancellationToken)
		{
			if (dueTimeMs < -1)
				throw new ArgumentOutOfRangeException("dueTimeMs", "Invalid due time");
			if (cancellationToken.IsCancellationRequested)
				return _sPreCanceledTask;
			if (dueTimeMs == 0)
				return _sPreCompletedTask;

			var tcs = new TaskCompletionSource<object>();
			var ctr = new CancellationTokenRegistration();
			var timer = new Timer(delegate(object self)
			{
				ctr.Dispose();
				((Timer) self).Dispose();
				tcs.TrySetResult(null);
			});
			if (cancellationToken.CanBeCanceled)
				ctr = cancellationToken.Register(delegate
				{
					timer.Dispose();
					tcs.TrySetCanceled();
				});

			timer.Change(dueTimeMs, -1);
			return tcs.Task;
		}

		private static Task GetPreCanceledTask()
		{
			var source = new TaskCompletionSource<object>();
			source.TrySetCanceled();
			return source.Task;
		}

		private static Task GetCompletedTask()
		{
			var source = new TaskCompletionSource<object>();
			source.TrySetResult(null);
			return source.Task;
		}
	}
}