// MMHash32.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2014 DarkCaster <dark.caster@outlook.com>
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

// A 32-bit MurMurHash v3 hash-function algorithm implementation, from my old projects.
// I do not remember exactly what examples\docs I've used as base while creating this code.
// So, if you think that I should mention you or place here a link to your code\site - contact me.

using System;

namespace DarkCaster.Hash
{
	/// <summary>
	/// A 32-bit MurMurHash v3 hash function
	/// </summary>
	public class MMHash32
	{
		private struct BitMagic
		{
			private const uint c1 = 0xcc9e2d51;
			private const uint c2 = 0x1b873593;

			private uint k1; //Currently processed data chunk
			private uint h1; //Temporary hash value
			private uint len; //Currently processed data len

			//Hash function must be initialized with seed.
			//Different seed = different results
			public static BitMagic Create( uint seed )
			{
				BitMagic result;
				result.h1 = seed;
				result.k1 = 0u;
				result.len = 0u;
				return result;
			}

			private void DoMagic()
			{
				k1 *= c1;
				k1 = ( k1 << 15 ) | ( k1 >> 17/*(32 - 15)*/); //RotL32(k1, 15);
				k1 *= c2;
				h1 ^= k1;
			}

			//Process data as 4-byte chunks
			public void ProceedChunk( byte[] chunk )
			{
				k1 = (uint)( chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24 );
				DoMagic();
				h1 = ( h1 << 13 ) | ( h1 >> 19/*(32 - 13)*/); //RotL32(h1, 13);
				h1 = h1 * 5 + 0xe6546b64;
				len += 4;
			}

			//Process last piece of data, and get output hash
			public uint ProceedLastChunk( byte[] chunk, uint chunkLen )
			{
				if( chunkLen == 4 )
					ProceedChunk(chunk);
				else
				{
					len += chunkLen;
					if( chunkLen == 3 )
						k1 = (uint)( chunk[0] | chunk[1] << 8 | chunk[2] << 16 );
					else if( chunkLen == 2 )
						k1 = (uint)( chunk[0] | chunk[1] << 8 );
					else if( chunkLen == 1 )
						k1 = (uint)( chunk[0] );
					if( chunkLen > 0 )
						DoMagic();
				}
				//Finalize hash
				h1 ^= len;
				h1 ^= h1 >> 16;
				h1 *= 0x85ebca6b;
				h1 ^= h1 >> 13;
				h1 *= 0xc2b2ae35;
				h1 ^= h1 >> 16;
				return h1;
			}
		}
		
		private static uint FillChunk( byte[] source, uint sourcePos, byte[] chunk )
		{
			uint chunkLen=0U;
			while( chunkLen < 4U && sourcePos < source.Length )
			{
				chunk[chunkLen] = source[sourcePos];
				++chunkLen;
				++sourcePos;
			}
			return chunkLen;
		}

		public static uint GetHash( byte[] stream, uint seed )
		{
			BitMagic magic=BitMagic.Create(seed);
			byte[] chunk=new byte[4];
			uint streamPos = 0U;
			uint chunkLen = FillChunk(stream, streamPos, chunk);
			streamPos += chunkLen;
			while( streamPos < stream.Length )
			{
				magic.ProceedChunk(chunk);
				chunkLen = FillChunk(stream, streamPos, chunk);
				streamPos += chunkLen;
			}
			return magic.ProceedLastChunk(chunk, chunkLen);
		}
		
		private readonly uint seed;
		
		public MMHash32()
		{
			seed=0U;
		}
		
		public MMHash32(uint seed)
		{
			this.seed=seed;
		}
		
		public uint GetHash( byte[] stream )
		{
			return GetHash(stream, seed);
		}
	}
}
