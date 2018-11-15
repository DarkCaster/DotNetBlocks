﻿
//autogenerated, any changes should be made at SafeEvents.tt

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DarkCaster.Events
{
	public sealed partial class SafeEvent<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
		private readonly object manageLock = new object();
		private readonly ReaderWriterLockSlim raiseRwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
				raiseRwLock.Dispose();
			}
		}

		public bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

	}
	public sealed partial class SafeEventDbg<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
		private readonly object manageLock = new object();
		private readonly ReaderWriterLockSlim raiseRwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
				raiseRwLock.Dispose();
			}
		}

		public bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

		public async Task<bool> Raise(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null)
		{
			
		}

	}
}