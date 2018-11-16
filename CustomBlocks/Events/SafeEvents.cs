﻿// SafeEvents.tt; SafeEvents.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016-2018 DarkCaster <dark.caster@outlook.com>
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

//autogenerated, any changes should be made at SafeEvents.tt

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using DarkCaster.Async;

namespace DarkCaster.Events
{
	/// <summary>
	/// SafeEvent class, for release usage
	/// </summary>
	public sealed partial class SafeEvent<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
		private readonly object manageLock = new object();
		private readonly AsyncRWLock raiseRwLock = new AsyncRWLock();

		private const int INVLIST_MIN_RESIZE_LIMIT = 64;
		private int invListUsedLen = 0;
		private bool invListRebuildNeeded = false;

		private EventHandler<T>[] invList = { null };
		private readonly HashSet<EventHandler<T>> dynamicSubscribers = new HashSet<EventHandler<T>>();

		//remove dublicates from target invocation list
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int RemoveDublicates(Delegate[] target)
		{
			var curLen = target.Length;
			for (int sp = 0; sp < curLen; ++sp)
				for (int tp = sp + 1; tp < curLen; ++tp)
					while (tp < curLen && target[tp].Equals(target[sp]))
					{
						target[tp] = target[curLen - 1];
						target[curLen - 1] = null;
						--curLen;
					}
			return curLen;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int UpdateInvListOnRise_Safe()
		{
			lock (manageLock)
			{
				if (!invListRebuildNeeded)
					return invListUsedLen;
				invListRebuildNeeded = false;
				//optionally recreate invocationList array if there is not enough space
				if (dynamicSubscribers.Count < (invListUsedLen / 3) && invList.Length >= INVLIST_MIN_RESIZE_LIMIT)
					invList = new EventHandler<T>[invList.Length / 2];
				else
				{
					var len = invList.Length;
					while (dynamicSubscribers.Count > len)
						len *= 2;
					if (len != invList.Length)
						invList = new EventHandler<T>[len];
				}
				//copy values and set invListUsedLen;
				dynamicSubscribers.CopyTo(invList, 0);
				invListUsedLen = dynamicSubscribers.Count;
				for (int i = invListUsedLen; i < invList.Length; ++i)
					invList[i] = null;
				return invListUsedLen;
			}
		}

		public event EventHandler<T> Event
		{
			add { Subscribe(value, true); }
			remove { Unsubscribe(value, true); }
		}

		public int SubCount
		{
			get
			{
				lock (manageLock)
					return dynamicSubscribers.Count;
			}
		}

		private bool isDisposed = false;

		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;
				//delay dispose in case when other publisher's thread is finishing it's work but still using ISafeEventCtrl methods.
				//such situation is already an error, so following 2 lines may be removed in future.
				raiseRwLock.EnterWriteLock();
				raiseRwLock.ExitWriteLock();
			}
		}

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

		public TResult SafeExec<TResult>(Func<TResult> method)
		{
			raiseRwLock.EnterReadLock();
			try { return method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public void SafeExec(Action method)
		{
			raiseRwLock.EnterReadLock();
			try { method(); }
			finally { raiseRwLock.ExitReadLock(); }
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
			{
				raiseRwLock.ExitWriteLock();
			}
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
			{
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<TResult> SafeExecAsync<TResult>(Func<Task<TResult>> method)
		{
			await raiseRwLock.EnterReadLockAsync();
			try { return await method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public async Task SafeExecAsync(Func<Task> method)
		{
			await raiseRwLock.EnterReadLockAsync();
			try { await method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public async Task<bool> RaiseAsync(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				if (preExec != null)
					await preExec();
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
					await postExec();
				return result;
			}
			finally
			{
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<bool> RaiseAsync(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				var args = await preExec();
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
					await postExec();
				return result;
			}
			finally
			{
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<bool> RaiseAsync(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				var pair = await preExec();
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
					await postExec();
				return result;
			}
			finally
			{
				raiseRwLock.ExitWriteLock();
			}
		}
	}
	/// <summary>
	/// SafeEventDbg class, SafeEvent with debug features
	/// </summary>
	public sealed partial class SafeEventDbg<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
		private readonly object manageLock = new object();
		private readonly AsyncRWLock raiseRwLock = new AsyncRWLock();

		private const int INVLIST_MIN_RESIZE_LIMIT = 64;
		private int invListUsedLen = 0;
		private bool invListRebuildNeeded = false;

		private Forwarder[] invList = { null };
		private readonly Dictionary<SafeEventDbg.DelegateHandle, Forwarder> dynamicSubscribers = new Dictionary<SafeEventDbg.DelegateHandle, Forwarder>();
		private bool recursiveRaiseCheck = false;
		private Delegate curDelegate = null;

		//remove dublicates from target invocation list
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int RemoveDublicates(Delegate[] target)
		{
			var curLen = target.Length;
			for (int sp = 0; sp < curLen; ++sp)
				for (int tp = sp + 1; tp < curLen; ++tp)
					while (tp < curLen && target[tp].Equals(target[sp]))
					{
						target[tp] = target[curLen - 1];
						target[curLen - 1] = null;
						--curLen;
					}
			return curLen;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int UpdateInvListOnRise_Safe()
		{
			lock (manageLock)
			{
				if (!invListRebuildNeeded)
					return invListUsedLen;
				invListRebuildNeeded = false;
				//optionally recreate invocationList array if there is not enough space
				if (dynamicSubscribers.Count < (invListUsedLen / 3) && invList.Length >= INVLIST_MIN_RESIZE_LIMIT)
					invList = new Forwarder[invList.Length / 2];
				else
				{
					var len = invList.Length;
					while (dynamicSubscribers.Count > len)
						len *= 2;
					if (len != invList.Length)
						invList = new Forwarder[len];
				}
				//copy values and set invListUsedLen;
				dynamicSubscribers.Values.CopyTo(invList, 0);
				invListUsedLen = dynamicSubscribers.Count;
				for (int i = invListUsedLen; i < invList.Length; ++i)
					invList[i] = null;
				return invListUsedLen;
			}
		}

		public event EventHandler<T> Event
		{
			add { Subscribe(value, true); }
			remove { Unsubscribe(value, true); }
		}

		public int SubCount
		{
			get
			{
				lock (manageLock)
					return dynamicSubscribers.Count;
			}
		}

		private bool isDisposed = false;

		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;
				//delay dispose in case when other publisher's thread is finishing it's work but still using ISafeEventCtrl methods.
				//such situation is already an error, so following 2 lines may be removed in future.
				raiseRwLock.EnterWriteLock();
				raiseRwLock.ExitWriteLock();
			}
		}

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
				if (ignoreErrors)
				{
					for (int i = 0; i < subLen; ++i)
					{
						var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
						if (dynamicSubscribers.ContainsKey(handle))
							continue;
						dynamicSubscribers.Add(handle, new Forwarder(subList[i]));
						invListRebuildNeeded = true;
					}
					return;
				}
				for (int i = 0; i < subLen; ++i)
				{
					var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
					if (dynamicSubscribers.ContainsKey(handle))
						throw new EventSubscriptionException("Subscriber's delegate list contains dublicates from active subscribers", subscriber, null);
				}
				for (int i = 0; i < subLen; ++i)
				{
					dynamicSubscribers.Add(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target), new Forwarder(subList[i]));
					invListRebuildNeeded = true;
				}
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
				if (ignoreErrors)
				{
					for (int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Remove(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target));
					return;
				}
				for (int i = 0; i < subLen; ++i)
				{
					var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
					if (!dynamicSubscribers.ContainsKey(handle))
						throw new EventSubscriptionException("Current subscribers list do not contain some subscribers requested for remove", null, null);
				}
				for (int i = 0; i < subLen; ++i)
					dynamicSubscribers.Remove(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target));
				invListRebuildNeeded = true;
			}
		}

		public TResult SafeExec<TResult>(Func<TResult> method)
		{
			raiseRwLock.EnterReadLock();
			try { return method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public void SafeExec(Action method)
		{
			raiseRwLock.EnterReadLock();
			try { method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				if (preExec != null)
					preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(sender, args); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}

		public bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				var args = preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(sender, args); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}

		public bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			raiseRwLock.EnterWriteLock();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				var pair = preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(pair.Key, pair.Value); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<TResult> SafeExecAsync<TResult>(Func<Task<TResult>> method)
		{
			await raiseRwLock.EnterReadLockAsync();
			try { return await method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public async Task SafeExecAsync(Func<Task> method)
		{
			await raiseRwLock.EnterReadLockAsync();
			try { await method(); }
			finally { raiseRwLock.ExitReadLock(); }
		}

		public async Task<bool> RaiseAsync(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				if (preExec != null)
					await preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(sender, args); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					await postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<bool> RaiseAsync(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				var args = await preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(sender, args); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					await postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}

		public async Task<bool> RaiseAsync(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			await raiseRwLock.EnterWriteLockAsync();
			try
			{
				if (recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				var pair = await preExec();
				if (exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for (int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(pair.Key, pair.Value); }
					catch (EventDbgException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				if (postExec != null)
					await postExec();
				return result;
			}
			finally
			{
				curDelegate = null;
				recursiveRaiseCheck = false;
				raiseRwLock.ExitWriteLock();
			}
		}
	}
}