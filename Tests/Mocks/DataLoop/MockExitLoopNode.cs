// MockExitLoopNode.cs
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Server;
using DarkCaster.DataTransfer.Config;

namespace Tests.Mocks.DataLoop
{
	//simple exit-tunnel for server logic.
	//incoming connections can be collected by using IncomingTunnel and IncomingConfig properties
	public sealed class MockExitLoopNode : INode
	{
		private readonly ConcurrentQueue<ITunnel> incomingTunnels = new ConcurrentQueue<ITunnel>();
		public ITunnel IncomingTunnel
		{
			get
			{
				if(incomingTunnels.TryDequeue(out var result))
					return result;
				return null;
			}
		}

		private readonly ConcurrentQueue<ITunnelConfig> incomingConfigs = new ConcurrentQueue<ITunnelConfig>();
		public ITunnelConfig IncomingConfig
		{
			get
			{
				if(incomingConfigs.TryDequeue(out var result))
					return result;
				return null;
			}
		}

		public async Task WaitForNewConnectionAsync(int timeout)
		{
			while((incomingTunnels.IsEmpty || incomingConfigs.IsEmpty) && timeout > 0)
			{
				await Task.Delay(10);
				timeout -= 10;
			}
			if(timeout <= 0)
				throw new Exception("WaitForNewConnectionAsync failed");
		}

		private readonly INode upstreamNode;
		private volatile INode downstreamNode;

		public MockExitLoopNode(INode upstream)
		{
			upstreamNode = upstream;
			downstreamNode = null;
			upstreamNode.RegisterDownstream(this);
		}

		public async Task InitAsync()
		{
			await upstreamNode.InitAsync();
		}

		//optional, if using ExitNode ontop of this
		public void RegisterDownstream(INode downstream)
		{
			downstreamNode = downstream;
		}

		public async Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			if(downstreamNode==null)
			{
				incomingConfigs.Enqueue(config);
				incomingTunnels.Enqueue(upstream);
				return;
			}
			else
			{
				await downstreamNode.OpenTunnelAsync(config, upstream);
			}
		}

		public async Task NodeFailAsync(Exception ex)
		{
			await downstreamNode.NodeFailAsync(ex);
		}

		public async Task ShutdownAsync()
		{
			await upstreamNode.ShutdownAsync();
		}

		public void Dispose()
		{
			upstreamNode.Dispose();
		}
	}
}
