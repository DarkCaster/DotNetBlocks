// SafeEventTestsStuff.cs
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
using DarkCaster.Events;

namespace Tests.SafeEventStuff
{
	public class TestEventArgs : EventArgs
	{
		public int Val { get; set; }
	}

	public interface IPublisher
	{
		ISafeEvent<TestEventArgs> TheEvent { get; }
		ISafeEventCtrl<TestEventArgs> TheEventCtrl { get; }
		bool Raise(ICollection<EventRaiseException> exceptions = null);
	}

	public interface ISubscriber
	{
		int Counter { get; set; }
		int LastValue { get; set; }
		EventHandler<TestEventArgs> OnEvent { get; }
	}

	public class SimpleSubscriber : ISubscriber
	{
		public int counter = 0;
		public int lastValue = 0;
		public void OnTestEvent(object sender, TestEventArgs args)
		{
			++counter;
			lastValue = args.Val;
		}
		public int Counter { get { return counter; } set { counter = value; } }
		public int LastValue { get { return lastValue; } set { lastValue = value; } }
		public EventHandler<TestEventArgs> OnEvent { get { return OnTestEvent; } }
	}

	public class SimplePublisher : IPublisher
	{
		private int counter;
		private SafeEvent<TestEventArgs> theEvent = new SafeEvent<TestEventArgs>();
		public ISafeEvent<TestEventArgs> TheEvent { get { return theEvent; } }
		public ISafeEventCtrl<TestEventArgs> TheEventCtrl { get { return theEvent; } }
		public SimplePublisher() { counter = 0; }
		public bool Raise(ICollection<EventRaiseException> exceptions=null)
		{
			++counter;
			return theEvent.Raise(this, new TestEventArgs() { Val = counter }, exceptions);
		}
	}
}
