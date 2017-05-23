// MockClientINode.cs
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
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Client;

namespace Tests.Mocks
{
	public class MockClientINode : INode
	{
		private readonly int minDelay;
		private readonly int maxDelay;
		private readonly float failProbability;
		private readonly float partialOpProbability;
		private readonly int noFailOpsCounter;

		public MockClientITunnel LastTunnel { get; set; }

		public MockClientINode(int minDelay, int maxDelay, float failProbability, float partialOpProbability, int noFailOpsCount)
		{
			this.minDelay = minDelay;
			this.maxDelay = maxDelay;
			this.failProbability = failProbability;
			this.partialOpProbability = partialOpProbability;
			this.noFailOpsCounter = noFailOpsCount;
			LastTunnel = null;
		}

		public ITunnel OpenTunnel(ITunnelConfig config)
		{
			if (config == null)
				throw new Exception("config parameter cannot be null!");
			LastTunnel=new MockClientITunnel(minDelay, maxDelay, failProbability, partialOpProbability, noFailOpsCounter);
			return LastTunnel;
		}
	}
}
