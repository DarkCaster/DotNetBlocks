using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;

namespace DarkCaster.Events
{
	public sealed partial class SafeEvent<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
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


	}
	public sealed partial class SafeEventDbg<T> : ISafeEventCtrl<T>, ISafeEvent<T>, IDisposable where T : EventArgs
	{
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


	}
}