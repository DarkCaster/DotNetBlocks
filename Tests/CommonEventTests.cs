// CommonEventTests.cs
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
using System.Collections.Generic;
using NUnit.Framework;
using DarkCaster.Events;
using Tests.SafeEventStuff;

namespace Tests
{
	public static class CommonEventTests
	{
		public static void SubscribeUnsubscribe(ISubscriber sub1, ISubscriber sub2, IPublisher pub)
		{
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//subscribe multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(sub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub1.OnEvent, true));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);

			//currect subscribe and unsubscribe
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub2.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub2.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);

			//unsubscribe 1-st subscriber multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent, true));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);

			var delegateList1 = (EventHandler<TestEventArgs>)Delegate.Combine((EventHandler<TestEventArgs>)sub1.OnEvent, (EventHandler<TestEventArgs>)sub2.OnEvent);

			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list with already registered delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to focre unsubscribe delegate list containing non subscribed delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to force subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list again
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to force unsubscribe delegate list
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//unsubscribe single delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//unsubscribe remaining delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//delegatelist, that contains dublicate
			var delegateList2 = (EventHandler<TestEventArgs>)Delegate.Combine(delegateList1, (EventHandler<TestEventArgs>)sub2.OnEvent);
			//try to subscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList2));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to force subscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList2));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to force unsubscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList2, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//subscribe delegate list and unsubscribe manually
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//not necessary, just precaution for my solace
			GC.KeepAlive(sub1);
			GC.KeepAlive(sub2);
			GC.KeepAlive(pub);
		}

		public class FailingSubscriber : ISubscriber
		{
			public int counter = 0;
			public int lastValue = 0;
			public void OnTestEvent(object sender, TestEventArgs args)
			{
				++counter;
				lastValue = args.Val;
				throw new Exception(string.Format("Expected failure. Counter={0}, LastValue={1}", counter, lastValue));
			}
			public int Counter { get { return counter; } set { counter = value; } }
			public int LastValue { get { return lastValue; } set { lastValue = value; } }
			public EventHandler<TestEventArgs> OnEvent { get { return OnTestEvent; } }
		}

