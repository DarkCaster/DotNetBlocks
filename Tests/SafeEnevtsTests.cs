// SafeEnevtsTests.cs
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
using NUnit.Framework;
using DarkCaster.Events;

namespace Tests
{
	[TestFixture]
	public class SafeEnevtsTests
	{
		public class TestEventArgs : EventArgs
		{
			public int Val { get; set; }
		}

		public class TestSubscriber
		{
			public volatile int lCounter;
			public volatile int pCounter;
			private readonly IEventPublisher<TestEventArgs> testEvent;

			private void OnTestEvent(object sender, TestEventArgs args)
			{
				lCounter++;
				pCounter = args.Val;
			}

			public TestSubscriber(IEventPublisher<TestEventArgs> testEvent)
			{
				lCounter = 0;
				pCounter = 0;
				this.testEvent = testEvent;
			}

			public void Subscribe()
			{
				testEvent.Subscribe(OnTestEvent);
			}

			public void Unsubscribe()
			{
				testEvent.Unsubscribe(OnTestEvent);
			}
		}

		public class TestSTPublisher
		{
			public SafeEventPublisher<TestEventArgs> testEvent = new SafeEventPublisher<TestEventArgs>();
			private int counter;

			public TestSTPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				testEvent.Raise(new TestEventArgs() { Val = counter });
			}
		}

		[Test]
		public void SingleEvent()
		{
			var publisher = new TestSTPublisher();
			var listener = new TestSubscriber(publisher.testEvent);
			listener.Subscribe();
			publisher.Raise();
			Assert.AreEqual(1, listener.lCounter);
			Assert.AreEqual(1, listener.pCounter);
			publisher.Raise();
			Assert.AreEqual(2, listener.lCounter);
			Assert.AreEqual(2, listener.pCounter);
			listener.Unsubscribe();
			publisher.Raise();
			Assert.AreEqual(2, listener.lCounter);
			Assert.AreEqual(2, listener.pCounter);
		}
	}
}
