﻿using System;
using System.Windows;
using Retlang.Channels;
using Retlang.Fibers;

namespace WpfExample
{
    public class UpdateController
    {
        private readonly IFiber fiber;
        private IDisposable timer;
        private readonly WindowChannels channels;

        public UpdateController(WindowChannels winChannels)
        {
            channels = winChannels;
            var threadFiber = new ThreadFiber();
            var subscriber = new ChannelSubscription<RoutedEventArgs>(threadFiber, OnStart);
            subscriber.Subscribe(channels.StartChannel);
            threadFiber.Start();
            fiber = threadFiber;
        }

        private void OnStart(RoutedEventArgs msg)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            else
            {
                timer = fiber.ScheduleOnInterval(OnTimer, 1000, 1000);
            }
        }

        private void OnTimer()
        {
            channels.TimeUpdate.Publish(DateTime.Now);
        }
    }
}