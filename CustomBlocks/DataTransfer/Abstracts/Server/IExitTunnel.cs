// IExitTunnel.cs
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
namespace DarkCaster.DataTransfer.Server
{
	/// <summary>
	/// Server side exit-tunnel.
	/// Object of this interface should be used to perform data transfer from user's code, instead of using INode directly.
	/// Read and write methods may be executed at the same time concurrently,
	/// but no two parallel read or write calls allowed.
	/// Dispose must be called after all read\write jobs is terminated, and it will close and dispose all upstrean tunnels.
	/// </summary>
	public interface IExitTunnel : ITunnel
	{
		/// <summary>
		/// Data read request, that blocks while awaiting for data.
		/// May return less data, than requested.
		/// Will throw TunnelEofException on closed connection, when no data left to read.
		/// ReadData should not be used after any exception is thrown.
		/// </summary>
		/// <returns>Bytes count that was actually read</returns>
		/// <param name="sz">Bytes count to read</param>
		/// <param name="buffer">Buffer, where to store received data</param>
		/// <param name="offset">Offset</param>
		int ReadData(int sz, byte[] buffer, int offset = 0);

		/// <summary>
		/// Data write request, that blocks execution while writing requested amound of data.
		/// May write less data, than requested.
		/// WriteData should not be used after any exception is thrown.
		/// May throw TunnelEofException when trying to write data on closed connection.
		/// </summary>
		/// <param name="sz">Bytes count to write</param>
		/// <param name="buffer">Buffer, where source data is located</param>
		/// <param name="offset">Offset</param>
		/// <returns>Bytes count that was actually written</returns>
		int WriteData(int sz, byte[] buffer, int offset = 0);
	}
}
