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
using System.Reflection;
using NUnit.Framework;
using DarkCaster.Events;

namespace Tests
{

	//obsolete
	/*
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
				testEvent.Raise(this, new TestEventArgs() { Val = counter });
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

		//Optional tests for MethodInfo GetHashCode and Equals compliance,
		//Generally, needed only for me to determine how this things behave in different situations
		//And make sure, that it behavior is the same at different .NET implementations (.NET, .NET Core, Mono)
		private class MTClass1
		{
			public void Method1() {}
			public bool Method2() { throw new NotSupportedException(); }
			public void Method3(bool param1) { throw new NotSupportedException(); }
			public void Method4(bool paramX) { throw new NotSupportedException(); }

			public void GenericMethod1<T>() { throw new NotSupportedException(); }
			public T GenericMethod2<T>() { throw new NotSupportedException(); }
			public void GenericMethod3<T>(T param1) { throw new NotSupportedException(); }
			public void GenericMethod4<T>(T param2) { throw new NotSupportedException(); }
		}

		private class MTClass2
		{
			public void Method1() { }
			public bool Method2() { throw new NotSupportedException(); }
			public void Method3(bool param1) { throw new NotSupportedException(); }
			public void Method4(bool paramX) { throw new NotSupportedException(); }

			public void GenericMethod1<T>() { }
			public T GenericMethod2<T>() { throw new NotSupportedException(); }
			public void GenericMethod3<T>(T param1) { throw new NotSupportedException(); }
			public void GenericMethod4<T>(T param2) { throw new NotSupportedException(); }
		}

		public delegate void Delegate1();
		public delegate bool Delegate2();
		public delegate void Delegate3(bool p);

		public delegate void GDelegate1<T>();
		public delegate T GDelegate2<T>();
		public delegate void GDelegate3<T>(T p);

		private MethodInfo GetMethodInfo(Delegate target)
		{
			return target.Method;
		}

		private void TestMethodInfo_AreEquals<D1,D2>(D1 target1, D2 target2)
		{
			TestMethodInfo_AreEquals((Delegate)(object)target1, (Delegate)(object)target2);
		}

		private void TestMethodInfo_AreNotEquals<D1, D2>(D1 target1, D2 target2)
		{
			TestMethodInfo_AreNotEquals((Delegate)(object)target1, (Delegate)(object)target2);
		}

		private void TestMethodInfo_AreEquals(Delegate target1, Delegate target2 )
		{
			var mi1 = GetMethodInfo(target1);
			var mi2 = GetMethodInfo(target2);
			Assert.AreEqual(mi1.GetHashCode(), mi2.GetHashCode());
			Assert.AreEqual(true, mi1.Equals(mi2));
			Assert.AreEqual(true, mi1 == mi2);
			Assert.AreEqual(false, mi1 != mi2);
		}

		private void TestMethodInfo_AreNotEquals(Delegate target1, Delegate target2)
		{
			var mi1 = GetMethodInfo(target1);
			var mi2 = GetMethodInfo(target2);
			Assert.AreNotEqual(mi1.GetHashCode(), mi2.GetHashCode());
			Assert.AreEqual(false, mi1.Equals(mi2));
			Assert.AreEqual(false, mi1 == mi2);
			Assert.AreEqual(true, mi1 != mi2);
		}

		[Test]
		public void MethodInfo_ComplianceTest()
		{
			var obj1 = new MTClass1();
			var obj2 = new MTClass2();

			TestMethodInfo_AreEquals<Delegate1, Delegate1>(obj1.Method1, obj1.Method1);
			TestMethodInfo_AreNotEquals<Delegate1, Delegate1>(obj1.Method1, obj1.GenericMethod1<bool>);
			TestMethodInfo_AreNotEquals<Delegate1, Delegate2>(obj1.Method1, obj1.Method2);
			TestMethodInfo_AreNotEquals<Delegate3, Delegate3>(obj1.Method3, obj1.Method4);

			TestMethodInfo_AreNotEquals<Delegate1, Delegate1>(obj1.Method1, obj2.Method1);
			TestMethodInfo_AreNotEquals<Delegate2, Delegate2>(obj1.Method2, obj2.Method2);
			TestMethodInfo_AreNotEquals<Delegate3, Delegate3>(obj1.Method3, obj2.Method3);
			TestMethodInfo_AreNotEquals<Delegate3, Delegate3>(obj1.Method4, obj2.Method4);

			TestMethodInfo_AreEquals<GDelegate1<bool>, GDelegate1<bool>>(obj1.Method1, obj1.Method1);
			TestMethodInfo_AreNotEquals<GDelegate1<bool>, GDelegate1<bool>>(obj1.Method1, obj1.GenericMethod1<bool>);
			TestMethodInfo_AreNotEquals<GDelegate2<bool>, GDelegate2<bool>>(obj1.Method2, obj1.GenericMethod2<bool>);
			TestMethodInfo_AreNotEquals<GDelegate3<bool>, GDelegate3<bool>>(obj1.Method3, obj1.GenericMethod3<bool>);

			TestMethodInfo_AreNotEquals<GDelegate1<bool>, GDelegate1<bool>>(obj1.GenericMethod1<bool>, obj2.GenericMethod1<bool>);
			TestMethodInfo_AreNotEquals<GDelegate2<bool>, GDelegate2<bool>>(obj1.GenericMethod2<bool>, obj2.GenericMethod2<bool>);
			TestMethodInfo_AreNotEquals<GDelegate3<bool>, GDelegate3<bool>>(obj1.GenericMethod3<bool>, obj2.GenericMethod3<bool>);
			TestMethodInfo_AreNotEquals<GDelegate3<bool>, GDelegate3<bool>>(obj1.GenericMethod4<bool>, obj2.GenericMethod4<bool>);

			var objX = new MTClass1();
			TestMethodInfo_AreEquals<Delegate1, Delegate1>(obj1.Method1, objX.Method1);
			TestMethodInfo_AreEquals<Delegate2, Delegate2>(obj1.Method2, objX.Method2);
			TestMethodInfo_AreEquals<Delegate3, Delegate3>(obj1.Method3, objX.Method3);
			TestMethodInfo_AreEquals<Delegate3, Delegate3>(obj1.Method4, objX.Method4);

			TestMethodInfo_AreEquals<GDelegate1<bool>, GDelegate1<bool>>(obj1.GenericMethod1<bool>, objX.GenericMethod1<bool>);
			TestMethodInfo_AreEquals<GDelegate2<bool>, GDelegate2<bool>>(obj1.GenericMethod2<bool>, objX.GenericMethod2<bool>);
			TestMethodInfo_AreEquals<GDelegate3<bool>, GDelegate3<bool>>(obj1.GenericMethod3<bool>, objX.GenericMethod3<bool>);
			TestMethodInfo_AreEquals<GDelegate3<bool>, GDelegate3<bool>>(obj1.GenericMethod4<bool>, objX.GenericMethod4<bool>);
		}
	}
	*/
}
