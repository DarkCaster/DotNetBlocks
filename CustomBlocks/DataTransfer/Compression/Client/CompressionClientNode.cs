// CompressionClientNode.cs
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

namespace DarkCaster.DataTransfer.Client.Compression
{
	public sealed class CompressionClientNode : INode
	{
		private readonly IBlockCompressorFactory comprFactory;
		private readonly INode downstream;
		private readonly int defaultBlockSz;
		private readonly int extraBlockSize;

		public CompressionClientNode(INode downstream, IBlockCompressorFactory comprFactory, int extraBlockSize = 0, int defaultBlockSz = 16384)
		{
			if (!comprFactory.MetadataPreviewSupported)
				throw new Exception("Metadata preview feature is not supported by selected compressor, cannot proceed!");
			this.downstream = downstream;
			this.comprFactory = comprFactory;
			this.defaultBlockSz = defaultBlockSz;
			this.extraBlockSize = extraBlockSize;
		}

		public async Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			//create downstream tunnel
			var dTun = await downstream.OpenTunnelAsync(config);
			//parse compressors-parameters
			var blockSz = config.Get<int>("compr_block_size");
			if(blockSz <= 0)
				blockSz = defaultBlockSz;
			blockSz += extraBlockSize;
			//try to read buffer size used by downstream tunnel
			int lastBSZ = 0;
			if (config.Get<bool>("use_auto_buff_size"))
				lastBSZ = config.Get<int>("last_buff_size");
			try
			{
				IBlockCompressor readCompressor = null;
				IBlockCompressor writeCompressor = null;
				if (lastBSZ > 0)
				{
					blockSz = lastBSZ - 4; //maximum compressor-metadata header size. TODO: dynamically detect from compressor
					if (blockSz < 1)
						throw new Exception("Automatically calculated blockSize is too small!");
					//create read and write compressors (may throw an error, if block size is invalid)
					readCompressor = comprFactory.GetCompressor(blockSz);
					writeCompressor = comprFactory.GetCompressor(blockSz);
				}
				else
				{
					//send compressor magic and block size
					var ng = new byte[4];
					ng[0] = (byte)(blockSz & 0xFF);
					ng[1] = (byte)((blockSz >> 8) & 0xFF);
					ng[2] = (byte)((blockSz >> 16) & 0xFF);
					ng[3] = (byte)((blockSz >> 24) & 0xFF);
					//send compressor magic and block size
					int ngPos = 0;
					while (ngPos < ng.Length)
						ngPos += await dTun.WriteDataAsync(ng.Length - ngPos, ng, ngPos);
					//receive block size confirmation from server
					ngPos = 0;
					while (ngPos < ng.Length)
						ngPos += await dTun.ReadDataAsync(ng.Length - ngPos, ng, ngPos);
					//set block size, reported by server
					blockSz = ng[0] | ng[1] << 8 | ng[2] << 16 | ng[3] << 24;
					//create read and write compressors (may throw an error, if block size is invalid)
					readCompressor = comprFactory.GetCompressor(blockSz);
					writeCompressor = comprFactory.GetCompressor(blockSz);
				}
				//save final block size to config, may be used by upstream node to perform correction to it's internal buffer's sizes
				config.Set<int>("last_buff_size", blockSz);
				return new CompressionClientTunnel(readCompressor, writeCompressor, dTun);
			}
			catch
			{
				//in case of error - disconnect and dispose downstream tunnel
				await dTun.DisconnectAsync();
				dTun.Dispose();
				//forward exception from current tunnel
				throw;
			}
		}
	}
}
