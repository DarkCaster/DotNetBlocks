﻿// MultiblockCompressionHelper.cs
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
namespace DarkCaster.Compression
{
	public static class MultiblockCompressionHelper
	{
		private delegate int CompressDelegate(byte[] input, int inSz, int inOffset, byte[] output, int outOffset);
		private delegate int DecompressDelegate(byte[] input, int inOffset, byte[] output, int outOffset);
		private delegate int GetOutBufSZDelegate(int inputSZ);
		private delegate int DecodeComprBlockSZ(byte[] buffer, int offset);

		private const int PAYLOAD_LEN1 = 63; //2 ^ 6 - 1 = 6 bits / 8 bit-header;
		private const int PAYLOAD_LEN2 = 16383; //2 ^ 14 - 1 = 14 bits / 16 bit-header;
		private const int PAYLOAD_LEN3 = 4194303; //2 ^ 22 - 1 = 22 bits / 24 bit-header;
		private const int PAYLOAD_LEN4 = 1073741823; //2 ^ 30 - 1 = 30 bits / 32-bit header;

		public static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, IThreadSafeBlockCompressor compressor)
		{
			var maxBlockSZ = compressor.MaxBlockSZ;
			return Compress(input, inSz, inOffset, output, outOffset, compressor.Compress, maxBlockSZ, compressor.GetOutBuffSZ(maxBlockSZ));
		}

