// SafeEventDbg.cs
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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace DarkCaster.Events
{
	internal static class SafeEventDbg
	{
		internal struct DelegateHandle
		{
			public readonly MethodInfo method;
			public readonly WeakReference weakTarget;

			public DelegateHandle(MethodInfo method, object target)
			{
				this.weakTarget = new WeakReference(target);
				this.method = method;
			}

			public override bool Equals(object obj)
			{
				if(!(obj is DelegateHandle))
					return false;
				var other = (DelegateHandle)obj;
				return method == other.method && ReferenceEquals(weakTarget.Target, other.weakTarget.Target);
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		internal static readonly Dictionary<MethodInfo, DynamicMethod> forwardersCache = new Dictionary<MethodInfo, DynamicMethod>();		
	}

	/// <summary>
	/// Variant of SafeEvent class, used for debug purposes
	/// </summary>
	public sealed class SafeEventDbg<T> : ISafeEventCtrl<T>, ISafeEvent<T> where T : EventArgs
	{
		private static readonly MethodInfo GetStrongTargetMethod = typeof(Forwarder).GetMethod("GetStrongTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly Type[] forwarderParams = new Type[] { typeof(Forwarder), typeof(object), typeof(EventArgs) };

		private static EventHandler<T> GenerateDelegate(MethodInfo method, object target)
		{
			if(target==null)
				return (EventHandler<T>)Delegate.CreateDelegate(typeof(EventHandler<T>), method);
			lock(SafeEventDbg.forwardersCache)
			{
				if(SafeEventDbg.forwardersCache.ContainsKey(method))
					return (EventHandler<T>)SafeEventDbg.forwardersCache[method].CreateDelegate(typeof(EventHandler<T>), target);
				var dynMethod = new DynamicMethod("InvokeEventOnObject", typeof(void), forwarderParams, typeof(Forwarder), true);
				var generator = dynMethod.GetILGenerator();
				generator.Emit(OpCodes.Ldarg_0); //stack: this
				generator.Emit(OpCodes.Call, GetStrongTargetMethod); //stack: weakTarget.Target
				generator.Emit(OpCodes.Castclass, method.DeclaringType); //stack: (<type of subscriber>)weakRef.Target 
				generator.Emit(OpCodes.Ldarg_1); //stack: (<type of subscriber>)weakRef.Target, sender
				generator.Emit(OpCodes.Ldarg_2); //stack: (<type of subscriber>)weakRef.Target, sender, args
				generator.Emit(OpCodes.Call, method); //stack: [empty]
				generator.Emit(OpCodes.Ret);
				var result = (EventHandler<T>)dynMethod.CreateDelegate(typeof(EventHandler<T>), target);
				SafeEventDbg.forwardersCache.Add(method, dynMethod);
				return result;
			}
		}

		private sealed class Forwarder
		{
			public readonly WeakReference weakTarget;
			public readonly MethodInfo method;
			public readonly EventHandler<T> fwdDelegate;

			public Forwarder(Delegate singleDelegate)
			{
				this.method = singleDelegate.Method;
				this.fwdDelegate = singleDelegate.Target == null ? GenerateDelegate(singleDelegate.Method, null) : GenerateDelegate(singleDelegate.Method, this);
				this.weakTarget = new WeakReference(singleDelegate.Target);
			}

			private object GetStrongTarget()
			{
				var result = weakTarget.Target;
				if(result == null)
					throw new EventDbgException(
						string.Format("Invoking callback delegate on dead object of type {0}. Did you remember to save your subscriber reference to field or variable, or call unsubscribe before object disposal ?", method.DeclaringType.ToString()),
						method.DeclaringType,
						null
					);
				return result;
			}
		}
		
		private const int INVLIST_MIN_RESIZE_LIMIT = 64;
		private int invListUsedLen = 0;
		private bool invListRebuildNeeded = false;
		private Forwarder[] invList = { null };
		private readonly Dictionary<SafeEventDbg.DelegateHandle, Forwarder> dynamicSubscribers = new Dictionary<SafeEventDbg.DelegateHandle, Forwarder>();

		private readonly object raiseLock = new object();
		private readonly object manageLock = new object();
		private bool recursiveRaiseCheck = false;
		private Delegate curDelegate = null;

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

		private int UpdateInvListOnRise_Safe()
		{
			lock(manageLock)
			{
				if(!invListRebuildNeeded)
					return invListUsedLen;
				invListRebuildNeeded = false;
				//optionally recreate invocationList array if there is not enough space
				if(dynamicSubscribers.Count < (invListUsedLen / 3) && invList.Length >= INVLIST_MIN_RESIZE_LIMIT)
					invList = new Forwarder[invList.Length / 2];
				else
				{
					var len = invList.Length;
					while(dynamicSubscribers.Count > len)
						len *= 2;
					if(len != invList.Length)
						invList = new Forwarder[len];
				}
				//copy values and set invListUsedLen;
				dynamicSubscribers.Values.CopyTo(invList, 0);
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
				if(ignoreErrors)
				{
					for(int i = 0; i < subLen; ++i)
					{
						var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
						if(dynamicSubscribers.ContainsKey(handle))
							continue;
						dynamicSubscribers.Add(handle, new Forwarder(subList[i]));
						invListRebuildNeeded = true;
					}
					return;
				}

				for(int i = 0; i < subLen; ++i)
				{
					var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
					if(dynamicSubscribers.ContainsKey(handle))
						throw new EventSubscriptionException("Subscriber's delegate list contains dublicates from active subscribers", subscriber, null);
				}

				for(int i = 0; i < subLen; ++i)
				{
					dynamicSubscribers.Add(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target), new Forwarder(subList[i]));
					invListRebuildNeeded = true;
				}
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

				if(ignoreErrors)
				{
					for(int i = 0; i < subLen; ++i)
						invListRebuildNeeded |= dynamicSubscribers.Remove(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target));
					return;
				}

				for(int i = 0; i < subLen; ++i)
				{
					var handle = new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target);
					if(!dynamicSubscribers.ContainsKey(handle))
						throw new EventSubscriptionException("Current subscribers list do not contain some subscribers requested for remove", null, null);
				}

				for(int i = 0; i < subLen; ++i)
					dynamicSubscribers.Remove(new SafeEventDbg.DelegateHandle(subList[i].Method, subList[i].Target));
				invListRebuildNeeded = true;
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
				if(recursiveRaiseCheck)
				{
					recursiveRaiseCheck = false;
					throw new EventDbgException(
						string.Format("Recursion detected while processing event callback on object of type {0}", curDelegate.Method.DeclaringType.FullName),
						curDelegate.Method.DeclaringType,
						curDelegate
					);
				}
				recursiveRaiseCheck = true;
				if(exceptions != null && exceptions.IsReadOnly)
					exceptions = null;
				var len = UpdateInvListOnRise_Safe();
				var result = true;
				for(int i = 0; i < len; ++i)
				{
					curDelegate = Delegate.CreateDelegate(typeof(EventHandler<T>), invList[i].weakTarget.Target, invList[i].method, false);
					try { invList[i].fwdDelegate(sender, args); }
					catch(EventDbgException ex)
					{
						throw ex;
					}
					catch(Exception ex)
					{
						if(exceptions != null)
							exceptions.Add(new EventRaiseException(string.Format("Subscriber's exception: {0}", ex.Message), curDelegate, ex));
						result = false;
					}
				}
				curDelegate = null;
				recursiveRaiseCheck = false;
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
