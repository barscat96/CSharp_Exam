using CSharp_Exam.Casino.Services.Types;

namespace CSharp_Exam.Casino.Services.Exceptions;

public class DataOperationFailed(ActionType action, string message) : Exception
{
    public override string Message 
        => action == ActionType.Save ? $"Saving failed: {message}" 
        : $"Loading failed: {message}";

    public override string ToString()
        => Message;
}