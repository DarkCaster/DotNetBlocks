// TcpServerNode.cs
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
using System.Threading;
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Server.Tcp
{
	public sealed class TcpServerNode : INode
	{
		private int isDisposed = 0;

		private readonly int port;
		private readonly string[] bindings;
		private readonly bool nodelay;
		private readonly int bufferSize;

		private readonly object listenersManageLock = new object();
		private Task[] listeners;

		private volatile INode downstream;

		public TcpServerNode(ITunnelConfig serverConfig)
		{
			port = serverConfig.Get<int>("local_port");
			//get port
			if(port == 0)
				port = 65000;
			//get addreses to bind
			var binding = serverConfig.Get<string>("bind");
			if(!string.IsNullOrEmpty(binding))
				bindings = binding.Split(';');
			else
			{
				bindings = new string[2];
				bindings[0] = "any_ip4";
				bindings[1] = "any_ip6";
			}
			for(int i = 0; i < bindings.Length; ++i)
				bindings[i] = bindings[i].ToLower();
			nodelay = serverConfig.Get<bool>("tcp_nodelay");
			bufferSize = serverConfig.Get<int>("tcp_buffer_size");
			listeners = new Task[bindings.Length];
		}

		public Task InitAsync()
		{
			lock(listenersManageLock)
				for(int i = 0; i < listeners.Length; ++i)
					listeners[i] = Task.Run(() => ListenerWorker(bindings[i]));
			return Task.FromResult(true);
		}

		private async Task ListenerWorker( string binding )
		{
			IPAddress addr = null;
			if(binding == "any_ip4")
				addr = IPAddress.Any;
			else if(!IPAddress.TryParse(binding, out addr))
				addr = (await Dns.GetHostAddressesAsync(binding))[0];
			var listener = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(new IPEndPoint(addr, port));
			listener.Listen(10);
			while( Interlocked.CompareExchange(ref isDisposed, 0, 0) == 0 )
			{
				var tSocket = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null);
				tSocket.NoDelay = nodelay;
				if(bufferSize!=0)
				{
					tSocket.ReceiveBufferSize = bufferSize;
					tSocket.SendBufferSize = bufferSize;
				}
				//TODO: create new tunnel
				//TODO: create itunnel config and populate it with diagnostic stuff like incoming ip address, port, etc
				//TODO: call downstream's OpenTunnelAsync
				throw new NotImplementedException("TODO");
			}
		}

		public void RegisterDownstream(INode downstream)
		{
			this.downstream = downstream;
		}

		public Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			throw new NotSupportedException("TcpServerNode::OpenTunnelAsync cannot be called by upstream, becaue this is a top node");
		}

		public Task NodeFailAsync(Exception ex)
		{
			throw new NotSupportedException("TcpServerNode::NodeFailAsync cannot be called by upstream, becaue this is a top node");
		}

		public Task ShutdownAsync()
		{
			Interlocked.CompareExchange(ref isDisposed, 1, 0);
			lock(listenersManageLock)
				for(int i = 0; i < listeners.Length; ++i)
					//TODO: task cancelation
					throw new NotImplementedException("TODO");
			return Task.FromResult(true);
		}

		public void Dispose()
		{
			//TODO: dispose all listener-sockets;
			throw new NotImplementedException("TODO");
		}
	}
}
