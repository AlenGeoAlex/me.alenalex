using System.Collections;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Bloggi.Backend.Api.Web.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// Initializes the logger for the application, configuring outputs, log level, and formats.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance used to configure the logger services and settings.</param>
    /// <returns>Returns an ILoggerFactory instance initialized with the configured logger.</returns>
    public static ILoggerFactory InitializeLogger(this WebApplicationBuilder builder)
    {
        var logLevel = string.IsNullOrWhiteSpace(builder.Configuration["LOG_LEVEL"]) ? 
            LogEventLevel.Information : 
            Enum.TryParse<LogEventLevel>(builder.Configuration["LOG_LEVEL"], out var level) ?
                level : LogEventLevel.Information;

        var loggerConfiguration = new LoggerConfiguration();
        
        
        loggerConfiguration.WriteTo
            .Console(restrictedToMinimumLevel: logLevel, theme: AnsiConsoleTheme.Code);

        var logPath = builder.Configuration["LOG_PATH"] ?? "/logs/bloggi/";

        if (!builder.Environment.IsDevelopment())
        {
            loggerConfiguration.WriteTo.File(
                path: Path.Join(logPath, "bloggi-.log"),
                restrictedToMinimumLevel: logLevel,
                rollingInterval: RollingInterval.Day,
                encoding: Encoding.UTF8,
                formatter: new JsonFormatter()
            );
        }
        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;
        builder.Services.AddSerilog(logger);
        
        logger.Information("Logger initialized");
        logger.Information("Log level: {logLevel}", logLevel);
        logger.Information("Log path: {logPath}", logPath);

        if (logLevel != LogEventLevel.Debug) return new SerilogLoggerFactory(logger);
        
        var envVariables = Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .Select(entry => $"{entry.Key}={entry.Value}")
            .ToArray();

        logger.Debug("ENV: [{environment}]", string.Join("], [", envVariables));

        return new SerilogLoggerFactory(logger);
    }
    
}