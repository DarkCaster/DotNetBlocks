// BaseTunnel.cs
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
using DarkCaster.Async;
using DarkCaster.Events;

namespace DarkCaster.DataTransfer
{
	public abstract class BaseTunnel : ITunnel
	{
		protected readonly AsyncRWLock opLock = new AsyncRWLock();
		protected readonly ISafeEventCtrl<TunnelStateEventArgs> evCtl;
		protected readonly ISafeEvent<TunnelStateEventArgs> ev;
		protected TunnelState state = TunnelState.Init;
		protected volatile bool isDisposed = false;

		protected BaseTunnel()
		{
#if DEBUG
			evCtl = new SafeEventDbg<TunnelStateEventArgs>();
			ev = (ISafeEvent<TunnelStateEventArgs>)evCtl;
#else
			evCtl = new SafeEvent<TunnelStateEventArgs>();
			ev = (ISafeEvent<TunnelStateEventArgs>)evCtl;
#endif
		}

		protected BaseTunnel(TunnelState defaultState = TunnelState.Init) : this()
		{
			state = defaultState;
		}

		public virtual ISafeEvent<TunnelStateEventArgs> StateChangeEvent { get { return ev; } }

		public virtual TunnelState State
		{
			get
			{
				opLock.EnterReadLock();
				try { return state; }
				finally { opLock.ExitReadLock(); }
			}
		}

		public virtual void Dispose()
		{
			if (isDisposed)
				return;
			Disconnect();
			isDisposed = true;
			evCtl.Dispose();
		}

		public abstract void Disconnect();
		public abstract int ReadData(int sz, byte[] buffer, int offset = 0);
		public abstract void WriteData(int sz, byte[] buffer, int offset = 0);
		public abstract Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0);
		public abstract Task WriteDataAsync(int sz, byte[] buffer, int offset = 0);
	}
}
