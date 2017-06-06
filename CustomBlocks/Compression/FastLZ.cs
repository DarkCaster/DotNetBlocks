// FastLZ.cs
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
// This is a C# port of FastLZ library by (c) Ariya Hidayat, MIT License
// See http://fastlz.org for more info.
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

namespace DarkCaster.Compression
{
	public static class FastLZ
	{
		private const int MAX_COPY = 32;
		private const int MAX_LEN = 264;
		private const int MAX_DISTANCE1 = 8192;
		private const int MAX_DISTANCE2 = MAX_DISTANCE1 - 1;
		private const int MAX_FARDISTANCE = (65535 + MAX_DISTANCE2 - 1);

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

		public static int Compress(byte[] input, int ip, int length, byte[] output, int op, int level)
		{
			//TODO: input params check
			int start = op;
			int ip_bound = ip + length - 2;
			int ip_limit = ip + length - 12;

			int[] htab = new int[HASH_SIZE];
			int hslot;
			uint hval = 0;

			uint copy;

			// sanity check
			if (length < 4)
			{
				if (length > 0)
				{
					/* create literal copy only */
					output[op++] = (byte)(length - 1);
					ip_bound++;
					while (ip <= ip_bound)
						output[op++] = input[ip++];
					return length + 1;
				}
				throw new ArgumentException("length param is not valid", nameof(length));
			}

			// initializes hash table
			for (hslot = 0; hslot < HASH_SIZE; ++hslot)
				htab[hslot] = ip;

			// we start with literal copy
			copy = 2;
			output[op++] = MAX_COPY - 1;
			output[op++] = input[ip++];
			output[op++] = input[ip++];

			// main loop
			while (ip < ip_limit)
			{
				int refb;
				uint distance;

				// minimum match length
				uint len = 3;

				// comparison starting-point
				int anchor = ip;

				if (level == 2)
				{
					// check for a run
					if (input[ip] == input[ip - 1] && FASTLZ_READU16(input, ip - 1) == FASTLZ_READU16(input, ip + 1))
					{
						distance = 1;
						ip += 3;
						refb = anchor - 1 + 3;
						goto match;
					}
				}

				/* find potential match */
				HASH_FUNCTION(ref hval, input, ip);
				hslot = (int)hval;
				refb = htab[hval];

				/* calculate distance to the match */
				distance = (uint)(anchor - refb);

				/* update hash table */
				htab[hslot] = anchor;

				/* is this a match? check the first 3 bytes */




				if (distance == 0 || (level < 2 && distance >= MAX_DISTANCE1) || (level == 2 && distance >= MAX_FARDISTANCE) ||
						input[refb++] != input[ip++] || input[refb++] != input[ip++] || input[refb++] != input[ip++])
					goto literal;

				if (level == 2)
				{
					// far, needs at least 5-byte match
					if (distance >= MAX_DISTANCE2)
					{
						if (input[ip++] != input[refb++] || input[ip++] != input[refb++])
							goto literal;
						len += 2;
					}
				}
			match:

				// last matched byte
				ip = (int)(anchor + len);

				// distance is biased
				distance--;

				if (distance == 0)
				{
					// zero distance means a run
					byte x = input[ip - 1];
					while (ip < ip_bound)
						if (input[refb++] != x)
							break;
						else
							ip++;
				}
				else
					for (;;)
					{
						// safe because the outer check against ip limit
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						if (input[refb++] != input[ip++]) break;
						while (ip < ip_bound)
							if (input[refb++] != input[ip++]) break;
						break;
					}

				// if we have copied something, adjust the copy count
				if (copy != 0)
					// copy is biased, '0' means 1 byte copy
					output[op - copy - 1] = (byte)(copy - 1);
				else
					// back, to overwrite the copy count
					op--;

				// reset literal counter
				copy = 0u;

				// length is biased, '1' means a match of 3 bytes
				ip -= 3;
				len = (uint)(ip - anchor);

				// encode the match
				if (level == 2)
				{
					if (distance < MAX_DISTANCE2)
					{
						if (len < 7)
						{
							output[op++] = (byte)((len << 5) + (distance >> 8));
							output[op++] = (byte)(distance & 255);
						}
						else
						{
							output[op++] = (byte)((7 << 5) + (distance >> 8));
							for (len -= 7; len >= 255; len -= 255)
								output[op++] = 255;
							output[op++] = (byte)len;
							output[op++] = (byte)(distance & 255);
						}
					}
					else
					{
						/* far away, but not yet in the another galaxy... */
						if (len < 7)
						{
							distance -= MAX_DISTANCE2;
							output[op++] = (byte)((len << 5) + 31);
							output[op++] = 255;
							output[op++] = (byte)(distance >> 8);
							output[op++] = (byte)(distance & 255);
						}
						else
						{
							distance -= MAX_DISTANCE2;
							output[op++] = (7 << 5) + 31;
							for (len -= 7; len >= 255; len -= 255)
								output[op++] = 255;
							output[op++] = (byte)len;
							output[op++] = 255;
							output[op++] = (byte)(distance >> 8);
							output[op++] = (byte)(distance & 255);
						}
					}

				}
				else
				{
					if (len > MAX_LEN - 2)
						while (len > MAX_LEN - 2)
						{
							output[op++] = (byte)((7 << 5) + (distance >> 8));
							output[op++] = MAX_LEN - 2 - 7 - 2;
							output[op++] = (byte)(distance & 255);
							len -= MAX_LEN - 2;
						}

					if (len < 7)
					{
						output[op++] = (byte)((len << 5) + (distance >> 8));
						output[op++] = (byte)(distance & 255);
					}
					else
					{
						output[op++] = (byte)((7 << 5) + (distance >> 8));
						output[op++] = (byte)(len - 7);
						output[op++] = (byte)(distance & 255);
					}
				}

				// update the hash at match boundary
				HASH_FUNCTION(ref hval, input, ip);
				htab[hval] = ip++;
				HASH_FUNCTION(ref hval, input, ip);
				htab[hval] = ip++;

				// assuming literal copy
				output[op++] = MAX_COPY - 1;

				continue;

			literal:
				output[op++] = input[anchor++];
				ip = anchor;
				copy++;
				if (copy == MAX_COPY)
				{
					copy = 0;
					output[op++] = MAX_COPY - 1;
				}
			}

			// left-over as literal copy
			ip_bound++;
			while (ip <= ip_bound)
			{
				output[op++] = input[ip++];
				copy++;
				if (copy == MAX_COPY)
				{
					copy = 0;
					output[op++] = MAX_COPY - 1;
				}
			}

			// if we have copied something, adjust the copy length
			if (copy != 0)
				output[op - copy - 1] = (byte)(copy - 1);
			else
				op--;

			if (level == 2)
				/* marker for fastlz2 */
				output[start] |= (1 << 5);

			return op - start;
		}
	}
}
