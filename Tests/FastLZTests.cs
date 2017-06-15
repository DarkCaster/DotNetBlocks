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
		public void CompressSampleLV1()
		{
			var output = new byte[FastLZData.SampleOutputLV1.Length];
			var control = FastLZData.SampleOutputLV1;
			var count = FastLZ.Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
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

	}
}
