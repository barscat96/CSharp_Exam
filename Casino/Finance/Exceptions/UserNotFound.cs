namespace CSharp_Exam.Casino.Finance.Exceptions;

public class UserNotFound(string name) : Exception
{
    public override string ToString()
        => Message;

    public override string Message
        => $"No such User '{name}' found";
}
