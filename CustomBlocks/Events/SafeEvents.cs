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
		private delegate bool Forwarder(object sender, T args);

		private sealed class WeakDelegate
		{
			private readonly MethodInfo method;
			private readonly WeakReference target;
			public Forwarder forwarder;

			public WeakDelegate(MethodInfo method, object target, bool generateForwarder=true)
			{
				if(target == null)
					this.target = null;
				else
					this.target = new WeakReference(target);
				this.method = method;
				if(generateForwarder)
					forwarder = null;
				else
					GenerateForwarder(target == null);
			}

			private static Forwarder GenerateForwarder(bool isStatic)
			{
				throw new NotImplementedException("TODO");
				/*var dynMethod = new DynamicMethod("InvokeEventOnObject", typeof(void), new Type[] { typeof(object), typeof(object), typeof(T) }, typeof(WeakDelegate), true);
				var generator = dynMethod.GetILGenerator();
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Castclass, method.DeclaringType);
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldarg_2);
				generator.Emit(OpCodes.Call, method);
				generator.Emit(OpCodes.Ret);*/
			}

			public override bool Equals(object obj)
			{
				if(!(obj is WeakDelegate))
					return false;
				var other = (WeakDelegate)obj;
				if(target==null && other.target==null)
					return method == other.method;
				if(target != null && other.target != null)
					return method == other.method && target.Target == other.target.Target;
				return false;
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		//TODO: check other structures for effectiveness
		private readonly HashSet<WeakDelegate> subscribers = new HashSet<WeakDelegate>();

		private readonly object locker = new object();

		public void Subscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			lock(locker)
				subscribers.Add(new WeakDelegate(subscriber.Method, subscriber.Target));
		}

		public void Unsubscribe(EventHandler<T> subscriber)
		{
			if(subscriber == null)
				throw new EventSubscriptionException(null, "Failed to add a null subscriber of type " + typeof(T).FullName, null);
			lock(locker)
				subscribers.Remove(new WeakDelegate(subscriber.Method, subscriber.Target, false));
		}

		//TODO: remove obsolete elements
		public void Raise(T args)
		{
			foreach(var el in subscribers)
				el.forwarder(this, args);
		}
	}
}
