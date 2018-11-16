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
				if (!(obj is DelegateHandle))
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

	public partial class SafeEventDbg<T> where T : EventArgs
	{
		private static readonly MethodInfo GetStrongTargetMethod = typeof(Forwarder).GetMethod("GetStrongTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly Type[] forwarderParams = new Type[] { typeof(Forwarder), typeof(object), typeof(EventArgs) };

		private static EventHandler<T> GenerateDelegate(MethodInfo method, object target)
		{
			if (target == null)
				return (EventHandler<T>)Delegate.CreateDelegate(typeof(EventHandler<T>), method);
			lock (SafeEventDbg.forwardersCache)
			{
				if (SafeEventDbg.forwardersCache.ContainsKey(method))
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
				if (result == null)
					throw new EventDbgException(
						string.Format("Invoking callback delegate on dead object of type {0}. Did you remember to save your subscriber reference to field or variable, or call unsubscribe before object disposal ?", method.DeclaringType.ToString()),
						method.DeclaringType,
						null
					);
				return result;
			}
		}
	}
}
