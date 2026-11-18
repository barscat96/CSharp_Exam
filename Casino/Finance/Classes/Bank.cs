using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Finance.Exceptions;
using CSharp_Exam.Casino.Models;
using CSharp_Exam.Casino.Services.Logging;
using System.Threading;

using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;
using static CSharp_Exam.Casino.Services.ConsoleService;
using static CSharp_Exam.Casino.Services.HashingService;

namespace CSharp_Exam.Casino.Finance.Classes;

partial class Bank
{
    public string Name { get; private set; }

    public static readonly ushort MaxCards = 4;
    public static readonly ushort MaxCryptoWallets = 4;

    public readonly Dictionary<uint, BankAccount> Accounts = [];

    private static readonly Dictionary<PaymentMethods, HashSet<PaymentMethod>> usedPaymentMethods = new()
    {
        [PaymentMethods.Card] = [],
        [PaymentMethods.Crypto] = []
    };

    private static readonly Random Random = Random.Shared;

    private string LogFilePath => $"Bank_{Name}_Logs.txt";
    private readonly LoggingService logger;

    private readonly ushort WaitTime = 2500;

    private enum AuthChoice { Register, LogIn, Quit }
    private enum SelectionResult { Proceed, Restart, Exit }

    private readonly string[] AuthChoices = ["Register", "LogIn", "Quit"];
    private readonly string[] CardPaymentMethods;
    private readonly string[] CryptoPaymentMethods;

    public Bank(string name)
    {
        Name = name;
        logger = new LoggingService(new FileLogProvider(LogFilePath));

        CardPaymentMethods = [.. CardIndexes.Values.Select(v => v.Name)];
        CryptoPaymentMethods = [.. CryptoIndexes.Values.Select(v => v.Name)];
    }

    private static string ConsoleInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    private static void ShowHeader(string title)
    {
        string line = $"| {title} |";
        string border = new('=', line.Length);

        Console.Clear();
        Console.WriteLine(border);
        WriteColoredLine(line, ConsoleColor.Blue);
        Console.WriteLine(border);
    }

    private uint GetNextAccountId()
        => Accounts.Count == 0 ? 1u : Accounts.Keys.Max() + 1;

    private BankAccount? FindAccountByUsername(string username)
        => Accounts.Values.FirstOrDefault(a =>
            string.Equals(a.user?.Username, username, StringComparison.OrdinalIgnoreCase));

