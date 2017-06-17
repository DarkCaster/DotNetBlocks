﻿// FastLZBlockCompressor.cs
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
using System.Runtime.CompilerServices;

namespace DarkCaster.Compression.FastLZ
{
	public class FastLZBlockCompressor : IBlockCompressor
	{
		private const int PAYLOAD_LEN1 = 31; //2 ^ 5 - 1 = 5 bits / 8 bit-header;
		private const int PAYLOAD_LEN2 = 8191; //2 ^ 13 - 1 = 13 bits / 16 bit-header;
		private const int PAYLOAD_LEN3 = 2097151; //2 ^ 21 - 1 = 21 bits / 24 bit-header;
		private const int MAX_BLOCK_SZ = 536870911; //2 ^ 29 - 1 = 29 bits / 32-bit header;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CalculateHeaderLength(int payloadLen)
		{
			if (payloadLen <= PAYLOAD_LEN1)
				return 1;
			if (payloadLen <= PAYLOAD_LEN2)
				return 2;
			if (payloadLen <= PAYLOAD_LEN3)
				return 3;
			if (payloadLen <= MAX_BLOCK_SZ)
				return 4;
			throw new Exception(string.Format("Payload length > MAX_BLOCK_SZ: {0} > {1}", payloadLen, MAX_BLOCK_SZ));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteHeader(byte[] buffer, int offset, int lenValue, int hdrsz, bool useCompr)
		{
			buffer[offset] = (byte)( (useCompr ? 0x1 : 0x0) << 7 | (lenValue & 0x1F));
			if (hdrsz == 1)
				return;
			buffer[offset + 1] = (byte)((lenValue >> 5) & 0xFF);
			if (hdrsz == 2)
			{
				buffer[offset] |= 0x20;
				return;
			}
			buffer[offset + 2] = (byte)((lenValue >> 13) & 0xFF);
			if (hdrsz == 3)
			{
				buffer[offset] |= 0x40;
				return;
			}
			buffer[offset + 3] = (byte)((lenValue >> 21) & 0xFF);
			buffer[offset] |= 0x60;
		}

		public int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset)
		{
			var hdrSz=CalculateHeaderLength(inSz);
			var comprSz=FastLZ.Compress(input, inOffset, inSz, output, outOffset+hdrSz);
			bool useCompr = true;
			if (comprSz >= inSz)
			{
				useCompr = false;
				comprSz = inSz;
				Buffer.BlockCopy(input, inOffset, output, outOffset + hdrSz, inSz);
			}
			WriteHeader(output, outOffset, comprSz, hdrSz, useCompr);
			return comprSz + hdrSz;
		}

		public int DecodeComprBlockSZ(byte[] buffer, int offset=0)
		{
			var hdrLen = DecodeMetadataSZ(buffer, offset);
			return DecodeComprDataSz(buffer, offset, hdrLen) + hdrLen;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int DecodeMetadataSZ(byte[] buffer, int offset=0)
		{
			return (buffer[offset] >> 5 & 0X3) + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int DecodeComprDataSz(byte[] buffer, int offset, int hdrLen)
		{
			int sz = buffer[offset] & 0x1F;
			int shift_val = 5;
			for (int shift = 1; shift < hdrLen; ++shift)
			{
				sz |= buffer[offset + shift] << shift_val;
				shift_val += 8;
			}
			return sz;
		}

		public int Decompress(byte[] input, int inOffset, byte[] output, int outOffset)
		{
			var headerSz = DecodeMetadataSZ(input, inOffset);
			var comprSz = DecodeComprDataSz(input, inOffset, headerSz);
			if (input[inOffset] >> 7 == 0)
			{
				Buffer.BlockCopy(input, inOffset + headerSz, output, outOffset, comprSz);
				return comprSz;
			}
			return FastLZ.Decompress(input, inOffset + headerSz, comprSz, output, outOffset, output.Length - outOffset);
		}

		public int MaxBlockSZ { get { return MAX_BLOCK_SZ; } }

		public int GetOutBuffSZ(int inputSZ)
		{
			return inputSZ + inputSZ / 32 + CalculateHeaderLength(inputSZ) + 1;
		}
	}
}
