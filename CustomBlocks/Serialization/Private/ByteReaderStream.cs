// ByteReaderStream.cs
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
	/// Implements only minimal functional, needed for current serialization helpers to work. 
	/// </summary>
	public sealed class ByteReaderStream : Stream
	{
		public override bool CanRead { get { throw new NotSupportedException("CanRead: SORRY, BUT NO!"); } }
		public override bool CanSeek { get { throw new NotSupportedException("CanSeek: SORRY, BUT NO!"); } }
		public override bool CanTimeout { get { throw new NotSupportedException("CanTimeout: SORRY, BUT NO!"); } }
		public override bool CanWrite { get { throw new NotSupportedException("CanWrite: SORRY, BUT NO!"); } }
		public override long Length { get { throw new NotSupportedException("Length: SORRY, BUT NO!"); } }

		public override long Position
		{
			get { throw new NotSupportedException("Position get: SORRY, BUT NO!"); }
			set { throw new NotSupportedException("Position set: SORRY, BUT NO!"); }
		}

		public override int ReadTimeout
		{
			get { throw new NotSupportedException("ReadTimeout get: SORRY, BUT NO!"); }
			set { throw new NotSupportedException("ReadTimeout set: SORRY, BUT NO!"); }
		}

		public override int WriteTimeout
		{
			get { throw new NotSupportedException("WriteTimeout get: SORRY, BUT NO!"); }
			set { throw new NotSupportedException("WriteTimeout set: SORRY, BUT NO!"); }
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{ throw new NotSupportedException("BeginRead: SORRY, BUT NO!"); }

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{ throw new NotSupportedException("BeginWrite: SORRY, BUT NO!"); }

		public override void Close()
		{ throw new NotSupportedException("Close: SORRY, BUT NO!"); }

		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{ throw new NotSupportedException("CopyToAsync: SORRY, BUT NO!"); }

		[Obsolete("CreateWaitHandle will throw NotSupportedException")]
		protected override WaitHandle CreateWaitHandle()
		{ throw new NotSupportedException("CreateWaitHandle: SORRY, BUT NO!"); }

		protected override void Dispose(bool disposing)
		{ throw new NotSupportedException("Dispose: SORRY, BUT NO!"); }

		public override int EndRead(IAsyncResult asyncResult)
		{ throw new NotSupportedException("EndRead: SORRY, BUT NO!"); }

		public override void EndWrite(IAsyncResult asyncResult)
		{ throw new NotSupportedException("EndWrite: SORRY, BUT NO!"); }

		public override void Flush()
		{ throw new NotSupportedException("Flush: SORRY, BUT NO!"); }

		public override Task FlushAsync(CancellationToken cancellationToken)
		{ throw new NotSupportedException("FlushAsync: SORRY, BUT NO!"); }

		public override int Read(byte[] buffer, int offset, int count)
		{ throw new NotSupportedException("Read: SORRY, BUT NO!"); }

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{ throw new NotSupportedException("ReadAsync: SORRY, BUT NO!"); }

		public override int ReadByte()
		{ throw new NotSupportedException("ReadAsync: SORRY, BUT NO!"); }

		public override long Seek(long offset, SeekOrigin origin)
		{ throw new NotSupportedException("Seek: SORRY, BUT NO!"); }

		public override void SetLength(long value)
		{ throw new NotSupportedException("SetLength: SORRY, BUT NO!"); }

		public override void Write(byte[] buffer, int offset, int count)
		{ throw new NotSupportedException("Write: SORRY, BUT NO!"); }

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{ throw new NotSupportedException("WriteAsync: SORRY, BUT NO!"); }

		public override void WriteByte(byte value)
		{ throw new NotSupportedException("WriteByte: SORRY, BUT NO!"); }
	}
}
