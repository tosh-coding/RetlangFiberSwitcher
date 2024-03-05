using System;

namespace AsyncFiberWorks.Channels
{
    /// <summary>
    /// Default Channel Implementation. Published messages are forwarded to all subscribers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Channel<T> : IChannel<T>
    {
        private readonly MessageHandlerList<T> _channel = new MessageHandlerList<T>();

        /// <summary>
        /// Subscribe a channel.
        /// </summary>
        /// <param name="receiveOnProducerThread">Subscriber.</param>
        /// <returns></returns>
        public IDisposable Subscribe(Action<T> receiveOnProducerThread)
        {
            return this._channel.AddHandler(receiveOnProducerThread);
        }

        /// <summary>
        /// <see cref="IPublisher{T}.Publish(T)"/>
        /// </summary>
        /// <param name="msg"></param>
        public void Publish(T msg)
        {
            _channel.Publish(msg);
        }

        ///<summary>
        /// Number of subscribers
        ///</summary>
        public int NumSubscribers { get { return _channel.Count; } }
    }
}
