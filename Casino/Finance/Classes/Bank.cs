using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

using CSharp_Exam.Casino.Users;

namespace CSharp_Exam.Casino.Finance.Classes;

class Bank
{
    public static readonly ushort MaxCards = 4;
    public static readonly ushort MaxCryptoWallets = 4;

    private Dictionary<uint, BankAccount> users = new();

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

    // TODO
    public void Register()
    {

    }

    public void Login()
    {

    }

    // ---------------------

    public BankAccount? GetAccountFromUser(User user)
    {
        foreach (BankAccount account in users.Values)
            if (account.user == user) return account;

        return null;
    }

    public bool RequestFundsWithdrawal(
    User user,
    decimal amount,
    string payment_id,
    ValidationRegistry.PaymentMethods payment_method)
    {
        BankAccount account = GetAccountFromUser(user)
            ?? throw new UserNotFound(user.Name);

        return HandlePaymentRequest(() => account.Withdraw(amount, payment_id, payment_method));
    }

    public bool RequestFundsDeposit(
        User user,
        decimal amount,
        string payment_id,
        ValidationRegistry.PaymentMethods payment_method)
    {
        BankAccount account = GetAccountFromUser(user)
            ?? throw new UserNotFound(user.Name);

        return HandlePaymentRequest(() => account.Deposit(amount, payment_id, payment_method));
    }
}