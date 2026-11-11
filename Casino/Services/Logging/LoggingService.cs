using System.Runtime.CompilerServices;

namespace CSharp_Exam.Casino.Services.Logging;

public class LoggingService
{
    private readonly LogProvider[] _logProviders;

    public static LoggingService Default { get; } = new LoggingService(new ConsoleLogProvider());

    public T? GetProvider<T>() where T : class
     => _logProviders.OfType<T>().FirstOrDefault();

    public LoggingService(params LogProvider[] logProviders)
    {
        if (logProviders.DistinctBy(p => p.GetType()).Count() != logProviders.Length)
            throw new ArgumentException("Duplicate log providers are not allowed");

        _logProviders = logProviders;
    }

    public void Log(
        LogLevel level,
        string message,
        Exception? exception = null,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string sourceMethod = "",
        [CallerLineNumber] int sourceLine = 0)
    {
        if (level == LogLevel.None
            || (level & (level - 1)) != 0
            || (level & ~LogLevel.All) != 0)
            throw new ArgumentException(nameof(level), "Invalid log level");

        LogEntry entry = new LogEntry(
            level,
            DateTime.Now,
            message,
            sourceFile,
            sourceMethod,
            sourceLine,
            exception);

        foreach (var provider in _logProviders)
            provider.Log(entry);
    }
}