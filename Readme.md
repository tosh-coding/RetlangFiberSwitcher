This is forked project from https://code.google.com/archive/p/retlang/

---

Retlang is a high performance C# threading library (see [Jetlang](http://code.google.com/p/jetlang/) for a version in Java).  The library is intended for use in [message based concurrency](http://en.wikipedia.org/wiki/Message_passing) similar to [event based actors in Scala](http://lamp.epfl.ch/~phaller/doc/haller07actorsunify.pdf).  The library does not provide remote messaging capabilities. It is designed specifically for high performance in-memory messaging.

# Features #
All messages to a particular [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) are delivered sequentially. Components can easily keep state without synchronizing data access or worrying about thread races.
  * Single [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) interface that can be backed by a [dedicated thread](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/ThreadFiber.cs), a [thread pool](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/PoolFiber.cs), or a [WinForms](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/FormFiber.cs)/[WPF](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/DispatcherFiber.cs) message pump.
  * Supports single or multiple subscribers for messages.
  * Subscriptions for single events or event batching.
  * [Single](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IScheduler.cs#16) or [recurring](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IScheduler.cs#25) event scheduling.
  * High performance design optimized for low latency and high scalability.
  * Publishing is thread safe, allowing easy integration with other threading models.
  * Low Lock Contention - Minimizing lock contention is critical for performance. Other concurrency solutions are limited by a single lock typically on a central thread pool or message queue. Retlang is optimized for low lock contention. Without a central bottleneck, performance easily scales to the needs of the application.
  * [Synchronous](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/RequestReplyChannel.cs)/[Asynchronous](http://code.google.com/p/retlang/source/browse/trunk/src/RetlangTests/Channels/ChannelTests.cs#171) request-reply support.
  * Single assembly with no dependencies except the CLR (4.0+).

Retlang relies upon four abstractions: [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs),
[IQueue](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IQueue.cs),  [IExecutor](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IExecutor.cs), and [IChannel](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/IChannel.cs).  An [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) is an abstraction for the [context of execution](http://en.wikipedia.org/wiki/Context_switch) (in most cases a thread).  An [IQueue](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IQueue.cs) is an abstraction for the data structure that holds the actions until the IFiber is ready for more actions.  The default implementation, [DefaultQueue](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/DefaultQueue.cs), is an unbounded storage that uses standard locking to notify when it has actions to execute.  An [IExecutor](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/IExecutor.cs) performs the actual execution.  It is useful as an injection point to achieve fault tolerance, performance profiling, etc.  The default implementation, [DefaultExecutor](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Core/DefaultExecutor.cs), simply executes actions.  An [IChannel](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/IChannel.cs) is an abstraction for the conduit through which two or more [IFibers](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) communicate (pass messages).

## Changes after forking ##

* Remove stubs of subscription and scheduling. For simplicity.
* Separate IFiberSlim from IFiber. The concrete classes of IFiberSlim are simple fibers.

## Schedule ##

* Make StubFiber thread-safe.
* Add fiber.SwitchTo method.

# Quick Start #

## Fibers ##
Four implementations of [IFibers](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) are included in Retlang.
  * _[ThreadFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/ThreadFiber.cs)_ - an [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) backed by a dedicated thread.  Use for frequent or performance-sensitive operations.
  * _[PoolFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/PoolFiber.cs)_ - an [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) backed by the .NET thread pool.  Note execution is still sequential and only executes on one pool thread at a time.  Use for infrequent, less performance-sensitive executions, or when one desires to not raise the thread count.
  * _[FormFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/FormFiber.cs)/[DispatchFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/DispatcherFiber.cs)_ - an [IFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/IFiber.cs) backed by a [WinForms](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/FormFiber.cs)/[WPF](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/DispatcherFiber.cs) message pump.  The [FormFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/FormFiber.cs)/[DispatchFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/DispatcherFiber.cs) entirely removes the need to call Invoke or BeginInvoke to communicate with a window from a different thread.
  * _[StubFiber](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Fibers/StubFiber.cs)_ - useful for deterministic testing.  Fine grain control is given over execution to make [testing races simple](http://grahamnash.blogspot.com/2010/01/stubfiber-how-to-deterministically-test_16.html).  Executes all actions on the caller thread.

## Channels ##
The main [IChannel](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/IChannel.cs) included in Retlang is simply called [Channel](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/Channel.cs).  Below are the main types of subscriptions.
  * _[Subscribe](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/ISubscriber.cs#19)_ - callback is executed for each message received.
  * _[SubscribeToBatch](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/ISubscriber.cs#34)_ - callback is executed on the interval provided with all messages received since the last interval.
  * _[SubscribeToKeyedBatch](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/ISubscriber.cs#45)_ - callback is executed on the interval provided with all messages received since the last interval where only the most recent message with a given key is delivered.
  * _[SubscribeToLast](http://code.google.com/p/retlang/source/browse/trunk/src/Retlang/Channels/ISubscriber.cs#55)_ - callback is executed on the interval provided with the most recent message received since the last interval.

Further documentation can be found baked-in, in the [unit tests](http://code.google.com/p/retlang/source/browse/#svn/trunk/src/RetlangTests), in the [user group](http://groups.google.com/group/retlang-dev), or visually [here](http://dl.dropbox.com/u/2053101/Retlang%20and%20Jetlang.mov) (courtesy of [Mike Roberts](http://mikebroberts.com/)).