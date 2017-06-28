// MultiblockCompressionHelper.cs
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
using System.Runtime.CompilerServices;
namespace DarkCaster.Compression
{
	public static class MultiblockCompressionHelper
	{
		private delegate int CompressDelegate(byte[] input, int inSz, int inOffset, byte[] output, int outOffset);
		private delegate int DecompressDelegate(byte[] input, int inOffset, byte[] output, int outOffset);
		private delegate int GetOutBufSZDelegate(int inputSZ);

		public static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, IThreadSafeBlockCompressor compressor)
		{
			return Compress(input, inSz, inOffset, output, outOffset, compressor.Compress);
		}

		public static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, IBlockCompressor compressor)
		{
			return Compress(input, inSz, inOffset, output, outOffset, compressor.Compress);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, CompressDelegate compress)
		{
			throw new NotImplementedException("TODO");
		}

		public static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, IThreadSafeBlockCompressor compressor)
		{
			return Decompress(input, inOffset, output, outOffset, compressor.Decompress);
		}

		public static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, IBlockCompressor compressor)
		{
			return Decompress(input, inOffset, output, outOffset, compressor.Decompress);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, DecompressDelegate decompress)
		{
			throw new NotImplementedException("TODO");
		}

		public static int DecodeComprBlockSZ(byte[] buffer, int offset = 0)
		{
			throw new NotImplementedException("TODO");
		}

		public static int GetOutBuffSZ(int inputSZ, IThreadSafeBlockCompressor compressor)
		{
			return GetOutBuffSZ(inputSZ, compressor.GetOutBuffSZ);
		}

		public static int GetOutBuffSZ(int inputSZ, IBlockCompressor compressor)
		{
			return GetOutBuffSZ(inputSZ, compressor.GetOutBuffSZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetOutBuffSZ(int inputSZ, GetOutBufSZDelegate getOutBufSZ)
		{
			throw new NotImplementedException("TODO");
		}
	}
}
