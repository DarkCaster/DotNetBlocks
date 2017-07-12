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
	public sealed class CompressionServerNode : INode
	{
		private readonly int maxBlockSZ;
		private readonly IBlockCompressorFactory comprFactory;
		private readonly INode upstreamNode;
		private volatile INode downstreamNode;

		public CompressionServerNode(ITunnelConfig serverConfig, INode upstream, IBlockCompressorFactory comprFactory, int defaultMaxBlockSZ = 65536)
		{
			this.comprFactory = comprFactory;
			maxBlockSZ = serverConfig.Get<int>("compr_max_block_size");
			if(maxBlockSZ == 0)
				maxBlockSZ = defaultMaxBlockSZ;
			upstreamNode = upstream;
			upstreamNode.RegisterDownstream(this);
		}

		public async Task InitAsync()
		{
			await upstreamNode.InitAsync();
		}

		public void RegisterDownstream(INode downstream)
		{
			downstreamNode = downstream;
		}

		public async Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream)
		{
			int blockSize = 0;
			try
			{
				var ng = new byte[5];
				//receive magic and block size
				int ngPos = 0;
				while(ngPos < ng.Length)
					ngPos += await upstream.ReadDataAsync(ng.Length - ngPos, ng, ngPos);
				CompressionMagicHelper.DecodeMagicAndBlockSZ(ng, 0, out short magic, out blockSize);
				//compare magic
				if(magic != comprFactory.Magic)
					throw new Exception("Magic mismatch");
				//check block size
				if(blockSize > maxBlockSZ)
					blockSize = maxBlockSZ;
				//send back valid block size
				CompressionMagicHelper.EncodeBlockSZ(blockSize, ng, 0);
				ngPos = 0;
				while(ngPos < 3)
					ngPos += await upstream.WriteDataAsync(3 - ngPos, ng, ngPos);
			}
			catch
			{
				//close upstream tunnel on any error, and return
				await upstream.DisconnectAsync();
				upstream.Dispose();
				return;
			}
			//add block size to config
			config.Set("compr_block_size", blockSize);
			//create read and write compressors
			var readCompr = comprFactory.GetCompressor(blockSize);
			var writeCompr = comprFactory.GetCompressor(blockSize);
			//create new tunnel and connect it with given upstream tunnel
			var tunnel = new CompressionServerTunnel(readCompr, writeCompr, upstream);
			//call downstream's OpenTunnelAsync and pass created and prepared tunnel to it
			await downstreamNode.OpenTunnelAsync(config, tunnel);
		}

		public async Task NodeFailAsync(Exception ex)
		{
			await downstreamNode.NodeFailAsync(ex);
		}

		public async Task ShutdownAsync()
		{
			await upstreamNode.ShutdownAsync();
		}

		public void Dispose()
		{
			upstreamNode.Dispose();
		}
	}
}
