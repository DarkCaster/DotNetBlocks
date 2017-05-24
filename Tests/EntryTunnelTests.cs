// EntryTunnelTests.cs
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
using NUnit.Framework;
using System.Threading;
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Client;
using Tests.Mocks;

namespace Tests
{
	[TestFixture]
	public class EntryTunnelTests
	{
		[Test]
		public void NewNode()
		{
			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			Assert.DoesNotThrow(() => new EntryNode(mockNode));
		}

		private int connectStateInvCount = 0;
		private volatile TunnelState connectState = TunnelState.Init;
		private volatile Exception connectEx = null;
		private volatile bool connectDone = false;

		private void NewTunnelConnectHandler (object sender, TunnelStateEventArgs args)
		{
			Interlocked.Increment(ref connectStateInvCount);
			connectState = args.State;
			connectEx = args.Ex;
			connectDone = true;
		}

		[Test]
		public void NewTunnel()
		{
			Interlocked.Exchange(ref connectStateInvCount, 0);
			connectState = TunnelState.Init;
			connectEx = null;
			connectDone = false;

			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			tunnel.StateChangeEvent.Subscribe(NewTunnelConnectHandler);
			Assert.DoesNotThrow(tunnel.Connect);

			while(!connectDone) {}
			Assert.AreEqual(TunnelState.Online, connectState);
			if (connectEx != null)
				throw connectEx;

			connectEx = null;
			connectDone = false;

			Assert.DoesNotThrow(tunnel.Disconnect);
			Assert.DoesNotThrow(tunnel.Disconnect); //multiple disconnect calls should be handled

			while (!connectDone) { }
			Assert.AreEqual(TunnelState.Offline, connectState);
			if (connectEx != null)
				throw connectEx;

			Assert.DoesNotThrow(tunnel.Dispose);
			Assert.DoesNotThrow(tunnel.Dispose); //multiple dispose calls should be handled

			Assert.AreEqual(2, Interlocked.CompareExchange(ref connectStateInvCount, 0, 0));

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1,dbgTunnel.DisconectCount);
			Assert.AreEqual(1,dbgTunnel.DisposeCount);
		}
	}
}
