using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Models;
using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;
using System.Text.Json.Serialization;

namespace CSharp_Exam.Casino.Finance.Classes;

partial class BankAccount(uint id, User? user_ = null)
{
    [JsonIgnore]
    public readonly uint Id = id;
    [JsonInclude]
    public User? user { get; private set; } = user_;
    [JsonInclude]
    public string PasswordHash { get; private set; } = string.Empty;
    [JsonInclude]
    public Dictionary<DateTime, string> TransactionsHistory { get; private set; } = new();
    [JsonInclude]
    public DateTime AccountCreatedAt { get; private set; } = DateTime.Now;

    [JsonInclude]
    public Dictionary<PaymentMethods, FixedArray<PaymentMethod>> PaymentMethods
    { get; private set; } = new()
    {
        [ValidationRegistry.PaymentMethods.Card] =
                new FixedArray<PaymentMethod>(Bank.MaxCards),

        [ValidationRegistry.PaymentMethods.Crypto] =
                new FixedArray<PaymentMethod>(Bank.MaxCryptoWallets)
    };

    public bool HasPaymentMethod(string payment_id, PaymentMethods payment_method)
        => PaymentMethods.TryGetValue(payment_method, out var methods)
       && methods.Any(x => string.Equals(x.Id, payment_id, StringComparison.Ordinal));

    public bool Withdraw(
        decimal amount, 
        string payment_id, 
        PaymentMethods payment_method)
    {
        if (amount <= 0) return false;

        PaymentMethod payment = PaymentMethods[payment_method]
        .FirstOrDefault(x => x.Id == payment_id);

        if (payment == default) throw new PaymentMethodNotFound(payment_id);

        payment.Balance -= amount;

        TransactionsHistory[DateTime.Now] = $"Withdrawal of {amount} from {payment_id} ({payment_method})";

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

        TransactionsHistory[DateTime.Now] = $"Deposit of {amount} to {payment_id} ({payment_method})";

        return true;
    }
}