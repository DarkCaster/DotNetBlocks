// FastLZTests.cs
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
			IBlockCompressor compressor = new FastLZBlockCompressor(true);
			var output = new byte[FastLZData.SampleOutputLV1.Length];
			CommonBlockCompressorTests.SampleDataCompress(
				compressor,
			  FastLZData.SampleInput,
				FastLZData.SampleInput.Length,
				0,
				output,
				0,
				FastLZData.SampleOutputLV1);
		}
	}
}
