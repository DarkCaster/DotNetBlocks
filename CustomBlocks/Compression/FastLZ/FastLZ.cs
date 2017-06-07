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

namespace DarkCaster.Compression.FastLZ
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

		public static int Compress(byte[] input, int iPos, int iSz, byte[] output, int oPos, bool fastSpeed=true)
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

				if (!fastSpeed)
				{
					// check for a run
					if (input[iPos] == input[iPos - 1] && FASTLZ_READU16(input, iPos - 1) == FASTLZ_READU16(input, iPos + 1))
					{
						distance = 1;
						iPos += 3;
						refb = anchor - 1 + 3;
						goto match;
					}
				}

				/* find potential match */
				HASH_FUNCTION(ref hval, input, iPos);
				hslot = (int)hval;
				refb = htab[hval];

				/* calculate distance to the match */
				distance = (uint)(anchor - refb);

				/* update hash table */
				htab[hslot] = anchor;

				/* is this a match? check the first 3 bytes */




				if (distance == 0 || (fastSpeed && distance >= MAX_DISTANCE1) || (!fastSpeed && distance >= MAX_FARDISTANCE) ||
						input[refb++] != input[iPos++] || input[refb++] != input[iPos++] || input[refb++] != input[iPos++])
					goto literal;

				if (!fastSpeed)
				{
					// far, needs at least 5-byte match
					if (distance >= MAX_DISTANCE2)
					{
						if (input[iPos++] != input[refb++] || input[iPos++] != input[refb++])
							goto literal;
						len += 2;
					}
				}
			match:

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
				if (!fastSpeed)
				{
					if (distance < MAX_DISTANCE2)
					{
						if (len < 7)
						{
							output[oPos++] = (byte)((len << 5) + (distance >> 8));
							output[oPos++] = (byte)(distance & 255);
						}
						else
						{
							output[oPos++] = (byte)((7 << 5) + (distance >> 8));
							for (len -= 7; len >= 255; len -= 255)
								output[oPos++] = 255;
							output[oPos++] = (byte)len;
							output[oPos++] = (byte)(distance & 255);
						}
					}
					else
					{
						/* far away, but not yet in the another galaxy... */
						if (len < 7)
						{
							distance -= MAX_DISTANCE2;
							output[oPos++] = (byte)((len << 5) + 31);
							output[oPos++] = 255;
							output[oPos++] = (byte)(distance >> 8);
							output[oPos++] = (byte)(distance & 255);
						}
						else
						{
							distance -= MAX_DISTANCE2;
							output[oPos++] = (7 << 5) + 31;
							for (len -= 7; len >= 255; len -= 255)
								output[oPos++] = 255;
							output[oPos++] = (byte)len;
							output[oPos++] = 255;
							output[oPos++] = (byte)(distance >> 8);
							output[oPos++] = (byte)(distance & 255);
						}
					}

				}
				else
				{
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

			if (!fastSpeed)
				/* marker for fastlz2 */
				output[start] |= (1 << 5);

			return oPos - start;
		}
	}
}
