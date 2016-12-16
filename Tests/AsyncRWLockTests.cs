using System;
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
	}
}
