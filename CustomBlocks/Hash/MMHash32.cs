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
		private const uint c1 = 0xcc9e2d51;
		private const uint c2 = 0x1b873593;
			
		public static uint GetHash( byte[] data, uint seed )
		{
			uint k1=0U; //Currently processed data chunk
			uint h1=seed; //Temporary hash value
			uint pos=0u; //Currently processed data pos
			
			int fullChunks=data.Length / 4;
			for(int i=0;i<fullChunks;++i)
			{
				k1 = (uint)( data[pos] | data[pos+1] << 8 | data[pos+2] << 16 | data[pos+3] << 24 );
				
				k1 *= c1;
				k1 = ( k1 << 15 ) | ( k1 >> 17/*(32 - 15)*/); //RotL32(k1, 15);
				k1 *= c2;
				h1 ^= k1;
				
				h1 = ( h1 << 13 ) | ( h1 >> 19/*(32 - 13)*/); //RotL32(h1, 13);
				h1 = h1 * 5 + 0xe6546b64;
				pos += 4U;
			}
			
			int remainder=data.Length % 4;
			if(remainder>0)
			{
				k1=0U;
				for(int i=0;i<remainder;++i)
					k1 |= (uint)(data[pos+i]<<(i*8));
				
				k1 *= c1;
				k1 = ( k1 << 15 ) | ( k1 >> 17/*(32 - 15)*/); //RotL32(k1, 15);
				k1 *= c2;
				h1 ^= k1;
				
				pos+=(uint)remainder;
			}
			
			//Finalize hash
			h1 ^= pos;
			h1 ^= h1 >> 16;
			h1 *= 0x85ebca6b;
			h1 ^= h1 >> 13;
			h1 *= 0xc2b2ae35;
			h1 ^= h1 >> 16;
			return h1;
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
