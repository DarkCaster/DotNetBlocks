﻿﻿// FastLZ.cs
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
// This is a PARTIAL C# port of FastLZ library by (c) Ariya Hidayat, MIT License
// See http://fastlz.org for more info.
//
// Only level 1 compression\decompression mode is currently implemented.
// Level 2 compression mode cause out-of-bounds access to input data buffer on certain patterns.
// (see Tests/FastLZSampleData/input.data.bad for example).
// This problem can be also triggered with original project
// (just feed sample input.data.bad contents to fastlz2_compress function,
// and add bounds checks for data access at FASTLZ_READU16 macro).
// So, level 2 compression is not ported for now.
// It may be implemented in future, when I figure out how exactly compression algorithm works.
//
// Most of the safety checks from original code was removed in order to improve performance,
// because this code intended to run in managed environment.
// In case of wrong input parameters (on compress) of bad input data (on decompress),
// it should not corrupt input buffer even without this safety checks.
// Some most basic checks still may be enabled at compile time manually.
//
// In order to improve compression speed, it using non-static class
// to define and reuse some internal stuff as class fields.
// So, in order to perform compression\decompression you must create an instance first.
// Each instance will consume about 8KiB of heap.
// Class instances are NOT THREAD SAFE, you cannot use compress or decompress from different threads.
// However, you can continue to use the same instance after any exception is thrown during compression or decompression.
//
// ---
//
// The MIT License (MIT)
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

#if DEBUG
// You can manually manage following 2 defines in order to enable\disable some basic safety checks
// even without checks, managed code will not allow to corrupt anything in case of bad input data on decompress,
// or incorrect parameters on compress.
#define PARAMS_CHECKS
#define DECOMPRESS_SAFETY_CHECKS
#endif

using System;
using System.Runtime.CompilerServices;

namespace DarkCaster.Compression.FastLZ
{
	/// <summary>
	/// FastLZ compressor. Not thread safe.
	/// Keep some internall stuff between runs in order to improve performance.
	/// Each class instance consume ~8KiB of heap.
	/// </summary>
	public sealed class FastLZ
	{
		private readonly int[] htab = new int[FastLZStatic.HASH_SIZE];

		public int Compress(byte[] input, int iPos, int iSz, byte[] output, int oPos)
		{
			return FastLZStatic.Compress(input, iPos, iSz, output, oPos, htab);
		}

		public int Decompress(byte[] input, int iPos, int iSz, byte[] output, int oPos)
		{
			return FastLZStatic.Decompress(input, iPos, iSz, output, oPos);
		}
	}

	/// <summary>
	/// Thread safe static version of FastLZ class.
	/// Run a little bit slower than FastLZ class, and with more pressure on heap and GC.
	/// </summary>
	public static class FastLZStatic
	{
		private const int MAX_COPY = 32;
		private const int MAX_LEN = 264;
		private const int MAX_DISTANCE = 8192;

