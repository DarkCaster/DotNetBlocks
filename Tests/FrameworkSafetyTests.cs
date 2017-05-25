using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
	/// <summary>
	/// Some tests for internal framework classes to test behavior in some specific cases.
	/// </summary>
	[TestFixture]
	public class FrameworkSafetyTests
	{
		// https://gist.github.com/huysentruitw/f6f10cc1e9a10f2ef9bd5ab18f0b4f47
		private static class ConcurrentDictionaryTestCase
		{
			private static ConcurrentDictionary<string, string> dict = new ConcurrentDictionary<string, string>();
			private static int count = 0;
			public static void Execute()
			{
				var t1 = new Thread(() =>
				dict.GetOrAdd("key1", x => {
					Interlocked.Increment(ref count);
					Thread.Sleep(TimeSpan.FromSeconds(1));
					return x;
				}));
				var t2 = new Thread(() =>
				dict.GetOrAdd("key2", x => {
					Interlocked.Increment(ref count);
					return x;
				}));

				var t3 = new Thread(() =>
				dict.GetOrAdd("key1", x => {
					Interlocked.Increment(ref count);
					return x;
				}));

				t1.Start();
				Thread.Sleep(500);
				t2.Start();
				t3.Start();
				t1.Join();
				t2.Join();
				t3.Join();
				if (count > 2)
					Assert.Fail(count.ToString());
			}
		}
		
		private static ReaderWriterLockSlim failingLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		
		//should fail
		private async Task<int> RWLockTestAsync()
		{
			for (int i = 0; i < 10; ++i) {
				failingLock.EnterReadLock();
				await Task.Delay(100);
				failingLock.ExitReadLock();
			}
			return 0;
		}
		
		[Test()]
		public void RWLockTest()
		{
			try
			{
				var task = Task.Run(async () => await RWLockTestAsync());
				var i = task.Result;
			}
			catch (AggregateException ex)
			{
				Assert.IsInstanceOf(typeof(SynchronizationLockException), ex.InnerException);
				return;
			}
			catch (Exception ex) { throw ex; }
		}

		private class ThreadSpawner
		{
			private int counter;
			public ThreadSpawner()
			{
				Task.Run(()=>Worker());
				for (int i = 0; i < 1250000000; ++i)
					if (Interlocked.CompareExchange(ref counter, i + 1, i) != i)
						throw new Exception("Thread write value while constructor is running!");
			}
			public void Worker()
			{
					Interlocked.Exchange(ref counter,-1);
			}
		}

		[Test]
		public void ThreadFromConstructor()
		{
			Assert.Throws(typeof(Exception),() => new ThreadSpawner());
		}

		private volatile bool asyncThreadIdTest_Stop = false;
		private int asyncThreadIdTest_WorkersConut = 0;
		private async Task<bool> ThreadIDTestWorker()
		{
			Interlocked.Increment(ref asyncThreadIdTest_WorkersConut);
			var random = new Random();
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var result = false;
			do
			{
				await Task.Delay(random.Next(0, 50));
				var newThreadId = Thread.CurrentThread.ManagedThreadId;
				if (newThreadId != threadId)
				{
					result = true;
					threadId = newThreadId;
				}
			} while (!asyncThreadIdTest_Stop);
			return result;
		}

		[Test]
		public void AsyncThreadIdTest()
		{
			asyncThreadIdTest_Stop = false;
			asyncThreadIdTest_WorkersConut = 0;
			const int workers = 500;
			Task<bool>[] tasks = new Task<bool>[workers];
			for (int i = 0; i < workers; ++i)
				tasks[i] = Task.Run(async () => await ThreadIDTestWorker());
			while(Interlocked.CompareExchange(ref asyncThreadIdTest_WorkersConut,0,0)<workers) {}
			Thread.Sleep(500);
			asyncThreadIdTest_Stop = true;
			var result = false;
			for (int i = 0; i < workers; ++i)
				result |= tasks[i].Result;
			Assert.True(result);
		}
	}
}
