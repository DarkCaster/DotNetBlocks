// SafeEventDbgTests.cs
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
using System.Threading;
using NUnit.Framework;
using DarkCaster.Events;
using Tests.SafeEventStuff;

namespace Tests
{
	[TestFixture]
	public class SafeEventDbgTests
	{
		[Test]
		public void SubscribeUnsubscribe()
		{
			var sub1 = new SimpleSubscriber();
			var sub2 = new SimpleSubscriber();
			var ev = new SafeEventDbg<TestEventArgs>();
			var pub = new SimplePublisher<SafeEventDbg<TestEventArgs>,SafeEventDbg<TestEventArgs>>(ev, ev);
			CommonEventTests.SubscribeUnsubscribe(sub1, sub2, pub);
		}

		[Test]
		public void SubscriberException()
		{
			var sub1 = new SimpleSubscriber();
			var sub2 = new SimpleSubscriber();
			var sub3 = new CommonEventTests.FailingSubscriber();
			var ev = new SafeEventDbg<TestEventArgs>();
			var pub = new SimplePublisher<SafeEventDbg<TestEventArgs>,SafeEventDbg<TestEventArgs>>(ev, ev);
			CommonEventTests.SubscriberException(sub1, sub2, sub3, pub);
		}

		[Test]
		public void Raise()
		{
			var sub1 = new SimpleSubscriber();
			var ev = new SafeEventDbg<TestEventArgs>();
			var pub = new SimplePublisher<SafeEventDbg<TestEventArgs>, SafeEventDbg<TestEventArgs>>(ev, ev);
			CommonEventTests.Raise(sub1, pub);
		}

		private class StaleSubscriber
		{
			public const int PLACEHOLDERLEN = 32768;
			private readonly byte[] placeHolder;
			~StaleSubscriber() { finalizerDone = true; }
			public StaleSubscriber()
			{
				placeHolder = new byte[PLACEHOLDERLEN];
				for(int i = 0; i < PLACEHOLDERLEN; ++i)
					placeHolder[0] =(byte)(i % 255);
			}
			public void OnTestEvent(object sender, TestEventArgs args) { throw new NotSupportedException("This method should never been run!"); }
		}

		private static volatile bool finalizerDone = false;

		[Test]
		public void StallObjectDetect()
		{
			finalizerDone = false;
			var ev = new SafeEventDbg<TestEventArgs>();
			var pub = new SimplePublisher<SafeEventDbg<TestEventArgs>, SafeEventDbg<TestEventArgs>>(ev, ev);
			long cnt = 0L;
			const int MAXITER = 5000;
			while(!finalizerDone)
			{
				pub.TheEvent.Subscribe((new StaleSubscriber()).OnTestEvent);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				++cnt;
				if(cnt > MAXITER)
					Assert.Inconclusive(string.Format("GC was expected, but, still, has not been run yet, cannot proceed this test. " +
					                                  "GC routine is not working as expected in your .NET runtime! " +
					                                  "At least {0} MiB of memory was consumed during this test while waiting GC to proceed.",
					                                  (StaleSubscriber.PLACEHOLDERLEN*MAXITER)/(1024*1024)));
			}
			try { pub.Raise(); }
			catch(Exception ex)
			{
				Assert.AreEqual(typeof(EventDbgException), ex.GetType());
				var dbgEx = (EventDbgException)ex;
				Assert.Null(dbgEx.subscriber);
				Assert.NotNull(dbgEx.declaringType);
				Assert.AreEqual(typeof(StaleSubscriber), dbgEx.declaringType);
				Assert.Pass();
			}
			Assert.Fail("Expected EventDbgException exception has not been triggered during event raise, test failed!");
		}
	}
}
