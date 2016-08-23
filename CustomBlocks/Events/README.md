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
  You can read more about weak events here: http://www.codeproject.com/Articles/29922/Weak-Events-in-C .
  Some ideas implemented in SafeEventDbg class was also inspired by that publication.
  * TODO (maybe): additional checks for custom EventArgs class implementation to be immutable (so, subscribers could not modify it's fields or properties)

## Features and notes when implementing subscriber's callback logic:
 * Subscribing to event is thread safe and atomic. Trying to subscribe the same event callback delegate multiple times considered as error:
   EventSubscriptionException is thrown in such cases. You can also pass a delegate list (because delegates are multicast),
   but EventSubscriptionException is thrown and no action will be done at all if any single delegate from this list is already subscribed.
   Also this exception is thrown, if multicast delegate contain dublicates. 
   There is a special param `ignoreErrors` that may be used to override this behavior
   in situations when you subscribe to event from diffetent places and\or do not want to perform any checks.
   When this parameter is used, subscription is done only for that delegates (single delegates from multicast delegate list) that was not already subscribed for this event,
   dublicates are simply ignored.
 * Unsubscribing from event is also thread safe and atomic. Trying to unsubscribe the same event callback delegate multiple times considered as error:
   EventSubscriptionException is thrown in such cases. You can also pass a delegate list (because delegates are multicast),
   but EventSubscriptionException is thrown and no action will be done at all if any single delegate from this list is already unsubscribed.
   Also this exception is thrown if multicast delegate contain dublicates.
   There is a special param `ignoreErrors` that may be used to override this behavior
   in situations when you unsubscribe from event from diffetent places in your code and\or do not want to perform any checks.
   When this parameter is used, unsubscribe is performed only for that delegates (single delegates from multicast delegate list) that is active now.
   = Notes for mitigating race conditions between ubsubscribe and event raise: =

   Usually, when performing event subscription management from different thread than thread wich is raising the event,
   there is no guarantee that event callback will not be triggered once again after unsubscribe is called.
   Even in situations when subscribtion management is performed from event callback method itself,
   callback may be triggered again in cases when single event is raised by multiple threads at the same time.
   With this event system implementation, there is not possible to raise same event in parallel, so calling to unsubscribe from event callback method
   guarantees that event callback method will not be triggered ever again after return from `Unsubscribe` method.
   But, also there is an additional `waitForRemoval` optional parameter, which can be used to unsubscribe from event when subscription management is performed by external thread.
   Calling to `Unsubscribe` will lock while current event callback is processing, and unsibscribe after event raise is complete.
   So be aware that using same shared locks (mutexes, semaphores, etc) inside callback's methods
   and external thread's methods that calling to `Unsubscribe` with `waitForRemoval=true` at the same time may lead to deadlocks.
