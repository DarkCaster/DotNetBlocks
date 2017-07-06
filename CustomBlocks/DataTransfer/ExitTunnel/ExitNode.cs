// ExitNode.cs
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
using System.Threading.Tasks;
using DarkCaster.Async;
using DarkCaster.Events;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Server
{
	public sealed class ExitNode : IExitNode
	{
		private bool isDisposed = false;
		private bool isShutdown = false;

		private readonly INode upstreamNode = null;
		private readonly AsyncRWLock upstreamLock = new AsyncRWLock();
		private readonly AsyncRunner asyncRunner = new AsyncRunner();
		private readonly ISafeEventCtrl<NewTunnelEventArgs> evCtl;
		private readonly ISafeEvent<NewTunnelEventArgs> ev;

		public ExitNode(INode upstream)
		{
			this.upstreamNode = upstream;
			upstream.RegisterDownstream(this);
#if DEBUG
			evCtl = new SafeEventDbg<NewTunnelEventArgs>();
			ev = (ISafeEvent<NewTunnelEventArgs>)evCtl;
#else
			evCtl = new SafeEvent<NewTunnelEventArgs>();
			ev = (ISafeEvent<NewTunnelEventArgs>)evCtl;
#endif
		}

		public ISafeEvent<NewTunnelEventArgs> IncomingConnectionEvent { get { return ev; } }

		public void Init()
		{
			upstreamLock.EnterReadLock();
			try
			{
				if (isDisposed || isShutdown)
					return;
				asyncRunner.ExecuteTask(upstreamNode.InitAsync);
			}
			finally { upstreamLock.ExitReadLock(); }
		}

		public async Task InitAsync()
		{
			await upstreamLock.EnterReadLockAsync();
			try
			{
				if(isDisposed || isShutdown)
					return;
				await upstreamNode.InitAsync();
			}
			finally { upstreamLock.ExitReadLock(); }
		}

		public void RegisterDownstream(INode downstream)
		{
			throw new NotSupportedException("ExitNode do not support communication with any downstream INode");
		}

		private void SpawnExitTunnel(ITunnelConfig config, ITunnel upstream)
		{
			var tunnel = new ExitTunnel(upstream);
			Task.Run(() => evCtl.Raise(this, new NewTunnelEventArgs(tunnel, config)));
		}

		public async Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			await upstreamLock.EnterReadLockAsync();
			try
			{
				if (isDisposed)
				{
					upstream.Dispose();
					return;
				}
				SpawnExitTunnel(config, upstream);
			}
			finally
			{
				upstreamLock.ExitReadLock();
			}
		}

		private async Task ShutdownAsyncWorker()
		{
			if(isShutdown)
				return;
			isShutdown = true;
			if(upstreamNode != null)
				await upstreamNode.ShutdownAsync();
		}

		public void Shutdown()
		{
			upstreamLock.EnterWriteLock();
			try
			{
				asyncRunner.ExecuteTask(ShutdownAsyncWorker);
			}
			finally
			{
				upstreamLock.ExitWriteLock();
			}
		}

		public async Task ShutdownAsync()
		{
			await upstreamLock.EnterWriteLockAsync();
			try
			{
				await ShutdownAsyncWorker();
			}
			finally
			{
				upstreamLock.ExitWriteLock();
			}
		}

		public void Dispose()
		{
			upstreamLock.EnterWriteLock();
			try
			{
				if (isDisposed)
					return;
				isDisposed = true;
				asyncRunner.ExecuteTask(ShutdownAsyncWorker);
				if (upstreamNode != null)
					upstreamNode.Dispose();
			}
			finally
			{
				upstreamLock.ExitWriteLock();
			}
		}
	}
}
