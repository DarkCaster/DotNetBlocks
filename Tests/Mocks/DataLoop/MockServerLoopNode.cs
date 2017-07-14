// MockServerLoopNode.cs
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
using DarkCaster.DataTransfer.Server;
using DarkCaster.DataTransfer.Config;

namespace Tests.Mocks.DataLoop
{
	public sealed class MockServerLoopNode : INode
	{
		private readonly Random random;
		private readonly AsyncRunner newConnRunner = new AsyncRunner();
		private readonly ITunnelConfigFactory configFactory;
		private readonly INode upstreamNode;
		private volatile INode downstreamNode;

		private readonly int minBlockSize;
		private readonly int maxBlockSize;
		private readonly int readTimeout;
		private readonly float nodeFailProb;
		private int noFailOpsCount;

		private int initCount;
		private int ncCount;
		private int regDsCount;
		private int shutdownCount;
		private int disposeCount;

		public MockServerLoopNode(ITunnelConfig serverConfig, INode upstream, ITunnelConfigFactory configFactory,
															int defaultMinBlockSize = 16, int defaultMaxBlockSize = 4096, int defaultReadTimeout = 5000,
															int defaultNoFailOpsCount = 1, float defaultNodeFailProb = 0.0f)
		{
			minBlockSize = serverConfig.Get<int>("mock_min_block_size");
			maxBlockSize = serverConfig.Get<int>("mock_max_block_size");
			readTimeout = serverConfig.Get<int>("mock_read_timeout");
			nodeFailProb = serverConfig.Get<float>("mock_fail_prob");
			noFailOpsCount = serverConfig.Get<int>("mock_nofail_ops_count");
			if(minBlockSize <= 0)
				minBlockSize = defaultMinBlockSize;
			if(maxBlockSize <= 0)
				maxBlockSize = defaultMaxBlockSize;
			if(readTimeout <= 0)
				readTimeout = defaultReadTimeout;
			if(nodeFailProb <= 0.001f)
				nodeFailProb = defaultNodeFailProb;
			if(noFailOpsCount <= 0)
				noFailOpsCount = defaultNoFailOpsCount;
			this.upstreamNode = upstream;
			this.configFactory = configFactory;
			this.random = new Random();
		}

		public void NewConnection(Storage clientReadStorage, Storage clientWriteStorage)
		{
			Interlocked.Increment(ref ncCount);
			//simulate fail
			if(--noFailOpsCount <= 0 && (float)random.NextDouble() < nodeFailProb)
			{
				newConnRunner.ExecuteTask(() => downstreamNode.NodeFailAsync(new Exception("Expected fail")));
				throw new Exception("Expected fail triggered");
			}
			var tunnel = new MockServerLoopTunnel(minBlockSize, maxBlockSize, clientWriteStorage, readTimeout, clientReadStorage);
			var config = configFactory.CreateNew();
			newConnRunner.ExecuteTask(() => downstreamNode.OpenTunnelAsync(config, tunnel));
		}

		public Task InitAsync()
		{
			Interlocked.Increment(ref initCount);
			return Task.FromResult(true);
		}

		public void RegisterDownstream(INode downstream)
		{
			Interlocked.Increment(ref regDsCount);
			this.downstreamNode = downstream;
		}

		public Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			throw new NotSupportedException("THIS IS A ROOT SERVER NODE");
		}

		public Task NodeFailAsync(Exception ex)
		{
			throw new NotSupportedException("THIS IS A ROOT SERVER NODE");
		}

		public Task ShutdownAsync()
		{
			Interlocked.Increment(ref shutdownCount);
			return Task.FromResult(true);
		}

		public void Dispose()
		{
			Interlocked.Increment(ref disposeCount);
		}

		public int InitCount { get { return Interlocked.CompareExchange(ref initCount, 0, 0); } }
		public int NcCount { get { return Interlocked.CompareExchange(ref ncCount, 0, 0); } }
		public int RegDsCount { get { return Interlocked.CompareExchange(ref regDsCount, 0, 0); } }
		public int ShutdownCount { get { return Interlocked.CompareExchange(ref shutdownCount, 0, 0); } }
		public int DisposeCount { get { return Interlocked.CompareExchange(ref disposeCount, 0, 0); } }
	}
}
