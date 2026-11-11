namespace CSharp_Exam.Casino.Finance.Exceptions;

class NotEnoughFunds(string username, string action, decimal amount) : Exception
{
    public override string ToString()
        => Message;

    public override string Message 
        => $"User '{username}' does not have enough funds to {action} {amount}$";
}