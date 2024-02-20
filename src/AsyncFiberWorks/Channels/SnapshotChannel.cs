﻿using System;
using AsyncFiberWorks.Fibers;

namespace AsyncFiberWorks.Channels
{
    ///<summary>
    /// A SnapshotChannel is a channel that allows for the transmission of an initial snapshot followed by incremental updates.
    /// The class is thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class SnapshotChannel<T> : ISnapshotChannel<T>
    {
        private readonly MessageHandlerList<T> _updatesChannel = new MessageHandlerList<T>();
        private readonly RequestReplyChannel<object, T> _requestChannel = new RequestReplyChannel<object, T>();

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="requester"></param>
        public void OnPrimedSubscribe(SnapshotRequest<T> requester)
        {
            requester.OnSubscribe(_requestChannel, _updatesChannel);
        }

        ///<summary>
        /// Publishes the incremental update.
        ///</summary>
        ///<param name="update"></param>
        public bool Publish(T update)
        {
            return _updatesChannel.Publish(update);
        }

        /// <summary>
        /// Responds to the request for an initial snapshot.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Only one responder can be handled within a single channel.</exception>
        public IDisposable ReplyToPrimingRequest(ISubscribableFiber fiber, Action<IRequest<object, T>> onRequest)
        {
            if (_requestChannel.NumSubscribers > 0)
            {
                throw new InvalidOperationException("Only one responder can be handled within a single channel.");
            }
            return _requestChannel.AddResponder(fiber, onRequest);
        }

        ///<summary>
        /// Number of subscribers
        ///</summary>
        public int NumSubscribers { get { return _requestChannel.NumSubscribers; } }
    }
}
