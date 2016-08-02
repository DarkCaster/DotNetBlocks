// SafeEvents.cs
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
	internal static class SafeEventPublisher
	{
		private static readonly MethodInfo GetStrongRef = typeof(WeakReference).GetMethod("get_Target");
		private static readonly Type[] forwarderParams = new Type[] { typeof(WeakReference)/*weakRefToSubscriber*/, typeof(object)/*sender*/, typeof(EventArgs)/*event args*/ };
		private delegate bool ForwarderDelegate(WeakReference reference, object sender, EventArgs args);

		internal struct WeakHandle
		{
			public readonly MethodInfo method;
			public readonly WeakReference weakTarget;

			public WeakHandle(MethodInfo method, object target)
			{
				this.weakTarget = new WeakReference(target);
				this.method = method;
			}

			public override bool Equals(object obj)
			{
				if(!(obj is WeakHandle))
					return false;
				var other = (WeakHandle)obj;
				if(other.weakTarget == null || weakTarget == null)
					return false;
				return method == other.method && ReferenceEquals(weakTarget.Target, other.weakTarget.Target);
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		internal sealed class WeakForwarder
		{
			public readonly WeakHandle handle;
			private readonly ForwarderDelegate forwarder;
			private bool isActive;

			public WeakForwarder(WeakHandle handle)
			{
				this.handle = handle;
				this.forwarder = GenerateForwarder(handle.method);
				this.isActive = true;
			}

			public bool Raise_Safe(object sender, EventArgs args)
			{
				lock(this)
					return isActive && forwarder(handle.weakTarget, sender, args);
			}

			public void MarkAsRemoved_Safe()
			{
				lock(this)
					isActive = false;
			}
		}

		private static readonly Dictionary<MethodInfo, ForwarderDelegate> forwarderCache = new Dictionary<MethodInfo, ForwarderDelegate>();

		private static ForwarderDelegate GenerateForwarder(MethodInfo method)
		{
			lock(forwarderCache)
			{
				if(forwarderCache.ContainsKey(method))
					return forwarderCache[method];
				var dynMethod = new DynamicMethod("InvokeEventOnObject", typeof(bool), forwarderParams, true);
				var generator = dynMethod.GetILGenerator();
				generator.Emit(OpCodes.Ldarg_0); //stack: weakRef
				generator.Emit(OpCodes.Call, GetStrongRef); //stack: weakRef.Target
				generator.Emit(OpCodes.Dup); //stack: weakRef.Target, weakRef.Target
				var continueLabel = generator.DefineLabel();
				generator.Emit(OpCodes.Brtrue_S, continueLabel); //stack: weakRef.Target
				generator.Emit(OpCodes.Pop); //stack: [empty]
				generator.Emit(OpCodes.Ldc_I4_S, 0); //stack: 0(false)
				generator.Emit(OpCodes.Ret);
				generator.MarkLabel(continueLabel); //stack: weakRef.Target
				generator.Emit(OpCodes.Castclass, method.DeclaringType); //stack: (EventHandler<T>)weakRef.Target 
				generator.Emit(OpCodes.Ldarg_1); //stack: (EventHandler<T>)weakRef.Target, sender
				generator.Emit(OpCodes.Ldarg_2); //stack: (EventHandler<T>)weakRef.Target, sender, args
				generator.Emit(OpCodes.Call, method); //stack: [empty]
				generator.Emit(OpCodes.Ldc_I4_S, 1); //stack: 1(true)
				generator.Emit(OpCodes.Ret);
				var result=(ForwarderDelegate)dynMethod.CreateDelegate(typeof(ForwarderDelegate));
				forwarderCache.Add(method, result);
				return result;
			}
		}
	}

	/// <summary>
	/// Work in progress. Class is used for some experiments now, it will be changed a lot in future.
	///
	/// Custom event system, that aims to mitigate some flaws of default c# events.
	/// Main features of this custom event system:
	/// - thread safety: TODO detailed description
	/// - weak references: TODO detailed description
	/// - optional async enevt invocation: TODO detailed description
	/// </summary>
	public class SafeEventPublisher<T> : IEventPublisher<T> where T : EventArgs
	{
		private readonly object raiseLock = new object();
		private readonly object managementLock = new object();
		private readonly Dictionary<SafeEventPublisher.WeakHandle, SafeEventPublisher.WeakForwarder> dynamicSubscribers = new Dictionary<SafeEventPublisher.WeakHandle, SafeEventPublisher.WeakForwarder>();
		private SafeEventPublisher.WeakForwarder[] invList = { null };
		private const int INVLIST_MIN_RESIZE_LIMIT = 64;
		private int invListUsedLen = 0;
		private bool invListRebuildNeeded = false;
		private bool recursiveRaiseCheck = false;

		private int UpdateInvListOnRise_Safe()
		{
			lock(managementLock)
			{
				if(!invListRebuildNeeded)
					return invListUsedLen;
				invListRebuildNeeded = false;
				//optionally recreate invocationList array if there is not enough space
				if(dynamicSubscribers.Count < (invListUsedLen / 3) && invList.Length >= INVLIST_MIN_RESIZE_LIMIT)
					invList = new SafeEventPublisher.WeakForwarder[invList.Length / 2];
				else
				{
					var len = invList.Length;
					while(dynamicSubscribers.Count > len)
						len *= 2;
					if(len != invList.Length)
						invList = new SafeEventPublisher.WeakForwarder[len];
				}
				//copy values and set invListUsedLen;
				dynamicSubscribers.Values.CopyTo(invList, 0);
				invListUsedLen = dynamicSubscribers.Count;
				for(int i = invListUsedLen; i < invList.Length; ++i)
					invList[i] = null;
				return invListUsedLen;
			}
		}

		public void Subscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			var method = subscriber.Method;
			var target = subscriber.Target;
			if(target == null)
				throw new NotImplementedException("TODO: static subscribers");
			lock(managementLock)
			{
				var key = new SafeEventPublisher.WeakHandle(method, target);
				if(dynamicSubscribers.ContainsKey(key))
					return;
				invListRebuildNeeded = true;
				var val = new SafeEventPublisher.WeakForwarder(key);
				dynamicSubscribers.Add(key, val);
			}
		}

		private void Remove_Safe(SafeEventPublisher.WeakHandle handle)
		{
			lock(managementLock)
			{
				if(!dynamicSubscribers.ContainsKey(handle))
					return;
				invListRebuildNeeded = true;
				dynamicSubscribers[handle].MarkAsRemoved_Safe();
				dynamicSubscribers.Remove(handle);
			}
		}

		public void Unsubscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			var method = subscriber.Method;
			var target = subscriber.Target;
			if(target == null)
				throw new NotImplementedException("TODO: static subscribers");
			var handle=new SafeEventPublisher.WeakHandle(method, target);
			Remove_Safe(handle);
		}

		//TODO: thread safe async variants of Raise
		public void Raise(T args)
		{
			lock(raiseLock)
			{
				if(recursiveRaiseCheck)
					throw new EventRaiseException("Recursive event activation detected in single thread!", null, null);
				recursiveRaiseCheck = true;
				var len = UpdateInvListOnRise_Safe();
				for(int i = 0; i < len; ++i)
					if(!invList[i].Raise_Safe(this, args))
						Remove_Safe(invList[i].handle);
				recursiveRaiseCheck = false;
			}
		}
	}
}
