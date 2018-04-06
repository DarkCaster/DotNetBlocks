// UUIDTests.cs
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
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class UUIDTests
	{
		private static long TrimTimestamp(long timeStamp)
		{
			return unchecked((long)((ulong)timeStamp & 0xFFFFFFFFFFF00000UL));
		}

		//verify that TicksPerMillisecond constant is within sane limits for our use.
		//should not fail for .NET or Mono.
		[Test]
		public void TicksPerMillisecond()
		{
			Assert.LessOrEqual(0xFFFFF / TimeSpan.TicksPerMillisecond, 1000);
		}

		[Test]
		public void SufficientDateTimeResolution()
		{
			long start = TrimTimestamp(DateTime.UtcNow.Ticks);
			long cur = TrimTimestamp(DateTime.UtcNow.Ticks);
			while (cur <= start)
				cur = TrimTimestamp(DateTime.UtcNow.Ticks);
			Assert.AreEqual(1048576, (cur - start));
			Assert.AreEqual(104, (cur - start)/TimeSpan.TicksPerMillisecond);
		}

		[Test]
		public void SufficientStopWatchResolution()
		{
			//multiplier that used to convert stopwatch-ticks to datetime-ticks
			var swTicksToDateTicksMult = (double)10000000 / (double)Stopwatch.Frequency;
			var sw = new Stopwatch();
			sw.Start();
			//warm-up, so there will be some initial time
			Thread.Sleep(new Random().Next(500, 2000));
			long start = TrimTimestamp((long)((double)sw.ElapsedTicks * swTicksToDateTicksMult));
			long cur = TrimTimestamp((long)((double)sw.ElapsedTicks * swTicksToDateTicksMult));
			while (cur <= start)
				cur = TrimTimestamp((long)((double)sw.ElapsedTicks * swTicksToDateTicksMult));
			sw.Stop();
			Assert.AreEqual(1048576, (cur - start));
			Assert.AreEqual(104, (cur - start) / TimeSpan.TicksPerMillisecond);
		}
	}
}
