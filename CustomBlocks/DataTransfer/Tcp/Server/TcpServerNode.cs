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
		private readonly CancellationTokenSource tcs = new CancellationTokenSource();
		private readonly int port;
		private readonly string[] bindings;
		private readonly bool nodelay;
		private readonly int bufferSize;

		// We do not need any additional synchronization here,
		// because ExitNode from the user's side provide all needed synchronization and thread safety
		// when running InitAsync, ShutdownAsync and Dispose methods.
		private Task[] listeners;
		private Socket[] sockets;

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
			sockets = new Socket[bindings.Length];
		}

		public async Task InitAsync()
		{
			try
			{
				for(int i = 0; i < listeners.Length; ++i)
				{
					IPAddress addr = null;
					if(bindings[i] == "any_ip4")
						addr = IPAddress.Any;
					else if(!IPAddress.TryParse(bindings[i], out addr))
						addr = (await Dns.GetHostAddressesAsync(bindings[i]))[0];
					sockets[i] = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					sockets[i].Bind(new IPEndPoint(addr, port));
					sockets[i].Listen(10);
					listeners[i] = Task.Run(() => ListenerWorker(sockets[i], tcs.Token));
				}
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
					var tSocket = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null).ConfigureAwait(false);
					tSocket.NoDelay = nodelay;
					if(bufferSize != 0)
					{
						tSocket.ReceiveBufferSize = bufferSize;
						tSocket.SendBufferSize = bufferSize;
					}
					//TODO: create new tunnel
					//TODO: create itunnel config and populate it with diagnostic stuff like incoming ip address, port, etc
					//TODO: call downstream's OpenTunnelAsync
					throw new NotImplementedException("TODO");
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
			for(int i = 0; i < sockets.Length; ++i)
				if(sockets[i] != null)
					await Task.Factory.FromAsync(
						(callback, state) => sockets[i].BeginDisconnect(true, callback, state),
						sockets[i].EndDisconnect, null).ConfigureAwait(false); //recheck this
			int len = 0;
			for(len = 0; len < listeners.Length; ++len)
				if(listeners[len] == null)
					break;
			if(len == 0)
				return;
			var tasks = new Task[len];
			for(int i = 0; i < len; ++i)
				tasks[i] = listeners[i];
			try { await Task.WhenAll(tasks); }
			catch(Exception ex)
			{
				var aex = ex as AggregateException;
				if(aex == null)
					throw;
				foreach(var e in aex.InnerExceptions)
					if(!(e is OperationCanceledException))
						throw;
			}
			//TODO: should not happen, retest
			for(int i = 0; i < len; ++i)
				if(!(tasks[i].IsCanceled || tasks[i].IsCompleted))
					throw new Exception("Some listener tasks still running!");
		}

		public void Dispose()
		{
			//do not check anything, because all operations must be already stopped by ExitTunnel from user's side.
			for(int i = 0; i < listeners.Length; ++i)
			{
				listeners[i].Dispose();
				sockets[i].Dispose();
			}
		}
	}
}
