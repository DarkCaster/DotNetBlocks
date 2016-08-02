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
		internal static readonly MethodInfo GetStrongRef = typeof(WeakReference).GetMethod("get_Target");

		internal static readonly Type[] dynamicForwarderParams = new Type[] { typeof(WeakReference)/*weakRefToSubscriber*/, typeof(object)/*sender*/, typeof(EventArgs)/*event args*/ };
		internal delegate bool DynamicForwarder(WeakReference reference, object sender, EventArgs args);

		internal sealed class DynamicWeakHandle
		{
			public readonly MethodInfo method;
			public readonly WeakReference weakTarget;
			public readonly DynamicForwarder forwarder;

			public DynamicWeakHandle(MethodInfo method, object target, bool generateForwarder = false)
			{
				this.weakTarget = new WeakReference(target);
				this.method = method;
				this.forwarder = generateForwarder ? GenerateDynamicForwarder(method) : null;
			}

			public override bool Equals(object obj)
			{
				if(!(obj is DynamicWeakHandle))
					return false;
				var other = (DynamicWeakHandle)obj;
				return method == other.method && ReferenceEquals(weakTarget.Target, other.weakTarget.Target);
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		private static Dictionary<MethodInfo, DynamicForwarder> dynamicForwarderCache = new Dictionary<MethodInfo, DynamicForwarder>();

		private static DynamicForwarder GenerateDynamicForwarder(MethodInfo method)
		{
			lock(dynamicForwarderCache)
			{
				if(dynamicForwarderCache.ContainsKey(method))
					return dynamicForwarderCache[method];
				var dynMethod = new DynamicMethod("InvokeEventOnObject", typeof(bool), dynamicForwarderParams, true);
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
				var result=(DynamicForwarder)dynMethod.CreateDelegate(typeof(DynamicForwarder));
				dynamicForwarderCache.Add(method, result);
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
		//TODO: check other structures for effectiveness
		private readonly HashSet<SafeEventPublisher.DynamicWeakHandle> subscribers = new HashSet<SafeEventPublisher.DynamicWeakHandle>();
		private readonly object locker = new object();

		public void Subscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			lock(locker)
				subscribers.Add(new SafeEventPublisher.DynamicWeakHandle(subscriber.Method, subscriber.Target, true));
		}

		public void Unsubscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			lock(locker)
				subscribers.Remove(new SafeEventPublisher.DynamicWeakHandle(subscriber.Method, subscriber.Target));
		}

		//TODO: remove obsolete elements
		public void Raise(T args)
		{
			foreach(var el in subscribers)
				el.forwarder(el.weakTarget, this, args);
		}
	}
}
