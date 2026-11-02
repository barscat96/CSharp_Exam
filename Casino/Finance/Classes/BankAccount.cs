using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Users;
using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

namespace CSharp_Exam.Casino.Finance.Classes;

class BankAccount(User? user_ = null)
{
    public User? user { get; } = user_;

    public Dictionary<ValidationRegistry.PaymentMethods, FixedArray<PaymentMethod>> PaymentMethods
    { get; } = new()
    {
        [ValidationRegistry.PaymentMethods.Card] =
                new FixedArray<PaymentMethod>(Bank.MaxCards),

        [ValidationRegistry.PaymentMethods.Crypto] =
                new FixedArray<PaymentMethod>(Bank.MaxCryptoWallets)
    };

    public bool HasPaymentMethod(string payment_id, ValidationRegistry.PaymentMethods payment_method)
        => PaymentMethods.TryGetValue(payment_method, out var methods)
       && methods.Any(x => string.Equals(x.Id, payment_id, StringComparison.Ordinal));

    public bool Withdraw(
        decimal amount, 
        string payment_id, 
        ValidationRegistry.PaymentMethods payment_method)
    {
        if (amount <= 0) return false;

        PaymentMethod payment = PaymentMethods[payment_method]
        .FirstOrDefault(x => x.Id == payment_id);

        if (payment == default) throw new PaymentMethodNotFound(payment_id);

        payment.Balance -= amount;

        return true;
    }

    public bool Deposit(
        decimal amount,
        string payment_id,
        ValidationRegistry.PaymentMethods payment_method)
    {
        if (amount <= 0) return false;

        PaymentMethod payment = PaymentMethods[payment_method]
        .FirstOrDefault(x => x.Id == payment_id);

        if (payment == default) throw new PaymentMethodNotFound(payment_id);

        payment.Balance += amount;

        return true;
    }
}