// CompressionServerNode.cs
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
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Private;

namespace DarkCaster.DataTransfer.Server.Compression
{
	public sealed class CompressionServerNode : ServerNodeBase
	{
		private readonly int maxBlockSZ;
		private readonly IBlockCompressorFactory comprFactory;
		private readonly bool useAutoBSZ;

		public CompressionServerNode(ITunnelConfig serverConfig, INode upstream, IBlockCompressorFactory comprFactory, int defaultMaxBlockSZ = 65536)
			: base(upstream)
		{
			this.comprFactory = comprFactory;
			maxBlockSZ = serverConfig.Get<int>("compr_max_block_size");
			if (maxBlockSZ == 0)
				maxBlockSZ = defaultMaxBlockSZ;
			useAutoBSZ = serverConfig.Get<bool>("use_auto_buff_size");
		}

		public override async Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			ITunnel tunnel = null;
			int blockSize = 0;
			try
			{
				int lastBSZ = 0;
				if (useAutoBSZ)
					lastBSZ = config.Get<int>("auto_buff_size");
				IBlockCompressor readCompr = null;
				IBlockCompressor writeCompr = null;
				if (lastBSZ > 0)
				{
					blockSize = lastBSZ - 4; //maximum compressor-metadata header size. TODO: dynamically detect from compressor
					if (blockSize < 1)
						throw new Exception("Automatically calculated blockSize is too small!");
					//create read and write compressors (may throw an error, if block size is invalid)
					readCompr = comprFactory.GetCompressor(blockSize);
					writeCompr = comprFactory.GetCompressor(blockSize);
				}
				else
				{
					var ng = new byte[4];
					//receive requested block size
					int ngPos = 0;
					while (ngPos < ng.Length)
						ngPos += await upstream.ReadDataAsync(ng.Length - ngPos, ng, ngPos);
					blockSize = ng[0] | ng[1] << 8 | ng[2] << 16 | ng[3] << 24;
					//check block size
					if (blockSize > maxBlockSZ)
						blockSize = maxBlockSZ;
					ng[0] = (byte)(blockSize & 0xFF);
					ng[1] = (byte)((blockSize >> 8) & 0xFF);
					ng[2] = (byte)((blockSize >> 16) & 0xFF);
					ng[3] = (byte)((blockSize >> 24) & 0xFF);
					//create read and write compressors (may throw an error, if block size is invalid)
					readCompr = comprFactory.GetCompressor(blockSize);
					writeCompr = comprFactory.GetCompressor(blockSize);
					//send valid block size back to client
					ngPos = 0;
					while (ngPos < ng.Length)
						ngPos += await upstream.WriteDataAsync(ng.Length - ngPos, ng, ngPos);
				}
				//add block size to config, may be used later for informational purposes
				config.Set<int>("compr_block_size", blockSize);
				config.Set<int>("buff_size", blockSize);
				//save final block size to config, may be used by downstream node to perform correction to it's internal buffer's sizes
				config.Set<int>("auto_buff_size", blockSize);
				//create new tunnel and connect it with given upstream tunnel
				tunnel = new CompressionServerTunnel(readCompr, writeCompr, upstream);
			}
			catch //TODO: add exceptions forwarding
			{
				//close upstream tunnel on any error, and return
				await upstream.DisconnectAsync();
				upstream.Dispose();
				return;
			}
			//call downstream's OpenTunnelAsync and pass created and prepared tunnel to it
			await downstreamNode.OpenTunnelAsync(config, tunnel);
		}
	}
}
