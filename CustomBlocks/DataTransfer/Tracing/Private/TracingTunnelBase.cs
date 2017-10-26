// TracingTunnelBase.cs
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
namespace DarkCaster.DataTransfer.Private
{
	public abstract class TracingTunnelBase
	{
		private readonly ITunnelBase uplink;
		private readonly Action<object, int, Exception> OnReadDelegate;
		private readonly Action<object, int, Exception> OnWriteDelegate;
		private readonly Action<object, Exception> OnDisconnectDelegate;
		private readonly Action<object, Exception> OnDisposeDelegate;

		protected TracingTunnelBase(ITunnelBase uplink,
		                            Action<object, int, Exception> OnReadDelegate, Action<object, int, Exception> OnWriteDelegate,
		                            Action<object, Exception> OnDisconnectDelegate, Action<object, Exception> OnDisposeDelegate)
		{
			this.uplink = uplink;
			this.OnReadDelegate = OnReadDelegate;
			this.OnWriteDelegate = OnWriteDelegate;
			this.OnDisconnectDelegate = OnDisconnectDelegate;
			this.OnDisposeDelegate = OnDisposeDelegate;
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			try
			{
				var dataRead = await uplink.ReadDataAsync(sz, buffer, offset);
				OnReadDelegate(this, dataRead, null);
				return dataRead;
			}
			catch(Exception ex)
			{
				OnReadDelegate(this, 0, ex);
				throw;
			}
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			try
			{
				var dataWrite = await uplink.WriteDataAsync(sz, buffer, offset);
				OnWriteDelegate(this, dataWrite, null);
				return dataWrite;
			}
			catch (Exception ex)
			{
				OnWriteDelegate(this, 0, ex);
				throw;
			}
		}

		public async Task DisconnectAsync()
		{
			try
			{
				await uplink.DisconnectAsync();
				OnDisconnectDelegate(this, null);
			}
			catch (Exception ex)
			{
				OnDisconnectDelegate(this, ex);
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				uplink.Dispose();
				OnDisposeDelegate(this, null);
			}
			catch (Exception ex)
			{
				OnDisposeDelegate(this, ex);
				throw;
			}
		}
	}
}
