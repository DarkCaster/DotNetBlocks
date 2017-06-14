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
			var count = FastLZ.Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0, true);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
		}

		[Test]
		public void CompressSampleLV2()
		{
			var output = new byte[FastLZData.SampleOutputLV2.Length];
			var control = FastLZData.SampleOutputLV2;
			var count = FastLZ.Compress(FastLZData.SampleInput, 0, FastLZData.SampleInput.Length, output, 0, false);
			Assert.AreEqual(control.Length,count);
			Assert.AreEqual(control,output);
		}

		/*public void FindMaxOverhead(bool fastSpeed)
		{
			int overhead = 0;
			var maxBsz = (new FastLZBlockCompressor(fastSpeed)).MaxBlockSZ;
			var random = new Random();
			int ilen = 1;
			while(ilen < maxBsz)
			{
				var input = new byte[ilen];
				random.NextBytes(input);
				var output = new byte[(int)(ilen * 2)];
				var count = FastLZ.Compress(input, 0, ilen, output, 0, fastSpeed);
				if (count - ilen > overhead)
					overhead = count - ilen;
				if (ilen > 8192)
					ilen = maxBsz;
				else
					++ilen;
			}
			for (int i = 0; i < 10;++i)
			{
				var input = new byte[ilen];
				random.NextBytes(input);
				var output = new byte[(int)(ilen * 1.5)];
				var count = FastLZ.Compress(input, 0, ilen, output, 0, fastSpeed);
				if (count - ilen > overhead)
					overhead = count - ilen;
			}

			Assert.Fail(overhead.ToString());
		}

		[Test]
		public void FindMaxOverhead()
		{
			FindMaxOverhead(true);
			FindMaxOverhead(false);
		}

		public void FindMinOverhead(bool fastSpeed)
		{
			var maxBsz = (new FastLZBlockCompressor(fastSpeed)).MaxBlockSZ;
			var random = new Random();
			int ilen = 1;
			int[] ovh = new int[512];
			while (ilen < 512)
			{
				var input = new byte[ilen];
				random.NextBytes(input);
				var output = new byte[(int)(ilen * 2)];
				var count = FastLZ.Compress(input, 0, ilen, output, 0, fastSpeed);
				ovh[ilen] = count - ilen;
				++ilen;
			}
			string result="";
			for (int i = 0; i < 512; ++i)
				result += i.ToString()+"+"+ovh[i] + ";";
			Assert.Fail(result);
		}

		[Test]
		public void FindMinOverhead()
		{
			FindMinOverhead(true);
			FindMinOverhead(false);
		}*/

		public void Compress_SmallSize(bool fastSpeed)
		{
			var compressor = new FastLZBlockCompressor(fastSpeed);
			for (int i = 1; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_HighComprData(compressor, i, 15);
		}

		[Test]
		public void Compress_SmallSize()
		{
			Compress_SmallSize(true);
			Compress_SmallSize(false);
		}

		public void Compress_SmallSize_Plane(bool fastSpeed)
		{
			var compressor = new FastLZBlockCompressor(fastSpeed);
			for (int i = 15; i < 16384; ++i)
				CommonBlockCompressorTests.Compress_PlaneData(compressor, i, 15);
		}

		[Test]
		public void Compress_SmallSize_Plane()
		{
			Compress_SmallSize_Plane(true);
			Compress_SmallSize_Plane(false);
		}
	}
}
