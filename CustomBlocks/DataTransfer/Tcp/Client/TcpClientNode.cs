// TcpClientNode.cs
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Client.Tcp
{
	public sealed class TcpClientNode : INode
	{
		public async Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			var host = config.Get<string>("remote_host");
			var port = config.Get<int>("remote_port");
			if(string.IsNullOrEmpty(host))
				throw new Exception("failed to get remote connection address from \"remote_host\" config parameter");
			if(port==0)
				throw new Exception("failed to get remote connection port from \"remote_port\" config parameter");
			//open tcp connection
			var addr = Dns.GetHostEntry(host).AddressList[0];
			var nodelay = config.Get<bool>("tcp_nodelay");
			var bufferSize = config.Get<int>("tcp_buffer_size");
			var client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				await Task.Factory.FromAsync(
				(callback, state) => client.BeginConnect(new IPEndPoint(addr, port), callback, state),
				client.EndConnect, null).ConfigureAwait(false);
				//apply some optional settings to socket
				client.NoDelay = nodelay;
				client.LingerState = new LingerOption(true, 0);
				if(bufferSize > 0)
				{
					client.ReceiveBufferSize = bufferSize;
					client.SendBufferSize = bufferSize;
				}
			}
			catch
			{
				client.Dispose();
				throw;
			}
			//create and return new itunnel object with this tcp connection
			return new TcpClientTunnel(client);
		}
	}
}
