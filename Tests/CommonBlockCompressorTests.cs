﻿// CommonBlockCompressorTests.cs
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
using NUnit.Framework;
using DarkCaster.Compression;

namespace Tests
{
	public static class CommonBlockCompressorTests
	{
		private const int CHUNKS_CNT = 128;
		private const int MIN_CHUNK_SZ = 1;
		private const int MAX_CHUNK_SZ = 16;
		private static readonly byte[][] chunks;

		static CommonBlockCompressorTests()
		{
			var random = new Random();
			chunks = new byte[CHUNKS_CNT][];
			for (int i = 0; i < CHUNKS_CNT; ++i)
			{
				chunks[i] = new byte[random.Next(MIN_CHUNK_SZ, MAX_CHUNK_SZ + 1)];
				random.NextBytes(chunks[i]);
			}
		}

		public static void GenerateHighComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if (length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var random = new Random();
			var maxLen = 32;
			if (maxLen > length / 2)
				maxLen = length / 2;
			if (maxLen < 8)
				maxLen = 8;
			while (offset < limit)
			{
				byte val = (byte)random.Next(0, 256);
				int len = (byte)random.Next(8, maxLen);
				//copy data chunk to buffer
				for (int i = 0; i < len; ++i)
				{
					buffer[offset++] = val;
					if (offset >= limit)
						break;
				}
			}
		}

		public static void GenerateComprData(byte[] buffer, int offset=0, int length=-1)
		{
			if (length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var random = new Random();
			while (offset < limit)
			{
				//select data chunk to copy
				var chunk = chunks[random.Next(0, CHUNKS_CNT)];
				//copy data chunk to buffer
				for (int i = 0; i < chunk.Length; ++i)
				{
					buffer[offset++] = chunk[i];
					if (offset >= limit)
						break;
				}
			}
		}

		public static void GenerateNonComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if (length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var random = new Random();
			while (offset < limit)
				buffer[offset++] = (byte)random.Next(0, 256);
		}

		public static void GenerateNonComprData2(IBlockCompressor compressor, int uniqueBlockSz, byte[] buffer, int offset = 0, int length = -1)
		{
			//TODO: fillup unique block
			var uniqueBlock = new byte[uniqueBlockSz];
			//TODO: check that joined-together unique blocks still produces uncompressible data

			//fillup target buffer with unique blocks.
			if (length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var ubPos = 0;
			while (offset < limit)
			{
				buffer[offset++] = uniqueBlock[ubPos++];
				if (ubPos >= uniqueBlockSz)
					ubPos = 0;
			}
		}

		public static void Compress_PlaneData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			byte val = (byte)new Random().Next(0, 256);
			for (int i = 0; i < input.Length; ++i)
				input[i] = val;
			var outLen = compressor.Compress(input, dataLen, 0, output, 0);
			if (dataLen < minLenStrictCheck)
				Assert.LessOrEqual(outLen, input.Length + compressor.DecodeMetadataSZ(output));
			else
				Assert.Less(outLen, input.Length + compressor.DecodeMetadataSZ(output));
		}

		public static void Compress_HighComprData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateHighComprData(input);
			var outLen = compressor.Compress(input, dataLen, 0, output, 0);
			if(dataLen<minLenStrictCheck)
				Assert.LessOrEqual(outLen, input.Length + compressor.DecodeMetadataSZ(output));
			else
				Assert.Less(outLen, input.Length + compressor.DecodeMetadataSZ(output));
		}

		public static void Compress_LowComprData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateComprData(input);
			var outLen = compressor.Compress(input, dataLen, 0, output, 0);
			if (dataLen < minLenStrictCheck)
				Assert.LessOrEqual(outLen, input.Length + compressor.DecodeMetadataSZ(output));
			else
				Assert.Less(outLen, input.Length + compressor.DecodeMetadataSZ(output));
		}

		public static void Compress_NonComprData(IBlockCompressor compressor, int dataLen)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateNonComprData(input);
			var outLen = compressor.Compress(input, dataLen, 0, output, 0);
			Assert.AreEqual(outLen, input.Length + compressor.DecodeMetadataSZ(output));
		}
	}
}
