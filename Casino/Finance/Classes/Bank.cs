//-- [ SYSTEM ]
using CSharp_Exam.Casino.Finance.Configs;

//-- [ EXCEPTIONS ]
using CSharp_Exam.Casino.Finance.Exceptions;

//-- [MODELS]
using CSharp_Exam.Casino.Models;

//-- [ SERVICES ]
using CSharp_Exam.Casino.Services.Logging;
using System.Text;
using System.Threading;

//-- [ CONFIGS ]
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;
using static CSharp_Exam.Casino.Services.ConsoleService;
using static CSharp_Exam.Casino.Services.HashingService;

namespace CSharp_Exam.Casino.Finance.Classes;

public partial class Bank
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

    private readonly string[] AuthChoices = [ "Register", "LogIn", "Quit" ];
    private readonly string[] CardPaymentMethods;
    private readonly string[] CryptoPaymentMethods;

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
    private static string ConsoleInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
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

        return prefix + new string([..
            Enumerable.Range(0, length - prefix.Length)
                .Select(_ => digits[Random.Next(digits.Length)])]
        );
    }

    private static void DisplaySelected(params PaymentMethod?[] methods)
    {
        bool every_null = true;

        foreach (PaymentMethod? method in methods)
        {
            if (method != null)
            {
                string method_name = method.Value.Method switch
                {
                    PaymentMethods.Card => "Card",
                    PaymentMethods.Crypto => "Crypto",
                    _ => "Unknown"
                };

                Console.WriteLine($"{method_name} - Name: {method.Value.Name} | Id: {method.Value.Id}");

                if (every_null) every_null = false;
            }
        }

        if (every_null)
            WriteColoredLine("No selected payment methods", ConsoleColor.Yellow);

        Console.WriteLine();
    }

    private void AddPaymentMethod(BankAccount account, bool cardRequired = false)
    {
        if (!Accounts.ContainsValue(account))
        { 
            logger.Log(LogLevel.Warning, $"Failed to add payment method for {account.user?.Username} | No such account found");
            throw new ArgumentException($"Account '{account.user?.Username}' does not exist!");
        }
        else if (account.HasFullPaymentMethods(PaymentMethods.Card) 
            && account.HasFullPaymentMethods(PaymentMethods.Crypto))
        {
            logger.Log(LogLevel.Warning, $"Failed to add payment method for {account.user?.Username} | Payment storage full");
            throw new ArgumentException($"Account '{account.user?.Username}' already has no payment storage left!");
        }


        bool work = true;
        while (work)
        {
            PaymentMethod? selectedCard = null, selectedCrypto = null;

            Console.Clear();
            Console.WriteLine("SELECT YOUR PAYMENT METHOD\n");  

            int choice;

            // CARD
            if (!account.HasFullPaymentMethods(PaymentMethods.Card))
            {
                if (cardRequired)
                {
                    Console.WriteLine("-- Card (REQUIRED) --");
                    choice = ChooseOption(
                        [.. CardPaymentMethods ]
                    );
                }
                else
                {
                    Console.WriteLine("-- Card --");
                    choice = ChooseOption(
                        [.. CardPaymentMethods, "Next", "Back"]
                    );
                }

                if (choice == CardPaymentMethods.Length + 1) break;
                else if (cardRequired && choice == CardPaymentMethods.Length) break;
                else if (choice != CardPaymentMethods.Length)
                {
                    var key = CardIndexes.ElementAt(choice).Key;
                    selectedCard = GetRandomPaymentMethod(PaymentMethods.Card, key);
                }
            }

            // CRYPTO
            if (!account.HasFullPaymentMethods(PaymentMethods.Crypto))
            {
                Console.WriteLine("\n-- Crypto --");
                choice = ChooseOption(
                    [.. CryptoPaymentMethods, "Skip", "Back"]
                );

                if (choice == CryptoPaymentMethods.Length + 1) continue;
                else if (choice != CryptoPaymentMethods.Length)
                {
                    var key = CryptoIndexes.ElementAt(choice).Key;
                    selectedCrypto = GetRandomPaymentMethod(PaymentMethods.Crypto, key);
                }
            }

            // CONFIRMATION
            Console.Clear();

            Console.WriteLine("SELECTED PAYMENT METHODS:");

            DisplaySelected([selectedCard, selectedCrypto]);

            choice = ChooseOption(["Confirm", "Retry", "Exit"]);

            switch (choice)
            {
                case 0:
                    {
                        work = false;

                        Console.Clear();
                        WriteColoredLine($"✅ Confirmed!", ConsoleColor.Green);

                        Console.WriteLine("// NEW PAYMENT METHODS \\\\");
                        DisplaySelected([selectedCard, selectedCrypto]);

                        StringBuilder msg = new($"Added new payment methods for {account.user!.Username} (Id: {account.Id})");

                        if (selectedCard != null)
                        {
                            msg.Append($"\nCard - Name: {selectedCard.Value.Name} | Id: {selectedCard.Value.Id}");
                            PaymentMethod card = selectedCard.Value;
                            card.Balance += 100_000.00m;
                            account.PaymentMethods[PaymentMethods.Card].Add(card);
                        }
                        if (selectedCrypto != null)
                        {
                            msg.Append($"\nCrypto - Name: {selectedCrypto.Value.Name} | Id: {selectedCrypto.Value.Id}");
                            PaymentMethod crypto = selectedCrypto.Value;
                            crypto.Balance += 100_000.00m;
                            account.PaymentMethods[PaymentMethods.Crypto].Add(crypto);
                        }

                        logger.Log(LogLevel.Info, msg.ToString());

                        break; 
                    }
                case 1:
                    {
                        WriteColoredLine("🔄 Preparing retry...", ConsoleColor.Magenta);
                            
                        Thread.Sleep(WaitTime / 2);

                        break;
                    }
                case 2:
                    {
                        work = false;

                        WriteColoredLine("ヾ(•ω•`) See you again!", ConsoleColor.Cyan);

                        break; 
                    }
            }
        }

        Thread.Sleep(WaitTime);
    }

    private void SelectPaymentMethod(BankAccount account)
    {
        PaymentMethod[] methods = [.. account.PaymentMethods.Values.SelectMany(x => x) ];
        string[] options = [
            .. methods
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name),
            "Exit"
        ];

        Console.WriteLine();
        int choice = ChooseOption(options);
        if (choice == methods.Length) return;

        PaymentMethod method = methods[choice];
        account.user!.SelectedMethod = method;

        WriteColored("➕ New Selected Method: ", ConsoleColor.Green);
        Console.Write($"Name {method.Name} | Id: {method.Id}\n");
    }

    public Bank(string name)
    {
        Name = name;
        logger = new LoggingService(new FileLogProvider(LogFilePath));

        CardPaymentMethods = [.. CardIndexes.Values.Select(v => v.Name)];
        CryptoPaymentMethods = [.. CryptoIndexes.Values.Select(v => v.Name)];
    }

    public BankAccount? Register()
    {
        Console.Clear();

        Console.WriteLine("================");
        WriteColoredLine("| REGISTRATION |", ConsoleColor.Blue);
        Console.WriteLine("================");

        string username = ConsoleInput("Enter your username: ");
        string password = ConsoleInput("Enter your password\n" +
            "(Hint: At least 1 letter, 1 number, 1 special character and 8 min length)" +
            "\n> ");

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

            AddPaymentMethod(account, cardRequired: true);

            WriteColoredLine("✅ Registration succesfully completed!", ConsoleColor.Green);
            logger.Log(LogLevel.Info, $"Registered New Account | Id: {id} / Username: {username}");

            Thread.Sleep(WaitTime);
            return account;
        }
        catch (Exception e)
        {
            WriteColoredLine(e.Message, ConsoleColor.DarkYellow);

            logger.Log(LogLevel.Warning, "Registration Failed", e);
        }

        Thread.Sleep(WaitTime);
        return null;
    }

    public BankAccount? Login()
    {
        if (Accounts.Count == 0)
        {
            WriteColoredLine("\nCannot Login, the accounts list is empty", ConsoleColor.DarkYellow);
            return null;
        }    

        Console.Clear();

        Console.WriteLine("===============");
        WriteColoredLine("|    LOGIN    |", ConsoleColor.Blue);
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

            WriteColoredLine("✅ Login succesfully completed!", ConsoleColor.Green);

            logger.Log(LogLevel.Info,
                $"Logged Into An Account | Id: {account.Id} / Username: {username}");

            Thread.Sleep(WaitTime);
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

    public BankAccount? Menu(BankAccount? existing_account = null)
    {
        if (existing_account != null)
        {
            if (!AccountExists(existing_account))
            { 
                WriteColoredLine($"Аккаут {existing_account.user?.Username} не iснує!", ConsoleColor.Red);
                return null;
            }

            while (true)
            {
                Console.Clear();
                WriteColoredLine($"-- Банкiвський аккаунт {existing_account.user!.Username} --");

                int choice = ChooseOption(["Перегляну методи оплати", "Вибрати метод оплати", "Вихiд"]);

                switch (choice)
                {
                    case 0:
                        existing_account.ShowBalance();
                        break;
                    case 1:
                        SelectPaymentMethod(existing_account);
                        break;
                    default:
                        return null;
                }

                Thread.Sleep(WaitTime);
            }
        }
        else
        {
            while (true)
            {
                Console.Clear();

                string title = $"/|| -- [ {Name} Bank ] -- ||\\";

                Console.WriteLine(new string('=', title.Length));
                WriteColoredLine(title, ConsoleColor.Blue);
                Console.WriteLine(new string('-', title.Length));
                Console.WriteLine();

                int choice = ChooseOption(AuthChoices);

                switch (choice)
                {
                    case 0:
                        {
                            BankAccount? account = Register();

                            if (account is not null)
                                return account;

                            break;
                        }

                    case 1:
                        {
                            BankAccount? account = Login();

                            if (account is not null)
                                return account;

                            break;
                        }

                    case 2:
                        return null;
                }

                Thread.Sleep(WaitTime);
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

    public bool AccountExists(BankAccount account)
    {
        if (account is null) return false;
        return Accounts.ContainsValue(account);
    }

    public bool RequestFundsWithdrawal(
    BankAccount account,
    decimal amount,
    string payment_id,
    PaymentMethods payment_method)
    {
        if (!AccountExists(account))
        {
            logger.Log(LogLevel.Warning, $"Вивiд коштiв невдався | Аккаунт {account.user?.Username} не знайдено");
            throw new ArgumentException($"Аккаунт {account.user?.Username} не iснує!");
        }

        bool success = HandlePaymentRequest(() => account.Withdraw(amount, payment_id, payment_method));

        if (success) 
        {
            WriteColoredLine("✅ Вивiд вдало завершено!", ConsoleColor.Green);

            logger.Log(LogLevel.Info,
                $"Вдалий вивiд {amount}$ | Id: {account.Id} Username: {account.user!.Username}");
        }
        else
        {
            WriteColoredLine("❌ Вивiд коштiв скасовано!", ConsoleColor.Red);

            logger.Log(LogLevel.Warning,
                $"Вивiд коштiв невдався {amount}$ | Id: {account.Id} Username: {account.user!.Username}");
        }

        return success;
    }

    public bool RequestFundsDeposit(
        BankAccount account,
        decimal amount,
        string payment_id,
        PaymentMethods payment_method)
    {
        if (!AccountExists(account))
        {
            logger.Log(LogLevel.Warning, $"Депозит коштiв невдався | Аккаунт {account.user?.Username} не знайдено");
            throw new ArgumentException($"Аккаунт {account.user?.Username} не iснує!");
        }

        bool success = HandlePaymentRequest(() => account.Deposit(amount, payment_id, payment_method));

        if (success)
        {
            WriteColoredLine("✅ Депозит вдало завершено!", ConsoleColor.Green);

            logger.Log(LogLevel.Info,
                $"Вдалий депозит {amount}$ | Id: {account.Id} Username: {account.user!.Username}");
        }
        else
        {
            WriteColoredLine("❌ Депозит коштiв скасовано!", ConsoleColor.Red);

            logger.Log(LogLevel.Warning,
                $"Депозит коштiв невдався {amount}$ | Id: {account.Id} Username: {account.user!.Username}");
        }

        return success;
    }
}