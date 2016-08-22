# Custom event system.

This is a simple event system, made on top of `EventHandler<EventArgs>` delegates.
It is not a functionally equivalent replacement for default events.
By using SafeEvent or SafeEventDbg classes, it is not possible to implement publisher logic that will be
fully independent and 100% tolerant to incorrect subscriber's behavior, or to implement 100% robust subscriber's logic.
This custom event system instead aims to simplify implementation of some common good event usage patterns,
simplify use of events in multithreaded applications and restrict some common unsafe and error-prone practices.

## Features that helps to implement a more robust (and yet relatively simple) publisher's side logic:
 * Event raising process is thread safe and will block when it is run from different threads simultaneously.
 * TODO: you can manually request lock-object and `lock` on it before raising an event, if you want to do some stuff atomically with event processing.
 * `Raise` method will block by default (while calling to subscriber's callbacks), but you can wrap it inside `Task` (for example)
   if you want to run event processing in parallel without waiting for subscriber's callback completion.
 * Exceptions thrown by subscribers will not cause failure at publisher side and will not interrupt callbacks execution for remaining subscribers.
   `Raise` method returns `true` if no exceptions was thrown by subscribers while processing the event, `false` in case when some subscribers was failed.
   For debug purposes, all exceptions can be collected to ICollection<EventRaiseException> container (will be appended in order of appearance) to be processed later,
   but it is optional and not required for normal operation.
 * Additional features of SafeEventDbg class to detect some specific flaws in publisher's and subscriber's logic (performance is bad, not for production use):
  * Checks for recursive event raise. Will throw exception on `Raise` method execution in case when recursion detected.
  * Checks for missing "unsubscribe" call from the subscriber's side.
  This check may help to detect a "stall" subscriber that disconnected from any logic, but still not unsubscribed from publisher.
  When using default events (or SafeEvent class), this will lead to memory leak, because event delegate at publisher's side store strong reference to subscriber-object,
  so subscriber may never be garbage collected. One possible solution is to use so called "weak events" that do not store strong reference to subscriber-objects.
  This will lead to some performance loss on event raise, and also require that subscriber logic is robust enogh not to break things
  when it is triggered in such disconnected (but still not garbage collected!) state.
  I think that instead it is more correct to use explicit unsubscribe call from subscriber's side at the right moment
  (when subscriber complete it's purpose and going to be disposed).
  So, this check will help to detect situations when you forget to unsubscribe or store link to subscriber object.
  You can read more about weak events here: http://www.codeproject.com/Articles/29922/Weak-Events-in-C . Some ideas implemented there, was also inspired by that publication)
  * TODO (maybe): additional checks for custom EventArgs class implementation to be immutable (so, subscribers could not modify it fields or properties)

## Features and notes when implementing subscriber's callback logic:
 * Subscribing to event is thread safe and atomic. Trying to subscribe the same event callback delegate multiple times considered as error,
   EventSubscriptionException is thrown in such cases. You can also pass a delegate list (because delegates are multicast by default),
   but no subscription will be done at all if any single delegate from this list is already subscribed.
   There is a special param `ignoreErrors` that may be used to override this behavior
   in situations when you subscribe to event from diffetent places and do not want to perform any checks.
   When this parametes is used, subscription is done only for that delegates (and single delegates from multicast delegate list) that was not subscribed earlier.
 * Unsubscribing from event is also thread safe and atomic. ...
