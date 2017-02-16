﻿namespace ConsoleApp
{
    using System;
    using Contracts;

   internal class Timer: IEventProducer<DateTimeOffset>
   {
       private readonly ILogger<Timer> _logger;
       private readonly TimeSpan _period;

       public Timer(
            ILogger<Timer> logger,
            TimeSpan period)
       {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _period = period;
            _logger.LogInfo("created");
        }

       public IDisposable Subscribe(IObserver<DateTimeOffset> observer)
       {
            return new System.Threading.Timer(
                state => { observer.OnNext(DateTimeOffset.Now);},
                null,
                TimeSpan.Zero,
                _period);
       }

        public override string ToString()
        {
            return _logger.InstanceName;
        }
    }
}