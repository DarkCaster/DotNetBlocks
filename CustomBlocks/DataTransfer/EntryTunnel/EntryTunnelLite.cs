// EntryTunnelLite.cs
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
using System.Threading;
using System.Threading.Tasks;
using DarkCaster.Async;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Client
{
	public sealed class EntryTunnelLite : IEntryTunnelLite
	{
		private readonly INode downstreamNode;
		private readonly ITunnelConfig config;

		private volatile TunnelState state = TunnelState.Init;
		private ITunnel downstream;
		private int isDisposed = 0;

		private EntryTunnelLite() { }

		public EntryTunnelLite(INode downstreamNode, ITunnelConfig config)
		{
			this.downstreamNode = downstreamNode;
			this.config = config;
		}

		public TunnelState State { get { return state; } }

		public TunnelState Connect()
		{
			throw new NotImplementedException("TODO");
		}

		public Task<TunnelState> ConnectAsync()
		{
			throw new NotImplementedException("TODO");
		}

		public int ReadData(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public int WriteData(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public void Disconnect()
		{
			throw new NotImplementedException("TODO");
		}

		public Task DisconnectAsync()
		{
			throw new NotImplementedException("TODO");
		}

		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref isDisposed, 1, 0) == 1)
				return;
		}
	}
}
