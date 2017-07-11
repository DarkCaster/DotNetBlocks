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
	/// Not thread safe, class instance may keep internall buffers to improve performance.
	/// Total memory consumption by clas instance may vary for different compression algorhitms and settings.
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
		/// Data length information should be stored in metadata header within input data buffer.
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
		/// This value is set when creating instance of IBlockCompressor.
		/// Additional memmory requirements for IBlockCompressor instance may also depend on this value.
		/// </summary>
		/// <value>Maximum size of the block.</value>
		int MaxBlockSZ { get; }

		/// <summary>
		/// Decodes full size of compressed data block, written to <paramref name="buffer"/> at selected <paramref name="offset"/> 
		/// </summary>
		/// <returns>Full size of compressed data block, including all service metadata</returns>
		/// <param name="buffer">Buffer that contains full data block with compressed data</param>
		/// <param name="offset">Offset</param>
		int DecodeComprBlockSZ(byte[] buffer, int offset=0);

		/// <summary>
		/// Decodes the size of the service metadata from data block with compressed data.
		/// May be used in data transfer scenarios when all compressed data is not available at once.
		/// </summary>
		/// <returns>The metadata size.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="offset">Offset.</param>
		int DecodeMetadataSZ(byte[] buffer, int offset = 0);

		/// <summary>
		/// How much data from beginning of the compressed block need to be read
		/// in order to DecodeMetadataSZ to work properly.
		/// May be used in data transfer scenarios when all compressed data is not available at once.
		/// If DecodeMetadataSZ cannot be decoded from incomplete compressed block
		/// this property should return 0
		/// </summary>
		/// <value>Minimum data from beginnig of compressed block need to be present in order to DecodeMetadataSZ to work</value>
		int MetadataPreviewSZ { get; }

		/// <summary>
		/// Calculate maximum size in bytes needed for output buffer in order to succsesfully compress data for specified input buffer size.
		/// This value include metadata overhead + possible worst case compression algorhitm overhead.
		/// You may use this information to tune output buffer size.
		/// </summary>
		/// <returns>Size of output buffer in bytes</returns>
		int GetOutBuffSZ(int inputSZ);
	}
}
