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
using System.Collections.Generic;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Server.Tcp
{
	public sealed class TcpServerNode : INode
	{
		private readonly CancellationTokenSource tcs = new CancellationTokenSource();
		private readonly int port;
		private readonly string[] bindings;
		private readonly bool nodelay;
		private readonly int bufferSize;

		// We do not need any additional synchronization here,
		// because ExitNode from the user's side provide all needed synchronization and thread safety
		// when running InitAsync, ShutdownAsync and Dispose methods.
		private List<Task> listeners = new List<Task>();
		private List<Socket> sockets = new List<Socket>();

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
				bindings = new string[1];
				bindings[0] = "any_ip4";
			}
			for(int i = 0; i < bindings.Length; ++i)
				bindings[i] = bindings[i].ToLower();
			nodelay = serverConfig.Get<bool>("tcp_nodelay");
			bufferSize = serverConfig.Get<int>("tcp_buffer_size");
		}

		public async Task InitAsync()
		{
			try
			{
				for(int i = 0; i < bindings.Length; ++i)
				{
					IPAddress[] addrs = null;
					if(bindings[i] == "any_ip4")
						addrs = new IPAddress[] { IPAddress.Any };
					else if(bindings[i] == "any_ip6")
						addrs = new IPAddress[] { IPAddress.IPv6Any };
					else if(IPAddress.TryParse(bindings[i], out IPAddress addr))
						addrs = new IPAddress[] { addr };
					else
					{
						addrs = await Dns.GetHostAddressesAsync(bindings[i]);
						if(addrs == null || addrs.Length==0)
							throw new Exception("Cannot resolve ip address for host: " + bindings[i]);
					}
					foreach(var addr in addrs)
					{
						var ep = new IPEndPoint(addr, port);
						var socket=new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
						socket.Bind(ep);
						socket.Listen(10);
						sockets.Add(socket);
						listeners.Add(Task.Run(() => ListenerWorker(socket, tcs.Token)));
					}
				}
				if(listeners.Count == 0)
					throw new Exception("No listeners started!");
			}
			catch(Exception ex)
			{
				await downstream.NodeFailAsync(ex);
			}
		}

		private async Task ListenerWorker( Socket listener, CancellationToken token )
		{
			try
			{
				while(!token.IsCancellationRequested)
				{
					//create new connection-socket
					var tSocket = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null).ConfigureAwait(false);
					tSocket.NoDelay = nodelay;
					if(bufferSize != 0)
					{
						tSocket.ReceiveBufferSize = bufferSize;
						tSocket.SendBufferSize = bufferSize;
					}
					//create tunnel config, and set connection info that will be forwarded to user's code
					var config = new TunnelConfig();
					config.Set("tcp_nodelay", nodelay);
					config.Set("tcp_buffer_size", bufferSize);
					var localEp = (IPEndPoint)tSocket.LocalEndPoint;
					var remoteEp = (IPEndPoint)tSocket.RemoteEndPoint;
					config.Set("local_port", localEp.Port);
					config.Set("local_host", localEp.Address.ToString());
					config.Set("local_addr", localEp.Address.GetAddressBytes());
					config.Set("remote_port", remoteEp.Port);
					config.Set("remote_host", remoteEp.Address.ToString());
					config.Set("remote_addr", remoteEp.Address.GetAddressBytes());
					//create new tunnel
					var tunnel = new TcpServerTunnel(tSocket);
					//call downstream's OpenTunnelAsync
					await downstream.OpenTunnelAsync(config, tunnel);
				}
				token.ThrowIfCancellationRequested();
			}
			catch(OperationCanceledException)
			{
				throw;
			}
			catch(Exception ex)
			{
				if(token.IsCancellationRequested)
					throw new OperationCanceledException(token);
				await downstream.NodeFailAsync(ex);
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

		public async Task ShutdownAsync()
		{
			tcs.Cancel();
			foreach(var socket in sockets)
				socket.Close();
			try { await Task.WhenAll(listeners); }
			catch {}
			foreach(var task in listeners)
				if(task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled && task.Status != TaskStatus.Faulted)
					throw new Exception("Some listener tasks still running!");
		}

		public void Dispose()
		{
			//do not check anything, because all operations must be already stopped by ExitTunnel from user's side.
			foreach(var socket in sockets)
				socket.Dispose();
			foreach(var task in listeners)
				task.Dispose();
		}
	}
}
