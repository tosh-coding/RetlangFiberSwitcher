using System;
using System.Collections.Generic;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public class PoolFiberSlim : IFiberSlim
    {
        private readonly object _lock = new object();
        private readonly IThreadPool _pool;
        private readonly IExecutor _executor;

        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;

        /// <summary>
        /// Create a pool fiber with the specified thread pool and specified executor.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolFiberSlim(IThreadPool pool, IExecutor executor)
        {
            _pool = pool;
            _executor = executor;
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool.
        /// </summary>
        public PoolFiberSlim(IExecutor executor) 
            : this(DefaultThreadPool.Instance, executor)
        {
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool and default executor.
        /// </summary>
        public PoolFiberSlim() 
            : this(DefaultThreadPool.Instance, new DefaultExecutor())
        {
        }

        /// <summary>
        /// Create a pool fiber with the specified thread pool and specified executor, and call the Start method.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static PoolFiberSlim StartNew(IThreadPool pool, IExecutor executor)
        {
            var fiber = new PoolFiberSlim(pool, executor);
            fiber.Start();
            return fiber;
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool and call the Start method.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static PoolFiberSlim StartNew(IExecutor executor)
        {
            var fiber = new PoolFiberSlim(executor);
            fiber.Start();
            return fiber;
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool and default executor, and call the Start method.
        /// </summary>
        /// <returns></returns>
        public static PoolFiberSlim StartNew()
        {
            var fiber = new PoolFiberSlim();
            fiber.Start();
            return fiber;
        }

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

            lock (_lock)
            {
                _queue.Add(action);
                if (_started == ExecutionState.Created)
                {
                    return;
                }
                if (!_flushPending)
                {
                    _pool.Queue(Flush);
                    _flushPending = true;
                }
            }
        }

        private void Flush(object state)
        {
            var toExecute = ClearActions();
            if (toExecute != null)
            {
                _executor.Execute(toExecute);
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        // don't monopolize thread.
                        _pool.Queue(Flush);
                    }
                    else
                    {
                        _flushPending = false;
                    }
                }
            }
        }

        private List<Action> ClearActions()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }
                Lists.Swap(ref _queue, ref _toPass);
                _queue.Clear();
                return _toPass;
            }
        }

        /// <summary>
        /// Start consuming actions.
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            //flush any pending events in queue
            Enqueue(() => { });
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            _started = ExecutionState.Stopped;
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}