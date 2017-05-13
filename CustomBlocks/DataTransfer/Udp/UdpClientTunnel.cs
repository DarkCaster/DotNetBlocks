// UdpClientTunnel.cs
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
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using DarkCaster.Events;
using DarkCaster.Async;

namespace DarkCaster.DataTransfer.Udp
{
	public class UdpClientTunnel : BaseTunnel
	{
		private readonly Socket client;

		public UdpClientTunnel(IPAddress[] addr, int port)
		{
			client = new Socket(SocketType.Dgram, ProtocolType.Udp);
			client.Bind(new IPEndPoint(IPAddress.Any, 0));
			client.Connect(addr, port);
		}

		public UdpClientTunnel(string host, int port)
			: this(Dns.GetHostEntry(host).AddressList, port) { }

		public UdpClientTunnel(IPAddress addr, int port)
			: this(new IPAddress[1] { addr }, port) { }

		public override int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			var curState = state;
			if (curState != TunnelState.Online)
				throw new UdpReadException(curState, "Cannot perform read from tunnel that is not online!", null);
			try
			{
				return client.Receive(buffer, offset, sz, SocketFlags.None);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == (int)SocketError.TimedOut)
					return 0;
				if (ex.ErrorCode == (int)SocketError.MessageSize)
					return sz;
				evCtl.Raise(this, new TunnelStateEventArgs(TunnelState.Offline), () => state = TunnelState.Offline);
				throw new UdpReadException(curState, "SocketException while trying to read data. ErrorCode=" + ((SocketError)ex.ErrorCode).ToString("G"), ex);
			}
			catch (Exception ex)
			{
				evCtl.Raise(this, new TunnelStateEventArgs(TunnelState.Offline), () => state = TunnelState.Offline);
				throw new UdpReadException(curState, "Error while trying to read data!", ex);
			}
		}

		public override void WriteData(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public override async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public override async Task WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public override void Disconnect()
		{
			throw new NotImplementedException("TODO");
		}

		public override void Dispose()
		{
			if (isDisposed)
				return;
			base.Dispose();
		}
	}
}
