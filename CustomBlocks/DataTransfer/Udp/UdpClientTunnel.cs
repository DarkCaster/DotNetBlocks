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

namespace DarkCaster.DataTransfer.Client
{
	public class UdpClientTunnel : BaseTunnel
	{
		private readonly Socket client;

		public UdpClientTunnel(IPAddress[] addr, int port, int timeout = 0)
		{
			client = new Socket(SocketType.Dgram, ProtocolType.Udp);
			client.Bind(new IPEndPoint(IPAddress.Any, 0));
			client.Connect(addr, port);
			client.ReceiveTimeout = timeout;
			client.SendTimeout = timeout;
		}

		public UdpClientTunnel(string host, int port, int timeout = 0)
			: this(Dns.GetHostEntry(host).AddressList, port, timeout) { }

		public UdpClientTunnel(IPAddress addr, int port, int timeout = 0)
			: this(new IPAddress[1] { addr }, port, timeout) { }

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
				SwitchToOffline();
				throw new UdpReadException(curState, "SocketException while trying to receive data. ErrorCode=" + ((SocketError)ex.ErrorCode).ToString("G"), ex);
			}
			catch (Exception ex)
			{
				SwitchToOffline();
				throw new UdpReadException(curState, "Error while trying to read data!", ex);
			}
		}

		public override int WriteData(int sz, byte[] buffer, int offset = 0)
		{
			var curState = state;
			if (curState != TunnelState.Online)
				throw new UdpReadException(curState, "Cannot perform write into tunnel that is not online!", null);
			try
			{
				return client.Send(buffer, offset, sz, SocketFlags.None);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == (int)SocketError.TimedOut || ex.ErrorCode == (int)SocketError.MessageSize)
					return 0;
				SwitchToOffline();
				throw new UdpReadException(curState, "SocketException while trying to send data. ErrorCode=" + ((SocketError)ex.ErrorCode).ToString("G"), ex);
			}
			catch (Exception ex)
			{
				SwitchToOffline();
				throw new UdpReadException(curState, "Error while trying to send data!", ex);
			}
		}

		public override async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			var curState = state;
			if (curState != TunnelState.Online)
				throw new UdpReadException(curState, "Cannot perform read from tunnel that is not online!", null);
			try
			{
				return await Task.Factory.FromAsync(
					(callback,state)=>client.BeginReceive(buffer,offset,sz,SocketFlags.None,callback,state),
					client.EndReceive,null).ConfigureAwait(false);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == (int)SocketError.TimedOut)
					return 0;
				if (ex.ErrorCode == (int)SocketError.MessageSize)
					return sz;
				SwitchToOffline();
				throw new UdpReadException(curState, "SocketException while trying to read data. ErrorCode=" + ((SocketError)ex.ErrorCode).ToString("G"), ex);
			}
			catch (Exception ex)
			{
				SwitchToOffline();
				throw new UdpReadException(curState, "Error while trying to read data!", ex);
			}
		}

		public override async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			var curState = state;
			if (curState != TunnelState.Online)
				throw new UdpReadException(curState, "Cannot perform write into tunnel that is not online!", null);
			try
			{
				return await Task.Factory.FromAsync(
					(callback, state) => client.BeginSend(buffer, offset, sz, SocketFlags.None, callback, state),
					client.EndSend, null).ConfigureAwait(false);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == (int)SocketError.TimedOut || ex.ErrorCode == (int)SocketError.MessageSize)
					return 0;
				SwitchToOffline();
				throw new UdpReadException(curState, "SocketException while trying to send data. ErrorCode=" + ((SocketError)ex.ErrorCode).ToString("G"), ex);
			}
			catch (Exception ex)
			{
				SwitchToOffline();
				throw new UdpReadException(curState, "Error while trying to send data!", ex);
			}
		}

		public override void Disconnect()
		{
			client.Close();
			SwitchToOffline();
		}

		public override void Dispose()
		{
			if (isDisposed)
				return;
			base.Dispose();
		}
	}
}
