﻿// FastLZBlockCompressorFactory.cs
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
namespace DarkCaster.Compression.FastLZ
{
	public sealed class FastLZBlockCompressorFactory : IBlockCompressorFactory
	{
		public IBlockCompressor GetCompressor()
		{
			return new FastLZBlockCompressor(false);
		}

		public IBlockCompressor GetCompressor(int maxBlockSize)
		{
			return new FastLZBlockCompressor(maxBlockSize, false);
		}

		public IThreadSafeBlockCompressor GetThreadSafeCompressor()
		{
			return new FastLZBlockCompressor(true);
		}

		public IThreadSafeBlockCompressor GetThreadSafeCompressor(int maxBlockSize)
		{
			return new FastLZBlockCompressor(maxBlockSize, true);
		}

		public void CalculateBlockAndMetaSizes(int limit, out int maxMetaSz, out int maxInputDataSz)
		{
			if (limit < 2)
				throw new ArgumentException("Requested limit is too low!", nameof(limit));
			maxInputDataSz = limit - 1;
			maxMetaSz = FastLZBlockCompressor.CalculateHeaderLength(maxInputDataSz);
			while (maxInputDataSz + maxMetaSz > limit)
			{
				--maxInputDataSz;
				maxMetaSz = FastLZBlockCompressor.CalculateHeaderLength(maxInputDataSz);
			}
		}

		public bool MetadataPreviewSupported { get { return true; } }

		public short Magic { get { return 0x0001; } }
	}
}
