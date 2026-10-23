namespace CSharp_Exam.Casino.Finance;

class Bank
{
    // User

    static bool ValidateCardOrCrypto(string input, ValidationRegistry.PaymentTypes paymentType)
    {
        if (paymentType == ValidationRegistry.PaymentTypes.Card)
        {
            
        }
        else if (paymentType == ValidationRegistry.PaymentTypes.Crypto)
        {

        }

        return true;
    }
}