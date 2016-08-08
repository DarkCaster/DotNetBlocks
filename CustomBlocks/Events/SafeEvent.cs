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
	/// 
	/// 
	/// TODO: check performance for different subscribers count, maybe switch to different underlying storage and simplify delegate management logic
	public sealed class SafeEvent<T> : ISafeEventCtrl <T>, ISafeEvent<T> where T : EventArgs
	{
		private EventHandler<T> curSubscribers;

		private readonly object raiseLock = new object();
		private readonly object manageLock = new object();

		//remove dublicates from target invocation list
		private int RemoveDublicates(Delegate[] target)
		{
			var curLen = target.Length;
			for(int sp = 0; sp < curLen; ++sp)
				for(int tp = sp + 1; tp < curLen; ++tp)
					while(tp < curLen && target[tp].Equals(target[sp]))
					{
						target[tp] = target[curLen - 1];
						target[curLen - 1] = null;
						--curLen;
					}
			return curLen;
		}

		//remove dublicates from target using source list
		private int RemoveDublicates(Delegate[] source, int usedSourceLen, Delegate[] target, int usedTargetLen)
		{
			for(int sp = 0; sp < usedSourceLen; ++sp)
				for(int tp = 0; tp < usedTargetLen; ++tp)
					while(tp < usedTargetLen && target[tp].Equals(source[sp]))
					{
						target[tp] = target[usedTargetLen - 1];
						target[usedTargetLen - 1] = null;
						--usedTargetLen;
					}
			return usedTargetLen;
		}

		public void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false)
		{
			if(subscriber == null)
			{
				if(ignoreErrors)
					return;
				throw new EventSubscriptionException(null, "Subscriber is null", null);
			}

			var subList = subscriber.GetInvocationList();
			var subLenPre = RemoveDublicates(subList);
			if(!ignoreErrors && subLenPre != subList.Length)
				throw new EventSubscriptionException(subscriber, "Subscriber's delegate list contains dublicates", null);

			lock(manageLock)
			{
				if(curSubscribers != null)
				{
					var curSubList = curSubscribers.GetInvocationList();
					var subLenPost = RemoveDublicates(curSubList, curSubList.Length, subList, subLenPre);
					if(subLenPost != subLenPre && !ignoreErrors)
						throw new EventSubscriptionException(subscriber, "Subscriber's delegate list contains dublicates from current list", null);
				}
				curSubscribers = (EventHandler<T>)Delegate.Combine(curSubscribers, Delegate.Combine(subList));
			}
		}

		private void Unsubscribe_Internal(Delegate[] subList, int subLen, bool ignoreErrors)
		{
			lock(manageLock)
			{
				if(curSubscribers == null)
				{
					if(ignoreErrors)
						return;
					throw new EventSubscriptionException(null, "Current subscribers list is already null", null);
				}
				var curList = curSubscribers.GetInvocationList();
				var newCurLen=RemoveDublicates(subList, subLen, curList, curList.Length);
				if(curList.Length-newCurLen!=subLen && !ignoreErrors)
					throw new EventSubscriptionException(null, "Current subscribers list do not contain some subscribers requested for remove", null);
				curSubscribers = (EventHandler<T>)Delegate.Combine(curList);
			}
		}

		public void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false, bool waitForRemoval = false)
		{
			if(subscriber == null)
			{
				if(ignoreErrors)
					return;
				throw new EventSubscriptionException(null, "Subscriber is null", null);
			}

			var subList = subscriber.GetInvocationList();
			var subLenPre = RemoveDublicates(subList);
			if(!ignoreErrors && subLenPre != subList.Length)
				throw new EventSubscriptionException(subscriber, "Subscriber's delegate list contains dublicates", null);

			if(waitForRemoval)
				lock(raiseLock)
					Unsubscribe_Internal(subList, subLenPre, ignoreErrors);
			else
				Unsubscribe_Internal(subList, subLenPre, ignoreErrors);
		}

		public void Raise(object sender, T args)
		{
			lock(raiseLock)
			{
				throw new NotImplementedException("TODO");
			}
		}

		public event EventHandler<T> Event
		{
			add { Subscribe(value); }
			remove { Unsubscribe(value, false); }
		}

		public int SubCount
		{
			get
			{
				lock(manageLock)
					return curSubscribers == null ? 0 : curSubscribers.GetInvocationList().Length;
			}
		}
	}
}
