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
using System.Threading.Tasks;

namespace DarkCaster.Async
{
	/// <summary>
	/// Based on ideas from https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock .
	/// Reader\writer lock implementation with support for async lock aquire\release.
	/// Lock aquire\release may be performed from different threads.
	/// </summary>
	public class AsyncRWLock
	{
		public async Task EnterReadLockAsync()
		{
			throw new NotImplementedException("TODO");
		}
		
		public async Task EnterWriteLockAsync()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void EnterReadLock()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void EnterWriteLock()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void ExitReadLock()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void ExitWriteLock()
		{
			throw new NotImplementedException("TODO");
		}
		
		private static Task CreateCompletedTask()
		{
			return Task.WhenAll();
		}
		
		public AsyncRWLock()
		{
			var x=CreateCompletedTask();
			
		}
	}
}
