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
			Assert.DoesNotThrow(()=>new AsyncRWLock());
		}
		
		private static void AssertCounters(AsyncRWLock rwLock, int currentReadCount, int waitingReadCount, bool writerIsActive, int waitingWriteCount)
		{
			Assert.AreEqual(currentReadCount,rwLock.CurrentReadCount);
			Assert.AreEqual(waitingReadCount,rwLock.WaitingReadCount);
			Assert.AreEqual(writerIsActive,rwLock.WriterIsActive);
			Assert.AreEqual(waitingWriteCount,rwLock.WaitingWriteCount);
		}
		
		[Test]
		public void ReadLock()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,2,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,2,0,false,0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,3,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,2,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void WriteLock()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,0,0,true,0);
			Assert.False(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void LockSeqence1()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void LockSeqence2()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.EnterReadLock();
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
		}
		
		[Test]
		public void ReadLockAsync()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,1,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,2,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,2,0,false,0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,3,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,2,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void WriteLockAsync()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock,0,0,true,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,0,0,true,0);
			Assert.False(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void LockSeqence1Async()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,1,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void LockSeqence2Async()
		{
			var rwLock=new AsyncRWLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,1,0,false,0);
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			AssertCounters(rwLock,2,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,1,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,0,0,false,0);
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void ReadLockMultithread()
		{
			var rwLock=new AsyncRWLock();
			var reader1=new Task(()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	rwLock.EnterReadLock();
			                           	Thread.Sleep(250);
			                           	rwLock.ExitReadLock();
			                     });
			var reader2=new Task(()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	rwLock.EnterReadLock();
			                           	Thread.Sleep(250);
			                           	rwLock.ExitReadLock();
			                     });
			var reader3=new Task(()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	rwLock.EnterReadLock();
			                           	Thread.Sleep(250);
			                           	rwLock.ExitReadLock();
			                     });
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			reader1.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,1,true,0);
			reader2.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,2,true,0);
			reader3.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,3,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,3,0,false,0);
			Thread.Sleep(100);
			AssertCounters(rwLock,3,0,false,0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,4,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,3,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,3,0,false,0);
			reader1.Wait();
			reader2.Wait();
			reader3.Wait();
			Assert.AreEqual(TaskStatus.RanToCompletion ,reader1.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, reader2.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, reader3.Status);
			AssertCounters(rwLock,0,0,false,0);
			Assert.True(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
		}
		
		[Test]
		public void ReadLockAsyncMultithread()
		{
			var rwLock=new AsyncRWLock();
			var reader1=new Task(async ()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	await rwLock.EnterReadLockAsync();
			                           	await Task.Delay(250);
			                           	rwLock.ExitReadLock();
			                     });
			var reader2=new Task(async ()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	await rwLock.EnterReadLockAsync();
			                           	await Task.Delay(250);
			                           	rwLock.ExitReadLock();
			                     });
			var reader3=new Task(()=>{
			                           	if(rwLock.TryEnterReadLock())
			                           		throw new Exception("TryEnterReadLock must return false!");
			                           	rwLock.EnterReadLock();
			                           	Thread.Sleep(250);
			                           	rwLock.ExitReadLock();
			                     });
			rwLock.EnterWriteLock();
			AssertCounters(rwLock,0,0,true,0);
			reader1.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,1,true,0);
			reader2.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,2,true,0);
			reader3.Start();
			Thread.Sleep(100);
			AssertCounters(rwLock,0,3,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,3,0,false,0);
			Thread.Sleep(100);
			AssertCounters(rwLock,3,0,false,0);
			Assert.True(rwLock.TryEnterReadLock());
			AssertCounters(rwLock,4,0,false,0);
			rwLock.ExitReadLock();
			AssertCounters(rwLock,3,0,false,0);
			Assert.False(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,3,0,false,0);
			reader1.Wait();
			reader2.Wait();
			reader3.Wait();
			Assert.AreEqual(TaskStatus.RanToCompletion, reader1.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, reader2.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, reader3.Status);
			AssertCounters(rwLock,0,0,false,0);
			Assert.True(rwLock.TryEnterWriteLock());
			AssertCounters(rwLock,0,0,true,0);
			rwLock.ExitWriteLock();
			AssertCounters(rwLock,0,0,false,0);
		}
	}
}
