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
using System.Threading;
using NUnit.Framework;
using DarkCaster.Events;
using Tests.SafeEventStuff;

namespace Tests
{
	public static class CommonEventTests
	{
		public static void SubscribeUnsubscribe(ISubscriber sub1, ISubscriber sub2, IPublisher pub)
		{
			//subscribe multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub1.OnEvent));
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(sub1.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub1.OnEvent, true));

			//currect subscribe and unsubscribe
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub2.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(sub2.OnEvent));

			//unsubscribe 1-st subscriber multiple times
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(sub1.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent, true));

			var delegateList1 = (EventHandler<TestEventArgs>)Delegate.Combine((EventHandler<TestEventArgs>)sub1.OnEvent, (EventHandler<TestEventArgs>)sub2.OnEvent);

			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			//try to subscribe delegate list with already registered delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList1));
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			//try to focre unsubscribe delegate list containing non subscribed delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1, true));
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			//try to force subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1, true));
			//try to unsubscribe delegate list
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(delegateList1));
			//try to unsubscribe delegate list again
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			//try to subscribe delegate list again
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList1));
			//unsubscribe single delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			//try to unsubscribe delegate list containing non subscribed delegate
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList1));
			//unsubscribe remaining delegate
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));

			//delegatelist, that contains dublicate
			var delegateList2 = (EventHandler<TestEventArgs>)Delegate.Combine(delegateList1, (EventHandler<TestEventArgs>)sub2.OnEvent);
			//try to subscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Subscribe(delegateList2));
			//try to force subscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			//try to unsubscribe delegate list that contains dublicates
			Assert.Throws(typeof(EventSubscriptionException), () => pub.TheEvent.Unsubscribe(delegateList2));
			//try to force unsubscribe delegate list that contains dublicates
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));

			//subscribe delegate list and unsubscribe manually
			Assert.DoesNotThrow(() => pub.TheEvent.Subscribe(delegateList2, true));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub2.OnEvent));
			Assert.DoesNotThrow(() => pub.TheEvent.Unsubscribe(sub1.OnEvent));
		}
	}
}
