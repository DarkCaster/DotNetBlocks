// IBlockCompressor.cs
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
namespace DarkCaster.Compression
{
	/// <summary>
	/// Interface for data compression class.
	/// Works with independend blocks of data.
	/// </summary>
	public interface IBlockCompressor
	{
		/// <summary>
		/// Compess specified amount of data from input buffer, and saves compressed data to output buffer.
		/// May also write metadata header to output data buffer, if it needed for decompression.
		/// </summary>
		/// <param name="input">Reference to the data to compress</param>
		/// <param name="inSz">Lenght of the data to compress</param>
		/// <param name="inOffset">Offset for input buffer</param>
		/// <param name="output">Buffer which will contain the compressed data</param>
		/// <param name="outOffset">Offset for output buffer</param>
		/// <returns>The size of the compressed data written to the output buffer</returns>
		int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset);

		/// <summary>
		/// Decompresses the data, previously compressed with Compress method.
		/// Data length information should be stored in header within input data buffer. 
		/// </summary>
		/// <param name="input">Reference to the data to decompress</param>
		/// <param name="inOffset">Offset for input buffer</param>
		/// <param name="output">Reference to a buffer which will contain the decompressed data</param>
		/// <param name="outOffset">Offset for output buffer</param>
		/// <returns>Returns decompressed size</returns>
		int Decompress(byte[] input, int inOffset, byte[] output, int outOffset);

		/// <summary>
		/// Gets the maximum size of uncompressed data block that may be processed by this compressor at once.
		/// You may use this information to tune input buffer size.
		/// </summary>
		/// <value>Maximum size of the block.</value>
		int MaxBlockSZ { get; }

		/// <summary>
		/// Gets the maximum overhead in bytes that may be applied to compressed data.
		/// Maximum size of compressed data that may be produced by compressor must be equal to MaxBlockSZ+MaxOverhead.
		/// You may use this information to tune output buffer size.
		/// </summary>
		/// <value>Maximum overhead value in bytes.</value>
		int MaxOverhead { get; }
	}
}