		public static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, IBlockCompressor compressor)
		{
			var maxBlockSZ = compressor.MaxBlockSZ;
			return Compress(input, inSz, inOffset, output, outOffset, compressor.Compress, maxBlockSZ, compressor.GetOutBuffSZ(maxBlockSZ));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CalculateHeaderSz(int blockCount)
		{
			if(blockCount <= PAYLOAD_LEN1)
				return 1;
			if(blockCount <= PAYLOAD_LEN2)
				return 2;
			if(blockCount <= PAYLOAD_LEN3)
				return 3;
			if(blockCount <= PAYLOAD_LEN4)
				return 4;
			throw new Exception(string.Format("blockCount > PAYLOAD_LEN4: {0} > {1}", blockCount, PAYLOAD_LEN4));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteHeader(byte[] buffer, int offset, int blocksNumber, int hdrsz)
		{
			buffer[offset] = (byte)(blocksNumber & 0x3F);
			if(hdrsz == 1)
				return;
			buffer[offset + 1] = (byte)((blocksNumber >> 6) & 0xFF);
			if(hdrsz == 2)
			{
				buffer[offset] |= 0x40;
				return;
			}
			buffer[offset + 2] = (byte)((blocksNumber >> 14) & 0xFF);
			if(hdrsz == 3)
			{
				buffer[offset] |= 0x80;
				return;
			}
			buffer[offset + 3] = (byte)((blocksNumber >> 22) & 0xFF);
			buffer[offset] |= 0xC0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, CompressDelegate compress, int maxBlockSZ, int maxBlockOutSZ)
		{
			//calculate how many blocks do we need
			int fullBlocksCnt = inSz / maxBlockSZ;
			int remain = inSz % maxBlockSZ;
			int remainBlocks = remain > 0 ? 1 : 0;
			//calculate size of the headers - block count and last block length
			var blkCountHdrSize = CalculateHeaderSz(fullBlocksCnt + remainBlocks);
			var lastBlkLenHdrSize = CalculateHeaderSz(remain);
			//setup output counter
			var outSz = blkCountHdrSize + lastBlkLenHdrSize;
			//compress data, block by block
			for(int i = 0; i < fullBlocksCnt; ++i)
				outSz += compress(input, maxBlockSZ, inOffset + i * maxBlockSZ, output, outOffset + outSz);
			if(remain > 0)
				outSz += compress(input, remain, inOffset + fullBlocksCnt * maxBlockSZ, output, outOffset + outSz);
			//write block count header to outSz
			WriteHeader(output, outOffset, fullBlocksCnt + remainBlocks, blkCountHdrSize);
			//write last block length header
			WriteHeader(output, outOffset + blkCountHdrSize, remain, lastBlkLenHdrSize);
			return outSz;
		}

		public static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, IThreadSafeBlockCompressor compressor)
		{
			return Decompress(input, inOffset, output, outOffset, compressor.Decompress, compressor.DecodeComprBlockSZ);
		}

		public static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, IBlockCompressor compressor)
		{
			return Decompress(input, inOffset, output, outOffset, compressor.Decompress, compressor.DecodeComprBlockSZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Decompress(byte[] input, int inOffset, byte[] output, int outOffset, DecompressDelegate decompress, DecodeComprBlockSZ decodeBSZ)
		{
			//read blocks count length
			var hdrLen = (input[inOffset] >> 6 & 0X3) + 1;
			//read compressed blocks count
			int bcnt = input[inOffset] & 0x3F;
			int shift_val = 6;
			for(int shift = 1; shift < hdrLen; ++shift)
			{
				bcnt |= input[inOffset + shift] << shift_val;
				shift_val += 8;
			}
			//read length of the header with uncompressed last block size
			hdrLen += (input[inOffset + hdrLen] >> 6 & 0X3) + 1;
			//setup counters
			int decLen = 0;
			int inPos = inOffset + hdrLen;
			//decompress data block by block
			for(int b = 0; b < bcnt; ++b)
			{
				decLen += decompress(input, inPos, output, outOffset + decLen);
				inPos += decodeBSZ(input, inPos);
			}
			return decLen;
		}

		public static int DecodeComprPayloadSZ(byte[] buffer, int offset, IThreadSafeBlockCompressor compressor)
		{
			return DecodeComprPayloadSZ(buffer, offset, compressor.DecodeComprBlockSZ);
		}

		public static int DecodeComprPayloadSZ(byte[] buffer, int offset, IBlockCompressor compressor)
		{
			return DecodeComprPayloadSZ(buffer, offset, compressor.DecodeComprBlockSZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int DecodeComprPayloadSZ(byte[] buffer, int offset, DecodeComprBlockSZ decodeBSZ)
		{
			//decode 1-st header size
			var payloadLen = (buffer[offset] >> 6 & 0X3) + 1;
			//decode block count from 1-st header
			int bcnt = buffer[offset] & 0x3F;
			int shift_val = 6;
			for(int shift = 1; shift < payloadLen; ++shift)
			{
				bcnt |= buffer[offset + shift] << shift_val;
				shift_val += 8;
			}
			//append 2-nd header size
			payloadLen += (buffer[offset + payloadLen] >> 6 & 0X3) + 1;
			//append compressed sizes of all data blocks
			for(int b = 0; b < bcnt; ++b)
				payloadLen += decodeBSZ(buffer, offset + payloadLen);
			return payloadLen;
		}

		public static int GetOutBuffSZ(int inputSZ, IThreadSafeBlockCompressor compressor)
		{
			int maxBlockSZ = compressor.MaxBlockSZ;
			return GetOutBuffSZ(inputSZ, maxBlockSZ, compressor.GetOutBuffSZ(maxBlockSZ));
		}

		public static int GetOutBuffSZ(int inputSZ, IBlockCompressor compressor)
		{
			int maxBlockSZ = compressor.MaxBlockSZ;
			return GetOutBuffSZ(inputSZ, maxBlockSZ, compressor.GetOutBuffSZ(maxBlockSZ));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetOutBuffSZ(int inputSZ, int maxBlockSZ, int maxOutBlockSZ)
		{
			int fullBlocksCnt = inputSZ / maxBlockSZ;
			int remain = inputSZ % maxBlockSZ;
			int remainBlocks = remain > 0 ? 1 : 0;
			var hdrSize = CalculateHeaderSz(fullBlocksCnt + remainBlocks);
			hdrSize += CalculateHeaderSz(remain);
			return hdrSize + maxOutBlockSZ * (fullBlocksCnt + remainBlocks);
		}
	}
}
