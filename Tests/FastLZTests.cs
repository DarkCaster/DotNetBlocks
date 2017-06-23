﻿﻿// FastLZTests.cs
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
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using DarkCaster.Compression;
using DarkCaster.Compression.FastLZ;
using Tests.SafeEventStuff;

namespace Tests
{
	[TestFixture]
	public class FastLZTests
	{
		[Test]
		public void Compress_SampleData()
		{
			var output = new byte[FastLZData.SampleOutputLV1.Length];
			var control = FastLZData.SampleOutputLV1;
			var count = new FastLZ().Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
		}

		[Test]
		public void Decompress_SampleData()
		{
			var control = FastLZData.SampleInput;
			var output = new byte[control.Length];
			var count = new FastLZ().Decompress(FastLZData.SampleOutputLV1, 0, FastLZData.SampleOutputLV1.Length, output, 0);
			Assert.AreEqual(control.Length, count);
			Assert.AreEqual(control, output);
		}

		/*[Test]
		public void CompressSampleLV2()
		{
			var output = new byte[FastLZData.SampleOutputLV2.Length];
			var control = FastLZData.SampleOutputLV2;
			var count = FastLZ.Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0, false);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
		}

		[Test]
		public void FindMinOverhead()
		{
			var maxBsz = (new FastLZBlockCompressor()).MaxBlockSZ;
			var random = new Random();
			int ilen = 1;
			int[] ovh = new int[16384];
			while (ilen < 16384)
			{
				var input = new byte[ilen];
				random.NextBytes(input);
				var output = new byte[(int)(ilen * 2)];
				var count = FastLZ.Compress(input, 0, ilen, output, 0);
				ovh[ilen] = count - ilen;
				++ilen;
			}
			string result="";
			for (int i = 0; i < 16384; ++i)
				result += i.ToString()+"+"+ovh[i] + ";";
			Assert.Fail(result);
		}*/

		private static Random random = new Random();
		private static FastLZ fastLz = new FastLZ();

		[Test]
		public void HighComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_HighComprData(compressor, i, 16);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_HighComprData(compressor, i, 16);
		}

