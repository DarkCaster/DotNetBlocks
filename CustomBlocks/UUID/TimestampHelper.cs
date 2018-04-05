// TimestampHelper.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2018 DarkCaster <dark.caster@outlook.com>
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
using System.Diagnostics;
using System.Security.Cryptography;

namespace DarkCaster.UUID
{
	public static class TimestampHelper
	{
		private static readonly object locker = new object();
		private static readonly Stopwatch counter = new Stopwatch();
		private static readonly long startTimestamp;
		private static readonly double swTicksToDateTicksMult = (double)10000000 / (double)Stopwatch.Frequency;
		//stuff for powering LCG algorithm that is used to fill lower 32 bits of timestamp
		//with unique 32-bit values (until LGC overflows, of course)
		private static ulong lastLCGValue;
		//used for overflow check when timestamp is not increased over previous invoke to GenerateGUID method
		private static ulong startLCGValue;
		//following constants ensures that LGC overflow will occur in 4294967290 rounds (this gives us almost 2^32 uinique values)
		private const ulong m = 4294967291UL;
		private const ulong a = 279470273UL;

		static TimestampHelper()
		{
			//Bind start timestamp to some common base (this date taken from RFC 4122)
			startTimestamp = DateTime.UtcNow.Ticks - new DateTime(1582, 10, 15, 00, 00, 00, DateTimeKind.Utc).Ticks;
			//start time counter
			counter.Start();
			using (var random = new RNGCryptoServiceProvider())
			{
				//set initial LGC counter to random value
				var bInit = new byte[4];
				random.GetBytes(bInit);
				ulong init = unchecked((uint)(bInit[0] | bInit[1] << 8 | bInit[2] << 16 | bInit[3] << 24));
				if (init < 1)
					init = 1;
				if (init > (m - 1))
					init = m - 1;
				lastLCGValue = init;
				startLCGValue = 0;
			}
		}

		public static void WriteScrambledTimestamp(byte[] buffer, int offset)
		{
			lock (locker)
			{
				throw new NotImplementedException("TODO:");
			}
		}

		public static DateTime ReadScrambledTimestamp(byte[] buffer, int offset)
		{
			throw new NotImplementedException("TODO:");
		}
	}
}
