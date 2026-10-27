using CSharp_Exam.Casino.Finance.Configs;
using CSharp_Exam.Casino.Finance.Exceptions;
using static CSharp_Exam.Casino.Finance.Configs.ValidationRegistry;

using CSharp_Exam.Casino.Users;

namespace CSharp_Exam.Casino.Finance.Classes;

class Bank
{
    public static readonly ushort MaxPaymentMethods = 3;

    // TODO
    private Dictionary<User, FixedArray<IConfigBase>> users = new(MaxPaymentMethods);

    
    public void Register()
    {

    }

    public void Login()
    {

    }

    // ---------------------


    public static uint RequestFundsWithdrawal(User user, uint amount,ValidationRegistry.PaymentMethods payment_method)
    {
        return 0;
    }

    public static string GetCardOrCrypto(string input, ValidationRegistry.PaymentMethods payment_method)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        IEnumerable<KeyValuePair<string, IConfigBase>> indexes =
            payment_method switch
            {
                ValidationRegistry.PaymentMethods.Card =>
                    ValidationRegistry.CardIndexes.Select(x =>
                        new KeyValuePair<string, IConfigBase>(x.Key, x.Value)),

                ValidationRegistry.PaymentMethods.Crypto =>
                    ValidationRegistry.CryptoIndexes.Select(x =>
                        new KeyValuePair<string, IConfigBase>(x.Key, x.Value)),

                _ => Enumerable.Empty<KeyValuePair<string, IConfigBase>>()
            };

        var match = indexes
            .Where(x =>
                input.StartsWith(x.Key, StringComparison.OrdinalIgnoreCase) &&
                input.Length >= x.Value.MinLength &&
                input.Length <= x.Value.MaxLength)
            .OrderByDescending(x => x.Key.Length)
            .FirstOrDefault();

        return match.Value?.Name ?? throw new PaymentMethodNotFound(input);
    }

    public static bool ValidateCardOrCrypto(string input, ValidationRegistry.PaymentMethods payment_method)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = string.Concat(input.Where(c => !char.IsWhiteSpace(c)));

        Dictionary<string, IConfigBase> indexes;

        if (payment_method == ValidationRegistry.PaymentMethods.Card)
            indexes = ValidationRegistry.CardIndexes;
        else if (payment_method == ValidationRegistry.PaymentMethods.Crypto)
            indexes = ValidationRegistry.CryptoIndexes;
        else
            return false;

        return indexes.Any(x =>
            input.StartsWith(x.Key, StringComparison.Ordinal) &&
            input.Length >= x.Value.MinLength &&
            input.Length <= x.Value.MaxLength
        );
    }
}