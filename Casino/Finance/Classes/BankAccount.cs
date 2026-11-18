using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Models;
using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

using System.Text.Json.Serialization;

namespace CSharp_Exam.Casino.Finance.Classes;

public partial class BankAccount
{
    [JsonIgnore]
    public readonly uint Id;
    [JsonInclude]
    public User? user { get; private set; }
    [JsonInclude]
    public Dictionary<DateTime, string> TransactionsHistory { get; private set; } = [];
    [JsonInclude]
    public DateTime AccountCreatedAt { get; private set; } = DateTime.Now;
    [JsonIgnore]
    public decimal TotalBalance => PaymentMethods.Values
        .SelectMany(methods => methods)
        .Sum(method => method.Balance);


    [JsonIgnore]
    public Dictionary<PaymentMethods, FixedArray<PaymentMethod>> PaymentMethods { get; private set; } = new()
    {
        [ValidationRegistry.PaymentMethods.Card] = new(Bank.MaxCards),
        [ValidationRegistry.PaymentMethods.Crypto] = new(Bank.MaxCryptoWallets)
    };

    // Serialization shape
    [JsonInclude, JsonPropertyName("PaymentMethods")]
    private Dictionary<PaymentMethods, List<PaymentMethod>> PaymentMethodsData
    {
        get => PaymentMethods.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        set => _loaded = value;
    }

    [JsonIgnore]
    private Dictionary<PaymentMethods, List<PaymentMethod>>? _loaded;

    [JsonConstructor]
    public BankAccount(uint id, User? user = null)
    {
        Id = id;
        this.user = user;
    }

    public bool HasPaymentMethod(string payment_id, PaymentMethods payment_method)
        => PaymentMethods.TryGetValue(payment_method, out var methods)
       && methods.Any(x => string.Equals(x.Id, payment_id, StringComparison.Ordinal));

    public bool HasFullPaymentMethods(PaymentMethods method)
    {
        if (PaymentMethods.TryGetValue(method, out var val))
            return val.IsFull;

        return false;
    }

    // By Yarche1
    public void ShowBalance()
    {
        Console.WriteLine($"Загальний баланс: {TotalBalance}$\n");

        foreach (var (type, methods) in PaymentMethods)
        {
            foreach (var method in methods)
            {
                if (method == default) continue;

                Console.WriteLine($"{type} {method.Name} | {method.Id}: {method.Balance}$");
            }
        }
    }

    public void ShowTransactions()
    {
        if (TransactionsHistory.Count == 0)
        {
            Console.WriteLine("No transactions yet");
            return;
        }

        foreach (var (time, description) in TransactionsHistory.OrderByDescending(t => t.Key))
            Console.WriteLine($"{time:dd.MM.yyyy HH:mm:ss} - {description}");
    }
    //--------------------------------------------------------------------------------------------

    public bool Withdraw(
        decimal amount, 
        string payment_id, 
        PaymentMethods payment_method)
    {
        if (amount <= 0) return false;

        PaymentMethod payment = PaymentMethods[payment_method]
        .FirstOrDefault(x => x.Id == payment_id);

        if (payment == default) throw new PaymentMethodNotFound(payment_id);

        Console.WriteLine($"{payment.Balance} | {amount}");

        if (payment.Balance - amount < 0)
            throw new ArgumentException("Невистачає коштiв!");

        payment.Balance -= amount;

        TransactionsHistory[DateTime.Now] = $"Вивiд {amount}$ з {payment_id} ({payment_method})";

        return true;
    }

    public bool Deposit(
        decimal amount,
        string payment_id,
        PaymentMethods payment_method)
    {
        if (amount <= 0) return false;

        PaymentMethod payment = PaymentMethods[payment_method]
        .FirstOrDefault(x => x.Id == payment_id);

        if (payment == default) throw new PaymentMethodNotFound(payment_id);

        payment.Balance += amount;

        TransactionsHistory[DateTime.Now] = $"Депозит {amount}$ на {payment_id} ({payment_method})";

        return true;
    }
}