// IEntryTunnel.cs
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
using DarkCaster.Events;

namespace DarkCaster.DataTransfer.Client
{
	/// <summary>
	/// Client side entry-tunnel.
	/// Object of this interface should be used to perform data transfer from user's code, instead of using INode directly.
	/// Methods defined here must be thread-safe.
	/// Concurrent reads and concurrent writes must be interlocked,
	/// read and write methods should be allowed to execute at the same time concurrently,
	/// disconnect must be interlocked with both read and write activity.
	/// StateChangeEvent event may be thrown anytime, and it must be thrown from separate thread.
	/// Do not call Dispose from state change event handler, because it will damage internal state of ISafeEvent object.
	/// </summary>
	public interface IEntryTunnel : IDisposable
	{
		/// <summary>
		/// Gets the current state of tunnel.
		/// This feature is using volatile field, but it change is not atomic with tunnel state change event.
		/// So use it only as informational property, do not plan any complex behaviour on it.
		/// </summary>
		/// <value>Current state</value>
		TunnelState State { get; }

		/// <summary>
		/// Event for tunnel state change notifications.
		/// </summary>
		ISafeEvent<TunnelStateEventArgs> StateChangeEvent { get; }

		/// <summary>
		/// Data read request. Blocks while awaiting for data.
		/// May return less data, than requested.
		/// May be used in offline state, to read remaining data from tunnel.
		/// In offline or init mode, return 0 when no data left.
		/// </summary>
		/// <returns>Bytes count that was actually read</returns>
		/// <param name="sz">Bytes count to read</param>
		/// <param name="buffer">Buffer, where to store received data</param>
		/// <param name="offset">Offset</param>
		int ReadData(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Data write request, that blocks execution while writing requested amound of data.
		/// May return less data-written count, than requested.
		/// In offline or init mode always return 0.
		/// </summary>
		/// <returns>Bytes count that was actually written</returns>
		/// <param name="sz">Bytes count to write</param>
		/// <param name="buffer">Buffer, where source data is located</param>
		/// <param name="offset">Offset</param>
		int WriteData(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Same as ReadData, but async
		/// </summary>
		Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Same as WriteData, but async
		/// </summary>
		Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Close tunnel connection.
		/// State change event will be thrown, and tunnel state will be set to "offline".
		/// May block while performing disconnect request.
		/// </summary>
		void Disconnect();

		/// <summary>
		/// Same as Disconnect, but async
		/// </summary>
		Task DisconnectAsync();
	}
}
