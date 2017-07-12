// CompressionTunnelBase.cs
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
using System.Threading.Tasks;
using DarkCaster.Compression;

namespace DarkCaster.DataTransfer.Private
{
	public abstract class CompressionTunnelBase
	{
		private readonly ITunnelBase uplink;
		private readonly IBlockCompressor readCompr;
		private readonly IBlockCompressor writeCompr;

		private int uncReadPos;
		private int uncReadBSZ;
		private int readPhase;

		private int compReadPos;
		private int compReadMSZ;
		private int compReadBSZ;

		private readonly byte[] readBuff;
		private readonly byte[] readBuffCompr;

		private readonly byte[] writeBuffCompr;

		protected CompressionTunnelBase(IBlockCompressor readCompr, IBlockCompressor writeCompr, ITunnelBase uplink)
		{
			this.uplink = uplink;
			this.readCompr = readCompr;
			this.writeCompr = writeCompr;

			readBuff = new byte[readCompr.MaxBlockSZ];
			readBuffCompr = new byte[readCompr.GetOutBuffSZ(readCompr.MaxBlockSZ)];
			uncReadPos = readCompr.MaxBlockSZ;
			uncReadBSZ = 0;
			readPhase = 0;

			writeBuffCompr = new byte[writeCompr.GetOutBuffSZ(writeCompr.MaxBlockSZ)];
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			//do we need to read new compressed block
			if(uncReadPos >= uncReadBSZ && readPhase == 0)
				readPhase = 1;
			//set-up counters for new compressed block
			if(readPhase == 1)
			{
				compReadBSZ = 0;
				compReadPos = 0;
				readPhase++;
			}
			//read compressed block preview from uplink
			if(readPhase == 2)
			{
				compReadPos += await uplink.ReadDataAsync(readCompr.MetadataPreviewSZ - compReadPos, readBuffCompr, compReadPos);
				if(compReadPos < readCompr.MetadataPreviewSZ)
					return 0;
				readPhase++;
			}
			//decode compressed block metadata size
			if(readPhase == 3)
			{
				compReadMSZ = readCompr.DecodeMetadataSZ(readBuffCompr, 0);
				readPhase++;
			}
			//read remaining compressed block metadata from uplink
			if(readPhase == 4)
			{
				compReadPos += await uplink.ReadDataAsync(compReadMSZ - compReadPos, readBuffCompr, compReadPos);
				if(compReadPos < compReadMSZ)
					return 0;
				readPhase++;
			}
			//decode compressed block size
			if(readPhase == 5)
			{
				compReadBSZ = readCompr.DecodeComprBlockSZ(readBuffCompr, 0);
				if(compReadBSZ > readBuffCompr.Length)
					throw new Exception("Too much data to decompress");
				readPhase++;
			}
			//read remaining compressed block data from uplink
			if(readPhase == 6)
			{
				compReadPos += await uplink.ReadDataAsync(compReadBSZ - compReadPos, readBuffCompr, compReadPos);
				if(compReadPos < compReadBSZ)
					return 0;
				readPhase++;
			}
			//decompress data to temporary buffer
			if(readPhase == 7)
			{
				uncReadBSZ = readCompr.Decompress(readBuffCompr, 0, readBuff, 0);
				uncReadPos = 0;
				readPhase = 0;
			}
			//read decompressed data from buffer
			if(readPhase==0)
			{
				if(sz > uncReadBSZ - uncReadPos)
					sz = uncReadBSZ - uncReadPos;
				Buffer.BlockCopy(readBuff, uncReadPos, buffer, offset, sz);
				uncReadPos += sz;
			}
			return sz;
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			if(sz > writeCompr.MaxBlockSZ)
				sz = writeCompr.MaxBlockSZ;
			//compress data
			var csz = writeCompr.Compress(buffer, sz, offset, writeBuffCompr, 0);
			//write all compressed data to uplink
			int wsz = 0;
			while(wsz < csz)
				wsz += await uplink.WriteDataAsync(csz - wsz, writeBuffCompr, wsz);
			return sz;
		}

		public async Task DisconnectAsync()
		{
			await uplink.DisconnectAsync();
		}

		public void Dispose()
		{
			uplink.Dispose();
		}
	}
}
