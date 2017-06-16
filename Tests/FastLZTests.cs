﻿﻿﻿// FastLZTests.cs
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
		public void CompressSampleLV1()
		{
			var output = new byte[FastLZData.SampleOutputLV1.Length];
			var control = FastLZData.SampleOutputLV1;
			var count = FastLZ.Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
		}

		[Test]
		public void DecompressSampleLV1()
		{
			var control = FastLZData.SampleInput;
			var output = new byte[control.Length];
			var count = FastLZ.Decompress(FastLZData.SampleOutputLV1, 0, FastLZData.SampleOutputLV1.Length, output, 0, control.Length);
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

		[Test]
		public void Compress_HiComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_HighComprData(compressor, i, 15);
		}

		[Test]
		public void Compress_LowComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_LowComprData(compressor, i, 512);
		}

		[Test]
		public void Compress_NonComprData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_NonComprData(compressor, i);
		}

		[Test]
		public void Compress_PlaneData()
		{
			var compressor = new FastLZBlockCompressor();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_PlaneData(compressor, i, 15);
		}

		[Test]
		public void Compress_HiComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Compress_HighComprData(compressor, compressor.MaxBlockSZ, 15);
		}

		[Test]
		public void Compress_LowComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Compress_LowComprData(compressor, compressor.MaxBlockSZ, 512);
		}

		[Test]
		public void Compress_NonComprData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Compress_NonComprData(compressor, compressor.MaxBlockSZ);
		}

		[Test]
		public void Compress_PlaneData_MaxBlock()
		{
			var compressor = new FastLZBlockCompressor();
			CommonBlockCompressorTests.Compress_PlaneData(compressor, compressor.MaxBlockSZ, 15);
		}

		public static void FastLZ_GenerateNonComprData(int uniqueBlockSz, byte[] buffer, int offset = 0, int length = -1)
		{
			var random = new Random();
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
					testOutLen = FastLZ.Compress(uniqueBlock, 0, uniqueBlockSz, testOutput, 0);
				}
				//check that 2 joined-together unique blocks still produces uncompressible data for selected compressor
				testOutput = new byte[uniqueBlockSz * 2 + (uniqueBlockSz * 2) / 32 + 1];
				var testInput = new byte[uniqueBlockSz * 2];
				Buffer.BlockCopy(uniqueBlock, 0, testInput, 0, uniqueBlockSz);
				Buffer.BlockCopy(uniqueBlock, 0, testInput, uniqueBlockSz, uniqueBlockSz);
				if (FastLZ.Compress(testInput, 0, uniqueBlockSz * 2, testOutput, 0) >= testOutput.Length-1)
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
		public void FastLZ_Compress_NonComprData_MaxBlock()
		{
			var dataLen = new FastLZBlockCompressor().MaxBlockSZ;
			var input = new byte[dataLen];
			var output = new byte[dataLen + dataLen / 32 + 1];
			FastLZ_GenerateNonComprData(16000, input);
			var outLen = FastLZ.Compress(input, 0, dataLen, output, 0);
			//test, that we have used all space in output-buffer (except for the last extra byte)
			Assert.AreEqual(output.Length, outLen + 1);
		}

		[Test]
		public void Compress_HighComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var random = new Random();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_HighComprData_WithOffset(compressor, i, random.Next(1, 16384), 15);
		}

		[Test]
		public void Compress_LowComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var random = new Random();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_LowComprData_WithOffset(compressor, i, random.Next(1, 16384), 15);
		}

		[Test]
		public void Compress_NonComprData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var random = new Random();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_NonComprData_WithOffset(compressor, i, random.Next(1, 16384));
		}

		[Test]
		public void Compress_PlaneData_WithOffset()
		{
			var compressor = new FastLZBlockCompressor();
			var random = new Random();
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_PlaneData_WithOffset(compressor, i, random.Next(1, 16384), 15);
		}
	}
}
