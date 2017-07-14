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
using DarkCaster.Async;
using DarkCaster.DataTransfer;
using DarkCaster.DataTransfer.Config;
using DarkCaster.Serialization.Binary;
using Tests.Mocks.DataLoop;
using NUnit.Framework;

using SNode = DarkCaster.DataTransfer.Server.INode;
using CNode = DarkCaster.DataTransfer.Client.INode;

namespace Tests
{
	public static class CommonDataTransferTests
	{
		public static void NewConnection(ITunnelConfig clTunConfig, CNode clientNode, MockClientLoopNode clientLoopMock,  SNode serverNode, MockServerLoopNode serverLoopMock)
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
			//create new connection
			var clTun = runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig));
			var svTun = serverMockExit.IncomingTunnel;
			var svCfg = serverMockExit.IncomingConfig;
			Assert.NotNull(svTun);
			Assert.NotNull(svCfg);
			Assert.AreEqual(1, serverLoopMock.NcCount);
			Assert.AreEqual(0, serverLoopMock.ShutdownCount);
			Assert.AreEqual(0, serverLoopMock.DisposeCount);
			//simulate server shutdown
			runner.ExecuteTask(serverMockExit.ShutdownAsync);
			serverMockExit.Dispose();
			Assert.AreEqual(1, serverLoopMock.ShutdownCount);
			Assert.AreEqual(1, serverLoopMock.DisposeCount);
			//try to open new connection again
			Assert.Throws(typeof(Exception), () => runner.ExecuteTask(() => clientNode.OpenTunnelAsync(clTunConfig)));
			Assert.IsNull(serverMockExit.IncomingConfig);
			Assert.IsNull(serverMockExit.IncomingTunnel);
		}

	}
}
