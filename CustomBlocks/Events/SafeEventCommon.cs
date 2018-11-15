// SafeEventCommon.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2018 DarkCaster <dark.caster@outlook.com>
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
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;

namespace DarkCaster.Events
{
	public sealed partial class SafeEvent<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
		public void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false)
		{
			if (subscriber == null)
			{
				if (ignoreErrors)
					return;
				throw new EventSubscriptionException("Subscriber is null", null, null);
			}

			var subList = subscriber.GetInvocationList();
			var subLen = RemoveDublicates(subList);
			if (!ignoreErrors && subLen != subList.Length)
				throw new EventSubscriptionException("Subscriber's delegate list contains dublicates", subscriber, null);

			lock (manageLock)
			{
				if (!ignoreErrors)
				{
					for (int i = 0; i < subLen; ++i)
						if (dynamicSubscribers.Contains((EventHandler<T>)subList[i]))
							throw new EventSubscriptionException("Subscriber's delegate list contains dublicates from active subscribers", subscriber, null);
					for (int i = 0; i < subLen; ++i)
						dynamicSubscribers.Add((EventHandler<T>)subList[i]);
					invListRebuildNeeded = true;
				}
				else
					for (int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Add((EventHandler<T>)subList[i]);
			}
		}

		public void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false)
		{
			if (subscriber == null)
			{
				if (ignoreErrors)
					return;
				throw new EventSubscriptionException("Subscriber is null", null, null);
			}

			var subList = subscriber.GetInvocationList();
			var subLen = RemoveDublicates(subList);
			if (!ignoreErrors && subLen != subList.Length)
				throw new EventSubscriptionException("Subscriber's delegate list contains dublicates", subscriber, null);

			lock (manageLock)
			{
				if (dynamicSubscribers.Count == 0)
				{
					if (ignoreErrors)
						return;
					throw new EventSubscriptionException("Current subscribers list is already empty", null, null);
				}
				if (!ignoreErrors)
				{
					for (int i = 0; i < subLen; ++i)
						if (!dynamicSubscribers.Contains((EventHandler<T>)subList[i]))
							throw new EventSubscriptionException("Current subscribers list do not contain some subscribers requested for remove", null, null);
					for (int i = 0; i < subLen; ++i)
						dynamicSubscribers.Remove((EventHandler<T>)subList[i]);
					invListRebuildNeeded = true;
				}
				else
					for (int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Remove((EventHandler<T>)subList[i]);
			}
		}

		public bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				if (preExec != null)
					preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					try { invList[i](sender, args); }
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), invList[i], ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{
				raiseRwLock.ExitWriteLock();
			}
		}

		public bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				var args = preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					try { invList[i](sender, args); }
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), invList[i], ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{ raiseRwLock.ExitWriteLock(); }
		}

		public bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				var pair = preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					try { invList[i](pair.Key, pair.Value); }
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), invList[i], ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{ raiseRwLock.ExitWriteLock(); }
		}
	}
}
