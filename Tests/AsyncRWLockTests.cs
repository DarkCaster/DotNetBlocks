using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using DarkCaster.Async;

namespace Tests
{
	[TestFixture]
	public class AsyncReadWriteLock
	{
		[Test]
		public void Init()
		{
			Assert.DoesNotThrow(() => new AsyncRWLock());
		}

		private static void AssertCounters(AsyncRWLock rwLock, int currentReadCount, int waitingReadCount, bool writerIsActive, int waitingWriteCount)
		{
			var currentReadCountActual = rwLock.CurrentReadCount;
			var waitingReadCountActual = rwLock.WaitingReadCount;
			var writerIsActiveActual = rwLock.WriterIsActive;
			var waitingWriteCountActual = rwLock.WaitingWriteCount;

			if(currentReadCountActual != currentReadCount)
				throw new Exception(string.Format("Expected CurrentReadCount is {0}, but actual is {1} ", currentReadCount, currentReadCountActual));
			if(waitingReadCountActual != waitingReadCount)
				throw new Exception(string.Format("Expected WaitingReadCount is {0}, but actual is {1} ", waitingReadCount, waitingReadCountActual));
			if(writerIsActiveActual != writerIsActive)
				throw new Exception(string.Format("Expected WriterIsActive is {0}, but actual is {1} ", writerIsActive, writerIsActiveActual));
			if(waitingWriteCountActual != waitingWriteCount)
				throw new Exception(string.Format("Expected WaitingWriteCount is {0}, but actual is {1} ", waitingWriteCount, waitingWriteCountActual));
		}

