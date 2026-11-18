using System.Linq;

using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Services.HashingService;
using static CSharp_Exam.Casino.Services.DataService;

namespace CSharp_Exam.Casino.Finance.Classes;

static class AuthenticationValidator
{
    private const ushort MinNameLength = 4;
    private const ushort MaxNameLength = 15;

    private const ushort MinPasswordLength = 8;
    private const ushort MaxPasswordLength = 20;

    public static bool ValidateUsername(string name)
        => !string.IsNullOrWhiteSpace(name)
        && name.Length >= MinNameLength
        && name.Length <= MaxNameLength;

    public static bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (password.Length < MinPasswordLength ||
            password.Length > MaxPasswordLength)
            return false;

        bool hasNumber = password.Any(char.IsDigit);
        bool hasLetter = password.Any(char.IsLetter);
        bool hasSpecial = password.Any(c =>
            char.IsPunctuation(c) || char.IsSymbol(c));

        return hasNumber && hasLetter && hasSpecial;
    }

    public static void ValidateCredentials(string username, string password)
    {
        if (!ValidateUsername(username))
            throw new InvalidCredentialsException(CredentialsType.Username, username);

        if (!ValidatePassword(password))
            throw new InvalidCredentialsException(CredentialsType.Password, password);
    }
}
