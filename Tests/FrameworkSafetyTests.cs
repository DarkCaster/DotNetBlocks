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
		
		//using code from https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock/
		public class SampleAsyncReaderWriterLock
		{
			public SampleAsyncReaderWriterLock()
			{
				m_readerReleaser = Task.FromResult(new Releaser(this, false));
				m_writerReleaser = Task.FromResult(new Releaser(this, true));
			}
		
			public struct Releaser : IDisposable
			{
				private readonly SampleAsyncReaderWriterLock m_toRelease;
				private readonly bool m_writer;

				internal Releaser(SampleAsyncReaderWriterLock toRelease, bool writer)
				{
					m_toRelease = toRelease;
					m_writer = writer;
				}

				public void Dispose()
				{
					if (m_toRelease != null) {
						if (m_writer)
							m_toRelease.WriterRelease();
						else
							m_toRelease.ReaderRelease();
					}
				}
			}
		
			private readonly Task<Releaser> m_readerReleaser;
			private readonly Task<Releaser> m_writerReleaser;
		
			private readonly Queue<TaskCompletionSource<Releaser>> m_waitingWriters = new Queue<TaskCompletionSource<Releaser>>();
			private TaskCompletionSource<Releaser> m_waitingReader = new TaskCompletionSource<Releaser>();
    
			private int m_status;
    
			private int m_readersWaiting;

			public Task<Releaser> ReaderLockAsync()
			{
				lock (m_waitingWriters) {
					if (m_status >= 0 && m_waitingWriters.Count == 0) {
						++m_status;
						return m_readerReleaser;
					} else {
						++m_readersWaiting;
						return m_waitingReader.Task.ContinueWith(t => t.Result);
					}
				}
			}
		
			public Task<Releaser> WriterLockAsync()
			{
				lock (m_waitingWriters) {
					if (m_status == 0) {
						m_status = -1;
						return m_writerReleaser;
					} else {
						var waiter = new TaskCompletionSource<Releaser>();
						m_waitingWriters.Enqueue(waiter);
						return waiter.Task;
					}
				}
			}
		
			public void ReaderRelease()
			{
				TaskCompletionSource<Releaser> toWake = null;

				lock (m_waitingWriters) {
					m_status--;
					if (m_status == 0 && m_waitingWriters.Count > 0) {
						m_status = -1;
						toWake = m_waitingWriters.Dequeue();
					}
				}

				if (toWake != null)
					toWake.SetResult(new Releaser(this, true));
			}
		
			public void WriterRelease()
			{
				TaskCompletionSource<Releaser> toWake = null;
				bool toWakeIsWriter = false;

				lock (m_waitingWriters) {
					if (m_waitingWriters.Count > 0) {
						toWake = m_waitingWriters.Dequeue();
						toWakeIsWriter = true;
					} else if (m_readersWaiting > 0) {
						toWake = m_waitingReader;
						m_status = m_readersWaiting;
						m_readersWaiting = 0;
						m_waitingReader = new TaskCompletionSource<Releaser>();
					} else
						m_status = 0;
				}

				if (toWake != null)
					toWake.SetResult(new Releaser(this, toWakeIsWriter));
			}
		}
		
		private static SampleAsyncReaderWriterLock asyncLock = new SampleAsyncReaderWriterLock();
		
		//should fail
		private async Task<int> AsyncRWLockTestAsync()
		{
			for (int i = 0; i < 10; ++i) 
			{
				using(var enter=await asyncLock.ReaderLockAsync())
				{
					await Task.Delay(50);
				}
				using(var enter=await asyncLock.WriterLockAsync())
				{
					await Task.Delay(50);
				}
			}
			return 0;
		}
		
		[Test]
		public void AsyncLockTest()
		{
			var task = Task.Run(async () => await AsyncRWLockTestAsync());
			var i = task.Result;
		}
		
				
		private async Task<int> AsyncRWLockTest2_Async()
		{
			for (int i = 0; i < 10; ++i) 
			{
				var enter=await asyncLock.ReaderLockAsync();
				await Task.Delay(50);
				enter.Dispose();
				enter=await asyncLock.WriterLockAsync();
				await Task.Delay(50);
				enter.Dispose();
			}
			return 0;
		}
		
		[Test]
		public void AsyncLockTest2()
		{
			var task = Task.Run(async () => await AsyncRWLockTest2_Async());
			var i = task.Result;
		}
	}
}
