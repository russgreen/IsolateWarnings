using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Async;
using Serilog.Sinks.GoogleAnalytics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace IsolateWarnings;

/// <summary>
/// Simple logger bootstrap without DI. Call Logging.Initialize() on startup and Logging.Shutdown() on shutdown.
/// </summary>
internal static class Logging
{
    private static bool _initialized;

    public static void Initialize(string appName, string? additionalContext = null)
    {
        if (_initialized)
        {
            return;
        }

        var baseLogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        Directory.CreateDirectory(baseLogDirectory);

        var logPath = System.IO.Path.Combine(baseLogDirectory, "Log.json");
        var cultureInfo = Thread.CurrentThread.CurrentCulture;
        var regionInfo = new RegionInfo(cultureInfo.LCID);
        var clientId = ClientIdProvider.GetOrCreateClientId();

        var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
#if DEBUG
        .MinimumLevel.Debug()
#endif
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString())
        .Enrich.WithProperty("RevitVersion", App.CtrApp.VersionNumber)

            // Local file - exclude usage tracking logs
            .WriteTo.Logger(l => l
                .Filter.ByExcluding(le => le.Properties.ContainsKey("UsageTracking"))
                .WriteTo.File(new JsonFormatter(), logPath,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7));

#if !DEBUG
        //write to google analytics
        loggerConfig = loggerConfig
        .WriteTo.GoogleAnalytics(opts =>
            {
                opts.MeasurementId = "##MEASUREMENTID##";
                opts.ApiSecret = "##APISECRET##";
                opts.ClientId = clientId;

                opts.FlushPeriod = TimeSpan.FromSeconds(1);
                opts.BatchSizeLimit = 1;
                opts.MaxEventsPerRequest = 1;
                opts.IncludePredicate = e => e.Properties.ContainsKey("UsageTracking");

                opts.GlobalParams["app_version"] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                opts.GlobalParams["app_country"] = regionInfo.EnglishName;
                opts.GlobalParams["revit_version"] = App.CtrApp.VersionNumber;

                opts.CountryId = regionInfo.TwoLetterISORegionName;
            });
#endif

        Log.Logger = loggerConfig.CreateLogger();

        _initialized = true;

        if (!string.IsNullOrWhiteSpace(additionalContext))
        {
            Log.Information("Logger initialized: {Context}", additionalContext);
        }
        else
        {
            Log.Information("Logger initialized");
        }
    }

    public static void Shutdown()
    {
        if (!_initialized) return;
        try
        {
            Log.Information("Logger shutting down");
            Log.CloseAndFlush();
        }
        finally
        {
            _initialized = false;
        }
    }
}
