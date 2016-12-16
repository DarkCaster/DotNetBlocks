using System;
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
		
		[Test]
		public void ReadLock()
		{
			var rwLock=new AsyncRWLock();
			rwLock.EnterReadLock();
			rwLock.ExitReadLock();
			
			rwLock.EnterReadLock();
			rwLock.EnterReadLock();
			Assert.False(rwLock.TryEnterWriteLock());
			Assert.True(rwLock.TryEnterReadLock());
			rwLock.ExitReadLock();
			Assert.False(rwLock.TryEnterWriteLock());
			rwLock.ExitReadLock();
			Assert.False(rwLock.TryEnterWriteLock());
			rwLock.ExitReadLock();
		}
		
		[Test]
		public void WriteLock()
		{
			var rwLock=new AsyncRWLock();
			rwLock.EnterWriteLock();
			rwLock.ExitWriteLock();
			
			rwLock.EnterWriteLock();
			Assert.False(rwLock.TryEnterWriteLock());
			Assert.False(rwLock.TryEnterReadLock());
			rwLock.ExitWriteLock();
		}
		
		[Test]
		public void LockSeqence1()
		{
			var rwLock=new AsyncRWLock();
			rwLock.EnterWriteLock();
			rwLock.ExitWriteLock();
			rwLock.EnterReadLock();
			rwLock.EnterReadLock();
			rwLock.ExitReadLock();
			rwLock.ExitReadLock();
		}
		
		[Test]
		public void LockSeqence2()
		{
			var rwLock=new AsyncRWLock();
			rwLock.EnterReadLock();
			rwLock.EnterReadLock();
			rwLock.ExitReadLock();
			rwLock.ExitReadLock();
			rwLock.EnterWriteLock();
			rwLock.ExitWriteLock();
		}
		
		[Test]
		public void ReadLockAsync()
		{
			var rwLock=new AsyncRWLock();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			rwLock.ExitReadLock();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			Assert.False(rwLock.TryEnterWriteLock());
			Assert.True(rwLock.TryEnterReadLock());
			rwLock.ExitReadLock();
			Assert.False(rwLock.TryEnterWriteLock());
			rwLock.ExitReadLock();
			Assert.False(rwLock.TryEnterWriteLock());
			rwLock.ExitReadLock();
		}
		
		[Test]
		public void WriteLockAsync()
		{
			var rwLock=new AsyncRWLock();
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			rwLock.ExitWriteLock();
			
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			Assert.False(rwLock.TryEnterWriteLock());
			Assert.False(rwLock.TryEnterReadLock());
			rwLock.ExitWriteLock();
		}
		
		[Test]
		public void LockSeqence1Async()
		{
			var rwLock=new AsyncRWLock();
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			rwLock.ExitWriteLock();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			rwLock.ExitReadLock();
			rwLock.ExitReadLock();
		}
		
		[Test]
		public void LockSeqence2Async()
		{
			var rwLock=new AsyncRWLock();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			Task.Run( async () => await rwLock.EnterReadLockAsync()).Wait();
			rwLock.ExitReadLock();
			rwLock.ExitReadLock();
			Task.Run( async () => await rwLock.EnterWriteLockAsync()).Wait();
			rwLock.ExitWriteLock();
		}
	}
}