		[Test]
		public void ReadLock()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock, 3, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void WriteLock()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 0, 0, true, 0);
			Assert.False(rwLock.TryEnterReadLock());
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void LockSeqence1()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void LockSeqence2()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
		}

		[Test]
		public void ReadLockAsync()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 1, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock, 3, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 2, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void WriteLockAsync()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock, 0, 0, true, 0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock, 0, 0, true, 0);
			Assert.False(rwLock.TryEnterReadLock());
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void LockSeqence1Async()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 1, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void LockSeqence2Async()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 1, 0, false, 0);
			Task.Run(async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock, 2, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Task.Run(async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		private volatile bool eventReset = false;
		private volatile int taskStatus = 0;

		[Test]
		public void ReadLockMultithread()
		{
			for(int i = 0; i < 10; ++i)
			{
				eventReset = false;
				taskStatus = 0;
				var rwLock = new AsyncRWLock();
				var reader1 = new Task(() =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus = 1;
					rwLock.EnterReadLock();
					while(!eventReset)
						Thread.Sleep(10);
					rwLock.ExitReadLock();
				});
				var reader2 = new Task(() =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus = 1;
					rwLock.EnterReadLock();
					while(!eventReset)
						Thread.Sleep(10);
					rwLock.ExitReadLock();
				});
				var reader3 = new Task(() =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus = 1;
					rwLock.EnterReadLock();
					while(!eventReset)
						Thread.Sleep(10);
					rwLock.ExitReadLock();
				});
				rwLock.EnterWriteLock();
				AssertCounters(rwLock, 0, 0, true, 0);
				reader1.Start();
				while(taskStatus == 0)
					Thread.Sleep(10);
				if(taskStatus < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 1, true, 0);
				reader2.Start();
				while(taskStatus == 0)
					Thread.Sleep(10);
				if(taskStatus < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 2, true, 0);
				reader3.Start();
				while(taskStatus == 0)
					Thread.Sleep(10);
				if(taskStatus < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 3, true, 0);
				rwLock.ExitWriteLock();
				AssertCounters(rwLock, 3, 0, false, 0);
				Thread.Sleep(100);
				AssertCounters(rwLock, 3, 0, false, 0);
				Assert.True(rwLock.TryEnterReadLock());
				AssertCounters(rwLock, 4, 0, false, 0);
				rwLock.ExitReadLock();
				AssertCounters(rwLock, 3, 0, false, 0);
				Assert.False(rwLock.TryEnterWriteLock());
				AssertCounters(rwLock, 3, 0, false, 0);
				eventReset = true;
				reader1.Wait();
				reader2.Wait();
				reader3.Wait();
				Assert.AreEqual(TaskStatus.RanToCompletion, reader1.Status);
				Assert.AreEqual(TaskStatus.RanToCompletion, reader2.Status);
				Assert.AreEqual(TaskStatus.RanToCompletion, reader3.Status);
				AssertCounters(rwLock, 0, 0, false, 0);
				Assert.True(rwLock.TryEnterWriteLock());
				AssertCounters(rwLock, 0, 0, true, 0);
				rwLock.ExitWriteLock();
				AssertCounters(rwLock, 0, 0, false, 0);
			}
		}

		private volatile bool eventReset_am = false;
		private volatile int taskStatus_am = 0;

		[Test]
		public void ReadLockAsyncMultithread()
		{
			for(int i = 0; i < 10; ++i)
			{
				eventReset_am = false;
				taskStatus_am = 0;
				var rwLock = new AsyncRWLock();
				AssertCounters(rwLock, 0, 0, false, 0);
				rwLock.EnterWriteLock();
				AssertCounters(rwLock, 0, 0, true, 0);
				var reader1 = Task.Run(async () =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus_am = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus_am = 1;
					await rwLock.EnterReadLockAsync();
					while(!eventReset_am)
						await Task.Delay(10);
					rwLock.ExitReadLock();
					return true;
				});
				while(taskStatus_am == 0)
					Thread.Sleep(10);
				if(taskStatus_am < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus_am = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 1, true, 0);
				var reader2 = Task.Run(async () =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus_am = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus_am = 1;
					await rwLock.EnterReadLockAsync();
					while(!eventReset_am)
						await Task.Delay(10);
					rwLock.ExitReadLock();
					return true;
				});
				while(taskStatus_am == 0)
					Thread.Sleep(10);
				if(taskStatus_am < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus_am = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 2, true, 0);
				var reader3 = Task.Run(() =>
				{
					if(rwLock.TryEnterReadLock())
					{
						taskStatus_am = -1;
						throw new Exception("TryEnterReadLock must return false!");
					}
					taskStatus_am = 1;
					rwLock.EnterReadLock();
					while(!eventReset_am)
						Thread.Sleep(10);
					rwLock.ExitReadLock();
					return true;
				});
				while(taskStatus_am == 0)
					Thread.Sleep(10);
				if(taskStatus_am < 0)
					Assert.Fail("taskStatus<0 !!!");
				taskStatus_am = 0;
				Thread.Sleep(100);
				AssertCounters(rwLock, 0, 3, true, 0);
				rwLock.ExitWriteLock();
				AssertCounters(rwLock, 3, 0, false, 0);
				Thread.Sleep(100);
				AssertCounters(rwLock, 3, 0, false, 0);
				Assert.True(rwLock.TryEnterReadLock());
				AssertCounters(rwLock, 4, 0, false, 0);
				rwLock.ExitReadLock();
				AssertCounters(rwLock, 3, 0, false, 0);
				Assert.False(rwLock.TryEnterWriteLock());
				AssertCounters(rwLock, 3, 0, false, 0);
				eventReset_am = true;
				Assert.True(reader1.Result);
				Assert.True(reader2.Result);
				Assert.True(reader3.Result);
				Assert.AreEqual(TaskStatus.RanToCompletion, reader1.Status);
				Assert.AreEqual(TaskStatus.RanToCompletion, reader2.Status);
				Assert.AreEqual(TaskStatus.RanToCompletion, reader3.Status);
				AssertCounters(rwLock, 0, 0, false, 0);
				Assert.True(rwLock.TryEnterWriteLock());
				AssertCounters(rwLock, 0, 0, true, 0);
				rwLock.ExitWriteLock();
				AssertCounters(rwLock, 0, 0, false, 0);
			}
		}

		[Test]
		public void ExitReadLockFail1()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitReadLock);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void ExitReadLockFail2()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitReadLock);
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void ExitReadLockFail3()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitReadLock);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void ExitWriteLockFail1()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitWriteLock);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock, 1, 0, false, 0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void ExitWriteLockFail2()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitWriteLock);
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		[Test]
		public void ExitWriteLockFail3()
		{
			var rwLock = new AsyncRWLock();
			AssertCounters(rwLock, 0, 0, false, 0);
			Assert.Throws(typeof(SynchronizationLockException), rwLock.ExitWriteLock);
			AssertCounters(rwLock, 0, 0, false, 0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock, 0, 0, true, 0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock, 0, 0, false, 0);
		}

		private class ValueIncrementor
		{
			private int val;
			public int GetValue()
			{
				return val;
			}
			public ValueIncrementor(int startValue)
			{
				val = startValue;
			}
			public void Increment()
			{
				val = val + 1;
			}
		}

		[Test]
		public void MultithreadedIncrement_WriteLock()
		{
			var rwLock = new AsyncRWLock();
			int startVal = 100;
			var incrementor = new ValueIncrementor(startVal);
			int iterations = 10000000; //using only write lock is more optimized
			Action taskDelegate1 = () => {
				for(int i = 0; i < iterations; ++i)
				{
					rwLock.EnterWriteLock();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			Action taskDelegate2 = () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					rwLock.EnterWriteLock();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			Parallel.Invoke(taskDelegate1,taskDelegate2);
			Assert.AreEqual(startVal + iterations * 2, incrementor.GetValue());
		}

		[Test]
		public void MultithreadedIncrement_ReadWriteLock()
		{
			var rwLock = new AsyncRWLock();
			int startVal = 100;
			var incrementor = new ValueIncrementor(startVal);
			int iterations = 1000000; //using read lock + write lock is slower than only using write lock
			Action taskDelegate1 = () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					rwLock.EnterWriteLock();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			Action taskDelegate2 = () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					rwLock.EnterReadLock();
					incrementor.Increment();
					rwLock.ExitReadLock();
				}
			};
			Parallel.Invoke(taskDelegate1, taskDelegate2);
			Assert.AreEqual(startVal + iterations * 2, incrementor.GetValue());
		}

		[Test]
		public void MultithreadedIncrementAsync_WriteLock()
		{
			var rwLock = new AsyncRWLock();
			int startVal = 100;
			var incrementor = new ValueIncrementor(startVal);
			int iterations = 10000000;
			Func<Task> taskDelegate1 = async () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					await rwLock.EnterWriteLockAsync();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			Func<Task> taskDelegate2 = async () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					await rwLock.EnterWriteLockAsync();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			var task1 = Task.Run(async () => await taskDelegate1());
			var task2 = Task.Run(async () => await taskDelegate2());
			task2.Wait();
			task1.Wait();
			Assert.AreEqual(startVal + iterations * 2, incrementor.GetValue());
		}

		[Test]
		public void MultithreadedIncrementAsync_ReadWriteLock()
		{
			var rwLock = new AsyncRWLock();
			int startVal = 100;
			var incrementor = new ValueIncrementor(startVal);
			int iterations = 100000000;
			Func<Task> taskDelegate1 = async () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					await rwLock.EnterWriteLockAsync();
					incrementor.Increment();
					rwLock.ExitWriteLock();
				}
			};
			Func<Task> taskDelegate2 = async () =>
			{
				for(int i = 0; i < iterations; ++i)
				{
					await rwLock.EnterReadLockAsync();
					incrementor.Increment();
					rwLock.ExitReadLock();
				}
			};
			var task1 = Task.Run(async () => await taskDelegate1());
			var task2 = Task.Run(async () => await taskDelegate2());
			task2.Wait();
			task1.Wait();
			Assert.AreEqual(startVal + iterations * 2, incrementor.GetValue());
		}
	}
}