		[Test]
		public void LowComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_LowComprData(compressor, i, 768);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_LowComprData(compressor, i, 768);
		}

		[Test]
		public void NonComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_NonComprData(compressor, i);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_NonComprData(compressor, i);
		}

		[Test]
		public void UniformData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_UniformData(compressor, i, 16);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_UniformData(compressor, i, 16);
		}

		[Test]
		public void HighComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Test_HighComprData(compressor, compressor.MaxBlockSZ, 16);
		}

		[Test]
		public void LowComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Test_LowComprData(compressor, compressor.MaxBlockSZ, 768);
		}

		[Test]
		public void NonComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Test_NonComprData(compressor, compressor.MaxBlockSZ);
		}

		[Test]
		public void UniformData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Test_UniformData(compressor, compressor.MaxBlockSZ, 16);
		}

		public static void FastLZ_GenerateNonComprData(int uniqueBlockSz, byte[] buffer, int offset = 0, int length = -1)
		{
			var uniqueBlock = new byte[uniqueBlockSz];
			bool checkOk = false;
			while (!checkOk)
			{
				//fillup unique block
				var testOutput = new byte[uniqueBlockSz + uniqueBlockSz / 32 + 1];
				int testOutLen = 0;
				while (testOutLen < testOutput.Length-1)
				{
					random.NextBytes(uniqueBlock);
					testOutLen = fastLz.Compress(uniqueBlock, 0, uniqueBlockSz, testOutput, 0);
				}
				//check that 2 joined-together unique blocks still produces uncompressible data for selected compressor
				testOutput = new byte[uniqueBlockSz * 2 + (uniqueBlockSz * 2) / 32 + 1];
				var testInput = new byte[uniqueBlockSz * 2];
				Buffer.BlockCopy(uniqueBlock, 0, testInput, 0, uniqueBlockSz);
				Buffer.BlockCopy(uniqueBlock, 0, testInput, uniqueBlockSz, uniqueBlockSz);
				if (fastLz.Compress(testInput, 0, uniqueBlockSz * 2, testOutput, 0) >= testOutput.Length-1)
					checkOk = true;
			}

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

		//May fail sometimes.
		//We need to generate thruly unique and uncompressible data (maybe pi digits array ?).
		//For now we just fill-up input data with smaller uncompresssible block (dynamically generated)
		//that is not multiple to 8192 bytes (so it will not allow compressor algorhitm to find any matches) 
		[Test]
		public void FastLZ_NonComprData_MaxBlock()
		{
			var dataLen = new FastLZBlockCompressor().MaxBlockSZ;
			var input = new byte[dataLen];
			var output = new byte[dataLen + dataLen / 32 + 1];
			FastLZ_GenerateNonComprData(16000, input);
			var outLen = fastLz.Compress(input, 0, dataLen, output, 0);
			//test, that we have used all space in output-buffer
			Assert.AreEqual(output.Length, outLen, 2);
			//decompress and verify
			var decOutput = new byte[dataLen];
			var decLen = fastLz.Decompress(output, 0, outLen, decOutput, 0);
			Assert.AreEqual(dataLen, decLen);
			for (int i = 0; i < decLen; ++i)
				if (decOutput[i] != input[i])
					throw new Exception(string.Format("FastLZ_Compress_NonComprData_MaxBlock: decOutput[{0}]!=input[{0}]", i));
		}

		[Test]
		public void HighComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_HighComprData_WithOffset(compressor, i, random.Next(1, 16384), 16);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_HighComprData_WithOffset(compressor, i, random.Next(1, 16384), 16);
		}

		[Test]
		public void LowComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_LowComprData_WithOffset(compressor, i, random.Next(1, 16384), 768);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_LowComprData_WithOffset(compressor, i, random.Next(1, 16384), 768);
		}

		[Test]
		public void NonComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_NonComprData_WithOffset(compressor, i, random.Next(1, 16384));
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_NonComprData_WithOffset(compressor, i, random.Next(1, 16384));
		}

		[Test]
		public void UniformData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_UniformData_WithOffset(compressor, i, random.Next(1, 16384),16);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_UniformData_WithOffset(compressor, i, random.Next(1, 16384),16);
		}

		[Test]
		public void HighComprData_CompressWithRandomOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var iter = 100;
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_HighComprData(compressor, i, (int)(i * 1.5), iter);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_HighComprData(compressor, i, (int)(i * 1.5), iter);
		}

		[Test]
		public void LowComprData_CompressWithRandomOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var iter = 100;
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_LowComprData(compressor, i, (int)(i * 1.5), iter);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_LowComprData(compressor, i, (int)(i * 1.5), iter);
		}

		[Test]
		public void NonComprData_CompressWithRandomOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var iter = 100;
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_NonComprData(compressor, i, (int)(i * 1.5), iter);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_NonComprData(compressor, i, (int)(i * 1.5), iter);
		}

		[Test]
		public void UniformData_CompressWithRandomOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var iter = 100;
			for (int i = 1; i < 1024; ++i)
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_UniformData(compressor, i, (int)(i * 1.5), iter);
			for (int i = 1024; i < 16384; i += random.Next(1, 65))
				CommonBlockCompressorTests.Test_CompressWithOffsetEquality_UniformData(compressor, i, (int)(i * 1.5), iter);
		}

		[Test]
		public void FastLZ_PartialUncompress()
		{
			var dataLen = 65536;
			var input = new byte[dataLen];
			var output = new byte[dataLen + dataLen / 32 + 1];
			CommonBlockCompressorTests.GenerateComprData(input);
			var cl=new FastLZ().Compress(input,0,dataLen,output,0);
			var tooShortArray = new byte[dataLen/2];
			Exception dEx = null;
			try
			{
				new FastLZ().Decompress(output, 0, cl, tooShortArray, 0);
			}
			catch(Exception ex)
			{
				dEx = ex;
			}
			Assert.NotNull(dEx);
			Assert.True(dEx is ArgumentException || dEx is IndexOutOfRangeException);
		}

		[Test]
		public void IncorrectParams()
		{
			var compressor = new FastLZBlockCompressor(8192);
			CommonBlockCompressorTests.Test_IncorrectParams(compressor);
		}
	}
}
