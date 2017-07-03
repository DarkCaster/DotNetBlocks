// EntryTunnelLite.cs
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
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Client
{
	public sealed class EntryTunnelLite : IEntryTunnelLite
	{
		private readonly INode downstreamNode;
		private readonly ITunnelConfig config;
		private readonly AsyncRWLock stateChangeLock = new AsyncRWLock();

		private readonly AsyncRunner stateChangeRunner = new AsyncRunner();
		private readonly AsyncRunner readRunner = new AsyncRunner();
		private readonly AsyncRunner writeRunner = new AsyncRunner();

		private volatile TunnelState state = TunnelState.Init;
		private ITunnel downstream;
		private int isDisposed = 0;

		private EntryTunnelLite() { }

		public EntryTunnelLite(INode downstreamNode, ITunnelConfig config)
		{
			this.downstreamNode = downstreamNode;
			this.config = config;
		}

		public TunnelState State { get { return state; } }

		private async Task<TunnelState> ConnectAsyncWorker()
		{
			if(state != TunnelState.Init)
				throw new Exception("Cannot perform connect in this state: " + state.ToString());
			try
			{
				downstream = await downstreamNode.OpenTunnelAsync(config);
			}
			catch(Exception)
			{
				state = TunnelState.Offline;
				throw;
			}
			state = TunnelState.Online;
			return TunnelState.Online;
		}

		public TunnelState Connect()
		{
			stateChangeLock.EnterWriteLock();
			try
			{
				return stateChangeRunner.ExecuteTask(ConnectAsyncWorker);
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}

		public async Task<TunnelState> ConnectAsync()
		{
			await stateChangeLock.EnterWriteLockAsync();
			try
			{
				return await ConnectAsyncWorker();
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}

		private async Task<int> ReadDataAsyncWorker(int sz, byte[] buffer, int offset = 0)
		{
			if(state == TunnelState.Init)
				return 0;
			try
			{
				return await downstream.ReadDataAsync(sz, buffer, offset);
			}
			catch(Exception ex)
			{
				if(state != TunnelState.Online)
					throw new TunnelEofException(ex);
				throw;
			}
		}

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			stateChangeLock.EnterReadLock();
			try
			{
				return readRunner.ExecuteTask(() => ReadDataAsyncWorker(sz, buffer, offset));
			}
			finally
			{
				stateChangeLock.ExitReadLock();
			}
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			await stateChangeLock.EnterReadLockAsync();
			try
			{
				return await ReadDataAsyncWorker(sz, buffer, offset);
			}
			finally
			{
				stateChangeLock.ExitReadLock();
			}
		}

		private async Task<int> WriteDataAsyncWorker(int sz, byte[] buffer, int offset = 0)
		{
			if(state == TunnelState.Init)
				return 0;
			if(state == TunnelState.Offline)
				throw new TunnelEofException();
			return await downstream.WriteDataAsync(sz, buffer, offset);
		}

		public int WriteData(int sz, byte[] buffer, int offset = 0)
		{
			stateChangeLock.EnterReadLock();
			try
			{
				return writeRunner.ExecuteTask(() => WriteDataAsyncWorker(sz, buffer, offset));
			}
			finally
			{
				stateChangeLock.ExitReadLock();
			}
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			await stateChangeLock.EnterReadLockAsync();
			try
			{
				return await WriteDataAsyncWorker(sz, buffer, offset);
			}
			finally
			{
				stateChangeLock.ExitReadLock();
			}
		}

		private async Task DisconnectAsyncWorker()
		{
			if(state == TunnelState.Offline)
				return;
			state = TunnelState.Offline;
			if(downstream == null)
				throw new Exception("Downstream ITunnel was not initialized!");
			await downstream.DisconnectAsync();
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
			if(Interlocked.CompareExchange(ref isDisposed, 1, 0) == 1)
				return;
			try { Disconnect(); }
			catch{ }
			stateChangeLock.EnterWriteLock();
			try
			{
				if(downstream != null)
					downstream.Dispose();
				//precaution, so any operation on downstream will throw an exception now.
				downstream = null;
			}
			finally
			{
				stateChangeLock.ExitWriteLock();
			}
		}
	}
}