		public static void Raise(ISubscriber sub1, IPublisher pub)
		{
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(1, sub1.Counter);
			Assert.AreEqual(1, sub1.LastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(1, sub1.Counter);
			Assert.AreEqual(1, sub1.LastValue);

			//not necessary, just precaution for my solace
			GC.KeepAlive(sub1);
			GC.KeepAlive(pub);
		}

		public static void SubscriberException(ISubscriber goodSub1, ISubscriber goodSub2, ISubscriber failingSub, IPublisher pub)
		{
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(goodSub1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			var exceptions = new List<EventRaiseException>();
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(true, pub.Raise(exceptions));
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(failingSub.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(goodSub2.OnEvent));
			Assert.AreEqual(3, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(false, pub.Raise(null));
			Assert.AreEqual(false, pub.Raise(exceptions));
			Assert.AreEqual(1, exceptions.Count);
			Assert.AreEqual(failingSub.OnEvent, exceptions[0].subscriber);
			Assert.AreEqual(4, goodSub1.Counter);
			Assert.AreEqual(4, goodSub1.LastValue);
			Assert.AreEqual(2, goodSub2.Counter);
			Assert.AreEqual(4, goodSub2.LastValue);
			Assert.AreEqual(2, failingSub.Counter);
			Assert.AreEqual(4, failingSub.LastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(failingSub.OnEvent));
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(true, pub.Raise(exceptions));
			Assert.AreEqual(1, exceptions.Count);
			Assert.AreEqual(failingSub.OnEvent, exceptions[0].subscriber);
			Assert.AreEqual(2, failingSub.Counter);
			Assert.AreEqual(4, failingSub.LastValue);
			Assert.AreEqual(6, goodSub1.Counter);
			Assert.AreEqual(6, goodSub1.LastValue);
			Assert.AreEqual(4, goodSub2.Counter);
			Assert.AreEqual(6, goodSub2.LastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(goodSub1.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(goodSub2.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//not necessary, just precaution for my solace
			GC.KeepAlive(goodSub1);
			GC.KeepAlive(goodSub2);
			GC.KeepAlive(failingSub);
			GC.KeepAlive(pub);
		}

		private static class StaticSubscriber1
		{
			public static int counter = 0;
			public static int lastValue = 0;
			public static void OnEvent(object sender, TestEventArgs args)
			{
				++counter;
				lastValue = args.Val;
			}
		}

		private static class StaticSubscriber2
		{
			public static int counter = 0;
			public static int lastValue = 0;
			public static void OnEvent(object sender, TestEventArgs args)
			{
				++counter;
				lastValue = args.Val;
			}
		}

		private static class FailingStaticSubscriber
		{
			public static int counter = 0;
			public static int lastValue = 0;
			public static void OnEvent(object sender, TestEventArgs args)
			{
				++counter;
				lastValue = args.Val;
				throw new Exception(string.Format("Expected failure. Counter={0}, LastValue={1}", counter, lastValue));
			}
		}

		public static void StaticSubscribeUnsubscribe(IPublisher pub)
		{
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//subscribe multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber1.OnEvent, true));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);

			//currect subscribe and unsubscribe
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);

			//unsubscribe 1-st subscriber multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent, true));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);

			var delegateList1 = (EventHandler<TestEventArgs>)Delegate.Combine((EventHandler<TestEventArgs>)StaticSubscriber1.OnEvent, (EventHandler<TestEventArgs>)StaticSubscriber2.OnEvent);

			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list with already registered delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to focre unsubscribe delegate list containing non subscribed delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to force subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list again
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to force unsubscribe delegate list
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//unsubscribe single delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			//unsubscribe remaining delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//delegatelist, that contains dublicate
			var delegateList2 = (EventHandler<TestEventArgs>)Delegate.Combine(delegateList1, (EventHandler<TestEventArgs>)StaticSubscriber2.OnEvent);
			//try to subscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList2));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			//try to force subscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to unsubscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList2));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			//try to force unsubscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList2, true));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//subscribe delegate list and unsubscribe manually
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//not necessary, just precaution for my solace
			GC.KeepAlive(pub);
		}

		public static void StaticRaise(IPublisher pub)
		{
			StaticSubscriber2.counter = 0;
			StaticSubscriber2.lastValue = 0;
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(1, StaticSubscriber2.counter);
			Assert.AreEqual(1, StaticSubscriber2.lastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(1, StaticSubscriber2.counter);
			Assert.AreEqual(1, StaticSubscriber2.lastValue);

			//not necessary, just precaution for my solace
			GC.KeepAlive(pub);
		}

		public static void StaticSubscriberException( IPublisher pub)
		{
			StaticSubscriber1.counter = 0;
			StaticSubscriber1.lastValue = 0;
			StaticSubscriber2.counter = 0;
			StaticSubscriber2.lastValue = 0;
			FailingStaticSubscriber.counter = 0;
			FailingStaticSubscriber.lastValue = 0;
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber1.OnEvent));
			Assert.AreEqual(1, pub.TheEventCtrl.SubCount);
			var exceptions = new List<EventRaiseException>();
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(true, pub.Raise(exceptions));
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(FailingStaticSubscriber.OnEvent));
			Assert.AreEqual(2, pub.TheEventCtrl.SubCount);
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(3, pub.TheEventCtrl.SubCount);
			Assert.AreEqual(false, pub.Raise(null));
			Assert.AreEqual(false, pub.Raise(exceptions));
			Assert.AreEqual(1, exceptions.Count);
			EventHandler<TestEventArgs> test = FailingStaticSubscriber.OnEvent;
			Assert.AreEqual(test, exceptions[0].subscriber);
			Assert.AreEqual(4, StaticSubscriber1.counter);
			Assert.AreEqual(4, StaticSubscriber1.lastValue);
			Assert.AreEqual(2, StaticSubscriber2.counter);
			Assert.AreEqual(4, StaticSubscriber2.lastValue);
			Assert.AreEqual(2, FailingStaticSubscriber.counter);
			Assert.AreEqual(4, FailingStaticSubscriber.lastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(FailingStaticSubscriber.OnEvent));
			Assert.AreEqual(true, pub.Raise(null));
			Assert.AreEqual(true, pub.Raise(exceptions));
			Assert.AreEqual(1, exceptions.Count);
			Assert.AreEqual(test, exceptions[0].subscriber);
			Assert.AreEqual(2, FailingStaticSubscriber.counter);
			Assert.AreEqual(4, FailingStaticSubscriber.lastValue);
			Assert.AreEqual(6, StaticSubscriber1.counter);
			Assert.AreEqual(6, StaticSubscriber1.lastValue);
			Assert.AreEqual(4, StaticSubscriber2.counter);
			Assert.AreEqual(6, StaticSubscriber2.lastValue);
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber1.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(StaticSubscriber2.OnEvent));
			Assert.AreEqual(0, pub.TheEventCtrl.SubCount);

			//not necessary, just precaution for my solace
			GC.KeepAlive(pub);
		}
	}
}
