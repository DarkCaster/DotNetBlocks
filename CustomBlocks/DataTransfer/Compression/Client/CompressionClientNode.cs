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

namespace DarkCaster.DataTransfer.Client.Compression
{
	public sealed class CompressionClientNode : INode
	{
		private readonly IBlockCompressorFactory comprFactory;
		private readonly INode downstream;
		private readonly int defaultBlockSz;

		public CompressionClientNode(INode downstream, IBlockCompressorFactory comprFactory, int defaultBlockSz=16384)
		{
			this.downstream = downstream;
			this.comprFactory = comprFactory;
			this.defaultBlockSz = defaultBlockSz;
		}

		public async Task<ITunnel> OpenTunnelAsync(ITunnelConfig config)
		{
			//create downstream tunnel
			var dTun = await downstream.OpenTunnelAsync(config);
			//parse compressors-parameters
			var blockSz = config.Get<int>("compr_block_size");
			if(blockSz == 0)
				blockSz = defaultBlockSz;
			//create separate compressors for read and write routines
			var readCompressor = comprFactory.GetCompressor(blockSz);
			var writeCompressor = comprFactory.GetCompressor(blockSz);
			try
			{
				//TODO:create and return compression-tunnel
				throw new NotImplementedException("TODO");
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
