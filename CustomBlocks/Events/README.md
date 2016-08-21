# Custom event system.

This is a simple custom event system, made on top of `EventHandler<EventArgs>` delegates.
It is not a functionally equivalent replacement for default events, but may be used as replacement to simplify event logic implementation.
This system aims to simplify implementation of some common event usage patterns, simplify use of events in multithreaded applications and restrict usage of some common unsafe and error-prone practices.