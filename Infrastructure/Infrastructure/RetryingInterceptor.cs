using System;
using System.Collections.Generic;
using System.Threading;
using Castle.DynamicProxy;
using NLog;

namespace Infrastructure
{
    class RetryingInterceptor : IInterceptor
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public int PauseBetweenCalls { get; set; }
        public int RetryCount { get; set; }

        public RetryingInterceptor()
        {
            RetryCount = 5;
            PauseBetweenCalls = 2000;
        }

        public void Intercept(IInvocation invocation)
        {
            var exx = new List<Exception>();
            for (int i = 0; i < RetryCount; i++)
            {
                if (i > 0) Thread.Sleep(PauseBetweenCalls);
                try
                {
                    invocation.Proceed();
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Operation failed {0} times {1}", i, ex.Message);
                    exx.Add(ex);
                }
            }
            Logger.Error("Operation failed still failing after {0} retries", RetryCount);
            throw new AggregateException(exx);
        }
    }
}
