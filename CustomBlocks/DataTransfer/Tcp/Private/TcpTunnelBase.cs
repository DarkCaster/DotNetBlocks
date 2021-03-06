﻿// TcpTunnelBase.cs
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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DarkCaster.DataTransfer.Private
{
	public abstract class TcpTunnelBase
	{
		protected class EOFException : Exception { public EOFException(Exception inner) : base(null, inner) { } }

		private readonly Socket socket;
		private int isClosed = 0;

		protected TcpTunnelBase(Socket socket)
		{
			this.socket = socket;
		}

		public virtual async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(Interlocked.CompareExchange(ref isClosed, 0, 0) != 0)
				throw new EOFException(new ObjectDisposedException(nameof(socket)));
			if(sz == 0)
				return 0;
			try
			{
				var dataRead = await Task.Factory.FromAsync(
				(callback, state) => socket.BeginReceive(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndReceive, null).ConfigureAwait(false);
				if (dataRead <= 0)
					throw new EOFException(null);
				return dataRead;
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.Shutdown || ex.SocketErrorCode == SocketError.Disconnecting || ex.SocketErrorCode == SocketError.ConnectionReset)
					throw new EOFException(ex);
				throw;
			}
			catch (ObjectDisposedException ex)
			{
				throw new EOFException(ex);
			}
		}

		public virtual async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(Interlocked.CompareExchange(ref isClosed, 0, 0) != 0)
				throw new EOFException(new ObjectDisposedException(nameof(socket)));
			if(sz == 0)
				return 0;
			try
			{
				return await Task.Factory.FromAsync(
				(callback, state) => socket.BeginSend(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndSend, null).ConfigureAwait(false);
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.Shutdown || ex.SocketErrorCode == SocketError.Disconnecting || ex.SocketErrorCode == SocketError.ConnectionReset)
					throw new EOFException(ex);
				throw;
			}
			catch (ObjectDisposedException ex)
			{
				throw new EOFException(ex);
			}
		}

		public Task DisconnectAsync()
		{
			if(Interlocked.CompareExchange(ref isClosed, 1, 0) == 0)
			{
				//According to mono sources (https://github.com/mono/mono/blob/master/mcs/class/System/System.Net.Sockets/Socket.cs)
				//Looks like, for now (oct 2017) this is the only reliable way to force-close connection and interrupt all other async calls currently executing on that Socket object.
				//It may change in future.
				//TODO: test on Windows/.NET/.NETCore
				socket.Dispose();
			}
			return Task.FromResult(true);
		}

		public void Dispose() { /* noop */ }
	}
}
