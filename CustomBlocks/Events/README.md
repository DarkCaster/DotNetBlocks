# Custom event system.

This is a simple custom event system, made on top of `EventHandler<EventArgs>` delegates.
It is not a functionally equivalent replacement for default events, but may be used as replacement to simplify event logic implementation.
This system aims to simplify implementation of some common event usage patterns, simplify use of events in multithreaded applications and restrict some common unsafe and error-prone practices.

## Key features and differences from default events
1. At publisher side
 * Event raising process is thread safe and will block when it is run from different threads simultaneously.
 * TODO: you can manually request lock object and lock on it before raising an event, if you want to do some stuff atomically with event.
 * `Raise` method will block by default, but you can wrap it inside Task if you want async event processing without waiting for completion, just make sure that EventArgs parameter is not changed while event is working.
 * TODO (or maybe not): you can use RaiseAsync and RaiseAsyncWait to simplify async event raise use patterns
 * Exceptions thrown by subscribers will not cause failure at publisher side and will not interrupt callback execution for remaining subscribers.
   `Raise` method will return `true` if no exceptions was thrown by subscribers while processing the event.
   For debug purposes, all exceptions can be collected to ICollection<EventRaiseException> container (will be appended in order of appearance) to be processed later, but it is optional.
