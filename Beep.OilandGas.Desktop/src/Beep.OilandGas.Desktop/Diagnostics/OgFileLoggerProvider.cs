using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// The desktop's rolling log: one file per day under the per-user data root (ADR-013).
/// </summary>
/// <remarks>
/// <para>
/// The failure store answers "what failed and under which reference"; this answers "what happened" for a
/// machine whose operator cannot open the database. The pipeline writes the failure under its reference, so
/// the two can be joined.
/// </para>
/// <para>
/// A logging provider must never throw into its caller — a logger that failed into the catch it was called
/// from would turn a recorded failure into an unrecorded one. A write that fails is therefore reported to
/// <see cref="LastResortLog"/> and the call returns, which is this product's documented last resort.
/// </para>
/// </remarks>
public sealed class OgFileLoggerProvider : ILoggerProvider
{
    private readonly string _directory;
    private readonly object _gate = new();

    public OgFileLoggerProvider(string? directory = null)
    {
        _directory = directory ?? OgPaths.LogDirectory;
    }

    public ILogger CreateLogger(string categoryName) =>
        new OgFileLogger(_directory, categoryName, _gate);

    public void Dispose()
    {
        // Nothing buffered: each line is written whole, so a process that ends keeps every line it logged.
    }

    private sealed class OgFileLogger(
        string directory,
        string category,
        object gate) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null)
            {
                return;
            }

            var line = $"{DateTimeOffset.UtcNow:O} [{(logLevel == LogLevel.Critical ? "CRIT" : Short(logLevel))}] " +
                       $"{category}: {formatter(state, exception)}";

            try
            {
                lock (gate)
                {
                    Directory.CreateDirectory(directory);
                    File.AppendAllText(
                        Path.Combine(directory, $"app-{DateTime.UtcNow:yyyyMMdd}.log"),
                        line + Environment.NewLine);
                }
            }
#pragma warning disable BEEP0003 // a logging provider must not throw into the caller; LastResortLog is this product's documented last resort
            catch (Exception writeFailure)
#pragma warning restore BEEP0003
            {
                LastResortLog.Write(
                    () => Console.Error.WriteLine($"[log] {writeFailure.GetType().FullName}: {line}"),
                    "a log line");
            }
        }

        private static string Short(LogLevel level) => level switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERR ",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "NONE",
        };
    }
}
