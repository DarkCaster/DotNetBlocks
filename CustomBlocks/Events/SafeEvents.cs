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

namespace DarkCaster.Events
{
	/// <summary>
	/// Custom event system, that aims to mitigate some flaws of default c# events.
	/// Main features of this custom event system:
	/// - thread safety: TODO detailed description
	/// - weak references: TODO detailed description
	/// - optional async enevt invocation: TODO detailed description
	/// </summary>
	public class SafeEventPublisher<T> : IEventPublisher<T> where T : EventArgs
	{
		private sealed class WeakDelegate
		{
			private readonly MethodInfo method;
			private readonly WeakReference target;
			private readonly bool isStatic;

			public WeakDelegate(MethodInfo method, object target)
			{
				if(target != null)
				{
					this.target = new WeakReference(target);
					this.isStatic = false;
				}
				else
				{
					this.target = null;
					this.isStatic = false;
				}
				this.method = method;
			}

			public override bool Equals(object obj)
			{
				if(!(obj is WeakDelegate))
					return false;
				var other = (WeakDelegate)obj;
				if(isStatic != other.isStatic)
					return false;
				if(isStatic)
					return method == other.method;
				else
					return method == other.method && target.Target == other.target.Target;
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		public void Subscribe(EventHandler<T> subscriber)
		{
			
		}

		public void Unsubscribe(EventHandler<T> subscriber)
		{
			

		}
	}
}
