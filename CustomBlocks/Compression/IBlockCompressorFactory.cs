// IBlockCompressorFactory.cs
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
	/// Factory for IBlockCompressor of selected type
	/// </summary>
	public interface IBlockCompressorFactory
	{
		/// <summary>
		/// Create\Get IBlockCompressor instance with default maxBlockSize
		/// </summary>
		/// <returns>The compressor instance</returns>
		IBlockCompressor GetCompressor();

		/// <summary>
		/// Create\Get IBlockCompressor instance with selected maxBlockSize
		/// </summary>
		/// <returns>The compressor instance</returns>
		IBlockCompressor GetCompressor(int maxBlockSize);

		/// <summary>
		/// Create\Get thread safe IBlockCompressor instance with default maxBlockSize
		/// </summary>
		/// <returns>The compressor instance</returns>
		IThreadSafeBlockCompressor GetThreadSafeCompressor();

		/// <summary>
		/// Create\Get thread safe IBlockCompressor instance with selected maxBlockSize
		/// </summary>
		/// <returns>The compressor instance</returns>
		IThreadSafeBlockCompressor GetThreadSafeCompressor(int maxBlockSize);

		/// <summary>
		/// Gets a value indicating whether selected compressor created by this factory supports metadata-preview feature,
		/// that may be used to decode compressed block size parameters from one or more bytes from block beginning.
		/// </summary>
		/// <value><c>true</c> if metadata preview supported; otherwise, <c>false</c>.</value>
		bool MetadataPreviewSupported { get; }

		/// <summary>
		/// Unique compressor ID. Fully-compatible compressors should have the same "magic" value.
		/// It may be used (for example) to perform additional protocol neogotiation
		/// when needed to ensure compressors compatibility on each sides.
		/// Value 0 means that "magic" feature is not supported.
		/// </summary>
		/// <value>The magic number, that represents selected compressor and it's main settings</value>
		short Magic { get; }

		/// <summary>
		/// Calculate maximum uncompressed input data size + output metadata size for final compressed block,
		/// so it will fit within selected limit after compression.
		/// May be used to calculate block sizes when embedding compressed data produced by IBlockCompressor.
		/// This method may be unsupprted on some compressors.
		/// </summary>
		/// <param name="limit">Maximum external limit in bytes, that must be respected</param>
		/// <param name="maxMetaSz">Maximum metadata size that will be produced within compressed data block</param>
		/// <param name="maxInputDataSz">Maximum allowed input data size that produce compressed data block that will fit within limit</param>
		void CalculateBlockAndMetaSizes(int limit, out int maxMetaSz, out int maxInputDataSz);
	}
}
