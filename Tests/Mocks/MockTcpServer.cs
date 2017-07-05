// MockTcpServer.cs
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
using System.Net;
using System.Net.Sockets;

namespace Tests.Mocks
{
	public class MockTcpServer : IDisposable
	{
		private readonly int port;
		public readonly Socket socket;
		public volatile Socket connection;
		public volatile Task server;
		private volatile bool isDisposed = false;

		public void RunServer()
		{
			server=Task.Run(() => ServerTask());
			var result = false;
			while(!result)
			{
				try
				{
					//open tcp connection
					var addr = Dns.GetHostEntry("localhost").AddressList[0];
					var client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					client.Connect(new IPEndPoint(addr, port));
					client.Dispose();
				}
				catch
				{
					continue;
				}
				result = true;
			}
		}

		private void ServerTask()
		{
			socket.Listen(1);
			while(!isDisposed)
				connection = socket.Accept();
		}

		public MockTcpServer(int port)
		{
			this.port = port;
			IPHostEntry ipHost = Dns.GetHostEntry("localhost");
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
			socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(ipEndPoint);
		}

		public void Dispose()
		{
			isDisposed = true;
			connection.Close();
			connection.Dispose();
			socket.Close();
			socket.Dispose();
		}
	}
}
