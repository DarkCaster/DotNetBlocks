// SafeEventTests.cs
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
	public class SafeEventTests
	{
		[Test]
		public void SubscribeUnsubscribe()
		{
			var sub1 = new SimpleSubscriber();
			var sub2 = new SimpleSubscriber();
			var ev = new SafeEvent<TestEventArgs>();
			var pub = new SimplePublisher<SafeEvent<TestEventArgs>,SafeEvent<TestEventArgs>>(ev, ev);
			CommonEventTests.SubscribeUnsubscribe(sub1, sub2, pub);
		}

		[Test]
		public void StaticSubscribeUnsubscribe()
		{
			var ev = new SafeEvent<TestEventArgs>();
			var pub = new SimplePublisher<SafeEvent<TestEventArgs>, SafeEvent<TestEventArgs>>(ev, ev);
			CommonEventTests.StaticSubscribeUnsubscribe(pub);
		}

		[Test]
		public void SubscriberException()
		{
			var sub1 = new SimpleSubscriber();
			var sub2 = new SimpleSubscriber();
			var sub3 = new CommonEventTests.FailingSubscriber();
			var ev = new SafeEvent<TestEventArgs>();
			var pub = new SimplePublisher<SafeEvent<TestEventArgs>,SafeEvent<TestEventArgs>>(ev, ev);
			CommonEventTests.SubscriberException(sub1, sub2, sub3, pub);
		}

		[Test]
		public void Raise()
		{
			var sub1 = new SimpleSubscriber();
			var ev = new SafeEvent<TestEventArgs>();
			var pub = new SimplePublisher<SafeEvent<TestEventArgs>, SafeEvent<TestEventArgs>>(ev, ev);
			CommonEventTests.Raise(sub1, pub);
		}
	}
}
