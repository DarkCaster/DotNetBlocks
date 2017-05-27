// ExitTunnelTests.cs
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
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Server;
using Tests.Mocks;

namespace Tests
{
	[TestFixture]
	public class ExitTunnelTests
	{
		[Test]
		public void NewNode()
		{
			var mockNode = new MockServerINode(0, 0, 0, 0, 10);
			Assert.DoesNotThrow(() => new ExitNode(mockNode));
		}

		private int incomingEvCount = 0;
		private ITunnel incomingTunnel = null;
		private volatile bool connectDone = false;

		private void NewTunnelHandler(object sender, NewTunnelEventArgs args)
		{
			Interlocked.Increment(ref incomingEvCount);
			incomingTunnel = args.Tunnel;
			connectDone = true;
		}

		private void WaitForConnectHandlerFired(int timelimit)
		{
			while (!connectDone)
			{
				if (timelimit <= 0)
					throw new Exception("WaitForConnectHandlerFired timed out!");
				Thread.Sleep(250);
				timelimit -= 250;
			}
			connectDone = false;
		}

		private void ResetConnectHandler()
		{
			incomingTunnel = null;
			connectDone = false;
		}

		[Test]
		public void NewTunnel()
		{
			Interlocked.Exchange(ref incomingEvCount, 0);
			ResetConnectHandler();

			var mockNode = new MockServerINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IExitNode node = null;
			Assert.DoesNotThrow(() => { node = new ExitNode(mockNode); });
			node.IncomingConnectionEvent.Subscribe(NewTunnelHandler);
			mockNode.CreateTunnel();

			WaitForConnectHandlerFired(2000);
			Assert.NotNull(incomingTunnel);

			Assert.DoesNotThrow(node.Dispose);
			Assert.DoesNotThrow(node.Dispose); //multiple dispose calls should be handled
			Assert.AreEqual(1, Interlocked.CompareExchange(ref incomingEvCount, 0, 0));
			incomingTunnel.Dispose();
			incomingTunnel.Dispose(); //multiple dispose calls should be handled
			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
		}
	}
}
