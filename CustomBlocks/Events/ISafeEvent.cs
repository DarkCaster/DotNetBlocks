// ISafeEvent.cs
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
		/// </summary>
		/// <param name="subscriber">Event callback - generic variant of EventHandler, where T is EventArgs</param>
		/// <param name="ignoreErrors">Do not throw errors while subscribing. Try to subscribe any elements from invocation list that is not already subscribed</param>
		void Subscribe(EventHandler<T> subscriber, bool ignoreErrors = false);

		/// <summary>
		/// Unsubscribe from event.
		/// Trying to unsubscribe multicast delegate that not contains entries already subscribed earlier or dublicate entries or null delegate
		/// will throw EventSubscriptionException, unless ignoreErrors is set to true.
		/// If waitForRemoval flag is set to false - method call will not block,
		/// but there is no guarantee that event callback will not be triggered again after unsubscribe method call is complete,
		/// so your event processing code should be robust enough to overcome such situation.  
		/// If waitForRemoval flag is set to true, event callback will not be triggered again after this method call is complete,
		/// but Unsubscribe method may temporary lock and wait for current event processing is complete.
		/// So you should not use locking in event processing and event management thread at the same time to avoid deadlocks.
		/// </summary>
		/// <param name="subscriber">Event callback method used at subscribe - generic variant of EventHandler, where T is EventArgs</param>
		/// <param name="waitForRemoval">Wait for removal. if true - it is guaranteed, thar event processing callback will not be triggered after unsubscribe</param>
		/// <param name="ignoreErrors">Do not throw errors while unsubscribing. Try to unsubscribe any elements from invocation list that have active subscription</param>
		void Unsubscribe(EventHandler<T> subscriber, bool ignoreErrors = false, bool waitForRemoval = false);

		/// <summary>
		/// Property for use as drop-in replacement for standard events;
		/// </summary>
		[Obsolete("There is no fully functionally identical drop-in replacement for standard events, so consider use Subscribe and Unsubscribe methods instead")]
		event EventHandler<T> Event;
	}
}
