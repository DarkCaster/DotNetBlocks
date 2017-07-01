// MultiblockCompressionHelper.cs
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
		private delegate int DecodeComprBlockSZ(byte[] buffer, int offset);

		private const int LEN_HALF = 0x7FFF;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteLen(byte[] target, int offset, int val, int hdrLen)
		{
			bool bigVal = hdrLen > 2;
			target[offset] = bigVal ? (byte)(val & 0x7F | 0x80) : (byte)(val & 0x7F);
			target[offset + 1] = (byte)((val >> 7) & 0xFF);
			if(bigVal)
			{
				target[offset + 2] = (byte)((val >> 15) & 0xFF);
				target[offset + 3] = (byte)((val >> 23) & 0xFF);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int ReadLen(byte[] target, int offset)
		{
			int result = 0;
			result |= target[offset];
			bool bigVal = (result & 0x80) > 0;
			result &= 0x7F;
			result |= target[offset + 1] << 7;
			if(bigVal)
			{
				result |= target[offset + 2] << 15;
				result |= target[offset + 3] << 23;
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CalculateRecordLen(int val)
		{
			return val > LEN_HALF ? 4 : 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int ReadRecordLen(byte[] target, int offset)
		{
			return (target[offset] & 0x80) > 0 ? 4 : 2;
		}

		public static int DecodeDecomprSZ(byte[] buffer, int offset)
		{
			return ReadLen(buffer, offset);
		}

		public static int DecodeComprSZ(byte[] buffer, int offset)
		{
			var uncHdrLen = ReadRecordLen(buffer, offset);
			var cmpHdrLen = ReadRecordLen(buffer, offset + uncHdrLen);
			return ReadLen(buffer, offset + uncHdrLen) + cmpHdrLen + uncHdrLen;
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
			//calculate maximum compressed size of data
			int blCnt = (inputSZ / maxBlockSZ) + (inputSZ % maxBlockSZ > 0 ? 1 : 0);
			//calculate records sizes
			int uncLenRecSz = CalculateRecordLen(inputSZ);
			var compPlLen = maxOutBlockSZ * blCnt;
			int compLenRecSz = CalculateRecordLen(compPlLen);
			return uncLenRecSz + compLenRecSz + compPlLen;
		}

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
		private static int Compress(byte[] input, int inSz, int inOffset, byte[] output, int outOffset, CompressDelegate compress, int maxBlockSZ, int maxOutBlockSZ)
		{
			//check input parameters
			if(input == null || inOffset < 0 || inSz < 0 || inOffset + inSz > input.Length)
				throw new ArgumentException("Input buffer parameters are incorrect!");
			if(output == null || outOffset < 0 || outOffset >= output.Length)
				throw new ArgumentException("Output buffer parameters are incorrect!");
			//calculate maximum compressed size of data
			int blCnt = (inSz / maxBlockSZ) + (inSz % maxBlockSZ > 0 ? 1 : 0);
			//check bounds
			if((long)(maxOutBlockSZ * blCnt) > int.MaxValue)
				throw new ArgumentException("inSz is too big for current compressor's parameters!", nameof(inSz));
			//calculate records sizes
			int uncLenRecSz = CalculateRecordLen(inSz);
			int compLenRecSz = CalculateRecordLen(maxOutBlockSZ * blCnt);
			//write uncompressed size
			var start = outOffset;
			WriteLen(output, start, inSz, uncLenRecSz);
			//set outOffset to area after header
			outOffset += compLenRecSz + uncLenRecSz;
			//compress data block by block
			int totalLen = 0;
			while(inSz > 0)
			{
				if(inSz >= maxBlockSZ)
				{
					totalLen += compress(input, maxBlockSZ, inOffset, output, outOffset + totalLen);
					inOffset += maxBlockSZ;
					inSz -= maxBlockSZ;
					continue;
				}
				totalLen += compress(input, inSz, inOffset, output, outOffset + totalLen);
				break;
			}
			//write total length
			WriteLen(output, start + uncLenRecSz, totalLen, compLenRecSz);
			return totalLen + compLenRecSz + uncLenRecSz;
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
			if(input == null || inOffset < 0 || inOffset >= input.Length)
				throw new ArgumentException("Input parameters are incorrect!");
			if(output == null || outOffset < 0 || outOffset >= output.Length)
				throw new ArgumentException("Output parameters are incorrect!");
			//read header-data
			var uncHdrLen = ReadRecordLen(input, inOffset); //uncompressed data size;
			var uncLen = ReadLen(input, inOffset); //read final uncompressed data size;
			inOffset += uncHdrLen;
			var cmpHdrLen = ReadRecordLen(input, inOffset); //compressed data size;
			var compLen = ReadLen(input, inOffset); //read total compressed payload size
			inOffset += cmpHdrLen;
			//decompress block by block
			int iLimit = inOffset + compLen;
			var iLen = 0;
			while(inOffset < iLimit)
			{
				iLen = decodeBSZ(input, inOffset);
				outOffset += decompress(input, inOffset, output, outOffset);
				inOffset += iLen;
			}
			return uncLen;
		}
	}
}
