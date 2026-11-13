//-- [ SYSTEM ]
using System.Threading;

//-- [ SERVICES ]
using CSharp_Exam.Casino.Services.Logging;
using static CSharp_Exam.Casino.Services.HashingService;
using static CSharp_Exam.Casino.Services.ConsoleService;

//-- [ CONFIGS ]
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

//-- [ EXCEPTIONS ]
using CSharp_Exam.Casino.Finance.Exceptions;

//-- [MODELS]
using CSharp_Exam.Casino.Models;
using CSharp_Exam.Casino.Services;

namespace CSharp_Exam.Casino.Finance.Classes;

partial class Bank
{
    public string Name { get; set; }

    public static readonly ushort MaxCards = 4;
    public static readonly ushort MaxCryptoWallets = 4;

    public readonly Dictionary<uint, BankAccount> Accounts = [];
    private static Dictionary<PaymentMethods, List<string>> usedPaymentMethods = new()
    {
        [PaymentMethods.Card] = [],
        [PaymentMethods.Crypto] = []
    };

    private string LogFilePath => $"Bank_{Name}_Logs.txt";
    private readonly LoggingService logger;

    private readonly ushort WaitTime = 2000;

    private readonly string[] Choices = { "Register", "LogIn", "Quit" };

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

    public void Register()
    {
        Console.Clear();

        Console.WriteLine("================");
        Console.WriteLine("| REGISTRATION |");
        Console.WriteLine("================");

        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput("Enter your password: ");

        try
        {
            AuthenticationValidator.ValidateCredentials(username, password);
            if (UsernameExists(username))
                throw new InvalidCredentialsException(CredentialsType.Username, username, true);

            uint id = Accounts.Count == 0 ? 1u : Accounts.Keys.Max() + 1;

            User user = new(id, username)
            {
                PasswordHash = HashPassword(password),
                RegisteredAt = DateTime.Now
            };

            BankAccount account = new(id, user);

            Accounts.Add(account.Id, account);

            WriteColoredLine("✅ Registration succesfully completed!", ConsoleColor.Green);
            logger.Log(LogLevel.Info, $"Registered New Account | Id: {id} / Username: {username}");
        }
        catch (Exception e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);

            logger.Log(LogLevel.Warning, "Registration Failed", e);
        }

        Thread.Sleep(WaitTime);
    }

    public BankAccount? Login()
    {
        Console.WriteLine("===============");
        Console.WriteLine("|    LOGIN    |");
        Console.WriteLine("===============");

        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput("Enter your password: ");

        try
        {
            AuthenticationValidator.ValidateCredentials(username, password);

            BankAccount? account = Accounts.Values.FirstOrDefault(a =>
                string.Equals(
                    a.user?.Username,
                    username,
                    StringComparison.OrdinalIgnoreCase
                ))
            ?? throw new InvalidCredentialsException(CredentialsType.Username, username);

            if (!VerifyPassword(password, account.user!.PasswordHash))
                throw new InvalidCredentialsException(CredentialsType.Password, password);

            logger.Log(LogLevel.Info,
                $"Logged Into An Account | Id: {account.Id} / Username: {username}");

            return account;
        }
        catch (Exception e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);

            logger.Log(LogLevel.Warning, "Login Failed", e);

            Thread.Sleep(WaitTime);
            return null;
        }
    }

    public void Menu()
    {
        int choice = 0;

        while (choice != 3)
        {
            Console.Clear();

            string title = $"/|| -- [ {Name} Bank ] -- ||\\";

            for (short i = 0; i < Name.Length + title.Length; ++i) Console.Write("=");
            Console.WriteLine(title);
            for (short i = 0; i < Name.Length + title.Length; ++i) Console.Write("=");

            choice = ChooseOption(Choices);

            switch (choice)
            {
                case 1:
                    Register();
                    break;
                case 2:
                    Login(); // TODO
                    break;
                case 3:
                    break;
                default:
                    WriteColoredLine("Invalid choice. Try Again", ConsoleColor.Yellow);
                    break;
            }
        }
    }

    public BankAccount? GetAccountFromUser(User user)
    {
        foreach (BankAccount account in Accounts.Values)
            if (account?.user == user) return account;

        return null;
    }
    
    public BankAccount? GetAccountByUsername(string username)
    {
        if (!AuthenticationValidator.ValidateUsername(username))
            return null;

        foreach (BankAccount account in Accounts.Values)
        {
            if (string.Compare(account.user?.Username, username, StringComparison.Ordinal) == 0)
                return account;
        }

        return null;
    }

    public bool TryGetAccount(uint id, out BankAccount? account)
        => Accounts.TryGetValue(id, out account);

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