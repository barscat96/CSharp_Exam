namespace CSharp_Exam.Casino.Finance.Exceptions;

enum CredentialsType
{
    Username,
    Password
}

class InvalidCredentialsException(CredentialsType type, string input, bool alreadyUsed = false) : Exception
{
    public override string ToString()
        => Message;

    public override string Message
        => type switch {
            CredentialsType.Username 
                => alreadyUsed ? $"Username {input} is already used" : $"Invalid Username '{input}'",
            CredentialsType.Password => $"Invalid Password '{input}'",
            _ => $"Invalid credential '{input}'"
        };
}