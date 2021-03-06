﻿// MockTunnelBase.cs
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
using System.Threading;
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Private;

namespace Tests.Mocks.DataLoop
{
	public abstract class MockTunnelBase : ITunnelBase
	{
		private readonly int minBlockSize;
		private readonly int maxBlockSize;
		private readonly Random writeRandom;
		private readonly Random readRandom;

		public byte[] readBlock;
		public int readBlockPos;

		private readonly Storage readStorage;
		private readonly Storage writeStorage;

		private readonly int readTimeout;
		private int readTimeleft;

		private readonly float failProb;
		private int noFailOpsCount;

		private int readOpsCount=0;
		private int writeOpsCount=0;
		private int disconnectOpsCount=0;
		private int disposeOpsCount=0;

		protected MockTunnelBase(int minBlockSize, int maxBlockSize, Storage readStorage, int readTimeout, Storage writeStorage, int noFailOpsCount, float failProb)
		{
			this.readStorage = readStorage;
			this.readTimeout = readTimeout;
			this.writeStorage = writeStorage;

			this.minBlockSize = minBlockSize;
			this.maxBlockSize = maxBlockSize;
			readBlock = new byte[0];
			readBlockPos = 0;
			readTimeleft = readTimeout;
			writeRandom = new Random();
			readRandom = new Random();
			this.failProb = failProb;
			this.noFailOpsCount = noFailOpsCount;
		}

		public async Task<int> ReadDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref readOpsCount);
			if(Interlocked.Decrement(ref noFailOpsCount) <= 0 && readRandom.NextDouble() < failProb)
				throw new MockLoopException("Read: expected fail triggered");
			if(sz == 0)
				return 0;
			if(readBlockPos >= readBlock.Length)
			{
				byte[] chunk = null;
				while((chunk = readStorage.ReadChunk()) == null)
				{
					await Task.Delay(10);
					readTimeleft -= 10;
					if(readTimeleft < 0)
						throw new Exception("READ TIMEOUT");
				}
				readBlock = chunk;
				readTimeleft = readTimeout;
				readBlockPos = 0;
			}
			if(sz > (readBlock.Length - readBlockPos))
				sz = readBlock.Length - readBlockPos;
			Buffer.BlockCopy(readBlock, readBlockPos, buffer, offset, sz);
			readBlockPos += sz;
			return sz;
		}

		public Task<int> WriteDataAsync(int sz, byte[] buffer, int offset = 0)
		{
			Interlocked.Increment(ref writeOpsCount);
			if(Interlocked.Decrement(ref noFailOpsCount) <= 0 && readRandom.NextDouble() < failProb)
				throw new MockLoopException("Write: expected fail triggered");
			if(sz == 0)
				return Task.FromResult(0);
			var bsz = writeRandom.Next(minBlockSize, maxBlockSize + 1);
			if(sz > bsz)
				sz = bsz;
			var chunk = new byte[sz];
			Buffer.BlockCopy(buffer, offset, chunk, 0, sz);
			writeStorage.WriteChunk(chunk);
			return Task.FromResult(sz);
		}

		public Task DisconnectAsync()
		{
			Interlocked.Increment(ref disconnectOpsCount);
			readStorage.Hangup();
			writeStorage.Hangup();
			return Task.FromResult(true);
		}

		public void Dispose()
		{
			Interlocked.Increment(ref disposeOpsCount);
		}

		public int ReadOpsCount { get { return Interlocked.CompareExchange(ref readOpsCount,0,0); }}
		public int WriteOpsCount { get { return Interlocked.CompareExchange(ref writeOpsCount, 0, 0); } }
		public int DisconnectOpsCount { get { return Interlocked.CompareExchange(ref disconnectOpsCount, 0, 0); } }
		public int DisposeOpsCount { get { return Interlocked.CompareExchange(ref disposeOpsCount, 0, 0); } }
	}
}
