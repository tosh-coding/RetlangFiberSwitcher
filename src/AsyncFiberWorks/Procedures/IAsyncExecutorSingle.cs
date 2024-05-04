using System;
using System.Threading.Tasks;

namespace AsyncFiberWorks.Procedures
{
    /// <summary>
    /// Executes a task.
    /// </summary>
    public interface IAsyncExecutorSingle
    {
        /// <summary>
        /// Executes a task.
        /// </summary>
        /// <param name="func">A function that returns a task.</param>
        /// <returns>A task that waits for the task to be completed.</returns>
        Task Execute(Func<Task> func);
    }
}