		private const int HASH_LOG = 13;
		internal const int HASH_SIZE = (1 << HASH_LOG);
		private const int HASH_MASK = (HASH_SIZE - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int HASH_FUNCTION(byte[] p, int offset)
		{
			unchecked
			{
				int result = p[offset] | p[offset + 1] << 8;
				result ^= (p[offset + 1] | p[offset + 2] << 8) ^ (result >> (16 - HASH_LOG));
				result &= HASH_MASK;
				return result;
			}
		}

		public static int Compress(byte[] input, int iPos, int iSz, byte[] output, int oPos)
		{
			var htab = new int[HASH_SIZE];
			return Compress(input, iPos, iSz, output, oPos, htab);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int Compress(byte[] input, int iPos, int iSz, byte[] output, int oPos, int[] htab)
		{
#if PARAMS_CHECKS
			if (input == null || iPos < 0 || iSz < 0 || iPos + iSz > input.Length)
				throw new ArgumentException("Input parameters are incorrect!");
			if (output == null || oPos < 0 || oPos >= output.Length)
				throw new ArgumentException("Output parameters are incorrect!");
#endif
			//set and define limits
			int start = oPos;
			int ip_limit = iPos + iSz - 12;
			int ip_bound = iPos + iSz - 2;

			// sanity check
			if (iSz < 4)
			{
				// create literal copy only
				output[oPos++] = (byte)(iSz - 1);
				ip_bound++;
				while (iPos <= ip_bound)
					output[oPos++] = input[iPos++];
				return iSz + 1;
			}

			// initialize hash table
			for (int i = 0; i < HASH_SIZE; ++i)
				htab[i] = iPos;

			// starting with literal copy
			int copy = 2;
			output[oPos++] = MAX_COPY - 1;
			output[oPos++] = input[iPos++];
			output[oPos++] = input[iPos++];

			int refb, distance, len, anchor;

			// main loop
			while (iPos < ip_limit)
			{
				// minimum match length
				len = 3;
				// comparison starting-point
				anchor = iPos;
				// find potential match
				int hval = HASH_FUNCTION(input, iPos);
				refb = htab[hval];

				// calculate distance to the match
				distance = anchor - refb;
				// update hash table
				htab[hval] = anchor;

				// is this a match? check the first 3 bytes
				if (distance == 0 ||
						distance >= MAX_DISTANCE ||
						input[refb++] != input[iPos++] ||
						input[refb++] != input[iPos++] ||
						input[refb++] != input[iPos++])
				{
					output[oPos++] = input[anchor++];
					iPos = anchor;
					copy++;
					if (copy == MAX_COPY)
					{
						copy = 0;
						output[oPos++] = MAX_COPY - 1;
					}
					continue;
				}

				// last matched byte
				iPos = anchor + len;

				// distance is biased
				distance--;

				if (distance == 0)
				{
					// zero distance means a run
					byte x = input[iPos - 1];
					for (; iPos < ip_bound; ++iPos)
						if (input[refb++] != x)
							break;
				}
				else
					while (iPos < ip_bound)
						if (input[refb++] != input[iPos++])
							break;

				// if we have copied something, adjust the copy count
				if (copy != 0)
					// copy is biased, '0' means 1 byte copy
					output[oPos - copy - 1] = (byte)(copy - 1);
				else
					// back, to overwrite the copy count
					oPos--;

				// reset literal counter
				copy = 0;

				// length is biased, '1' means a match of 3 bytes
				iPos -= 3;
				len = iPos - anchor;
				// encode the match
				while (len > MAX_LEN - 2)
				{
					output[oPos++] = (byte)((7 << 5) + (distance >> 8));
					output[oPos++] = MAX_LEN - 11; /* 2 - 7 - 2 */
					output[oPos++] = (byte)(distance & 255);
					len -= MAX_LEN - 2;
				}

				if (len < 7)
				{
					output[oPos++] = (byte)((len << 5) + (distance >> 8));
					output[oPos++] = (byte)(distance & 255);
				}
				else
				{
					output[oPos++] = (byte)((7 << 5) + (distance >> 8));
					output[oPos++] = (byte)(len - 7);
					output[oPos++] = (byte)(distance & 255);
				}

				// update the hash at match boundary
				htab[HASH_FUNCTION(input, iPos)] = iPos++;
				htab[HASH_FUNCTION(input, iPos)] = iPos++;

				// assuming literal copy
				output[oPos++] = MAX_COPY - 1;
			}

			// left-over as literal copy
			ip_bound++;
			while (iPos <= ip_bound)
			{
				output[oPos++] = input[iPos++];
				copy++;
				if (copy == MAX_COPY)
				{
					copy = 0;
					output[oPos++] = MAX_COPY - 1;
				}
			}

			// if we have copied something, adjust the copy length
			if (copy != 0)
				output[oPos - copy - 1] = (byte)(copy - 1);
			else
				oPos--;

			return oPos - start;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Decompress(byte[] input, int iPos, int iSz, byte[] output, int oPos)
		{
#if PARAMS_CHECKS
			if (input == null || iPos < 0 || iSz < 0 || iPos + iSz > input.Length)
				throw new ArgumentException("Input parameters are incorrect!");
			if (output == null || oPos < 0 || oPos >= output.Length)
				throw new ArgumentException("Output parameters are incorrect!");
#endif
			int ip_limit = iPos + iSz;
			int start = oPos;

			int ctrl = input[iPos++] & 31;
			bool loop = true;
			int refb, len, ofs;

			do
			{
				refb = oPos;
				len = ctrl >> 5;
				ofs = (ctrl & 31) << 8;

				if (ctrl >= 32)
				{
					len--;
					refb -= ofs;
					if (len == 7 - 1)
						len += input[iPos++];
					refb -= input[iPos++];
#if DECOMPRESS_SAFETY_CHECKS
					if (refb <= start)
						throw new Exception("Decompression failed! (refb <= start)");
#endif
					if (iPos < ip_limit)
						ctrl = input[iPos++];
					else
						loop = false;

					if (refb == oPos)
					{
						/* optimize copy for a run */
						byte b = output[refb - 1];
						output[oPos++] = b;
						output[oPos++] = b;
						output[oPos++] = b;
						for (; len > 0; --len)
							output[oPos++] = b;
					}
					else
					{
						/* copy from reference */
						refb--;
						len += 3;
						//check for positions overlaping
						if (refb + len > oPos)
							//copy manually, if source and target positions overlaps
							for (; len > 0; --len)
								output[oPos++] = output[refb++];
						else
						{
							Buffer.BlockCopy(output, refb, output, oPos, len);
							oPos += len;
						}
					}
				}
				else
				{
#if DECOMPRESS_SAFETY_CHECKS
					if (iPos + ctrl >= ip_limit)
						throw new Exception("Decompression failed! (ip + ctrl >= ip_limit)");
#endif
					ctrl++;
					if (ctrl > 8)
					{
						Buffer.BlockCopy(input, iPos, output, oPos, ctrl);
						iPos += ctrl;
						oPos += ctrl;
					}
					else
						for (; ctrl > 0; ctrl--)
							output[oPos++] = input[iPos++];

					loop = iPos < ip_limit;
					if (loop)
						ctrl = input[iPos++];
				}
			}
			while (loop);

			return oPos - start;
		}
	}
}
