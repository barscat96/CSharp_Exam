using CSharp_Exam.Casino.Admin;
using CSharp_Exam.Casino.Finance.Classes;
using CSharp_Exam.Casino.Games;
using CSharp_Exam.Casino.Stats;
using CSharp_Exam.Casino.Services;
using CSharp_Exam.Casino.Services.Logging;

namespace CSharp_Exam.Casino;

public class Casino
{
    public readonly string Name;
    private readonly LoggingService logger;
    private readonly string LogFilePath;
    private readonly int WaitTime = 2500;

    private Bank? SelectedBank;
    private BankAccount? CurrentUser;

    private readonly List<ConsoleColor> colors = [
        ConsoleColor.Yellow,
        ConsoleColor.Magenta,
        ConsoleColor.Red,
        ConsoleColor.Cyan
    ];

    private static readonly decimal MinDep = 100.00m;

    private readonly string[] MenuChoices = ["Iгри", "Депнути кошти", "Керувати аккаунтом", "Змiнити Банк", "Вихiд"];
    private readonly string[] ManagingChoices = [ "Профiль", "Керувати банковським аккаунтом", "Вихiд" ];
    private readonly Dictionary<string, decimal> DepOptions = new()
    {    
        ["100$"]     = 100.00m, 
        ["500$"]     = 500.00m, 
        ["1000$"]    = 1_000.00m, 
        ["5.000$"]   = 5_000.00m, 
        ["10.000$"]  = 10_000.00m, 
        ["50.000$"]  = 50_000.00m, 
        ["100.000$"] = 100_000.00m 
    };
    private readonly Dictionary<string, IGame> Games = new()
    {
        ["Покер"] = new PokerGame(),
        ["Блекджек"] = new Blackjack(),
        ["Слоти"] = new Slots(),
        ["Рулетка"] = new Roulette(),
        ["Кубики"] = new Dice()
    };

    public Casino(string name)
    {
        Name = name;
        LogFilePath = $"Casino_{Name}_Logs.txt";
        logger = new(new FileLogProvider(LogFilePath));
    }

    private int ChooseBank(Dictionary<string, Bank> banks)
        => ConsoleService.ChooseOption([.. banks.Keys, "Вихiд"]);

    private void DisplayTitle()
    {
        string title = $"| //-- {Name} -- \\\\ |";

        Console.Write('+');
        Console.Write(new string('=', title.Length - 2));
        Console.Write('+');

        Console.WriteLine();
        int i = 0;
        foreach (char c in title)
        {
            if (i >= colors.Count) i = 0;
            ConsoleService.WriteColored(c.ToString(), colors[i]);
        }
        Console.WriteLine();

        Console.Write('+');
        Console.Write(new string('=', title.Length - 2));
        Console.Write('+');

        Console.WriteLine();
    }

    private void ShowProfile()
    {
        Console.Clear();

        Console.WriteLine(CurrentUser!.user);
        Console.WriteLine();

        ConsoleService.ChooseOption("Вихiд");
    }

    private void ManageAccount()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Manage Account {CurrentUser!.user!.Username}");

            int choice = ConsoleService.ChooseOption(ManagingChoices);

            switch (choice)
            {
                case 0:
                    ShowProfile();
                    break;
                case 1:
                    SelectedBank!.Menu(CurrentUser);
                    break;
                default:
                    return;
            }
        }
    }
    
    private void GamesMenu()
    {
        Console.WriteLine();
        if (CurrentUser!.user!.Balance < MinDep)
        {
            ConsoleService.WriteColoredLine($"❔ Мiнiмальний баланс щоб грати: {MinDep}$ " +
                $"| Ваш баланс: {CurrentUser!.user!.Balance}$", ConsoleColor.DarkYellow);
            return;
        }

        int choice = ConsoleService.ChooseOption([.. Games.Keys, "Вихiд"]);

        if (choice == Games.Count) return;

        IGame game = Games.ElementAt(choice).Value;
        game.Play(CurrentUser!.user!);
    }

    private void Deposit()
    {
        Console.WriteLine();
        int choice = ConsoleService.ChooseOption([.. DepOptions.Keys, "Вихiд"]);
        if (choice == DepOptions.Count) return;

        decimal amount = DepOptions.ElementAt(choice).Value;

        try
        {
            bool success = SelectedBank!.RequestFundsWithdrawal(
                CurrentUser!,
                amount, 
                CurrentUser!.user!.SelectedMethod.Id,
                CurrentUser!.user!.SelectedMethod.Method
            );

            if (success)
                CurrentUser!.user!.Balance += amount;
        }
        catch (Exception e)
        {
            ConsoleService.WriteColoredLine(e.Message, ConsoleColor.Red);
            logger.Log(LogLevel.Warning, $"Невдалося депнути {amount}$");
        }

        Thread.Sleep(WaitTime);
    }

    private bool Menu()
    {
        while (true)
        {
            Console.Clear();
            int choice = ConsoleService.ChooseOption(MenuChoices);
            switch (choice)
            {
                case 0:
                    GamesMenu();
                    break;
                case 1:
                    Deposit();
                    break;
                case 2:
                    ManageAccount();
                    break;
                case 3:
                    return false;
                case 4:
                    return true;
            }

            Thread.Sleep(WaitTime);
        }
    }

    public void Start(Dictionary<string, Bank> banks)
    {
        while (true)
        {
            Console.Clear();

            DisplayTitle();

            Console.WriteLine("-- SELECT YOUR BANK --");

            int choice = ChooseBank(banks);
            if (choice == banks.Count)
            {
                ConsoleService.WriteColoredLine("ヾ(•ω•`) Заходьте ще!", ConsoleColor.Cyan);
                return;
            }

            SelectedBank = banks.ElementAt(choice).Value;
            CurrentUser = SelectedBank.Menu();

            if (CurrentUser is null)
            {
                ConsoleService.WriteColoredLine($"Failed to authenticate the bank account", ConsoleColor.Red);
                Thread.Sleep(WaitTime);
                continue;
            }

            UserService.AddUser(CurrentUser!.user);

            ManageAccount();

            bool end = Menu();

            if (end)
            {
                if (CurrentUser!.user!.Balance != 0)
                { 
                    try
                    {
                        bool success = SelectedBank!.RequestFundsDeposit(
                            CurrentUser!,
                            CurrentUser!.user!.Balance,
                            CurrentUser!.user!.SelectedMethod.Id,
                            CurrentUser!.user!.SelectedMethod.Method
                        );
                        ConsoleService.WriteColoredLine($"Ви забираєте з собою {CurrentUser!.user!.Balance}$ !", ConsoleColor.Green); 
                    }
                    catch (Exception e)
                    {
                        ConsoleService.WriteColoredLine(e.Message, ConsoleColor.Red);
                        logger.Log(LogLevel.Warning, $"Невдалося вивiсти {CurrentUser!.user!.Balance}$");
                    }
                }
                ConsoleService.WriteColoredLine("ヾ(•ω•`) Заходьте ще!", ConsoleColor.Cyan);
                return;
            }
        }
    }
}
