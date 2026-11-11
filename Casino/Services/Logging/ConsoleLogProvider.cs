namespace CSharp_Exam.Casino.Services.Logging;

public class ConsoleLogProvider : LogProvider
{
    public LogLevel EnabledLogLevels { get; set; } = LogLevel.All;

    public readonly Dictionary<LogLevel, ConsoleColor> LogLevelColors = new()
    {
        [LogLevel.Debug] = ConsoleColor.Gray,
        [LogLevel.Info] = ConsoleColor.White,
        [LogLevel.Warning] = ConsoleColor.Yellow,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Critical] = ConsoleColor.DarkRed
    };

    public override void Log(LogEntry entry)
    {
        if ((entry.Level & EnabledLogLevels) == 0) return;

        ConsoleColor prevColor = Console.ForegroundColor;

        if (LogLevelColors.TryGetValue(entry.Level, out ConsoleColor color))
            Console.ForegroundColor = color;

        Console.WriteLine(Format(entry));

        Console.ForegroundColor = prevColor;
    }
}
