using System;

namespace AsyncFiberWorks.Channels
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IRequestPublisher<R, M>
    {
        /// <summary>
        /// Send request on the channel.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callbackOnReceive"></param>
        /// <returns></returns>
        IDisposable SendRequest(R request, Action<M> callbackOnReceive);
    }
}
