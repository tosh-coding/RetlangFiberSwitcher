﻿using System;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    ///<summary>
    /// A SnapshotChannel is a channel that allows for the transmission of an initial snapshot followed by incremental updates.
    /// The class is thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class SnapshotChannel<T> : ISnapshotChannel<T>
    {
        private readonly InternalChannel<T> _updatesChannel = new InternalChannel<T>();
        private readonly RequestReplyChannel<object, T> _requestChannel = new RequestReplyChannel<object, T>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="control"></param>
        ///<param name="receive"></param>
        ///<param name="timeoutInMs">For initial snapshot</param>
        ///<param name="registry"></param>
        /// <returns></returns>
        public IDisposable PrimedSubscribe(IExecutionContext fiber, Action<SnapshotRequestControlEvent> control, Action<T> receive, int timeoutInMs, ISubscriptionRegistry registry)
        {
            return new SnapshotRequest<T>(_requestChannel, _updatesChannel, fiber, control, receive, timeoutInMs, registry);
        }

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="control"></param>
        ///<param name="receive"></param>
        ///<param name="timeoutInMs">For initial snapshot</param>
        /// <returns></returns>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<SnapshotRequestControlEvent> control, Action<T> receive, int timeoutInMs)
        {
            return PrimedSubscribe(fiber, control, receive, timeoutInMs, fiber.FallbackDisposer);
        }

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="control"></param>
        ///<param name="receive"></param>
        ///<param name="timeoutInMs">For initial snapshot</param>
        /// <returns></returns>
        public IDisposable PrimedSubscribe(IExecutionContext fiber, Action<SnapshotRequestControlEvent> control, Action<T> receive, int timeoutInMs)
        {
            return PrimedSubscribe(fiber, control, receive, timeoutInMs, null);
        }

        ///<summary>
        /// Publishes the incremental update.
        ///</summary>
        ///<param name="update"></param>
        public bool Publish(T update)
        {
            return _updatesChannel.Publish(update);
        }

        ///<summary>
        /// Ressponds to the request for an initial snapshot.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="reply">returns the snapshot update</param>
        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply)
        {
            return _requestChannel.Subscribe(fiber, request => request.SendReply(reply()));
        }

        ///<summary>
        /// Ressponds to the request for an initial snapshot.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="reply">returns the snapshot update</param>
        ///<param name="fallbackRegistry"></param>
        public IDisposable ReplyToPrimingRequest(IExecutionContext fiber, Func<T> reply, ISubscriptionRegistry fallbackRegistry)
        {
            return _requestChannel.Subscribe(fiber, request => request.SendReply(reply()), fallbackRegistry);
        }

        ///<summary>
        /// Ressponds to the request for an initial snapshot.
        ///</summary>
        ///<param name="executionContext">the target executor to receive the message</param>
        ///<param name="reply">returns the snapshot update</param>
        public void PersistentReplyToPrimingRequest(IExecutionContext executionContext, Func<T> reply)
        {
            _requestChannel.PersistentSubscribe(executionContext, request => request.SendReply(reply()));
        }


        ///<summary>
        /// Number of subscribers
        ///</summary>
        public int NumSubscribers { get { return _requestChannel.NumSubscribers; } }

        ///<summary>
        /// Number of persistent subscribers.
        ///</summary>
        public int NumPersistentSubscribers { get { return _requestChannel.NumPersistentSubscribers; } }
    }
}