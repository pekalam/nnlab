using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            Log.Fatal<string>(e.Exception!, "Unhandled exception");
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal<string>((Exception)e.ExceptionObject, "Unhandled exception");
        }
    }

}
