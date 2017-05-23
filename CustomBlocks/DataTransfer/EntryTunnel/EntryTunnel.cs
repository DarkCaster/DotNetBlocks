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
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Client
{
	public sealed class EntryTunnel : IEntryTunnel
	{
		private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);
		private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
		private readonly ISafeEventCtrl<TunnelStateEventArgs> evCtl;
		private readonly ISafeEvent<TunnelStateEventArgs> ev;
		private readonly INode downstreamNode;
		private readonly ITunnelConfig config;

		private volatile TunnelState state = TunnelState.Init;
		private ITunnel downstream;
		private int isDisposed = 0;
		private int stateChangeWorkers = 0;

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

		public EntryTunnel(INode downstreamNode, ITunnelConfig config) : this()
		{
			this.downstreamNode = downstreamNode;
			this.config = config;
		}

		public TunnelState State { get { return state; } }

		public void Connect()
		{
			Interlocked.Increment(ref stateChangeWorkers);
			writeLock.Wait();
			readLock.Wait();
			Task.Run(() =>
			{
				var pendingState = TunnelState.Online;
				Exception tEx = null;
				try
				{
					//should be written and read later without issues,
					//because access to this field is synchronized with readLock and writeLock
					downstream=downstreamNode.OpenTunnel(config);
				}
				catch (Exception ex)
				{
					tEx = ex;
				}
				finally
				{
					evCtl.Raise(this, new TunnelStateEventArgs(pendingState, tEx), () => {
						state = pendingState;
						writeLock.Release();
						readLock.Release();
					});
					Interlocked.Decrement(ref stateChangeWorkers);
				}
			});
		}

		public ISafeEvent<TunnelStateEventArgs> StateChangeEvent { get { return ev; } }

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			readLock.Wait();
			try
			{
				return downstream.ReadData(sz, buffer, offset);
			}
			catch (Exception ex)
			{
				if (state != TunnelState.Online)
					throw new TunnelEofException(ex);
				CommitDisconnect(ex, false);
				throw;
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
					throw new TunnelEofException();
				return downstream.WriteData(sz, buffer, offset);
			}
			catch (TunnelEofException)
			{
				throw;
			}
			catch (Exception ex)
			{
				CommitDisconnect(ex, false);
				throw;
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
				return await downstream.ReadDataAsync(sz, buffer, offset);
			}
			catch (Exception ex)
			{
				if (state != TunnelState.Online)
					throw new TunnelEofException(ex);
				CommitDisconnect(ex, false);
				throw;
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
					throw new TunnelEofException();
				return await downstream.WriteDataAsync(sz, buffer, offset);
			}
			catch (TunnelEofException)
			{
				throw;
			}
			catch (Exception ex)
			{
				CommitDisconnect(ex, false);
				throw;
			}
			finally
			{
				writeLock.Release();
			}
		}

		private void CommitDisconnect(Exception iEx, bool releaseLocks)
		{
			Interlocked.Increment(ref stateChangeWorkers);
			Task.Run(() =>
			{
				try
				{
					evCtl.Raise(this, () =>
					{
						try
						{
							if (state == TunnelState.Offline)
								throw new OfflineSwitchException();
							try { downstream.Disconnect(); }
							catch (Exception ex) { if (iEx == null) iEx = ex; }
							state = TunnelState.Offline;
							return new TunnelStateEventArgs(TunnelState.Offline, iEx);
						}
						finally
						{
							if (releaseLocks)
							{
								// we need this to allow user's event handlers to call read\write methods
								// if we blocked it before raising event
								readLock.Release();
								writeLock.Release();
							}
						}
					});
				}
				catch (OfflineSwitchException) {}
				Interlocked.Decrement(ref stateChangeWorkers);
			});
		}

		public void Disconnect()
		{
			writeLock.Wait();
			readLock.Wait();
			CommitDisconnect(null, true);
		}

		public async Task DisconnectAsync()
		{
			await writeLock.WaitAsync();
			await readLock.WaitAsync();
			CommitDisconnect(null, true);
		}

		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref isDisposed, 1, 0) == 1)
				return;
			Task.Run(async () =>
			{
				//perform disconnect
				await DisconnectAsync();
				//wait for all event handlers is complete,
				//so it is still possible to read remaining data from disconnect event handler, and finish all your data transfer stuff.
				int sleep = 1;
				const int max_sleep = 50;
				while (Interlocked.CompareExchange(ref stateChangeWorkers, 0, 0) > 0)
				{
					await Task.Delay(sleep);
					sleep *= 2;
					if (sleep > max_sleep)
						sleep = max_sleep;
				}
				//actually disposing entry-tunnel's internal stuff.
				//if you still need to use object after disconnect, do not use dispose before all data read\write work is complete. 
				downstream.Dispose();
				evCtl.Dispose();
				writeLock.Dispose();
				readLock.Dispose();
			});
		}
	}
}
