// CompressionDataTransferNodeTests.cs
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
using DarkCaster.Compression.FastLZ;
using DarkCaster.Serialization.Binary;
using DarkCaster.DataTransfer.Config;
using DarkCaster.DataTransfer.Client.Compression;
using DarkCaster.DataTransfer.Server.Compression;
using Tests.Mocks.DataLoop;
using NUnit.Framework;

using SNode = DarkCaster.DataTransfer.Server.INode;
using CNode = DarkCaster.DataTransfer.Client.INode;

namespace Tests
{
	[TestFixture]
	public class DT_CompressionNodeTests
	{
		[Test]
		public void NewConnection()
		{
			var maxSvBlockSizes = new int[] { 1, 2, 16, 50, 64, 1000, 1024, 32768 };
			var maxClBlockSizes = new int[] { 1, 2, 16, 50, 64, 1000, 1024, 32768 };

			foreach (var maxSvBlockSize in maxSvBlockSizes)
				foreach (var maxClBlockSize in maxClBlockSizes)
				{
					var svConfig = new TunnelConfig();
					//add mock parameters
					svConfig.Set("mock_min_block_size", 64);
					svConfig.Set("mock_max_block_size", 128);
					svConfig.Set("mock_read_timeout", 5000);
					svConfig.Set("mock_fail_prob", 0.0f);
					svConfig.Set("mock_nofail_ops_count", int.MaxValue);
					//add compression parameters
					svConfig.Set("compr_max_block_size", maxSvBlockSize);
					var serverLoopMock = new MockServerLoopNode(svConfig, new TunnelConfigFactory(new BinarySerializationHelperFactory()));
					var serverComprNode = new CompressionServerNode(svConfig, serverLoopMock, new FastLZBlockCompressorFactory(),maxSvBlockSize);
					var clConfig = new TunnelConfig();
					var clientLoopMock = new MockClientLoopNode();
					var clientComprNode = new CompressionClientNode(clientLoopMock, new FastLZBlockCompressorFactory(), maxClBlockSize, maxClBlockSize);
					CommonDataTransferTests.NewConnection(clConfig, clientComprNode, clientLoopMock, serverComprNode, serverLoopMock);
				}
		}

		[Test]
		public void ReadWrite()
		{
			var svConfig = new TunnelConfig();
			//add mock parameters
			svConfig.Set("mock_min_block_size", 4096);
			svConfig.Set("mock_max_block_size", 8192);
			svConfig.Set("mock_read_timeout", 5000);
			svConfig.Set("mock_fail_prob", 0.0f);
			svConfig.Set("mock_nofail_ops_count", int.MaxValue);
			//add compression parameters
			svConfig.Set("compr_max_block_size", 16384);
			var serverLoopMock = new MockServerLoopNode(svConfig, new TunnelConfigFactory(new BinarySerializationHelperFactory()));
			var serverComprNode = new CompressionServerNode(svConfig, serverLoopMock, new FastLZBlockCompressorFactory());
			var clConfig = new TunnelConfig();
			var clientLoopMock = new MockClientLoopNode();
			var clientComprNode = new CompressionClientNode(clientLoopMock, new FastLZBlockCompressorFactory());
			CommonDataTransferTests.ReadWrite(clConfig, clientComprNode, clientLoopMock, serverComprNode, serverLoopMock);
		}

		[Test]
		public void MultithreadedReadWrite()
		{
			var svConfig = new TunnelConfig();
			//add mock parameters
			svConfig.Set("mock_min_block_size", 4096);
			svConfig.Set("mock_max_block_size", 8192);
			svConfig.Set("mock_read_timeout", 5000);
			svConfig.Set("mock_fail_prob", 0.1f);
			svConfig.Set("mock_nofail_ops_count", 1000);
			//add compression parameters
			svConfig.Set("compr_max_block_size", 16384);
			var serverLoopMock = new MockServerLoopNode(svConfig, new TunnelConfigFactory(new BinarySerializationHelperFactory()));
			var serverComprNode = new CompressionServerNode(svConfig, serverLoopMock, new FastLZBlockCompressorFactory());
			var clConfig = new TunnelConfig();
			var clientLoopMock = new MockClientLoopNode();
			var clientComprNode = new CompressionClientNode(clientLoopMock, new FastLZBlockCompressorFactory());
			CommonDataTransferTests.MultithreadedReadWrite(clConfig, clientComprNode, clientLoopMock, serverComprNode, serverLoopMock);
		}
	}
}
