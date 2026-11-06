namespace CSharp_Exam.Casino.Services.Logging;

[Flags]
public enum LogLevel
{
    None = 0,

    Debug = 1 << 0,
    Info = 1 << 1,
    Warning = 1 << 2,
    Error = 1 << 3,
    Critical = 1 << 4,

    All = Debug | Info | Warning | Error | Critical
}

public record struct LogEntry
(
    LogLevel Level,
    DateTime Timestamp,
    string Message,
    string SourceFile = "",
    string SourceMethod = "",
    int SourceLine = 0,
    Exception? Exception = null
);