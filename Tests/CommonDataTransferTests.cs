// CommonDataTransferTests.cs
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
using DarkCaster.DataTransfer.Config;
using Tests.Mocks.DataLoop;
using NUnit.Framework;

using STunnel = DarkCaster.DataTransfer.Server.ITunnel;
using SNode = DarkCaster.DataTransfer.Server.INode;
using CTunnel = DarkCaster.DataTransfer.Client.ITunnel;
using CNode = DarkCaster.DataTransfer.Client.INode;

namespace Tests
{
	public static class CommonDataTransferTests
	{
		public static void NewConnection(ITunnelConfig clTunConfig, CNode clientNode, MockClientLoopNode clientLoopMock,  SNode serverNode, MockServerLoopNode serverLoopMock)
		{
			clTunConfig.Set("mock_nofail_ops_count", 1);
			clTunConfig.Set("mock_fail_prob", 0.001f);
			clientLoopMock.SetServerEntry(serverLoopMock);
			var serverMockExit = new MockExitLoopNode(serverNode);
			Assert.AreEqual(1, serverLoopMock.RegDsCount);
			Assert.AreEqual(0, serverLoopMock.InitCount);
			var runner = new AsyncRunner();
			runner.ExecuteTask(serverMockExit.InitAsync);
			Assert.AreEqual(1, serverLoopMock.InitCount);
			Assert.IsNull(serverMockExit.IncomingConfig);
			Assert.IsNull(serverMockExit.IncomingTunnel);
			//create new connection(s)
			var cnt = 50;
			var clTuns = new CTunnel[cnt];
			for(int i = 0; i < cnt;++i)
			{
				clTuns[i] = runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig));
				runner.ExecuteTask(() => serverMockExit.WaitForNewConnectionAsync(5000));
				var svTun = serverMockExit.IncomingTunnel;
				var svCfg = serverMockExit.IncomingConfig;
				Assert.NotNull(svTun);
				Assert.NotNull(svCfg);
			}
			Assert.AreEqual(cnt, serverLoopMock.NcCount);
			Assert.AreEqual(0, serverLoopMock.ShutdownCount);
			Assert.AreEqual(0, serverLoopMock.DisposeCount);
			//simulate server shutdown
			runner.ExecuteTask(serverMockExit.ShutdownAsync);
			serverMockExit.Dispose();
			Assert.AreEqual(1, serverLoopMock.ShutdownCount);
			Assert.AreEqual(1, serverLoopMock.DisposeCount);
			//try to open new connection again
			Assert.Throws(typeof(MockLoopException), () => runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig)));
			Assert.IsNull(serverMockExit.IncomingConfig);
			Assert.IsNull(serverMockExit.IncomingTunnel);
		}

		private static void GenerateHighComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if(length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var maxLen = 32;
			if(maxLen > length / 2)
				maxLen = length / 2;
			if(maxLen < 8)
				maxLen = 8;
			var random = new Random();
			while(offset < limit)
			{
				byte val = (byte)random.Next(0, 256);
				int len = (byte)random.Next(8, maxLen);
				//copy data chunk to buffer
				for(int i = 0; i < len; ++i)
				{
					buffer[offset++] = val;
					if(offset >= limit)
						break;
				}
			}
		}

		public static void ReadWrite(ITunnelConfig clTunConfig, CNode clientNode, MockClientLoopNode clientLoopMock, SNode serverNode, MockServerLoopNode serverLoopMock)
		{
			clientLoopMock.SetServerEntry(serverLoopMock);
			var serverMockExit = new MockExitLoopNode(serverNode);
			Assert.AreEqual(1, serverLoopMock.RegDsCount);
			Assert.AreEqual(0, serverLoopMock.InitCount);
			var runner = new AsyncRunner();
			runner.ExecuteTask(serverMockExit.InitAsync);
			Assert.AreEqual(1, serverLoopMock.InitCount);
			Assert.IsNull(serverMockExit.IncomingConfig);
			Assert.IsNull(serverMockExit.IncomingTunnel);
			var clTun = runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig));
			runner.ExecuteTask(() => serverMockExit.WaitForNewConnectionAsync(5000));
			var svTun = serverMockExit.IncomingTunnel;
			var svCfg = serverMockExit.IncomingConfig;
			Assert.NotNull(svTun);
			Assert.NotNull(svCfg);
			Assert.AreEqual(1, serverLoopMock.NcCount);
			Assert.AreEqual(0, serverLoopMock.ShutdownCount);
			Assert.AreEqual(0, serverLoopMock.DisposeCount);
			var sourceData = new byte[262144];
			//try writing data to client tunnel
			GenerateHighComprData(sourceData);
			//write source data
			int pos = 0;
			while(pos < sourceData.Length)
				pos += runner.ExecuteTask(() => clTun.WriteDataAsync(sourceData.Length - pos, sourceData, pos));
			//read from the other end
			var countolData=new byte[262144];
			pos = 0;
			while(pos < countolData.Length)
				pos += runner.ExecuteTask(() => svTun.ReadDataAsync(countolData.Length - pos, countolData, pos));
			Assert.AreEqual(sourceData, countolData);
			//try writing data to server tunnel
			GenerateHighComprData(sourceData);
			//write source data to server tunnel
			pos = 0;
			while(pos < sourceData.Length)
				pos += runner.ExecuteTask(() => svTun.WriteDataAsync(sourceData.Length - pos, sourceData, pos));
			//read from the other end
			pos = 0;
			while(pos < countolData.Length)
				pos += runner.ExecuteTask(() => clTun.ReadDataAsync(countolData.Length - pos, countolData, pos));
			Assert.AreEqual(sourceData, countolData);
			//simulate server shutdown
			runner.ExecuteTask(serverMockExit.ShutdownAsync);
			serverMockExit.Dispose();
			Assert.AreEqual(1, serverLoopMock.ShutdownCount);
			Assert.AreEqual(1, serverLoopMock.DisposeCount);
			//try to open new connection again
			Assert.Throws(typeof(MockLoopException), () => runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig)));
			Assert.IsNull(serverMockExit.IncomingConfig);
			Assert.IsNull(serverMockExit.IncomingTunnel);
		}

		private delegate Task<int> ReadDataAsyncDelegate(int sz, byte[] buffer, int offset);
		private delegate Task<int> WriteDataAsyncDelegate(int sz, byte[] buffer, int offset);
		private static volatile bool start;

		private static async Task<Exception> ReadWorker(ReadDataAsyncDelegate readDelegate, byte[] controlData)
		{
			var testData = new byte[controlData.Length];
			var random = new Random();
			while(true)
			{
				if(!start)
				{
					await Task.Delay(10);
					continue;
				}
				random.NextBytes(testData);
				try
				{
					int pos = 0;
					while(pos < testData.Length)
						pos += await readDelegate(testData.Length - pos, testData, pos);
				}
				//expected failure
				catch(MockLoopException ex)
				{
					return ex;
				}
				for(int i = 0; i < testData.Length; ++i)
					if(testData[i] != controlData[i])
						throw new Exception("Data verification failed");
			}
		}

		private static async Task<Exception> WriteWorker(WriteDataAsyncDelegate writeDelegate, byte[] sourceData)
		{
			while(true)
			{
				if(!start)
				{
					await Task.Delay(10);
					continue;
				}
				try
				{
					int pos = 0;
					while(pos < sourceData.Length)
						pos += await writeDelegate(sourceData.Length - pos, sourceData, pos);
				}
				//expected failure
				catch(MockLoopException ex)
				{
					return ex;
				}
			}
		}

		private static void Reset()
		{
			start = false;
		}

		private static void Start()
		{
			start = true;
		}
		public static void MultithreadedReadWrite(ITunnelConfig clTunConfig, CNode clientNode, MockClientLoopNode clientLoopMock, SNode serverNode, MockServerLoopNode serverLoopMock)
		{
		}
	}
}
