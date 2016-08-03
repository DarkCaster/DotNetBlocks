// SafeEvent.cs
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
namespace DarkCaster
{
	/// <summary>
	/// Events implementation, that aims to mitigate some typical problems with default c# events.
	/// This particular class designed to effectively work with a small count of simultaneous subscribers, as standard events.
	/// It does not scale good, but performance is similar to regular c# events.
	///
	/// Key features of this custom events class:
	/// Thread safety for publisher: raising event is thread safe and will block when it is run from different threads simultaneously.
	/// Thread safety for subscriber: subscription management methods are thread safe.
	/// It is also possible to unsubscribe in such way that subscriber's event callback will not be ever triggered after unsubscribe method exit
	/// (optional behavior, see interface docs).
	///
	/// Things to consider at subscriber side:
	/// Always unsubscribe from event when object is about to be disposed: in my opinion (and in this implementation),
	/// it is a subscriber's responsibility to notify publisher when it is about to be removed from use,
	/// because publisher does not know (and should not know) anything about internal organization of subscriber
	/// and when it is safe to remove it from invocation list. Use of so called "weak" events (based on weak references) is generally a bad idea,
	/// because there ALWAYS WILL be such moment in lifetime of subscriber when it is already removed and disconnected from program logic,
	/// but still not garbage collected. So subscriber's logic should be robust enough to be called in such "disconnected" state (also there is a needless overhead).
	/// There is no way to accurately predict from the publisher side when exactly subscriber is disposed and disconnected (especially in multithreaded scenario).
	/// That why it is a subscriber's responsibility to unsubscribe from publisher when it is shuting down.
	/// This events implementation will not trigger subscriber's event callback after unsubscribe method is exited.
	///
	/// Also there is special "debug" version of SafeEvent class, that may be used to debug situations when you forgot to unsubscribe
	/// (see it's docs for detailed description)
	/// </summary>
	public class SafeEvent
	{
		public SafeEvent()
		{
		}
	}
}

