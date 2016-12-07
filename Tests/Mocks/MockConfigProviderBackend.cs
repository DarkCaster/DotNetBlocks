// MockConfigProviderBackend.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016 DarkCaster <dark.caster@outlook.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using DarkCaster.Config.Private;

namespace Tests.Mocks
{
	/// <summary>
	///  Storage backend mock for use with tests of FileConfigProvider
	/// </summary>
	public class MockConfigProviderBackend : IConfigStorageBackend, IDisposable
	{
		private readonly bool writeAllowed;
		private readonly ReaderWriterLockSlim readLock;
		private readonly SemaphoreSlim writeLock;
		private readonly Random random;
		private readonly float failProb;
		
		private byte[] cachedData;
		private int fetchCount;
		private int writeCount;
		private int deleteCount;
		private int writeAllowedCount;
		
		//methods for external control, stats get
		public void ResetCnt()
		{
			Interlocked.Exchange(ref fetchCount,0);
			Interlocked.Exchange(ref writeCount,0);
			Interlocked.Exchange(ref deleteCount,0);
			Interlocked.Exchange(ref writeAllowedCount,0);
		}
		
		public int FetchCount { get { return fetchCount; }}
		public int WriteCount { get { return writeCount; }}
		public int DeleteCount { get { return deleteCount; }}
		public int WriteAllowedCount { get { return writeAllowedCount; }}
		
		MockConfigProviderBackend(bool writeAllowed, byte[] data=null, float failProb=0.0f)
		{
			this.writeAllowed=writeAllowed;
			this.cachedData=data;
			this.readLock=new ReaderWriterLockSlim();
			this.writeLock=new SemaphoreSlim(1, 1);
			this.fetchCount=0;
			this.writeCount=0;
			this.deleteCount=0;
			this.writeAllowedCount=0;
			this.random=new Random();
			this.failProb=failProb;
		}
		
		private void SuddenFail()
		{
			if( ((float)random.NextDouble()) < failProb )
				throw new Exception("Whoops, sudden fail!");
		}
		
		public byte[] Fetch()
		{
			Interlocked.Increment(ref fetchCount);
			readLock.EnterReadLock();
			try
			{
				SuddenFail();
				if(cachedData!=null)
				{
					var result=new byte[cachedData.Length];
					Buffer.BlockCopy(cachedData,0,result,0,cachedData.Length);
					return result;
				}
				return null;
			}
			finally{ readLock.ExitReadLock(); }
		}
		
		public void MarkForDelete()
		{
			Interlocked.Increment(ref deleteCount);
			SuddenFail();
		}
		
		public bool IsWriteAllowed { get { Interlocked.Increment(ref writeAllowedCount); return writeAllowed; } }
		
		public void Commit(byte[] data)
		{
			Interlocked.Increment(ref writeCount);
			if(data == null)
				throw new ArgumentNullException("data","Cannot write null data!");
			writeLock.Wait();
			try
			{
				if(!writeAllowed)
					throw new Exception("Write is not allowed!");
				SuddenFail();
				readLock.EnterWriteLock();
				cachedData=data;
				readLock.ExitWriteLock();
			}
			finally { writeLock.Release(); }
		}
		
		public async Task CommitAsync(byte[] data)
		{
			Interlocked.Increment(ref writeCount);
			if(data == null)
				throw new ArgumentNullException("data","Cannot write null data!");
			await writeLock.WaitAsync();
			try
			{
				if(!writeAllowed)
					throw new Exception("Write is not allowed!");
				SuddenFail();
				readLock.EnterWriteLock();
				cachedData=data;
				readLock.ExitWriteLock();
			}
			finally { writeLock.Release(); }
		}
		
		public void Dispose()
		{
			writeLock.Dispose();
			readLock.Dispose();
		}
	}
}
