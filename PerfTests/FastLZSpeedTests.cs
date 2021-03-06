﻿// FastLZSpeedTests.cs
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
using System.Diagnostics;
using DarkCaster.Compression;
using DarkCaster.Compression.FastLZ;

namespace PerfTests
{
	public static class FastLZSpeedTests
	{
		private const int CHUNKS_CNT = 128;
		private const int MIN_CHUNK_SZ = 1;
		private const int MAX_CHUNK_SZ = 16;
		private static readonly byte[][] chunks;
		private static readonly Random random = new Random();

		static FastLZSpeedTests()
		{
			chunks = new byte[CHUNKS_CNT][];
			for(int i = 0; i < CHUNKS_CNT; ++i)
			{
				chunks[i] = new byte[random.Next(MIN_CHUNK_SZ, MAX_CHUNK_SZ + 1)];
				random.NextBytes(chunks[i]);
			}
		}

		public static void GenerateHighComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if(length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			var maxLen = 32;
			if(maxLen > length / 2)
				maxLen = length / 2;
			if(maxLen < 8)
				maxLen = 8;
			while(offset < limit)
			{
				byte val = (byte)random.Next(0, 256);
				int len = (byte)random.Next(8, maxLen);
				//copy data chunk to buffer
				for(int i = 0; i < len; ++i)
				{
					buffer[offset++] = val;
					if(offset >= limit)
						break;
				}
			}
		}

		public static void GenerateComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if(length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			while(offset < limit)
			{
				//select data chunk to copy
				var chunk = chunks[random.Next(0, CHUNKS_CNT)];
				//copy data chunk to buffer
				for(int i = 0; i < chunk.Length; ++i)
				{
					buffer[offset++] = chunk[i];
					if(offset >= limit)
						break;
				}
			}
		}

		public static void GenerateNonComprData(byte[] buffer, int offset = 0, int length = -1)
		{
			if(length < 0)
				length = buffer.Length - offset;
			int limit = offset + length;
			while(offset < limit)
				buffer[offset++] = (byte)random.Next(0, 256);
		}

		public static void CompressionSpeed()
		{
			for(int mode = 0; mode < 5; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 5000;
				int blockSz = 65536;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}
				else if(mode == 4)
				{
					input = Tests.FastLZData.SampleInput;
					output = new byte[input.Length * 2];
					blockSz = input.Length;
					iter *= 2;
				}

				var sw = new Stopwatch();
				var process = Process.GetCurrentProcess();

				var startCpuTime = process.TotalProcessorTime;
				sw.Start();
				for(int i = 0; i < iter; ++i)
					fastLz.Compress(input, 0, input.Length, output, 0);
				sw.Stop();
				var stopCpuTime = process.TotalProcessorTime;

				var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
				var diff = sw.Elapsed.TotalSeconds;
				var mb = (long)iter * blockSz / (1024 * 1024);
				var speed = mb / diff;

				Console.WriteLine(string.Format("Compress (mode={3}): {0} MiB in {1:##.##} S. Speed=={2:##.###} MiB/S.", mb, diff, speed, mode));
				Console.WriteLine(string.Format("Compress (mode={1}): total processor time {0:##.###} S", cpuDiff, mode));
			}
		}

