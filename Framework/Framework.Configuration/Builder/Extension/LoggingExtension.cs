namespace ZTR.Framework.Configuration.Builder.Extension
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Serilog;

    #pragma warning disable CS1591
    public static class LoggingExtension
    {
        /// <summary>
        /// Adds the logging.
        /// </summary>
        /// <param name="logging">The logging.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddLogging(this ILoggingBuilder logging, IConfiguration configuration)
        {
            logging.AddConfiguration(configuration);
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();
            logging.AddFileLogging(configuration);

            return logging;
        }

        /// <summary>
        /// Adds the file logging.
        /// </summary>
        /// <param name="loggingBuilder">The logging builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="defaultLogFilePath">The default log file path.</param>
        /// <param name="defaultLogFileSizeInBytes">The default log file size in bytes.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddFileLogging(this ILoggingBuilder loggingBuilder, IConfiguration configuration, string defaultLogFilePath = "Logs/Log-.txt", int defaultLogFileSizeInBytes = 134217728)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .ReadFrom.Configuration(configuration)
                .WriteTo.File(
                    defaultLogFilePath,
                    fileSizeLimitBytes: defaultLogFileSizeInBytes,
                    rollingInterval: RollingInterval.Hour,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 15,
                    shared: true,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext:l}.{Method}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            loggingBuilder.AddSerilog(logger);
            return loggingBuilder;
        }
    }
    #pragma warning restore CS1591
}
