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
using System.Threading;
using NUnit.Framework;
using DarkCaster.Events;

namespace Tests
{

	//a work-in-progress tests, will be changed in future.
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

		public class TestSubscriberForWeakRefTest
		{
			private Action<int> extCallback;
			public int intCounter;
			private string debugTag;

			public TestSubscriberForWeakRefTest(IEventPublisher<TestEventArgs> testEvent, Action<int> extCallback, string tag)
			{
				testEvent.Subscribe(OnTestEvent);
				this.extCallback = extCallback;
				intCounter = 0;
				debugTag = tag;
			}

			private void OnTestEvent(object sender, TestEventArgs args)
			{
				intCounter += 1;
				debugTag = debugTag + "_invoke:" + args.Val;
				extCallback(args.Val);
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
		public void OneSubscriber_Sync()
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

		[Test]
		public void MultipleSubscribers_Sync()
		{
			var publisher = new TestSTPublisher();
			var listeners = new TestSubscriber[128];
			for(int i = 0; i < listeners.Length; ++i)
				listeners[i] = new TestSubscriber(publisher.testEvent);
			for(int i = 0; i < listeners.Length; ++i)
				listeners[i].Subscribe();
			publisher.Raise();
			for(int i = 0; i < listeners.Length; ++i)
			{
				Assert.AreEqual(1, listeners[i].lCounter);
				Assert.AreEqual(1, listeners[i].pCounter);
			}
			publisher.Raise();
			for(int i = 0; i < listeners.Length; ++i)
			{
				Assert.AreEqual(2, listeners[i].lCounter);
				Assert.AreEqual(2, listeners[i].pCounter);
			}
			for(int i = 0; i < listeners.Length/2; ++i)
				listeners[i].Unsubscribe();
			publisher.Raise();
			for(int i = 0; i < listeners.Length/2; ++i)
			{
				Assert.AreEqual(2, listeners[i].lCounter);
				Assert.AreEqual(2, listeners[i].pCounter);
			}
			for(int i = listeners.Length / 2; i < listeners.Length; ++i)
			{
				Assert.AreEqual(3, listeners[i].lCounter);
				Assert.AreEqual(3, listeners[i].pCounter);
			}
			for(int i = 0; i < listeners.Length / 2; ++i)
				listeners[i].Subscribe();
			publisher.Raise();
			for(int i = 0; i < listeners.Length / 2; ++i)
			{
				Assert.AreEqual(3, listeners[i].lCounter);
				Assert.AreEqual(4, listeners[i].pCounter);
			}
			for(int i = listeners.Length / 2; i < listeners.Length; ++i)
			{
				Assert.AreEqual(4, listeners[i].lCounter);
				Assert.AreEqual(4, listeners[i].pCounter);
			}
		}

		private volatile int counter=0;

		private void OnWeakRefCallback(int val)
		{
			counter += 1;
		}

		//TODO:
		[Test]
		public void WeakReferenceTest()
		{
			var publisher = new TestSTPublisher();
			var listener1 = new TestSubscriberForWeakRefTest(publisher.testEvent, OnWeakRefCallback, "obj1");
			var listener2 = new TestSubscriberForWeakRefTest(publisher.testEvent, OnWeakRefCallback, "obj2");

			publisher.Raise();
			GC.KeepAlive(listener1);
			Volatile.Write(ref listener1, null);
			Assert.AreEqual(2, counter);

			//run garbage collector and wait for it completion
			GC.Collect();
			GC.WaitForPendingFinalizers();

			publisher.Raise();
			Assert.AreEqual(3, counter);
			 
			GC.KeepAlive(listener2);
		}
	}
}
