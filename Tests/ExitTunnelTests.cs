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
			IExitNode node=null;
			Assert.DoesNotThrow(() => { node = new ExitNode(mockNode); });
			Assert.DoesNotThrow(node.Shutdown);
			Assert.DoesNotThrow(node.Dispose);
			Assert.AreEqual(1,mockNode.disposeCount);
			Assert.AreEqual(1, mockNode.shutdownCount);
		}

		private int incomingEvCount = 0;
		private IExitTunnel incomingTunnel = null;
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
			Assert.AreEqual(1, dbgTunnel.DisconnectCount);
			node.Dispose();
			Assert.AreEqual(1, mockNode.disposeCount);
			Assert.AreEqual(1, mockNode.shutdownCount);
		}

		private volatile Exception readEx = null;
		private volatile Exception writeEx = null;
		private volatile bool readDone = false;
		private volatile bool writeDone = false;
		private int readOps = 0;
		private int writeOps = 0;

		private readonly byte[] rwBuffer = new byte[4096];

		private void ResetReadWriteTasks()
		{
			readEx = null;
			writeEx = null;
			Interlocked.Exchange(ref writeOps, 0);
			Interlocked.Exchange(ref readOps, 0);
			readDone = false;
			writeDone = false;
		}

		private void ReadTask(IExitTunnel tunnel)
		{
			try
			{
				while (true)
				{
					tunnel.ReadData(4096, rwBuffer, 0);
					Interlocked.Increment(ref readOps);
				}
			}
			catch (Exception ex)
			{
				readEx = ex;
			}
			readDone = true;
		}

		private void WriteTask(IExitTunnel tunnel)
		{
			try
			{
				while (true)
				{
					tunnel.WriteData(4096, rwBuffer, 0);
					Interlocked.Increment(ref writeOps);
				}
			}
			catch (Exception ex)
			{
				writeEx = ex;
			}
			writeDone = true;
		}

		private async Task ReadTaskAsync(IExitTunnel tunnel)
		{
			try
			{
				while (true)
				{
					await tunnel.ReadDataAsync(4096, rwBuffer, 0);
					Interlocked.Increment(ref readOps);
				}
			}
			catch (Exception ex)
			{
				readEx = ex;
			}
			readDone = true;
		}

		private async Task WriteTaskAsync(IExitTunnel tunnel)
		{
			try
			{
				while (true)
				{
					await tunnel.WriteDataAsync(4096, rwBuffer, 0);
					Interlocked.Increment(ref writeOps);
				}
			}
			catch (Exception ex)
			{
				writeEx = ex;
			}
			writeDone = true;
		}

		[Test]
		public void TunnelReadWrite()
		{
			Interlocked.Exchange(ref incomingEvCount, 0);
			ResetConnectHandler();
			ResetReadWriteTasks();

			var mockNode = new MockServerINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IExitNode node = null;
			Assert.DoesNotThrow(() => { node = new ExitNode(mockNode); });
			node.IncomingConnectionEvent.Subscribe(NewTunnelHandler);
			mockNode.CreateTunnel();

			WaitForConnectHandlerFired(2000);
			IExitTunnel tunnel = (IExitTunnel)incomingTunnel;

			Task.Run(() => ReadTask(tunnel));
			Task.Run(() => WriteTask(tunnel));

			//after a while underlying mock itunnel will fail
			Thread.Sleep(4000);

			Assert.AreEqual(1, Interlocked.CompareExchange(ref incomingEvCount, 0, 0));
			Assert.AreEqual(true, readDone);
			Assert.AreEqual(true, writeDone);

			Assert.DoesNotThrow(tunnel.Dispose);

			if (readEx.GetType() != typeof(TunnelEofException) && readEx.GetType() != typeof(MockServerITunnel.MockServerException))
				Assert.Fail("readEx is an unexpected exception: " + readEx);
			if (writeEx.GetType() != typeof(TunnelEofException) && writeEx.GetType() != typeof(MockServerITunnel.MockServerException))
				Assert.Fail("readEx is an unexpected exception: " + writeEx);

			Assert.Greater(Interlocked.CompareExchange(ref readOps, 0, 0), 0);
			Assert.Greater(Interlocked.CompareExchange(ref writeOps, 0, 0), 0);

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.AreEqual(1, dbgTunnel.DisconnectCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadAsyncCount, Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteAsyncCount, Interlocked.CompareExchange(ref writeOps, 0, 0));

			node.Dispose();
			Assert.AreEqual(1, mockNode.disposeCount);
			Assert.AreEqual(1, mockNode.shutdownCount);
		}

		[Test]
		public void TunnelReadWriteAsync()
		{
			Interlocked.Exchange(ref incomingEvCount, 0);
			ResetConnectHandler();
			ResetReadWriteTasks();

			var mockNode = new MockServerINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IExitNode node = null;
			Assert.DoesNotThrow(() => { node = new ExitNode(mockNode); });
			node.IncomingConnectionEvent.Subscribe(NewTunnelHandler);
			mockNode.CreateTunnel();

			WaitForConnectHandlerFired(2000);
			IExitTunnel tunnel = (IExitTunnel)incomingTunnel;

			Task.Run(async () => await ReadTaskAsync(tunnel));
			Task.Run(async () => await WriteTaskAsync(tunnel));

			//after a while underlying mock itunnel will fail
			Thread.Sleep(4000);

			Assert.AreEqual(1, Interlocked.CompareExchange(ref incomingEvCount, 0, 0));
			Assert.AreEqual(true, readDone);
			Assert.AreEqual(true, writeDone);
			Assert.DoesNotThrow(tunnel.Dispose);

			if (readEx.GetType() != typeof(TunnelEofException) && readEx.GetType() != typeof(MockServerITunnel.MockServerException))
				Assert.Fail("readEx is an unexpected exception: " + readEx);
			if (writeEx.GetType() != typeof(TunnelEofException) && writeEx.GetType() != typeof(MockServerITunnel.MockServerException))
				Assert.Fail("readEx is an unexpected exception: " + writeEx);

			Assert.Greater(Interlocked.CompareExchange(ref readOps, 0, 0), 0);
			Assert.Greater(Interlocked.CompareExchange(ref writeOps, 0, 0), 0);

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.AreEqual(1, dbgTunnel.DisconnectCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadAsyncCount, Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteAsyncCount, Interlocked.CompareExchange(ref writeOps, 0, 0));

			node.Dispose();
			Assert.AreEqual(1, mockNode.disposeCount);
			Assert.AreEqual(1, mockNode.shutdownCount);
		}
	}
}
