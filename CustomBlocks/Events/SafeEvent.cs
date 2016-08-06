// SafeEvent.cs
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

namespace DarkCaster.Events
{
	/// <summary>
	/// Simple wrapper on top of events, that aims to simplify some particular usage patterns,
	/// and restrict usage of some other unsafe and error-prone practices.
	/// This particular class designed to effectively work with a small count of simultaneous subscribers, as standard events.
	/// It does not scale good, but performance is similar to regular c# events (it is based on default events).
	/// Also there is a special "debug" version of SafeEvent class, that may be used to debug situations when you forgot to unsubscribe,
	/// check for recursive invoke and perform some other checks (see it's docs for detailed description);
	///
	/// Key features of this custom events class:
	/// Publisher: raising event is thread safe and will block when it is run from different threads simultaneously.
	/// Subscriber: it is possible to unsubscribe in such way that subscriber's event callback will not be ever triggered after unsubscribe method exit
	/// (optional behavior, see interface docs).
	///
	/// Things to consider at subscriber side:
	/// Always unsubscribe from event when object is about to be disposed: in my opinion (and in this implementation),
	/// it is a subscriber's responsibility to notify publisher when it is about to be removed from use,
	/// because publisher does not know (and should not know) anything about internal organization of subscriber
	/// and when it is safe to remove it from invocation list. Use of so called "weak" events (based on weak references) is generally a bad idea,
	/// because there ALWAYS WILL be such moment in lifetime of subscriber when it is already removed and disconnected from program logic,
	/// but still not garbage collected. So subscriber's logic should be robust enough to be called in such "disconnected" state (also there is a needless overhead).
	/// There is no way to accurately predict from the publisher side when exactly subscriber is disposed and disconnected (especially in multithreaded scenario).
	/// That why it is a subscriber's responsibility to unsubscribe from publisher when it is shuting down.
	/// This events implementation guaranteed not to trigger subscriber's event callback after unsubscribe method is exited (when waitForRemoval param set to true).
	/// </summary>
	public sealed class SafeEvent<T> : ISafeEventCtrl <T>, ISafeEvent<T> where T : EventArgs
	{
		private EventHandler<T> curSubscribers;

		private readonly object raiseLock = new object();
		private readonly object manageLock = new object();

		private int CheckAndRemoveDublicates(Delegate[] target)
		{
			var curLen = target.Length;
			for(int sp = 0; sp < curLen; ++sp)
				for(int tp = sp + 1; tp < curLen; ++tp)
					while(target[tp].Equals(target[sp]) && tp < curLen)
					{
						target[tp] = target[curLen - 1];
						target[curLen - 1] = null;
						--curLen;
					}
			return curLen;
		}

		private int RemoveDublicatesFromTarget(Delegate[] source, Delegate[] target, int curTargetLen)
		{
			//remove dublicates from target using source list
			for(int sp = 0; sp < source.Length; ++sp)
				for(int tp = 0; tp < curTargetLen; ++tp)
					while(target[tp].Equals(source[sp]) && tp < curTargetLen)
					{
						target[tp] = target[curTargetLen - 1];
						target[curTargetLen - 1] = null;
						--curTargetLen;
					}
			return curTargetLen;
		}

		public void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false)
		{
			if(subscriber == null)
			{
				if(ignoreErrors)
					return;
				throw new EventSubscriptionException(null, "Trying to subscribe a null delegate", null);
			}

			var newSubList = subscriber.GetInvocationList();
			var newSubLen1 = CheckAndRemoveDublicates(newSubList);
			if(!ignoreErrors && newSubLen1 != newSubList.Length)
				throw new EventSubscriptionException(subscriber, "Delegate list contain dublicates", null);

			lock(manageLock)
			{
				if(curSubscribers != null)
				{
					var curSubList = curSubscribers.GetInvocationList();
					var newSubLen2 = RemoveDublicatesFromTarget(curSubList, newSubList, newSubLen1);
					if(newSubLen2 != newSubLen1 && !ignoreErrors)
						throw new EventSubscriptionException(subscriber, "New delegate list contain dublicates from current list", null);
				}
				Delegate.Combine(curSubscribers, Delegate.Combine(newSubList));
			}
		}

		public void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false, bool waitForRemoval = false)
		{
			throw new NotImplementedException("TODO");
		}

		public void Raise(object sender, T args)
		{
			lock(raiseLock)
			{
				//TODO: switch to eventList?.Invoke(this, args);
				var invokeList = curSubscribers;
				if(invokeList != null)
					invokeList.Invoke(sender, args);
			}
		}

		public event EventHandler<T> Event
		{
			add { Subscribe(value); }
			remove { Unsubscribe(value, false); }
		}
	}
}
