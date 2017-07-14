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

		public MockClientLoopNode(int defaultMinBlockSize = 16, int defaultMaxBlockSize = 4096, int defaultReadTimeout = 5000)
		{
			this.defaultMinBlockSize = defaultMinBlockSize;
			this.defaultMaxBlockSize = defaultMaxBlockSize;
			this.defaultReadTimeout = defaultReadTimeout;
			entry = null;
		}

		public void SetServerEntry(MockServerLoopNode serverLoopEntry)
		{
			entry = serverLoopEntry;
		}

		public async Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			int minBlockSize = config.Get<int>("mock_min_block_size");
			int maxBlockSize = config.Get<int>("mock_max_block_size");
			int readTimeout = config.Get<int>("mock_read_timeout");

			if(minBlockSize <= 0)
				minBlockSize = defaultMinBlockSize;
			if(maxBlockSize <= 0)
				maxBlockSize = defaultMaxBlockSize;
			if(readTimeout <= 0)
				readTimeout = defaultReadTimeout;
			
			var clientReadStg = new Storage();
			var clientWriteStg = new Storage();
			var tunnel = new MockClientLoopTunnel(minBlockSize, maxBlockSize, clientReadStg, readTimeout, clientWriteStg);
			try
			{
				entry.NewConnection(clientReadStg, clientWriteStg);
			}
			catch
			{
				await tunnel.DisconnectAsync();
				tunnel.Dispose();
			}
			return tunnel;
		}
	}
}
