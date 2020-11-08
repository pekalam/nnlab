using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;

namespace Common.Logging
{
    internal static class SerilogLoggingConfiguration
    {
        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}";

        [Conditional("RELEASE")]
        private static void ConfigureRelease(LoggerConfiguration c)
        {
            c.Enrich.WithThreadId().Enrich.WithMemoryUsage()
                .MinimumLevel.Warning()
                .WriteTo.File("log.txt", fileSizeLimitBytes: 1024 * 1024 * 512, rollOnFileSizeLimit: true, outputTemplate: OutputTemplate)
                .WriteTo.Console(outputTemplate: OutputTemplate);
        }

        private static void ConfigureDebug(LoggerConfiguration c)
        {
#if RELEASE
#else
            c.Enrich.WithThreadId().Enrich.WithMemoryUsage()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: OutputTemplate);
#endif
        }

        public static void Configure()
        {
            var config = new LoggerConfiguration();
            ConfigureRelease(config);
            ConfigureDebug(config);

            Log.Logger = new SerilogLogging(config.CreateLogger());
        }
    }

    internal class SerilogLogging : ILogger
    {
        private readonly Logger _logger;

        public SerilogLogging(Logger logger)
        {
            _logger = logger;
        }

        public void Information(string msg)
        {
            _logger.Information(msg);
        }

        public void Information<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            _logger.Information(exception, template, param1);
        }

        public void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public void Debug<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            _logger.Debug(exception, template, param1);
        }

        public void Warning(string msg)
        {
            _logger.Warning(msg);
        }

        public void Warning<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            _logger.Warning(exception, template, param1);

        }

        public void Error(string msg)
        {
            _logger.Error(msg);
        }

        public void Error<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            _logger.Error(exception, template, param1);

        }

        public void Fatal(string msg)
        {
            _logger.Fatal(msg);
        }

        public void Fatal<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            _logger.Fatal(exception, template, param1);

        }
    }

    public interface ILogger
    {
        void Information(string msg);
        void Information<T>(Exception exception, string? template = null, T? param1 = null) where T : class;
        void Debug(string msg);
        void Debug<T>(Exception exception, string? template = null, T? param1 = null) where T : class;
        void Warning(string msg);
        void Warning<T>(Exception exception, string? template = null, T? param1 = null) where T : class;
        void Error(string msg);
        void Error<T>(Exception exception, string? template = null, T? param1 = null) where T : class;
        void Fatal(string msg);
        void Fatal<T>(Exception exception, string? template = null, T? param1 = null) where T : class;
    }

    internal class DefaultLogger : ILogger {
        public void Information(string msg)
        {
            
        }

        public void Information<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            
        }

        public void Debug(string msg)
        {
            
        }

        public void Debug<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            
        }

        public void Warning(string msg)
        {
            
        }

        public void Warning<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            
        }

        public void Error(string msg)
        {
            
        }

        public void Error<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            
        }

        public void Fatal(string msg)
        {
            
        }

        public void Fatal<T>(Exception exception, string? template = null, T? param1 = default(T?)) where T : class
        {
            
        }
    }

    public static class Log
    {
        public static ILogger Logger { get; internal set; } = new DefaultLogger();

        [Conditional("DEBUG")]
        public static void Information<T>(Exception exception, string? template = null, T? param1 = null) where T : class => Logger.Information(exception, template, param1);
        [Conditional("DEBUG")]
        public static void Debug<T>(Exception exception, string? template = null, T? param1 = null) where T : class => Logger.Debug(exception,template,param1);

        public static void Warning<T>(Exception exception, string? template = null, T? param1 = null) where T : class => Logger.Warning(exception,template,param1);
        public static void Error<T>(Exception exception, string? template = null, T? param1 = null) where T : class => Logger.Error(exception,template,param1);
        public static void Fatal<T>(Exception exception, string? template = null, T? param1 = null) where T : class => Logger.Fatal(exception,template,param1);

        [Conditional("DEBUG")]
        public static void Information(string msg) => Logger.Information(msg);
        [Conditional("DEBUG")]
        public static void Debug(string msg) => Logger.Debug(msg);

        public static void Warning(string msg) => Logger.Warning(msg);
        public static void Fatal(string msg) => Logger.Fatal(msg);


    }
}
