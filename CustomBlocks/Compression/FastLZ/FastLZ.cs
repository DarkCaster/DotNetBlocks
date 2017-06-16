// FastLZ.cs
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

using System;

namespace DarkCaster.Compression.FastLZ
{
	public static class FastLZ
	{
		private const int MAX_COPY = 32;
		private const int MAX_LEN = 264;
		private const int MAX_DISTANCE = 8192;

		private static uint FASTLZ_READU16(byte[] p, int offset)
		{
			return (uint)(p[offset] | (p[offset + 1] << 8));
		}

		private const int HASH_LOG = 13;
		private const int HASH_SIZE = (1 << HASH_LOG);
		private const uint HASH_MASK = (HASH_SIZE - 1);

		private static void HASH_FUNCTION(ref uint v, byte[] p, int offset)
		{
			unchecked
			{
				v = FASTLZ_READU16(p, offset);
				v ^= FASTLZ_READU16(p, offset + 1) ^ (v >> (16 - HASH_LOG));
				v &= HASH_MASK;
			}
		}

		public static int Compress(byte[] input, int iPos, int iSz, byte[] output, int oPos)
		{
			//TODO: input params check
			int start = oPos;
			int ip_bound = iPos + iSz - 2;
			int ip_limit = iPos + iSz - 12;

			int[] htab = new int[HASH_SIZE];
			int hslot;
			uint hval = 0;

			uint copy;

			// sanity check
			if (iSz < 4)
			{
				if (iSz > 0)
				{
					/* create literal copy only */
					output[oPos++] = (byte)(iSz - 1);
					ip_bound++;
					while (iPos <= ip_bound)
						output[oPos++] = input[iPos++];
					return iSz + 1;
				}
				throw new ArgumentException("length param is not valid", nameof(iSz));
			}

			// initializes hash table
			for (hslot = 0; hslot < HASH_SIZE; ++hslot)
				htab[hslot] = iPos;

			// we start with literal copy
			copy = 2;
			output[oPos++] = MAX_COPY - 1;
			output[oPos++] = input[iPos++];
			output[oPos++] = input[iPos++];

			// main loop
			while (iPos < ip_limit)
			{
				int refb;
				uint distance;

				// minimum match length
				uint len = 3;

				// comparison starting-point
				int anchor = iPos;

				// find potential match
				HASH_FUNCTION(ref hval, input, iPos);
				hslot = (int)hval;
				refb = htab[hval];

				// calculate distance to the match
				distance = (uint)(anchor - refb);

				// update hash table
				htab[hslot] = anchor;

				// is this a match? check the first 3 bytes
				if (distance == 0 ||
						distance >= MAX_DISTANCE ||
						input[refb++] != input[iPos++] ||
						input[refb++] != input[iPos++] ||
						input[refb++] != input[iPos++])
					goto literal;

				// last matched byte
				iPos = (int)(anchor + len);

				// distance is biased
				distance--;

				if (distance == 0)
				{
					// zero distance means a run
					byte x = input[iPos - 1];
					while (iPos < ip_bound)
						if (input[refb++] != x)
							break;
						else
							iPos++;
				}
				else
					for (;;)
					{
						// safe because the outer check against ip limit
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						if (input[refb++] != input[iPos++]) break;
						while (iPos < ip_bound)
							if (input[refb++] != input[iPos++]) break;
						break;
					}

				// if we have copied something, adjust the copy count
				if (copy != 0)
					// copy is biased, '0' means 1 byte copy
					output[oPos - copy - 1] = (byte)(copy - 1);
				else
					// back, to overwrite the copy count
					oPos--;

				// reset literal counter
				copy = 0u;

				// length is biased, '1' means a match of 3 bytes
				iPos -= 3;
				len = (uint)(iPos - anchor);

				// encode the match
				if (len > MAX_LEN - 2)
					while (len > MAX_LEN - 2)
					{
						output[oPos++] = (byte)((7 << 5) + (distance >> 8));
						output[oPos++] = MAX_LEN - 2 - 7 - 2;
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
				HASH_FUNCTION(ref hval, input, iPos);
				htab[hval] = iPos++;
				HASH_FUNCTION(ref hval, input, iPos);
				htab[hval] = iPos++;

				// assuming literal copy
				output[oPos++] = MAX_COPY - 1;

				continue;

			literal:
				output[oPos++] = input[anchor++];
				iPos = anchor;
				copy++;
				if (copy == MAX_COPY)
				{
					copy = 0;
					output[oPos++] = MAX_COPY - 1;
				}
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

		public static int Decompress(byte[] input, int ip, int length, byte[] output, int op, int maxout)
		{
			//TODO: check input params

			int ip_limit = ip + length;
			int op_limit = op + maxout;
			int start = op;

			int ctrl = input[ip++] & 31;
			bool loop = true;

			do
			{
				int refb = op;
				int len = ctrl >> 5;
				int ofs = (ctrl & 31) << 8;

				if (ctrl >= 32)
				{
					len--;
					refb -= ofs;
					if (len == 7 - 1)
						len += input[ip++];
					refb -= input[ip++];

					if (op + len + 3 > op_limit)
						throw new Exception("Decompression failed! (op + len + 3 > op_limit)");

					if (refb <= start)
						throw new Exception("Decompression failed! (refb <= start)");

					if (ip < ip_limit)
						ctrl = input[ip++];
					else
						loop = false;

					if (refb == op)
					{
						/* optimize copy for a run */
						byte b = output[refb - 1];
						output[op++] = b;
						output[op++] = b;
						output[op++] = b;
						for (; len > 0; --len)
							output[op++] = b;
					}
					else
					{
						/* copy from reference */
						refb--;
						output[op++] = output[refb++];
						output[op++] = output[refb++];
						output[op++] = output[refb++];

						for (; len > 0; --len)
							output[op++] = output[refb++];
					}
				}
				else
				{
					ctrl++;

					if (op + ctrl > op_limit)
						throw new Exception("Decompression failed! (op + ctrl > op_limit)");

					if (ip + ctrl > ip_limit)
						throw new Exception("Decompression failed! (ip + ctrl > ip_limit)");

					output[op++] = input[ip++];
					for (--ctrl; ctrl > 0; ctrl--)
						output[op++] = input[ip++];

					loop = ip < ip_limit;
					if (loop)
						ctrl = input[ip++];
				}
			}
			while (loop);

			return op - start;
		}
	}
}
