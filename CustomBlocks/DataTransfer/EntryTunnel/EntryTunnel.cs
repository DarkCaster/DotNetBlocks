// EntryTunnel.cs
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
using DarkCaster.Events;

namespace DarkCaster.DataTransfer.Client
{
	public sealed class EntryTunnel : IEntryTunnel
	{
		private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);
		private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);

		private volatile TunnelState state = TunnelState.Init;
		private volatile bool isDisposed = false;

		private int stateChangeTasksRunning = 0;
		private readonly ISafeEventCtrl<TunnelStateEventArgs> evCtl;
		private readonly ISafeEvent<TunnelStateEventArgs> ev;

		private readonly INode downstreamNode;
		private ITunnel downstream;

		private class OfflineSwitchException : Exception { }

		private EntryTunnel()
		{
#if DEBUG
			evCtl = new SafeEventDbg<TunnelStateEventArgs>();
			ev = (ISafeEvent<TunnelStateEventArgs>)evCtl;
#else
			evCtl = new SafeEvent<TunnelStateEventArgs>();
			ev = (ISafeEvent<TunnelStateEventArgs>)evCtl;
#endif
		}

		public EntryTunnel(INode downstreamNode) : this()
		{
			this.downstreamNode = downstreamNode;
		}

		public TunnelState State { get { return state; } }

		private void WaitStateChangeTasks()
		{
			int sleep = 1;
			const int max_sleep = 50;
			while (Interlocked.CompareExchange(ref stateChangeTasksRunning, 0, 0) != 0)
			{
				Thread.Sleep(sleep);
				sleep *= 2;
				if (sleep > max_sleep)
					sleep = max_sleep;
			}
		}

		private async Task WaitStateChangeTasksAsync()
		{
			int sleep = 1;
			const int max_sleep = 50;
			while (Interlocked.CompareExchange(ref stateChangeTasksRunning, 0, 0) != 0)
			{
				await Task.Delay(sleep);
				sleep *= 2;
				if (sleep > max_sleep)
					sleep = max_sleep;
			}
		}

		public void InitTask()
		{
			Interlocked.Increment(ref stateChangeTasksRunning);
			Task.Run(()=>
			{
				try
				{
					try
					{
						Volatile.Write(ref downstream, downstreamNode.OpenTunnel());
						Thread.MemoryBarrier(); //additional safeguard, may be redundant. 
					}
					catch (TunnelException ex)
					{
						evCtl.Raise(this, new TunnelStateEventArgs(TunnelState.Offline, ex), () => { state = TunnelState.Offline; });
						return;
					}
					evCtl.Raise(this, new TunnelStateEventArgs(TunnelState.Online, null), () => { state = TunnelState.Online; });
				}
				finally
				{
					Interlocked.Decrement(ref stateChangeTasksRunning);
				}
			});
		}

		private void SwitchToOffline(TunnelException ex)
		{
			try
			{
				evCtl.Raise(this, new TunnelStateEventArgs(TunnelState.Offline, ex), () =>
				{
					if (state == TunnelState.Offline)
						throw new OfflineSwitchException();
					state = TunnelState.Offline;
				});
			}
			catch (OfflineSwitchException) { }
		}

		private void SwitchToOfflineTask(TunnelException ex)
		{
			Interlocked.Increment(ref stateChangeTasksRunning);
			Task.Run(()=>
			{
				SwitchToOffline(ex);
				Interlocked.Decrement(ref stateChangeTasksRunning);
			});
		}

		public ISafeEvent<TunnelStateEventArgs> StateChangeEvent { get { return ev; } }

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			readLock.Wait();
			try
			{
				if (state == TunnelState.Init)
					return 0;
				try
				{
					return downstream.ReadData(sz, buffer, offset);
				}
				catch (TunnelException ex)
				{
					SwitchToOfflineTask(ex);
					return 0;
				}
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
				if (state != TunnelState.Online)
					return 0;
				try
				{
					return downstream.WriteData(sz, buffer, offset);
				}
				catch (TunnelException ex)
				{
					SwitchToOfflineTask(ex);
					return 0;
				}
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
				if (state == TunnelState.Init)
					return 0;
				try
				{
					return await downstream.ReadDataAsync(sz, buffer, offset);
				}
				catch (TunnelException ex)
				{
					SwitchToOfflineTask(ex);
					return 0;
				}
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
				if (state != TunnelState.Online)
					return 0;
				try
				{
					return await downstream.WriteDataAsync(sz, buffer, offset);
				}
				catch (TunnelException ex)
				{
					SwitchToOfflineTask(ex);
					return 0;
				}
			}
			finally
			{
				writeLock.Release();
			}
		}

		public void Disconnect()
		{
			writeLock.Wait();
			readLock.Wait();
			WaitStateChangeTasks();
			Interlocked.Increment(ref stateChangeTasksRunning);
			Task.Run(() =>
			{
				try
				{
					TunnelException tEx = null;
					try { downstream.Disconnect(); }
					catch (TunnelException ex) { tEx = ex; }
					SwitchToOffline(tEx);
				}
				finally
				{
					Interlocked.Decrement(ref stateChangeTasksRunning);
					readLock.Release();
					writeLock.Release();
				}
			});
		}

		public async Task DisconnectAsync()
		{
			await writeLock.WaitAsync();
			await readLock.WaitAsync();
			await WaitStateChangeTasksAsync();
			try
			{
				TunnelException tEx = null;
				try { downstream.Disconnect(); }
				catch (TunnelException ex) { tEx = ex; }
				SwitchToOfflineTask(tEx);
			}
			finally
			{
				readLock.Release();
				writeLock.Release();
			}
		}

		public void Dispose()
		{
			writeLock.Wait();
			readLock.Wait();
			WaitStateChangeTasks();
			if (isDisposed)
				return;
			TunnelException tEx = null;
			try { downstream.Dispose(); }
			catch (TunnelException ex) { tEx = ex; }
			SwitchToOfflineTask(tEx);
			WaitStateChangeTasks();
			isDisposed = true;
			evCtl.Dispose();
			writeLock.Release();
			writeLock.Dispose();
			readLock.Release();
			readLock.Dispose();
		}
	}
}
