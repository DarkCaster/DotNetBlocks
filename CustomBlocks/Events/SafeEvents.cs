﻿// SafeEvents.cs
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

			public WeakForwarder(WeakHandle handle)
			{
				this.handle = handle;
				this.forwarder = GenerateForwarder(handle.method);
			}

			public bool Raise(object sender, EventArgs args)
			{
				return forwarder(handle.weakTarget, sender, args);
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
		private readonly Dictionary<SafeEventPublisher.WeakHandle, SafeEventPublisher.WeakForwarder> dynamicSubscribers = new Dictionary<SafeEventPublisher.WeakHandle, SafeEventPublisher.WeakForwarder>();

		private readonly object locker = new object();

		public void Subscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);

			lock(locker)
			{
				var key = new SafeEventPublisher.WeakHandle(subscriber.Method, subscriber.Target);
				var val = new SafeEventPublisher.WeakForwarder(key);
				dynamicSubscribers.Add(key, val);
			}
		}

		public void Unsubscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			lock(locker)
				dynamicSubscribers.Remove(new SafeEventPublisher.WeakHandle(subscriber.Method, subscriber.Target));
		}

		//TODO: remove obsolete elements
		public void Raise(T args)
		{
			foreach(var el in dynamicSubscribers)
				el.Value.Raise(this, args);
		}
	}
}
