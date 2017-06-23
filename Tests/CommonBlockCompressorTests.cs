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
		private static readonly Random random = new Random();

		static CommonBlockCompressorTests()
		{
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
			while (offset < limit)
				buffer[offset++] = (byte)random.Next(0, 256);
		}

		public static void Test_UniformData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			byte val = (byte)new Random().Next(0, 256);
			for (int i = 0; i < input.Length; ++i)
				input[i] = val;
			var outLen = Compress_WithOffset(compressor, input, 0, dataLen, output, 0, minLenStrictCheck);
			var decOutput=new byte[dataLen];
			Decompress_WithOffset(compressor, output, 0, decOutput, 0, input, dataLen);
		}

		public static void Test_HighComprData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateHighComprData(input);
			var outLen = Compress_WithOffset(compressor, input, 0, dataLen, output, 0, minLenStrictCheck);
			var decOutput = new byte[dataLen];
			Decompress_WithOffset(compressor, output, 0, decOutput, 0, input, dataLen);
		}

		public static void Test_LowComprData(IBlockCompressor compressor, int dataLen, int minLenStrictCheck)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateComprData(input);
			var outLen = Compress_WithOffset(compressor, input, 0, dataLen, output, 0, minLenStrictCheck);
			var decOutput = new byte[dataLen];
			Decompress_WithOffset(compressor, output, 0, decOutput, 0, input, dataLen);
		}

		public static void Test_NonComprData(IBlockCompressor compressor, int dataLen)
		{
			var input = new byte[dataLen];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)];
			GenerateNonComprData(input);
			var outLen = Compress_WithOffset(compressor, input, 0, dataLen, output, 0, dataLen + 1);
			var decOutput = new byte[dataLen];
			Decompress_WithOffset(compressor, output, 0, decOutput, 0, input, dataLen);
		}

		public static int Compress_WithOffset(IBlockCompressor compressor, byte[] input, int offset, int sz, byte[] output, int outOffset, int minLenStrictCheck)
		{
			var outLen = compressor.Compress(input, sz, offset, output, outOffset);
			if (sz < minLenStrictCheck)
				Assert.LessOrEqual(outLen, sz + compressor.DecodeMetadataSZ(output,outOffset));
			else
				Assert.Less(outLen, sz + compressor.DecodeMetadataSZ(output,outOffset));
			return outLen;
		}

		public static void Decompress_WithOffset(IBlockCompressor compressor, byte[] input, int offset, byte[] output, int outOffset, byte[] control, int contolSZ)
		{
			var outLen = compressor.Decompress(input, offset, output, outOffset);
			Assert.AreEqual(contolSZ, outLen);
			for (int i = outOffset; i < outOffset + outLen; ++i)
				if (output[i] != control[i])
					throw new Exception(string.Format("Decompress_WithOffset: output[{0}]!=control[{0}]", i));
		}

		public static void Test_HighComprData_WithOffset(IBlockCompressor compressor, int dataLen, int maxDataOffset, int minLenStrictCheck)
		{
			var input = new byte[dataLen+maxDataOffset];
			var output = new byte[compressor.GetOutBuffSZ(dataLen)+maxDataOffset];
			var dataOffset = random.Next(1, maxDataOffset + 1);
			GenerateHighComprData(input,dataOffset);
			var outLen = Compress_WithOffset(compressor, input, dataOffset, dataLen, output, dataOffset, minLenStrictCheck);
			var decOutput = new byte[dataLen + maxDataOffset];
			Decompress_WithOffset(compressor, output, dataOffset, decOutput, dataOffset, input, dataLen);
		}

		public static void Test_LowComprData_WithOffset(IBlockCompressor compressor, int dataLen, int maxDataOffset, int minLenStrictCheck)
		{
			var input = new byte[dataLen + maxDataOffset];
			var output = new byte[compressor.GetOutBuffSZ(dataLen) + maxDataOffset];
			var dataOffset = random.Next(1, maxDataOffset + 1);
			GenerateComprData(input, dataOffset);
			var outLen = Compress_WithOffset(compressor, input, dataOffset, dataLen, output, dataOffset, minLenStrictCheck);
			var decOutput = new byte[dataLen + maxDataOffset];
			Decompress_WithOffset(compressor, output, dataOffset, decOutput, dataOffset, input, dataLen);
		}

		public static void Test_NonComprData_WithOffset(IBlockCompressor compressor, int dataLen, int maxDataOffset)
		{
			var input = new byte[dataLen + maxDataOffset];
			var output = new byte[compressor.GetOutBuffSZ(dataLen) + maxDataOffset];
			var dataOffset = random.Next(1, maxDataOffset + 1);
			GenerateNonComprData(input, dataOffset);
			var outLen = Compress_WithOffset(compressor, input, dataOffset, dataLen, output, dataOffset, dataLen + 1);
			var decOutput = new byte[dataLen + maxDataOffset];
			Decompress_WithOffset(compressor, output, dataOffset, decOutput, dataOffset, input, dataLen);
		}

		public static void Test_UniformData_WithOffset(IBlockCompressor compressor, int dataLen, int maxDataOffset, int minLenStrictCheck)
		{
			var input = new byte[dataLen + maxDataOffset];
			var output = new byte[compressor.GetOutBuffSZ(dataLen) + maxDataOffset];
			var dataOffset = random.Next(1, maxDataOffset + 1);
			var val = (byte)random.Next(0, 256);
			for (int i = dataOffset; i < dataOffset+dataLen; ++i)
				input[i] = val;
			var outLen = Compress_WithOffset(compressor, input, dataOffset, dataLen, output, dataOffset, minLenStrictCheck);
			var decOutput = new byte[dataLen + maxDataOffset];
			Decompress_WithOffset(compressor, output, dataOffset, decOutput, dataOffset, input, dataLen);
		}

		public static void Test_CompressWithOffsetEquality(IBlockCompressor compressor, byte[] input, int maxDataOffset, int iter)
		{
			var control = new byte[compressor.GetOutBuffSZ(input.Length)];
			var controlLen = compressor.Compress(input, input.Length, 0, control, 0);
			var source = new byte[input.Length + maxDataOffset];
			var output = new byte[compressor.GetOutBuffSZ(input.Length) + maxDataOffset];
			for (int i = 0; i < iter; ++i)
			{
				var sourceOffset = random.Next(0, maxDataOffset + 1);
				Buffer.BlockCopy(input, 0, source, sourceOffset, input.Length);
				var outputOffset=random.Next(0, maxDataOffset + 1);
				var outputLen = compressor.Compress(source, input.Length, sourceOffset, output, outputOffset);
				Assert.AreEqual(controlLen, outputLen);
				for (int j = 0; j < controlLen; ++j)
					if (control[j] != output[outputOffset + j])
						throw new Exception(string.Format("control[{0}] != output[{1}+{0}]", j, outputOffset));
			}
		}

		public static void Test_CompressWithOffsetEquality_NonComprData(IBlockCompressor compressor, int inputSz, int maxDataOffset, int iter)
		{
			var input = new byte[inputSz];
			GenerateNonComprData(input);
			Test_CompressWithOffsetEquality(compressor, input, maxDataOffset, iter);
		}

		public static void Test_CompressWithOffsetEquality_HighComprData(IBlockCompressor compressor, int inputSz, int maxDataOffset, int iter)
		{
			var input = new byte[inputSz];
			GenerateHighComprData(input);
			Test_CompressWithOffsetEquality(compressor, input, maxDataOffset, iter);
		}

		public static void Test_CompressWithOffsetEquality_LowComprData(IBlockCompressor compressor, int inputSz, int maxDataOffset, int iter)
		{
			var input = new byte[inputSz];
			GenerateComprData(input);
			Test_CompressWithOffsetEquality(compressor, input, maxDataOffset, iter);
		}

		public static void Test_CompressWithOffsetEquality_UniformData(IBlockCompressor compressor, int inputSz, int maxDataOffset, int iter)
		{
			var input = new byte[inputSz];
			byte val = (byte)random.Next(0, 256);
			for (int i = 0; i < input.Length; ++i)
				input[i] = val;
			Test_CompressWithOffsetEquality(compressor, input, maxDataOffset, iter);
		}

		public static void Test_IncorrectParams(IBlockCompressor compressor)
		{
			var bsz = compressor.MaxBlockSZ;
			var input = new byte[bsz];
			var output = new byte[compressor.GetOutBuffSZ(bsz)];
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(null, bsz, 0, output, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(input, -1, 0, output, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(input, bsz+1, 0, output, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(input, 0, bsz+1, output, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(input, bsz, 0, null, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Compress(input, bsz, 0, output, output.Length));

			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(null, 0, input, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(output, -1, input, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(output, output.Length, input, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(output, 0, null, 0));
			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(output, 0, input, bsz));
			Assert.Throws(typeof(ArgumentException), () => compressor.Decompress(output, 0, input, -1));
		}
	}
}
