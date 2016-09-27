// ISafeEventCtrl.cs
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
using System.Collections.Generic;

namespace DarkCaster.Events
{
	/// <summary>
	/// Control interface for custom event classes.
	/// For use by publisher to raise events and gather usage statistics.
	/// </summary>
	public interface ISafeEventCtrl<T> : IDisposable where T : EventArgs
	{
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
		bool Raise(Func<KeyValuePair<object,T>> preExec, Action postExec = null, ICollection<EventRaiseException> exceptions = null);
		
		/// <summary>
		/// Get current active subscriber's count.
		/// </summary>
		int SubCount { get; }
	}
}
