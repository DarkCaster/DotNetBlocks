// MockServerINode.cs
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
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Server;

namespace Tests.Mocks
{
	public class MockServerINode : INode
	{
		public int initCount = 0;
		public int disposeCount = 0;

		private readonly int minDelay;
		private readonly int maxDelay;
		private readonly float failProbability;
		private readonly float partialOpProbability;
		private readonly int noFailOpsCount;

		private volatile INode downstream;

		public MockServerITunnel LastTunnel { get; set; }

		public MockServerINode(int minDelay, int maxDelay, float failProbability, float partialOpProbability, int noFailOpsCount)
		{
			this.minDelay = minDelay;
			this.maxDelay = maxDelay;
			this.failProbability = failProbability;
			this.partialOpProbability = partialOpProbability;
			this.noFailOpsCount = noFailOpsCount;
			LastTunnel = null;
		}

		public void Init()
		{
			Interlocked.Increment(ref initCount);
		}

		public void RegisterDownstream(INode downstream)
		{
			this.downstream = downstream;
		}

		public Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			throw new NotSupportedException("NOPE");
		}

		public void CreateTunnel()
		{
			LastTunnel = new MockServerITunnel(minDelay, maxDelay, failProbability, partialOpProbability, noFailOpsCount);
			Task.Run(async () => await downstream.OpenTunnelAsync(null, LastTunnel));
		}

		public void Dispose()
		{
			Interlocked.Increment(ref disposeCount);
		}
	}
}
