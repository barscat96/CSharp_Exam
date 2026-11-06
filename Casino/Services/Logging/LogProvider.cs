namespace CSharp_Exam.Casino.Services.Logging;

public abstract class LogProvider
{
    public abstract void Log(LogEntry entry);

    protected virtual string Format(LogEntry entry)
    {
        string message = $"[{entry.Timestamp.ToShortTimeString()}] " +
            $"[{entry.Level}] " +
            $"(Source: {entry.SourceFile}:{entry.SourceLine} in {entry.SourceMethod}) " +
            entry.Message;

        if (entry.Exception != null)
            message += Environment.NewLine + entry.Exception.Message;

        return message;
    }
}