// TcpTunnelBase.cs
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
using System.Threading.Tasks;

namespace DarkCaster.DataTransfer.Private
{
	public abstract class TcpTunnelBase
	{
		protected class EOFException : Exception { public EOFException(Exception inner) : base(null, inner) { } }

		private readonly Socket socket;

		protected TcpTunnelBase(Socket socket)
		{
			this.socket = socket;
		}

		public virtual async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			var dataRead = await Task.Factory.FromAsync(
				(callback, state) => socket.BeginReceive(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndReceive, null).ConfigureAwait(false);
			if(dataRead <= 0)
				throw new EOFException(null);
			return dataRead;
		}

		public virtual async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			try
			{
				return await Task.Factory.FromAsync(
				(callback, state) => socket.BeginSend(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndSend, null).ConfigureAwait(false);
			}
			catch(SocketException ex)
			{
				if(ex.SocketErrorCode == SocketError.Shutdown || ex.SocketErrorCode == SocketError.Disconnecting)
					throw new EOFException(ex);
				throw;
			}
		}

		public async Task DisconnectAsync()
		{
			try { socket.Shutdown(SocketShutdown.Both); }
			//TODO: ignore only some types of SocketExceptions
			catch(SocketException) { }
			try
			{
				await Task.Factory.FromAsync(
				(callback, state) => socket.BeginDisconnect(true, callback, state),
				socket.EndDisconnect, null).ConfigureAwait(false);
			}
			//TODO: ignore only some types of SocketExceptions
			catch(SocketException) { }
		}

		public void Dispose()
		{
			socket.Dispose();
		}
	}
}
