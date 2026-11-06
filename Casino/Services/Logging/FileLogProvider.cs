namespace CSharp_Exam.Casino.Services.Logging;

public class FileLogProvider(string filePath) : LogProvider
{
    public LogLevel EnabledLogLevels { get; set; } = LogLevel.All;
    private readonly string _filePath = filePath;

    public override void Log(LogEntry entry)
    {
        if ((entry.Level & EnabledLogLevels) == 0) return;


        File.AppendAllText(_filePath, Format(entry) + Environment.NewLine);
    }
}