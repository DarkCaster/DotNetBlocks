// TcpClientTests.cs
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
using System.Net.Sockets;
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Client.Tcp;
using DarkCaster.Async;
using Tests.Mocks;

namespace Tests
{
	[TestFixture]
	public class TcpClientTests
	{
		[Test]
		public void Connect()
		{
			var port = 55551;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			runner.ExecuteTask(tunnel.DisconnectAsync);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void ReadBlock()
		{
			var port = 55552;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[1024];
			int read = -1;
			runner.AddTask(() => tunnel.ReadDataAsync(1024, buffer, 0), (x) => read = x);
			runner.AddTask(async () => { await Task.Delay(1000); await tunnel.DisconnectAsync(); });
			try
			{
				runner.RunPendingTasks();
			}
			catch(AggregateException ex)
			{
				Assert.AreSame(typeof(SocketException), ex.InnerException.GetType());
			}
			Assert.AreEqual(-1, read);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void ReadLessData()
		{
			var port = 55553;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[1024];
			var sbuffer = new byte[1];
			mock.connection.Send(sbuffer);
			int read = runner.ExecuteTask(() => tunnel.ReadDataAsync(1024, buffer, 0));
			Assert.AreEqual(1, read);
			runner.ExecuteTask(tunnel.DisconnectAsync);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void ReadNull()
		{
			var port = 55554;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[1024];
			int read = runner.ExecuteTask(() => tunnel.ReadDataAsync(0, buffer, 0));
			Assert.AreEqual(0, read);
			runner.ExecuteTask(tunnel.DisconnectAsync);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void WriteNull()
		{
			var port = 55555;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[1024];
			int write = runner.ExecuteTask(() => tunnel.WriteDataAsync(0, buffer, 0));
			Assert.AreEqual(0, write);
			runner.ExecuteTask(tunnel.DisconnectAsync);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void WriteBlock()
		{
			var port = 55556;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[16 * 1024 * 1024];
			int write = -1;
			runner.AddTask(() => tunnel.WriteDataAsync(16 * 1024 * 1024, buffer, 0), (x) => write = x);
			runner.AddTask(async()=> { await Task.Delay(1000); await tunnel.DisconnectAsync(); });
			try
			{
				runner.RunPendingTasks();
			}
			catch(AggregateException ex)
			{
				Assert.AreSame(typeof(SocketException), ex.InnerException.GetType());
			}
			Assert.AreEqual(-1, write);
			tunnel.Dispose();
			mock.Dispose();
		}

		[Test]
		public void WriteData()
		{
			var port = 55557;
			var mock = new MockTcpServer(port);
			mock.RunServer();
			var config = new TunnelConfig();
			config.Set("remote_host", "localhost");
			config.Set("remote_port", port);
			var runner = new AsyncRunner();
			var tunnel = runner.ExecuteTask(async () => { return await new TcpClientNode().OpenTunnelAsync(config); });
			var buffer = new byte[1];
			int write = runner.ExecuteTask(() => tunnel.WriteDataAsync(1, buffer, 0));
			Assert.AreEqual(1, write);
			runner.ExecuteTask(tunnel.DisconnectAsync);
			tunnel.Dispose();
			mock.Dispose();
		}
	}
}
