using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Common.Logging
{
    public static class Bootstraper
    {
        public static void Configure()
        {
            SerilogLoggingConfiguration.Configure();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        public static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            //todo
            Log.Logger.LogCritical(e.Exception!, "Unhandled exception");
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.LogCritical((Exception)e.ExceptionObject, "Unhandled exception");
        }
    }

}
