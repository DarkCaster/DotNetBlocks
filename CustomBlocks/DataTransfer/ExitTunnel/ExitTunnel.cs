// ExitTunnel.cs
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
using DarkCaster.Async;

namespace DarkCaster.DataTransfer.Server
{
	public sealed class ExitTunnel : IExitTunnel
	{
		private readonly AsyncRunner readRunner = new AsyncRunner();
		private readonly AsyncRunner writeRunner = new AsyncRunner();
		private readonly AsyncRunner stateChangeRunner = new AsyncRunner();
		private readonly AsyncRWLock stateChangeLock = new AsyncRWLock();
		private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);
		private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
		private readonly ITunnel upstream;
		private volatile bool isDisconnected = false;
		private int isDisposed = 0;

		public ExitTunnel(ITunnel upstream)
		{
			this.upstream = upstream;
		}

		private async Task<int> WriteDataWorkerAsync(int sz, byte[] buffer, int offset)
		{
			if(isDisconnected)
				throw new TunnelEofException();
			try
			{
				return await upstream.WriteDataAsync(sz, buffer, offset);
			}
			catch
			{
				if(isDisconnected)
					throw new TunnelEofException();
				throw;
			}
		}

		private async Task<int> ReadDataWorkerAsync(int sz, byte[] buffer, int offset)
		{
			try
			{
				return await upstream.ReadDataAsync(sz, buffer, offset);
			}
			catch
			{
				if(isDisconnected)
					throw new TunnelEofException();
				throw;
			}
		}

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			readLock.Wait();
			try
			{
				return readRunner.ExecuteTask(() => ReadDataWorkerAsync(sz, buffer, offset));
			}
			finally
			{
				readLock.Release();
			}
		}

		public int WriteData(int sz, byte[] buffer, int offset = 0)
		{
			writeLock.Wait();
			try
			{
				return writeRunner.ExecuteTask(() => WriteDataWorkerAsync(sz, buffer, offset));
			}
			finally
			{
				writeLock.Release();
			}
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			await readLock.WaitAsync();
			try
			{
				return await ReadDataWorkerAsync(sz, buffer, offset);
			}
			finally
			{
				readLock.Release();
			}
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			await writeLock.WaitAsync();
			try
			{
				return await WriteDataWorkerAsync(sz, buffer, offset);
			}
			finally
			{
				writeLock.Release();
			}
		}

		private async Task DisconnectAsyncWorker()
		{
			if(isDisconnected)
				return;
			await upstream.DisconnectAsync();
			isDisconnected = true;
		}

		public void Disconnect()
		{
			stateChangeLock.EnterWriteLock();
			try
			{
				stateChangeRunner.ExecuteTask(DisconnectAsyncWorker);
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}

		public async Task DisconnectAsync()
		{
			await stateChangeLock.EnterWriteLockAsync();
			try
			{
				await DisconnectAsyncWorker();
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}

		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref isDisposed, 1, 0) != 0)
				return;
			Disconnect();
			stateChangeLock.EnterWriteLock();
			try
			{
				upstream.Dispose();
				readRunner.Dispose();
				writeRunner.Dispose();
				stateChangeRunner.Dispose();
				readLock.Dispose();
				writeLock.Dispose();
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}
	}
}
