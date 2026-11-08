using CSharp_Exam.Casino.Finance.Exceptions;
using CSharp_Exam.Casino.Models;
using CSharp_Exam.Casino.Services.Logging;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

namespace CSharp_Exam.Casino.Finance.Classes;

partial class Bank
{
    public string Name { get; }

    public static readonly ushort MaxCards = 4;
    public static readonly ushort MaxCryptoWallets = 4;

    private Dictionary<uint, BankAccount> users = new();

    private string LogFilePath => $"Bank_{Name}_Logs.txt";
    private LoggingService logger;

    private bool HandlePaymentRequest(Func<bool> action)
    {
        try
        {
            return action();
        }
        catch (PaymentMethodNotFound e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    private string ConsoleInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public Bank(string name)
    {
        Name = name;
        logger = new LoggingService(new FileLogProvider(LogFilePath));
    }

    // TODO
    public void Register()
    {
        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput("Enter your password: ");

        // Validate password and username
        BankAccount account = new((uint)users.Count + 1);
        users.Add(account.Id, account);

        logger.Log(LogLevel.Info, "Registered New Account");
    }

    public BankAccount? Login()
    {
        string username = ConsoleInput("Enter your username: ");
        // Validate username
        string password = ConsoleInput("Enter your password: ");
        // Validate password

        logger.Log(LogLevel.Info, "Loged Into An Account");

        return new(1);
    }
    // ---------------------

    public BankAccount? GetAccountFromUser(User user)
    {
        foreach (BankAccount account in users.Values)
            if (account?.user?.Id == user.Id) return account;

        return null;
    }

    public bool RequestFundsWithdrawal(
    User user,
    decimal amount,
    string payment_id,
    PaymentMethods payment_method)
    {
        BankAccount account = GetAccountFromUser(user)
            ?? throw new UserNotFound(user.Username);

        return HandlePaymentRequest(() => account.Withdraw(amount, payment_id, payment_method));
    }

    public bool RequestFundsDeposit(
        User user,
        decimal amount,
        string payment_id,
        PaymentMethods payment_method)
    {
        BankAccount account = GetAccountFromUser(user)
            ?? throw new UserNotFound(user.Username);

        return HandlePaymentRequest(() => account.Deposit(amount, payment_id, payment_method));
    }
}