    private static bool HandlePaymentRequest(Func<bool> action)
    {
        try
        {
            return action();
        }
        catch (PaymentMethodNotFound e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);
            return false;
        }
    }

    private static PaymentMethod GetRandomPaymentMethod(PaymentMethods method, string prefix)
    {
        var indexes = method switch
        {
            PaymentMethods.Card => CardIndexes,
            PaymentMethods.Crypto => CryptoIndexes,
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        while (true)
        {
            string id = GeneratePaymentId(prefix, indexes[prefix]);
            string name = PaymentValidator.GetCardOrCrypto(id, method);

            if (!usedPaymentMethods[method].Any(x => x.Id == id))
                return new(name, id, 0m, method);
        }
    }

    private static string GeneratePaymentId(string prefix, PaymentConfig config)
    {
        const string digits = "0123456789";

        int length = Random.Next(config.MinLength, config.MaxLength + 1);

        if (length < prefix.Length)
            throw new InvalidOperationException("Invalid payment configuration");

        var randomPart = Enumerable
            .Range(0, length - prefix.Length)
            .Select(_ => digits[Random.Next(digits.Length)]);

        return prefix + new string([.. randomPart]);
    }

    private static void DisplaySelected(params PaymentMethod?[] methods)
    {
        bool nothingSelected = true;

        foreach (PaymentMethod? method in methods)
        {
            if (method is null)
                continue;

            string methodName = method.Value.Method switch
            {
                PaymentMethods.Card => "Card",
                PaymentMethods.Crypto => "Crypto",
                _ => "Unknown"
            };

            Console.WriteLine($"{methodName} - Name: {method.Value.Name} | Id: {method.Value.Id}");
            nothingSelected = false;
        }

        if (nothingSelected)
            WriteColoredLine("No selected payment methods", ConsoleColor.Yellow);

        Console.WriteLine();
    }

    private SelectionResult ChooseCard(bool cardRequired, out PaymentMethod? card)
    {
        card = null;

        string[] options = cardRequired
            ? [.. CardPaymentMethods, "Back"]
            : [.. CardPaymentMethods, "Next", "Back"];

        int choice = ChooseOption(options);

        if (choice == options.Length - 1)
            return SelectionResult.Exit;

        if (choice < CardPaymentMethods.Length)
        {
            var key = CardIndexes.ElementAt(choice).Key;
            card = GetRandomPaymentMethod(PaymentMethods.Card, key);
        }

        return SelectionResult.Proceed;
    }

    private SelectionResult ChooseCrypto(out PaymentMethod? crypto)
    {
        crypto = null;

        string[] options = [.. CryptoPaymentMethods, "Skip", "Back"];
        int choice = ChooseOption(options);

        if (choice == options.Length - 1)
            return SelectionResult.Restart;

        if (choice < CryptoPaymentMethods.Length)
        {
            var key = CryptoIndexes.ElementAt(choice).Key;
            crypto = GetRandomPaymentMethod(PaymentMethods.Crypto, key);
        }

        return SelectionResult.Proceed;
    }

    private bool ConfirmSelection(PaymentMethod? card, PaymentMethod? crypto)
    {
        const int Confirm = 0, Retry = 1;

        Console.Clear();
        Console.WriteLine("SELECTED PAYMENT METHODS:");
        DisplaySelected([card, crypto]);

        int choice = ChooseOption(["Confirm", "Retry", "Exit"]);

        switch (choice)
        {
            case Confirm:
                Console.Clear();
                WriteColoredLine("✅ Confirmed!", ConsoleColor.Green);
                Console.WriteLine("// NEW PAYMENT METHODS \\\\");
                DisplaySelected([card, crypto]);
                return false;

            case Retry:
                WriteColoredLine("🔄 Preparing retry...", ConsoleColor.Magenta);
                Thread.Sleep(WaitTime / 2);
                return true;

            default:
                WriteColored("ヾ(•ω•`) See you again!\n", ConsoleColor.Cyan);
                return false;
        }
    }

    private void AddPaymentMethod(BankAccount account, bool cardRequired = false)
    {
        string username = account.user?.Username ?? "unknown";

        if (!Accounts.ContainsValue(account))
        {
            logger.Log(LogLevel.Warning, $"Failed to add payment method for {username} | No such account found");
            throw new ArgumentException($"Account '{username}' does not exist!");
        }

        if (account.HasFullPaymentMethods(PaymentMethods.Card)
            && account.HasFullPaymentMethods(PaymentMethods.Crypto))
        {
            logger.Log(LogLevel.Warning, $"Failed to add payment method for {username} | Payment storage full");
            throw new ArgumentException($"Account '{username}' already has no payment storage left!");
        }

        bool working = true;
        while (working)
        {
            PaymentMethod? selectedCard = null;
            PaymentMethod? selectedCrypto = null;

            Console.Clear();
            Console.WriteLine("SELECT YOUR PAYMENT METHOD\n");

            if (!account.HasFullPaymentMethods(PaymentMethods.Card))
            {
                Console.WriteLine(cardRequired ? "-- Card (REQUIRED) --" : "-- Card --");

                if (ChooseCard(cardRequired, out selectedCard) == SelectionResult.Exit)
                    break;
            }

            if (!account.HasFullPaymentMethods(PaymentMethods.Crypto))
            {
                Console.WriteLine("\n-- Crypto --");

                if (ChooseCrypto(out selectedCrypto) == SelectionResult.Restart)
                    continue;
            }

            working = ConfirmSelection(selectedCard, selectedCrypto);
        }

        Thread.Sleep(WaitTime);
    }

    public BankAccount? Register()
    {
        ShowHeader("REGISTRATION");

        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput(
            "Enter your password\n" +
            "(Hint: at least 1 letter, 1 number, 1 special character and 8 min length)\n" +
            "> ");

        try
        {
            AuthenticationValidator.ValidateCredentials(username, password);

            if (UsernameExists(username))
                throw new InvalidCredentialsException(CredentialsType.Username, username, true);

            uint id = GetNextAccountId();

            var user = new User(id, username)
            {
                PasswordHash = HashPassword(password),
                RegisteredAt = DateTime.Now
            };

            var account = new BankAccount(id, user);
            Accounts.Add(account.Id, account);

            AddPaymentMethod(account, cardRequired: true);

            WriteColoredLine("✅ Registration successfully completed!", ConsoleColor.Green);
            logger.Log(LogLevel.Info, $"Registered new account | Id: {id} / Username: {username}");

            Thread.Sleep(WaitTime);
            return account;
        }
        catch (Exception e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);
            logger.Log(LogLevel.Warning, "Registration failed", e);

            Thread.Sleep(WaitTime);
            return null;
        }
    }

    public BankAccount? Login()
    {
        if (Accounts.Count == 0)
        {
            WriteColoredLine("\nCannot log in: there are no registered accounts yet", ConsoleColor.DarkYellow);
            Thread.Sleep(WaitTime);
            return null;
        }

        ShowHeader("LOGIN");

        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput("Enter your password: ");

        try
        {
            AuthenticationValidator.ValidateCredentials(username, password);

            var account = FindAccountByUsername(username)
                ?? throw new InvalidCredentialsException(CredentialsType.Username, username);

            if (!VerifyPassword(password, account.user!.PasswordHash))
                throw new InvalidCredentialsException(CredentialsType.Password, password);

            WriteColoredLine("✅ Login successfully completed!", ConsoleColor.Green);
            logger.Log(LogLevel.Info, $"Logged in | Id: {account.Id} / Username: {username}");

            return account;
        }
        catch (Exception e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);
            logger.Log(LogLevel.Warning, "Login failed", e);

            Thread.Sleep(WaitTime);
            return null;
        }
    }

    public BankAccount? Menu()
    {
        while (true)
        {
            Console.Clear();

            string title = $"/|| -- [ {Name} Bank ] -- ||\\";

            Console.WriteLine(new string('=', title.Length));
            WriteColoredLine(title, ConsoleColor.Blue);
            Console.WriteLine(new string('-', title.Length));
            Console.WriteLine();

            BankAccount? account = null;

            switch ((AuthChoice)ChooseOption(AuthChoices))
            {
                case AuthChoice.Register: account = Register(); break;
                case AuthChoice.LogIn: account = Login(); break;
                case AuthChoice.Quit: return null;
            }

            if (account is not null)
                return account;

            Thread.Sleep(WaitTime);
        }
    }

    public BankAccount? GetAccountFromUser(User user)
        => Accounts.Values.FirstOrDefault(a => a?.user == user);

    public BankAccount? GetAccountByUsername(string username)
    {
        if (!AuthenticationValidator.ValidateUsername(username))
            return null;

        return FindAccountByUsername(username);
    }

    public bool TryGetAccount(uint id, out BankAccount? account)
        => Accounts.TryGetValue(id, out account);

    public bool AccountExists(BankAccount account)
        => account is not null && Accounts.ContainsValue(account);

    public bool RequestFundsWithdrawal(
        BankAccount account,
        decimal amount,
        string paymentId,
        PaymentMethods paymentMethod)
        => ProcessFundsRequest(
            "Withdrawal", "withdraw", account, amount,
            () => account.Withdraw(amount, paymentId, paymentMethod));

    public bool RequestFundsDeposit(
        BankAccount account,
        decimal amount,
        string paymentId,
        PaymentMethods paymentMethod)
        => ProcessFundsRequest(
            "Deposit", "deposit", account, amount,
            () => account.Deposit(amount, paymentId, paymentMethod));

    private bool ProcessFundsRequest(
        string noun,
        string verb,
        BankAccount account,
        decimal amount,
        Func<bool> operation)
    {
        string username = account?.user?.Username ?? "unknown";

        if (!AccountExists(account!))
        {
            logger.Log(LogLevel.Warning, $"Failed to {verb} funds | Account {username} not found");
            throw new ArgumentException($"Account {username} does not exist!");
        }

        bool success = HandlePaymentRequest(operation);
        string details = $"{amount}$ | Id: {account!.Id} Username: {account.user!.Username}";

        if (success)
        {
            WriteColoredLine($"✅ {noun} successfully completed!", ConsoleColor.Green);
            logger.Log(LogLevel.Info, $"Successful {noun.ToLower()} of {details}");
        }
        else
        {
            WriteColoredLine($"❌ {noun} failed!", ConsoleColor.Red);
            logger.Log(LogLevel.Warning, $"Failed {noun.ToLower()} of {details}");
        }

        return success;
    }
}
