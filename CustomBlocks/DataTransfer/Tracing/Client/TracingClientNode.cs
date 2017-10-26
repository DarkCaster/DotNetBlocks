// TracingClientNode.cs
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
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Client.Tracing
{
	public sealed class TracingClientNode : INode
	{
		private readonly INode downstream;
		private readonly Action<object, object, Exception> OnNewTunnel;
		private readonly Action<object, int, Exception> OnReadDelegate;
		private readonly Action<object, int, Exception> OnWriteDelegate;
		private readonly Action<object, Exception> OnDisconnectDelegate;
		private readonly Action<object, Exception> OnDisposeDelegate;

		public TracingClientNode(INode downstream,
			Action<object, object, Exception> OnNewTunnel, Action<object, int, Exception> OnReadDelegate,Action<object, int, Exception> OnWriteDelegate,
			Action<object, Exception> OnDisconnectDelegate, Action<object, Exception> OnDisposeDelegate)
		{
			this.downstream = downstream;
			this.OnNewTunnel = OnNewTunnel;
			this.OnReadDelegate = OnReadDelegate;
			this.OnWriteDelegate = OnWriteDelegate;
			this.OnDisconnectDelegate = OnDisconnectDelegate;
			this.OnDisposeDelegate = OnDisposeDelegate;
		}

		public async Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			//create downstream
			var dTun = await downstream.OpenTunnelAsync(config);
			TracingClientTunnel tun = null;
			try
			{
				tun = new TracingClientTunnel(dTun, OnReadDelegate, OnWriteDelegate, OnDisconnectDelegate, OnDisposeDelegate);
			}
			catch(Exception ex)
			{
				//in case of error - disconnect and dispose downstream tunnel
				await dTun.DisconnectAsync();
				dTun.Dispose();
				OnNewTunnel(this, null, ex);
				//forward exception from current tunnel
				throw;
			}
			OnNewTunnel(this, tun, null);
			return tun;
		}
	}
}
