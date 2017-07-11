// ITunnelBase.cs
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
	public interface ITunnelBase : IDisposable
	{
		/// <summary>
		/// Data read request, async.
		/// Should not be used directly, use IEntryTunnel\IExitTunnel instead.
		/// May return less data, than requested.
		/// May be used in offline state, to read remaining data from tunnel.
		/// </summary>
		/// <returns>Bytes count that was actually read, before disconnect</returns>
		/// <param name="sz">Bytes count to read</param>
		/// <param name="buffer">Buffer, where to store received data</param>
		/// <param name="offset">Offset</param>
		Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Data write request, async.
		/// Should not be used directly, use IEntryTunnel\IExitTunnel instead.
		/// May return less data, than requested.
		/// May NOT be used in offline state (may or may not throw exception).
		/// </summary>
		/// <param name="sz">Bytes count to write</param>
		/// <param name="buffer">Buffer, where source data is located</param>
		/// <param name="offset">Offset</param>
		Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Perform disconnect.
		/// </summary>
		Task DisconnectAsync();
	}
}
