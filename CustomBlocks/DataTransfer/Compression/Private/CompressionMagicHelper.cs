// CompressionMagicHelper.cs
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
namespace DarkCaster.DataTransfer.Private
{
	public static class CompressionMagicHelper
	{
		public static void EncodeMagic(short magic, byte[] buffer, int offset)
		{
			buffer[offset] = (byte)(magic & 0xFF);
			buffer[offset + 1] = (byte)((magic >> 8) & 0xFF);
		}

		public static short DecodeMagic(byte[] buffer, int offset)
		{
			return unchecked((short)(buffer[offset + 1] << 8 | buffer[offset]));
		}

		public static void EncodeBlockSZ(int size, byte[] buffer, int offset)
		{
			if(size > 0xFFFFFF)
				throw new Exception("Block size too big");
			buffer[offset] = (byte)(size & 0xFF);
			buffer[offset + 1] = (byte)((size >> 8) & 0xFF);
			buffer[offset + 2] = (byte)((size >> 16) & 0xFF);
		}

		public static int DecodeBlockSZ(byte[] buffer, int offset)
		{
			return unchecked((short)(buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16));
		}

		public static void EncodeMagicAndBlockSZ(short magic, int bSize, byte[] buffer, int offset)
		{
			EncodeMagic(magic, buffer, offset);
			EncodeBlockSZ(bSize, buffer, offset + 2);
		}

		public static void DecodeMagicAndBlockSZ(byte[] buffer, int offset, out short magic, out int bSize)
		{
			magic = DecodeMagic(buffer, offset);
			bSize = DecodeBlockSZ(buffer, offset + 2);
		}
	}
}
