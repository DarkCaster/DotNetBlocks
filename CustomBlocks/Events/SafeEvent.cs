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
using System.Collections.Generic;

namespace DarkCaster.Events
{
	/// <summary>
	/// Description of SafeEvent.
	/// </summary>
	public sealed class SafeEvent<T> : ISafeEventCtrl <T>, ISafeEvent<T> where T : EventArgs
	{
		private const int INVLIST_MIN_RESIZE_LIMIT = 64;
		private int invListUsedLen = 0;
		private bool invListRebuildNeeded = false;
		private EventHandler<T>[] invList = { null };
		private readonly HashSet<EventHandler<T>> dynamicSubscribers=new HashSet<EventHandler<T>>();
		
		
		private readonly object raiseLock = new object();
		private readonly object manageLock = new object();
		
		//remove dublicates from target invocation list
		private static int RemoveDublicates(Delegate[] target)
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
		
		private int UpdateInvListOnRise_Safe()
		{
			lock(manageLock)
			{
				if(!invListRebuildNeeded)
					return invListUsedLen;
				invListRebuildNeeded = false;
				//optionally recreate invocationList array if there is not enough space
				if(dynamicSubscribers.Count < (invListUsedLen / 3) && invList.Length >= INVLIST_MIN_RESIZE_LIMIT)
					invList = new EventHandler<T>[invList.Length / 2];
				else
				{
					var len = invList.Length;
					while(dynamicSubscribers.Count > len)
						len *= 2;
					if(len != invList.Length)
						invList = new EventHandler<T>[len];
				}
				//copy values and set invListUsedLen;
				dynamicSubscribers.CopyTo(invList, 0);
				invListUsedLen = dynamicSubscribers.Count;
				for(int i = invListUsedLen; i < invList.Length; ++i)
					invList[i] = null;
				return invListUsedLen;
			}
		}
		
		public void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false)
		{
			if(subscriber == null)
			{
				if(ignoreErrors)
					return;
				throw new EventSubscriptionException("Subscriber is null", null, null);
			}

			var subList = subscriber.GetInvocationList();
			var subLen = RemoveDublicates(subList);
			if(!ignoreErrors && subLen != subList.Length)
				throw new EventSubscriptionException("Subscriber's delegate list contains dublicates", subscriber, null);

			lock(manageLock)
			{
				if(!ignoreErrors)
				{
					for(int i = 0; i < subLen; ++i)
						if(dynamicSubscribers.Contains((EventHandler<T>)subList[i]))
							throw new EventSubscriptionException("Subscriber's delegate list contains dublicates from active subscribers", subscriber, null);
					for(int i = 0; i < subLen; ++i)
						dynamicSubscribers.Add((EventHandler<T>)subList[i]);
					invListRebuildNeeded=true;
				}
				else
					for(int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Add((EventHandler<T>)subList[i]);
			}
		}
		
		private void Unsubscribe_Internal(Delegate[] subList, int subLen, bool ignoreErrors)
		{
			lock(manageLock)
			{
				if(dynamicSubscribers.Count == 0)
				{
					if(ignoreErrors)
						return;
					throw new EventSubscriptionException("Current subscribers list is already empty", null, null);
				}
				if(!ignoreErrors)
				{
					for(int i = 0; i < subLen; ++i)
						if(!dynamicSubscribers.Contains((EventHandler<T>)subList[i]))
							throw new EventSubscriptionException("Current subscribers list do not contain some subscribers requested for remove", null, null);
					for(int i = 0; i < subLen; ++i)
						dynamicSubscribers.Remove((EventHandler<T>)subList[i]);
					invListRebuildNeeded=true;
				}
				else
					for(int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Remove((EventHandler<T>)subList[i]);
			}
		}
		
		public void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false, bool waitForRemoval = false)
		{
			if(subscriber == null)
			{
				if(ignoreErrors)
					return;
				throw new EventSubscriptionException("Subscriber is null", null, null);
			}

			var subList = subscriber.GetInvocationList();
			var subLen = RemoveDublicates(subList);
			if(!ignoreErrors && subLen != subList.Length)
				throw new EventSubscriptionException("Subscriber's delegate list contains dublicates", subscriber, null);

			if(waitForRemoval)
				lock(raiseLock)
					Unsubscribe_Internal(subList, subLen, ignoreErrors);
			else
				Unsubscribe_Internal(subList, subLen, ignoreErrors);
		}
		
		public bool Raise(object sender, T args, ICollection<EventRaiseException> exceptions = null)
		{
			lock(raiseLock)
			{
				if(exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for(int i = 0; i < len; ++i)
				{
					try { invList[i](sender, args); }
					catch(Exception ex)
					{
						if(exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), invList[i], ex));
						result = false;
					}
				}
				return result;
			}
		}
		
		public event EventHandler<T> Event
		{
			add { Subscribe(value, true); }
			remove { Unsubscribe(value, true, false); }
		}

		public int SubCount
		{
			get
			{
				lock(manageLock)
					return dynamicSubscribers.Count;
			}
		}
		
		public object RaiseLock
		{
			get
			{
				return raiseLock;
			}
		}
	}
}
