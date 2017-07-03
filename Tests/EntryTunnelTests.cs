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

		[Test]
		public void NewTunnel()
		{
			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			var state = TunnelState.Init;
			Assert.AreEqual(TunnelState.Init, tunnel.State);
			Assert.DoesNotThrow(() => { state = tunnel.Connect(); });
			Assert.AreEqual(TunnelState.Online, state);
			Assert.AreEqual(TunnelState.Online, tunnel.State);
			Assert.DoesNotThrow(tunnel.Disconnect);
			Assert.DoesNotThrow(tunnel.Disconnect); //multiple disconnect calls should be handled
			Assert.AreEqual(TunnelState.Offline, tunnel.State);
			Assert.DoesNotThrow(tunnel.Dispose);
			Assert.DoesNotThrow(tunnel.Dispose); //multiple dispose calls should be handled
			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisconectCount);
		}

		[Test]
		public void TunnelDispose()
		{
			var mockNode = new MockClientINode(0, 0, 0, 0, 10);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			var state = TunnelState.Init;
			Assert.AreEqual(TunnelState.Init, tunnel.State);
			Assert.DoesNotThrow(() => { state = tunnel.Connect(); });
			Assert.AreEqual(TunnelState.Online, state);
			Assert.AreEqual(TunnelState.Online, tunnel.State);
			Assert.DoesNotThrow(tunnel.Dispose);
			Assert.DoesNotThrow(tunnel.Dispose); //multiple dispose calls should be handled
			Assert.AreEqual(TunnelState.Offline, tunnel.State);

			var dbgTunnel = mockNode.LastTunnel;
			Assert.NotNull(dbgTunnel);
			Assert.AreEqual(1, dbgTunnel.DisconectCount);

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

		private void ReadTask(IEntryTunnelLite tunnel)
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
				if(ex.GetType() != typeof(TunnelEofException))
					tunnel.Disconnect();
				readEx = ex;
			}
			readDone = true;
		}

		private void WriteTask(IEntryTunnelLite tunnel)
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
				if(ex.GetType() != typeof(TunnelEofException))
					tunnel.Disconnect();
				writeEx = ex;
			}
			writeDone = true;
		}

		private async Task ReadTaskAsync(IEntryTunnelLite tunnel)
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
				if(ex.GetType() != typeof(TunnelEofException))
					tunnel.Disconnect();
				readEx = ex;
			}
			readDone = true;
		}

		private async Task WriteTaskAsync(IEntryTunnelLite tunnel)
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
			ResetReadWriteTasks();

			var mockNode = new MockClientINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			var state = TunnelState.Init;
			Assert.AreEqual(TunnelState.Init,tunnel.State);
			Assert.DoesNotThrow(() => { state = tunnel.Connect(); });
			Assert.AreEqual(TunnelState.Online, tunnel.State);

			var read=Task.Run(()=>ReadTask(tunnel));
			var write=Task.Run(()=>WriteTask(tunnel));


			//after a while underlying mock itunnel will fail, so read and write workers will also fail
			read.Wait(5000);
			Assert.AreEqual(TaskStatus.RanToCompletion, read.Status);
			write.Wait(5000);
			Assert.AreEqual(TaskStatus.RanToCompletion, write.Status);

			//failed tunnel will be disconnected by worker after first error
			Assert.AreEqual(TunnelState.Offline, tunnel.State);

			Assert.DoesNotThrow(tunnel.Dispose);

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
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadAsyncCount,Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteAsyncCount,Interlocked.CompareExchange(ref writeOps, 0, 0));
		}

		[Test]
		public void TunnelReadWriteAsync()
		{
			ResetReadWriteTasks();

			var mockNode = new MockClientINode(0, 10, 0.25f, 0.9f, 500);
			var config = new MockITunnelConfigFactory().CreateNew();
			IEntryNode node = null;
			Assert.DoesNotThrow(() => { node = new EntryNode(mockNode); });
			var tunnel = node.OpenTunnel(config);
			var state = TunnelState.Init;
			Assert.AreEqual(TunnelState.Init, tunnel.State);
			Assert.DoesNotThrow(() => { state = tunnel.Connect(); });
			var read=Task.Run(async () => await ReadTaskAsync(tunnel));
			var write=Task.Run(async () => await WriteTaskAsync(tunnel));
			Assert.AreEqual(TunnelState.Online, tunnel.State);

			//after a while underlying mock itunnel will fail, so read and write workers will also fail
			read.Wait(5000);
			Assert.AreEqual(TaskStatus.RanToCompletion, read.Status);
			write.Wait(5000);
			Assert.AreEqual(TaskStatus.RanToCompletion, write.Status);

			//failed tunnel will be disconnected by worker after first error
			Assert.AreEqual(TunnelState.Offline, tunnel.State);

			Assert.DoesNotThrow(tunnel.Dispose);

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
			Assert.AreEqual(1, dbgTunnel.DisposeCount);
			Assert.GreaterOrEqual(dbgTunnel.ReadAsyncCount, Interlocked.CompareExchange(ref readOps, 0, 0));
			Assert.GreaterOrEqual(dbgTunnel.WriteAsyncCount, Interlocked.CompareExchange(ref writeOps, 0, 0));
		}
	}
}
