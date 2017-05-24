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
using System.Threading.Tasks;
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

		private void CheckForConnectHandlerExceptions(Type expectedExType)
		{
			var testedEx = connectEx;
			connectEx = null;
			if (testedEx == null && expectedExType != null)
			{
				throw new Exception("connectEx is null, but expected exception of type :" + expectedExType.ToString());
			}
			if (testedEx != null && (expectedExType == null || testedEx.GetType() != expectedExType))
			{
				throw testedEx;
			}
		}

		private void ResetConnectHandler()
		{
			connectState = TunnelState.Init;
			connectEx = null;
			connectDone = false;
		}

		[Test]
		public void NewTunnel()
		{
			Interlocked.Exchange(ref connectStateInvCount, 0);
			ResetConnectHandler();

			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			tunnel.StateChangeEvent.Subscribe(NewTunnelConnectHandler);
			Assert.DoesNotThrow(tunnel.Connect);

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Online, connectState);

			Assert.DoesNotThrow(tunnel.Disconnect);
			Assert.DoesNotThrow(tunnel.Disconnect); //multiple disconnect calls should be handled

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Offline, connectState);

			Assert.DoesNotThrow(tunnel.Dispose);
			Assert.DoesNotThrow(tunnel.Dispose); //multiple dispose calls should be handled

			Assert.AreEqual(2, Interlocked.CompareExchange(ref connectStateInvCount, 0, 0));

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1,dbgTunnel.DisconectCount);
		}

		[Test]
		public void TunnelDispose()
		{
			Interlocked.Exchange(ref connectStateInvCount, 0);
			ResetConnectHandler();

			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			tunnel.StateChangeEvent.Subscribe(NewTunnelConnectHandler);
			Assert.DoesNotThrow(tunnel.Connect);

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Online, connectState);

			Assert.DoesNotThrow(tunnel.Dispose);
			Assert.DoesNotThrow(tunnel.Dispose); //multiple dispose calls should be handled

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Offline, connectState);

			Assert.AreEqual(2, Interlocked.CompareExchange(ref connectStateInvCount, 0, 0));

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisconectCount);

			int timelimit = 5000;
			while (dbgTunnel.DisposeCount==0)
			{
				if (timelimit <= 0)
					throw new Exception("dbgTunnel.DisposeCount!=0 timed out!");
				Thread.Sleep(250);
				timelimit -= 250;
			}
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
		}

		private volatile Exception readEx=null;
		private volatile Exception writeEx=null;
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

		private void ReadTask(IEntryTunnel tunnel)
		{
			try
			{
				while(true)
				{
					tunnel.ReadData(4096, rwBuffer, 0);
					Interlocked.Increment(ref readOps);
				}
			}
			catch(Exception ex)
			{
				readEx = ex;
			}
			readDone = true;
		}

		private void WriteTask(IEntryTunnel tunnel)
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

		private async Task ReadTaskAsync(IEntryTunnel tunnel)
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

		private async Task WriteTaskAsync(IEntryTunnel tunnel)
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
			Interlocked.Exchange(ref connectStateInvCount, 0);
			ResetConnectHandler();
			ResetReadWriteTasks();

			var mockNode = new MockClientINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);

			tunnel.StateChangeEvent.Subscribe(NewTunnelConnectHandler);
			Assert.DoesNotThrow(tunnel.Connect);

			Task.Run(()=>ReadTask(tunnel));
			Task.Run(()=>WriteTask(tunnel));

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Online, connectState);

			//after a while underlying mock itunnel will fail, and entry tunnel will be switched to online

			WaitForConnectHandlerFired(10000);
			CheckForConnectHandlerExceptions(typeof(MockClientITunnel.MockClientException));
			Assert.AreEqual(TunnelState.Offline, connectState);

			Assert.DoesNotThrow(tunnel.Dispose);

			Assert.AreEqual(2, Interlocked.CompareExchange(ref connectStateInvCount, 0, 0));
			Assert.AreEqual(true, readDone);
			Assert.AreEqual(true, writeDone);
			if (readEx.GetType() != typeof(TunnelEofException) && readEx.GetType() != typeof(MockClientITunnel.MockClientException))
				Assert.Fail("readEx is an unexpected exception: " + readEx);
			if (writeEx.GetType() != typeof(TunnelEofException) && writeEx.GetType() != typeof(MockClientITunnel.MockClientException))
				Assert.Fail("readEx is an unexpected exception: " + writeEx);
			Assert.Greater(Interlocked.CompareExchange(ref readOps, 0, 0),0);
			Assert.Greater(Interlocked.CompareExchange(ref writeOps, 0, 0),0);

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisconectCount);

			int timelimit = 5000;
			while (dbgTunnel.DisposeCount == 0)
			{
				if (timelimit <= 0)
					throw new Exception("dbgTunnel.DisposeCount!=0 timed out!");
				Thread.Sleep(250);
				timelimit -= 250;
			}
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadCount,Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteCount,Interlocked.CompareExchange(ref writeOps, 0, 0));
		}

		[Test]
		public void TunnelReadWriteAsync()
		{
			Interlocked.Exchange(ref connectStateInvCount, 0);
			ResetConnectHandler();
			ResetReadWriteTasks();

			var mockNode = new MockClientINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);

			tunnel.StateChangeEvent.Subscribe(NewTunnelConnectHandler);
			Assert.DoesNotThrow(tunnel.Connect);

			Task.Run(async () => await ReadTaskAsync(tunnel));
			Task.Run(async () => await WriteTaskAsync(tunnel));

			WaitForConnectHandlerFired(2000);
			CheckForConnectHandlerExceptions(null);
			Assert.AreEqual(TunnelState.Online, connectState);

			//after a while underlying mock itunnel will fail, and entry tunnel will be switched to online

			WaitForConnectHandlerFired(10000);
			CheckForConnectHandlerExceptions(typeof(MockClientITunnel.MockClientException));
			Assert.AreEqual(TunnelState.Offline, connectState);

			Assert.DoesNotThrow(tunnel.Dispose);

			Assert.AreEqual(2, Interlocked.CompareExchange(ref connectStateInvCount, 0, 0));
			Assert.AreEqual(true, readDone);
			Assert.AreEqual(true, writeDone);
			if (readEx.GetType() != typeof(TunnelEofException) && readEx.GetType() != typeof(MockClientITunnel.MockClientException))
				Assert.Fail("readEx is an unexpected exception: " + readEx);
			if (writeEx.GetType() != typeof(TunnelEofException) && writeEx.GetType() != typeof(MockClientITunnel.MockClientException))
				Assert.Fail("readEx is an unexpected exception: " + writeEx);
			Assert.Greater(Interlocked.CompareExchange(ref readOps, 0, 0), 0);
			Assert.Greater(Interlocked.CompareExchange(ref writeOps, 0, 0), 0);

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisconectCount);

			int timelimit = 5000;
			while (dbgTunnel.DisposeCount == 0)
			{
				if (timelimit <= 0)
					throw new Exception("dbgTunnel.DisposeCount!=0 timed out!");
				Thread.Sleep(250);
				timelimit -= 250;
			}
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadAsyncCount, Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteAsyncCount, Interlocked.CompareExchange(ref writeOps, 0, 0));
		}
	}
}
