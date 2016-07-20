// ByteWriterStream.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016 DarkCaster <dark.caster@outlook.com>
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DarkCaster.Serialization.Private
{
	/// <summary>
	/// Service class for use with serialization herlpers.
	/// Used to overwrite data INSIDE already created byte array at specified offset.
	/// </summary>
	public sealed class ByteWriterStream : Stream
	{
		private readonly byte[] source;
		private readonly int lowerBound;
		private int upperBound;

		private volatile bool isDisposed;
		private int pos;

		public ByteWriterStream(byte[] source, int offset = 0)
		{
			if(source == null)
				throw new ArgumentException("source array cannot be null", "source");
			this.source = source;
			//some checks and calculations
			if(offset < 0)
				throw new ArgumentException("offset < 0", "offset");
			lowerBound = upperBound = pos = offset;
			isDisposed = false;
		}

		public override bool CanRead { get { return false; } }

		public override bool CanSeek { get { return !isDisposed; } }

		public override bool CanWrite { get { return !isDisposed; } }

		public override long Length
		{
			get
			{
				if(isDisposed)
					throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
				return upperBound - lowerBound;
			}
		}

		public override long Position
		{
			get
			{
				if(isDisposed)
					throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
				return pos - lowerBound;
			}
			set
			{
				if(isDisposed)
					throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
				pos = unchecked((int)(value + lowerBound));
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{ throw new NotSupportedException("BeginRead"); }

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{ throw new NotSupportedException("BeginWrite"); }

		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{ throw new NotSupportedException("CopyToAsync"); }

		protected override void Dispose(bool disposing)
		{ isDisposed = true; }

		public override int EndRead(IAsyncResult asyncResult)
		{ throw new NotSupportedException("EndRead"); }

		public override void EndWrite(IAsyncResult asyncResult)
		{ throw new NotSupportedException("EndWrite"); }

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{ throw new NotSupportedException("ReadAsync"); }

		public override int Read(byte[] buffer, int offset, int count)
		{ throw new NotSupportedException("Read"); }

		public override int ReadByte()
		{ throw new NotSupportedException("ReadByte"); }

		public override long Seek(long offset, SeekOrigin origin)
		{
			if(isDisposed)
				throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
			switch(origin)
			{
			case SeekOrigin.Begin:
				pos = unchecked((int)(lowerBound + offset));
				break;
			case SeekOrigin.Current:
				pos += unchecked((int)offset);
				break;
			default:
				pos = unchecked((int)(upperBound + offset));
				break;
			}
			return Position;
		}

		public override void Flush() {/* NOOP */}

		public override void SetLength(long value)
		{
			if(isDisposed)
				throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
			if(value < 0)
				throw new IOException("requested length < 0");
			upperBound = unchecked((int)(lowerBound + value));
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{ throw new NotSupportedException("WriteAsync"); }

		public override void Write(byte[] buffer, int offset, int count)
		{
			if(isDisposed)
				throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
			if(offset < 0)
				throw new ArgumentOutOfRangeException("offset", "offset < 0");
			if(count < 0)
				throw new ArgumentOutOfRangeException("count", "count < 0");
			if(buffer == null)
				throw new ArgumentNullException("buffer", "buffer is null");
			if(offset + count > buffer.Length)
				throw new ArgumentException("offset+count > buffer.Length");
			if(count == 0)
				return;
			Buffer.BlockCopy(buffer, offset, source, pos, count);
			pos += count;
			if(pos > upperBound)
				upperBound = pos;
		}

		public override void WriteByte(byte value)
		{
			if(isDisposed)
				throw new ObjectDisposedException("ByteWriterStream", "ByteWriterStream is disposed!");
			source[pos++] = value;
			if(pos > upperBound)
				upperBound = pos;
		}
	}
}
