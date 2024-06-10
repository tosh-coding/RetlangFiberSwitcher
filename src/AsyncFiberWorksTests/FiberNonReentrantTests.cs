﻿using AsyncFiberWorks.Core;
using AsyncFiberWorks.Fibers;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncFiberWorksTests
{
    [TestFixture]
    public class FiberNonReentrantTests
    {
        [Test]
        public async Task AsyncNonReentrantTest()
        {
            var originalFiber = new AsyncFiber();
            var fiber = new AsyncNonReentrantFiberFilter(originalFiber);
            int counter = 0;

            fiber.Enqueue(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                counter += 1000;
            });

            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(200).ConfigureAwait(false);
                fiber.Enqueue(() =>
                {
                    counter += 200;
                    return Task.CompletedTask;
                });
            }

            var tcs = new TaskCompletionSource<byte>();
            originalFiber.Enqueue(() =>
            {
                tcs.SetResult(0);
                return Task.CompletedTask;
            });

            await tcs.Task.ConfigureAwait(false);
            Assert.AreEqual(1200, counter);
        }

        [Test]
        public void NonReentrantTest()
        {
            var originalFiber = new PoolFiber();
            var fiber = new NonReentrantFiberFilter(originalFiber);
            int counter = 0;

            fiber.Enqueue(() =>
            {
                Thread.Sleep(500);
                counter += 1000;
            });

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(200);
                fiber.Enqueue(() =>
                {
                    counter += 200;
                });
            }

            var resetEvent = new ManualResetEvent(false);
            originalFiber.Enqueue(() =>
            {
                resetEvent.Set();
            });

            resetEvent.WaitOne();
            Assert.AreEqual(1200, counter);
        }
    }
}