		public static void DecompressionSpeed()
		{
			for(int mode = 0; mode < 5; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 16000;
				int blockSz = 65536;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}
				else if(mode == 4)
				{
					input = Tests.FastLZData.SampleInput;
					output = new byte[input.Length * 2];
					blockSz = input.Length;
					iter *= 2;
				}

				var len = fastLz.Compress(input, 0, input.Length, output, 0);

				var sw = new Stopwatch();
				var process = Process.GetCurrentProcess();

				var startCpuTime = process.TotalProcessorTime;
				sw.Start();
				for(int i = 0; i < iter; ++i)
					fastLz.Decompress(output, 0, len, input, 0);
				sw.Stop();
				var stopCpuTime = process.TotalProcessorTime;

				var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
				var diff = sw.Elapsed.TotalSeconds;
				var mb = (long)iter * blockSz / (1024 * 1024);
				var speed = mb / diff;

				Console.WriteLine(string.Format("Decompress (mode={3}): {0} MiB in {1:##.##} S. Speed=={2:##.###} MiB/S.", mb, diff, speed, mode));
				Console.WriteLine(string.Format("Decompress (mode={1}): total processor time {0:##.###} S", cpuDiff, mode));
			}
		}

		public static void RelativeCompressSpeed()
		{
			var sw = new Stopwatch();
			sw.Start();
			sw.Stop();

			for(int mode = 0; mode < 5; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 20000;
				int blockSz = 4096;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}
				else if(mode == 4)
				{
					input = Tests.FastLZData.SampleInput;
					output = new byte[input.Length * 2];
					blockSz = input.Length;
				}

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					fastLz.Compress(input, 0, input.Length, output, 0);
				sw.Stop();

				var diff = sw.Elapsed.TotalSeconds;
				var total = (long)iter * blockSz;
				var speed = total / diff;

				var compressor = new FastLZBlockCompressor(blockSz);

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					compressor.Compress(input, input.Length, 0, output, 0);
				sw.Stop();

				diff = sw.Elapsed.TotalSeconds;
				total = (long)iter * blockSz;
				var speed2 = total / diff;
				double ratio = speed2 / speed;
				Console.WriteLine(string.Format("Compress (mode={0}): FastLZBlockCompressor/FastLZ speed ratio=={1:0.#########}", mode, ratio));
			}
		}

		public static void RelativeDecompressSpeed()
		{
			var sw = new Stopwatch();
			sw.Start();
			sw.Stop();

			for(int mode = 0; mode < 5; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 64000;
				int blockSz = 4096;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}
				else if(mode == 4)
				{
					input = Tests.FastLZData.SampleInput;
					output = new byte[input.Length * 2];
					blockSz = input.Length;
				}

				var len = fastLz.Compress(input, 0, input.Length, output, 0);

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					fastLz.Decompress(output, 0, len, input, 0);
				sw.Stop();

				var diff = sw.Elapsed.TotalSeconds;
				var total = (long)iter * blockSz;
				var speed = total / diff;

				var compressor = new FastLZBlockCompressor(blockSz);
				len = compressor.Compress(input, input.Length, 0, output, 0);

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					compressor.Decompress(output, 0, input, 0);
				sw.Stop();

				diff = sw.Elapsed.TotalSeconds;
				total = (long)iter * blockSz;
				var speed2 = total / diff;
				double ratio = speed2 / speed;
				Console.WriteLine(string.Format("Decompress (mode={0}): FastLZBlockCompressor/FastLZ speed ratio=={1:0.#########}", mode, ratio));
			}
		}

		public static void MultiblockRelativeCompressSpeed()
		{
			var sw = new Stopwatch();
			sw.Start();
			sw.Stop();

			for(int mode = 0; mode < 4; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 2000;
				int blockSz = 262144;
				int mbBlockSz = 1024;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
				{
					var outOffset = 0;
					var inOffset = 0;
					while(inOffset < blockSz)
					{
						outOffset += fastLz.Compress(input, inOffset, mbBlockSz, output, outOffset);
						inOffset += mbBlockSz;
					}
				}
				sw.Stop();

				var diff = sw.Elapsed.TotalSeconds;
				var total = (long)iter * blockSz;
				var speed = total / diff;

				IBlockCompressor compressor = new FastLZBlockCompressor(1024, false);

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					MultiblockCompressionHelper.Compress(input, input.Length, 0, output, 0, compressor);
				sw.Stop();

				diff = sw.Elapsed.TotalSeconds;
				total = (long)iter * blockSz;
				var speed2 = total / diff;
				double ratio = speed2 / speed;
				Console.WriteLine(string.Format("Compress (mode={0}): MultiblockCompressionHelper/FastLZ speed ratio=={1:0.#########}", mode, ratio));
			}
		}

		public static void MultiblockRelativeDecompressSpeed()
		{
			var sw = new Stopwatch();
			sw.Start();
			sw.Stop();

			for(int mode = 0; mode < 4; ++mode)
			{
				var fastLz = new FastLZ();
				int iter = 8000;
				int blockSz = 262144;
				int mbBlockSz = 1024;

				var input = new byte[blockSz];
				var output = new byte[input.Length * 2];

				if(mode == 0)
					GenerateNonComprData(input);
				else if(mode == 1)
					GenerateComprData(input);
				else if(mode == 2)
					GenerateHighComprData(input);
				else if(mode == 3)
				{
					var val = (byte)random.Next(0, 256);
					for(int i = 0; i < input.Length; ++i)
						input[i] = val;
				}

				var outOffset = 0;
				var inOffset = 0;
				int offsetsLen = blockSz / mbBlockSz;
				var offsets = new int[offsetsLen];
				var lengths = new int[offsetsLen];

				var pos = 0;
				while(inOffset < blockSz)
				{
					offsets[pos] = outOffset;
					lengths[pos] = fastLz.Compress(input, inOffset, mbBlockSz, output, outOffset);
					outOffset += lengths[pos++];
					inOffset += mbBlockSz;
				}

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					for(int o = 0; o < offsetsLen; ++o)
						fastLz.Decompress(output, offsets[o], lengths[o], input, mbBlockSz * o);
				sw.Stop();

				var diff = sw.Elapsed.TotalSeconds;
				var total = (long)iter * blockSz;
				var speed = total / diff;

				IBlockCompressor compressor = new FastLZBlockCompressor(1024, false);
				MultiblockCompressionHelper.Compress(input, input.Length, 0, output, 0, compressor);

				sw.Reset();
				sw.Start();
				for(int i = 0; i < iter; ++i)
					MultiblockCompressionHelper.Decompress(output, 0, input, 0, compressor);
				sw.Stop();

				diff = sw.Elapsed.TotalSeconds;
				total = (long)iter * blockSz;
				var speed2 = total / diff;
				double ratio = speed2 / speed;
				Console.WriteLine(string.Format("Decompress (mode={0}): MultiblockCompressionHelper/FastLZ speed ratio=={1:0.#########}", mode, ratio));
			}
		}
	}
}
