﻿// SafeEventsInterfaces.tt; SafeEventsInterfaces.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016-2018 DarkCaster <dark.caster@outlook.com>
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

//autogenerated, any changes should be made at SafeEventsInterfaces.tt

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
		/// Recursive execution is NOT allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		/// <returns>Return value from your method</returns>
		TResult SafeExec<TResult>(Func<TResult> method);

		/// <summary>
		/// Wait for event raise process to complete and execute your code.
		/// This will only ensure that event raise process is NOT running in it's own thread at the same time when executing your code,
		/// SafeExec method may be run in parallel with other subscribers.
		/// Recursive execution is NOT allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		void SafeExec(Action method);

		/// <summary>
		/// Wait for event raise process to complete and execute your code.
		/// This will only ensure that event raise process is NOT running in it's own thread at the same time when executing your code,
		/// SafeExec method may be run in parallel with other subscribers.
		/// Recursive execution is NOT allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		/// <returns>Return value from your method</returns>
		Task<TResult> SafeExecAsync<TResult>(Func<Task<TResult>> method);

		/// <summary>
		/// Wait for event raise process to complete and execute your code.
		/// This will only ensure that event raise process is NOT running in it's own thread at the same time when executing your code,
		/// SafeExec method may be run in parallel with other subscribers.
		/// Recursive execution is NOT allowed.
		/// All other methods from "ISafeEvent" can be also wrapped by "SafeExec".
		/// </summary>
		/// <param name="method">Your code goes here</param>
		Task SafeExecAsync(Func<Task> method);

		/// <summary>
		/// Property for use as drop-in replacement for standard events;
		/// </summary>
		[Obsolete("This is not a fully functionally identical drop-in replacement for standard events, so consider use Subscribe and Unsubscribe methods instead")]
		event EventHandler<T> Event;
	}

	/// <summary>
	/// Control interface for custom event classes. For use by publisher to raise events and gather usage statistics.
	/// </summary>
	[ObsoleteAttribute("This interface is obsolete and keep for compatibility. Consider to use ISafeEventCtrlLite instead.", false)]
	public interface ISafeEventCtrl<T> : IDisposable where T : EventArgs
	{
		/// <summary>
		/// Get current active subscriber's count.
		/// </summary>
		int SubCount { get; }

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="args">Event arguments</param>
		/// <param name="preExec">Your optional code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide link to sender object and event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="args">Event arguments</param>
		/// <param name="preExec">Your optional code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide link to sender object and event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);
	}

	/// <summary>
	/// Control interface for custom event classes (new version, dispose not needed anymore). For use by publisher to raise events and gather usage statistics.
	/// </summary>
	public interface ISafeEventCtrlLite<T> where T : EventArgs
	{
		/// <summary>
		/// Get current active subscriber's count.
		/// </summary>
		int SubCount { get; }

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="args">Event arguments</param>
		/// <param name="preExec">Your optional code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(object sender, T args, Action preExec = null, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(object sender, Func<T> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide link to sender object and event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		bool Raise(Func<KeyValuePair<object, T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="args">Event arguments</param>
		/// <param name="preExec">Your optional code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.
		/// Unhandled exception that is thrown from your code will interrupt Raise method execution, unlock it's own locks, and forwarded further to caller.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(object sender, T args, Func<Task> preExec = null, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="sender">Event sender object</param>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(object sender, Func<Task<T>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="preExec">Your code that will be executed inside Raise method locks right before to start calling event callbacks.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// Your code must generate and provide link to sender object and event arguments on return.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// But you can also set your own locks inside this code to lock down external-thread access to your important stuff while event-raise is processing.
		/// Event callbacks are executed inside thread that started Raise method,
		/// so it is possible to access your locked stuff from event-callbacks without deadlocks if you are using proper locking mechanisms</param>
		/// <param name="postExec">Your optional code that will be executed right after event raise in it's context.
		/// May be used to create atomic and interlocked logic, that must be executed with event-raise.
		/// External threads from subscribers that using "SafeExec" wrapper from ISafeEvent will be locked while executing your custom code within event-raise.
		/// You can disarm your own locks here, that was set in preExec.</param>
		/// <param name="exceptions">Optional storage to register exceptions happened during event raise</param>
		/// <returns>true - no exceptions was thrown while performing event raise.
		/// false - some subscriber's callbacks was failed, exceptions happened during raise may be collected to storage passed as "exceptions" param</returns>
		Task<bool> RaiseAsync(Func<Task<KeyValuePair<object, T>>> preExec, Func<Task> postExec = null, ICollection<EventRaiseException> exceptions = null);
	}
}