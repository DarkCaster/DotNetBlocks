// MockClientITunnel.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
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
using DarkCaster.DataTransfer.Client;

namespace Tests.Mocks
{
	public class MockClientITunnel : ITunnel
	{
		private readonly int minDelay;
		private readonly int maxDelay;
		private readonly float failProbability;
		private readonly float partialOpProbability;
		private readonly Random random = new Random();
		private readonly object randomLock = new object();

		private int isDisconnected = 0;
		private int isDisposed = 0;
		private int noFailOpsCounter;

		private int readCount = 0;
		private int writeCount = 0;
		private int readAsyncCount = 0;
		private int writeAsyncCount = 0;
		private int disconnectCount = 0;
		private int disposeCount = 0;

		public MockClientITunnel(int minDelay, int maxDelay, float failProbability, float partialOpProbability, int noFailOpsCount)
		{
			this.minDelay = minDelay;
			this.maxDelay = maxDelay;
			this.failProbability = failProbability;
			this.partialOpProbability = partialOpProbability;
			this.noFailOpsCounter = noFailOpsCount;
		}

		private int GenDelay()
		{
			lock (randomLock)
				return random.Next(minDelay, maxDelay);
		}

		private bool GenFail()
		{
			lock (randomLock)
				return random.NextDouble() <= failProbability;
		}

		private bool GenPartial()
		{
			lock (randomLock)
				return random.NextDouble() <= partialOpProbability;
		}

		private int GenPartialOpSz(int sz)
		{
			lock (randomLock)
				return random.Next(0, sz);
		}

		private void CheckParams(int sz, byte[] buffer, int offset = 0)
		{
			if (buffer == null || sz <= 0 || offset < 0 || offset + sz > buffer.Length)
				throw new ArgumentException("Input parameters are incorrect");
			if (Interlocked.CompareExchange(ref isDisconnected, 0, 0) != 0)
				throw new Exception("MockClientTunnel is already disconnected!");
			if (Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0)
				throw new ObjectDisposedException("MockClientTunnel is already disposed!");
		}

		private int ReadWriteData(int sz)
		{
			if (Interlocked.Decrement(ref noFailOpsCounter) >= 0)
			{
				var delay = GenDelay();
				if (delay > 0)
					Thread.Sleep(delay);
				return sz;
			}
			else
			{
				if (GenFail())
					throw new Exception("Fail!");
				var delay = GenDelay();
				if (delay > 0)
					Thread.Sleep(delay);
				if (GenPartial())
					return GenPartialOpSz(sz);
				return sz;
			}
		}

		private async Task<int> ReadWriteDataAsync(int sz)
		{
			if (Interlocked.Decrement(ref noFailOpsCounter) >= 0)
			{
				var delay = GenDelay();
				if (delay > 0)
					await Task.Delay(delay);
				return sz;
			}
			else
			{
				if (GenFail())
					throw new Exception("Fail!");
				var delay = GenDelay();
				if (delay > 0)
					await Task.Delay(delay);
				if (GenPartial())
					return GenPartialOpSz(sz);
				return sz;
			}
		}

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref readCount);
			CheckParams(sz, buffer, offset);
			return ReadWriteData(sz);
		}

		public int WriteData(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref writeCount);
			CheckParams(sz, buffer, offset);
			return ReadWriteData(sz);
		}

		public Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref readAsyncCount);
			CheckParams(sz, buffer, offset);
			return ReadWriteDataAsync(sz);
		}

		public Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref writeAsyncCount);
			CheckParams(sz, buffer, offset);
			return ReadWriteDataAsync(sz);
		}

		//Dispose and disconnect logic is simplified for internal ITunnel classes.
		//Disconnect is not responsible for checking tunnel state.
		//Dispose is responsible only for disposing it's own stuff, and should not call disconnect.
		//Top IEntryTunnel object that incapsulate all internal ITunnel objects must call disconnect on dispose
		//and must not call dispose or disconnect twice.

		public void Disconnect()
		{
			Interlocked.Increment(ref disconnectCount);
			if (Interlocked.CompareExchange(ref isDisconnected, 1, 0) != 0)
				throw new Exception("MockClientTunnel is already disconnected!");
		}

		public void Dispose()
		{
			Interlocked.Increment(ref disposeCount);
			if (Interlocked.CompareExchange(ref isDisposed, 1, 0) != 0)
				throw new ObjectDisposedException("Dispose on ITunnel objects should be only run once!");
		}

		public int ReadCount { get { return Interlocked.CompareExchange(ref readCount, 0, 0); } }
		public int WriteCount { get { return Interlocked.CompareExchange(ref writeCount, 0, 0); } }
		public int ReadAsyncCount { get { return Interlocked.CompareExchange(ref readAsyncCount, 0, 0); } }
		public int WriteAsyncCount { get { return Interlocked.CompareExchange(ref writeAsyncCount, 0, 0); } }
		public int DisconectCount { get { return Interlocked.CompareExchange(ref disconnectCount, 0, 0); } }
		public int DisposeCount { get { return Interlocked.CompareExchange(ref disposeCount, 0, 0); } }
	}
}
