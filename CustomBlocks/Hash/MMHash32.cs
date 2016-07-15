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

// A 32-bit MurMurHash v3 hash-function algorithm implementation.
// Based on this doc: https://en.wikipedia.org/wiki/MurmurHash

using System;

namespace DarkCaster.Hash
{
	/// <summary>
	/// A 32-bit MurMurHash v3 hash function
	/// Use static GetHash method (seed param must be passed),
	/// or create MMHash32 object with some seed, and use GetHash method (without seed param).
	/// </summary>
	public class MMHash32
	{
		public static uint GetHash( byte[] data, uint seed )
		{
			uint k=0U; //key
			uint hash=seed; //seed
			uint pos=0u; //len
			
			int fullChunks=data.Length / 4;
			for(int i=0;i<fullChunks;++i)
			{
				unchecked
				{
					k = (uint)( data[pos] | data[pos+1] << 8 | data[pos+2] << 16 | data[pos+3] << 24 );
					k *= 0xcc9e2d51;
					k = ( k << 15 ) | ( k >> 17 ); // k <- (k ROL r1)
					k *= 0x1b873593;
					hash ^= k;
					hash = ( hash << 13 ) | ( hash >> 19 ); // hash <- (hash ROL r2)
					hash = hash * 5 + 0xe6546b64;
				}
				pos += 4U;
			}
			
			int remainder=data.Length % 4;
			if(remainder>0)
			{
				k=0U;
				for(int i=0;i<remainder;++i)
					k |= (uint)(data[pos+i]<<(i*8));
				unchecked
				{
					k *= 0xcc9e2d51;
					k = ( k << 15 ) | ( k >> 17 ); // remainingBytes <- (remainingBytes ROL r1)
					k *= 0x1b873593;
					hash ^= k;
				}
				pos+=(uint)remainder;
			}
			
			//Finalize hash
			unchecked
			{
				hash ^= pos;
				hash ^= hash >> 16;
				hash *= 0x85ebca6b;
				hash ^= hash >> 13;
				hash *= 0xc2b2ae35;
				hash ^= hash >> 16;
			}
			return hash;
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
