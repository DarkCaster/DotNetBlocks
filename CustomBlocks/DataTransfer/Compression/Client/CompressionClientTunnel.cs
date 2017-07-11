// CompressionClientTunnel.cs
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

namespace DarkCaster.DataTransfer.Client.Compression
{
	public sealed class CompressionClientTunnel : ITunnel
	{
		private readonly ITunnel downstream;
		private readonly IBlockCompressor readCompr;
		private readonly IBlockCompressor writeCompr;

		private int uncReadPos;
		private int uncReadBSZ;
		private int readPhase;

		private readonly int writeBlkSz;
		private readonly byte[] readBuff;
		private readonly byte[] readBuffCompr;

		public CompressionClientTunnel(IBlockCompressor readCompr, IBlockCompressor writeCompr, ITunnel downstream)
		{
			this.downstream = downstream;
			this.readCompr = readCompr;
			this.writeCompr = writeCompr;

			readBuff = new byte[readCompr.MaxBlockSZ];
			readBuffCompr=new byte[readCompr.GetOutBuffSZ(readCompr.MaxBlockSZ)];
			uncReadPos = readCompr.MaxBlockSZ;
			uncReadBSZ = 0;
			readPhase = 0;
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			//do we need to read new compressed block
			if(uncReadPos >= uncReadBSZ && readPhase == 0)
				readPhase = 1;
			//TODO: read compressed block preview
			//TODO: decode compressed block metadata size
			//TODO: read remaining compressed block metadata
			//TODO: decode compressed block size
			//TODO: read remaining compressed block data
			//TODO: decompress data
			//TODO: read decompressed data
			throw new NotImplementedException("TODO");
		}

		public async Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			if(sz == 0)
				return 0;
			//TODO: compress data
			//TODO: write all compressed data to downstream
			throw new NotImplementedException("TODO");
		}

		public async Task DisconnectAsync()
		{
			await downstream.DisconnectAsync();
		}

		public void Dispose()
		{
			downstream.Dispose();
		}
	}
}
