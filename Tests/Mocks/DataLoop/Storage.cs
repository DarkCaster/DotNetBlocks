// Storage.cs
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
using System.Collections.Concurrent;

namespace Tests.Mocks.DataLoop
{
	public class Storage
	{
		private readonly ConcurrentQueue<byte[]> chunks = new ConcurrentQueue<byte[]>();
		private volatile bool hangup = false;
		public void Reset() { while(chunks.TryDequeue(out byte[] result)) {} }

		public void Hangup()
		{
			hangup = true;
		}

		public byte[] ReadChunk()
		{
			if(chunks.TryDequeue(out byte[] chunk))
				return chunk;
			if(hangup)
				throw new Exception("HANGUP");
			return null;
		}

		public void WriteChunk(byte[] chunk)
		{
			if(hangup)
				throw new Exception("HANGUP");
			chunks.Enqueue(chunk);
		}
	}
}
