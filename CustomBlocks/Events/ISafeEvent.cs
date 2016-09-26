﻿// ISafeEvent.cs
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

namespace DarkCaster.Events
{
	/// <summary>
	/// Generic interface for various custom events and messaging implementations. TODO: detailed description
	/// </summary>
	public interface ISafeEvent<T> where T : EventArgs
	{
		/// <summary>
		/// Subscribe for event. Method is thread safe.
		/// Trying to subscribe multicast delegate that contains entries already subscribed earlier or dublicate entries or null delegate
		/// will throw EventSubscriptionException, unless ignoreErrors is set to true.
		/// If publisher logic runs in its own thread there is no guarantee that subscribe will be performed not at the same time as event-raise calls,
		/// so your code should be robust enough.
		/// If you need that event raise is not performed at the same time with subscribe - wrap this call with "SafeExec".
		/// </summary>
		/// <param name="subscriber">Event callback - generic variant of EventHandler, where T is EventArgs</param>
		/// <param name="ignoreErrors">Do not throw errors while subscribing. Try to subscribe any elements from invocation list that is not already subscribed</param>
		void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false);

		/// <summary>
		/// Unsubscribe from event.
		/// Trying to unsubscribe multicast delegate that not contains entries already subscribed earlier or dublicate entries or null delegate
		/// will throw EventSubscriptionException, unless ignoreErrors is set to true.
		/// If publisher logic runs in its own thread there is no guarantee that unsubscribe will be performed not at the same time as event-raise calls,
		/// so your code should be robust enough.
		/// If you need that event raise is not performed at the same time or after unsubscribe - wrap this call with "SafeExec".
		/// </summary>
		/// <param name="subscriber">Event callback method used at subscribe - generic variant of EventHandler, where T is EventArgs</param>
		/// <param name="ignoreErrors">Do not throw errors while unsubscribing. Try to unsubscribe any elements from invocation list that have active subscription</param>
		void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false);
		
		/// <summary>
		/// Wait for event raise process to complete and execute your code.
		/// This will only ensure that event raise process is NOT running in it's own thread at the same time when executing your code,
		/// SafeExec method may be run in parallel with other subscribers.
		/// Recursive execution is allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		/// <returns>Return value from your method</returns>
		TResult SafeExec<TResult>(Func<TResult> method);
		
		/// <summary>
		/// Wait for event raise process to complete and execute your code.
		/// This will only ensure that event raise process is NOT running in it's own thread at the same time when executing your code,
		/// SafeExec method may be run in parallel with other subscribers.
		/// Recursive execution is allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		void SafeExec(Action method);

		/// <summary>
		/// Property for use as drop-in replacement for standard events;
		/// </summary>
		[Obsolete("This is not a fully functionally identical drop-in replacement for standard events, so consider use Subscribe and Unsubscribe methods instead")]
		event EventHandler<T> Event;
	}
}
