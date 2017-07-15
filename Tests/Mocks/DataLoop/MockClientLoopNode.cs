// MockClientLoopNode.cs
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
using DarkCaster.DataTransfer.Client;
using DarkCaster.DataTransfer.Config;

namespace Tests.Mocks.DataLoop
{
	public sealed class MockClientLoopNode : INode
	{
		private volatile MockServerLoopNode entry;
		private readonly int defaultMinBlockSize;
		private readonly int defaultMaxBlockSize;
		private readonly int defaultReadTimeout;
		private readonly int defaultNoFailOpsCount;
		private readonly float defaultNodeFailProb;

		public MockClientLoopNode(int defaultMinBlockSize = 16, int defaultMaxBlockSize = 4096, int defaultReadTimeout = 5000, int defaultNoFailOpsCount = 1, float defaultNodeFailProb = 0.0f)
		{
			this.defaultMinBlockSize = defaultMinBlockSize;
			this.defaultMaxBlockSize = defaultMaxBlockSize;
			this.defaultReadTimeout = defaultReadTimeout;
			this.defaultNoFailOpsCount = defaultNoFailOpsCount;
			this.defaultNodeFailProb = defaultNodeFailProb;
			entry = null;
		}

		public void SetServerEntry(MockServerLoopNode serverLoopEntry)
		{
			entry = serverLoopEntry;
		}

		public Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			int minBlockSize = config.Get<int>("mock_min_block_size");
			int maxBlockSize = config.Get<int>("mock_max_block_size");
			int readTimeout = config.Get<int>("mock_read_timeout");
			float nodeFailProb = config.Get<float>("mock_fail_prob");
			int noFailOpsCount = config.Get<int>("mock_nofail_ops_count");

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
			
			var clientReadStg = new Storage();
			var clientWriteStg = new Storage();
			//initiate new connection for server node, and set-up shared storage
			entry.NewConnection(clientReadStg, clientWriteStg);
			//create new tunnel connection, with shared storage
			return Task.FromResult((ITunnel)new MockClientLoopTunnel(minBlockSize, maxBlockSize, clientReadStg, readTimeout, clientWriteStg, noFailOpsCount, nodeFailProb));
		}
	}
}
