// AsyncRWLock.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DarkCaster.Async
{
	/// <summary>
	/// Based on ideas from https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock .
	/// Simple reader\writer lock implementation with support for async lock aquire.
	/// Lock aquire\release may be performed from different threads.
	/// </summary>
	public class AsyncRWLock
	{
		private readonly object opLock;
		private readonly Task completedTask;
		
		private readonly Queue<TaskCompletionSource<bool>> writerAwaiters;
		private bool writerIsActive;
		private int writersWaiting;
		
		private TaskCompletionSource<bool> readerAwaiter;
		private int readersActive;
		private int readersWaiting;
		
		public AsyncRWLock()
		{
			this.opLock=new object();
			this.completedTask=Task.FromResult(false);
			this.writerAwaiters = new Queue<TaskCompletionSource<bool>>();
			this.writerIsActive=false;
			this.writersWaiting=0;
			this.readerAwaiter = new TaskCompletionSource<bool>();
			this.readersActive=0;
			this.readersWaiting=0;
		}
		
		public Task EnterReadLockAsync()
		{
			lock(opLock)
			{
				if(writerIsActive || writersWaiting > 0)
				{
					++readersWaiting;
					return readerAwaiter.Task.ContinueWith(t => t.Result);
				}
				++readersActive;
				return completedTask;
			}
		}
		
		public Task EnterWriteLockAsync()
		{
			lock(opLock)
			{
				if(writerIsActive || readersActive > 0)
				{
					++writersWaiting;
					var writerAwaiter=new TaskCompletionSource<bool>();
					writerAwaiters.Enqueue(writerAwaiter);
					return writerAwaiter.Task;
				}
				writerIsActive=true;
				return completedTask;
			}
		}
		
		public void EnterReadLock()
		{
			EnterReadLockAsync().Wait();
		}
		
		public void EnterWriteLock()
		{
			EnterWriteLockAsync().Wait();
		}
		
		public bool TryEnterReadLock()
		{
			lock(opLock)
			{
				if(writerIsActive || writersWaiting > 0)
					return false;
				EnterReadLockAsync().Wait();
				return true;
			}
		}
		
		public bool TryEnterWriteLock()
		{
			lock(opLock)
			{
				if(readersActive > 0 || writerIsActive)
					return false;
				EnterWriteLockAsync().Wait();
				return true;
			}
		}
		
		public int WaitingWriteCount
		{
			get
			{
				lock(opLock)
					return writersWaiting;
			}
		}
		
		public int WaitingReadCount
		{
			get
			{
				lock(opLock)
					return readersWaiting;
			}
		}
		
		public int CurrentReadCount
		{
			get
			{
				lock(opLock)
					return readersActive;
			}
		}
		
		public bool WriterIsActive
		{
			get
			{
				lock(opLock)
					return writerIsActive;
			}
		}
		
		public void ExitReadLock()
		{
			lock(opLock)
			{
				if(writerIsActive)
					throw new SynchronizationLockException("ExitReadLock call was called after EnterWriteLock!");
				if(readersActive<1)
					throw new SynchronizationLockException("Excessive ExitReadLock call detected!");
				--readersActive;
				if(readersActive == 0 && writersWaiting > 0)
				{
					writerIsActive=true;
					writersWaiting--;
					writerAwaiters.Dequeue().SetResult(true);
				}
			}
		}
		
		public void ExitWriteLock()
		{
			lock(opLock)
			{
				if(readersActive>0)
					throw new SynchronizationLockException(string.Format("ExitWriteLock call was called while {0} readers still active", readersActive));
				if(!writerIsActive)
					throw new SynchronizationLockException("Excessive ExitWriteLock call detected!");
				if(writersWaiting>0)
				{
					writerIsActive=true;
					writersWaiting--;
					writerAwaiters.Dequeue().SetResult(true);
					return;
				}
				writerIsActive=false;
				if(readersWaiting>0)
				{
					readersActive=readersWaiting;
					readersWaiting=0;
					readerAwaiter.SetResult(true);
					readerAwaiter=new TaskCompletionSource<bool>();
				}
			}
		}
	}
}
