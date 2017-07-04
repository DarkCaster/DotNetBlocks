// TcpClientTunnel.cs
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

namespace DarkCaster.DataTransfer.Client.Tcp
{
	public sealed class TcpClientTunnel : ITunnel
	{
		private readonly Socket socket;

		public TcpClientTunnel(Socket socket)
		{
			this.socket = socket;
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			return await Task.Factory.FromAsync(
				(callback, state) => socket.BeginReceive(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndReceive, null).ConfigureAwait(false);
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			return await Task.Factory.FromAsync(
				(callback, state) => socket.BeginSend(buffer, offset, sz, SocketFlags.None, callback, state),
				socket.EndSend, null).ConfigureAwait(false);
		}

		public async Task DisconnectAsync()
		{
			socket.Shutdown(SocketShutdown.Both);
			await Task.Factory.FromAsync(
				(callback, state) => socket.BeginDisconnect(true, callback, state),
				socket.EndDisconnect, null).ConfigureAwait(false);
		}

		public void Dispose()
		{
			socket.Dispose();
		}
	}
}
