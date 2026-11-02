using CSharp_Exam.Casino.Finance.Classes;
using CSharp_Exam.Casino.Finance.Configs;


// -- Testing
Bank Central_Bank = new();

string str = "4149498274028402";

try
{ 
    if (PaymentValidator.ValidateCardOrCrypto(str, ValidationRegistry.PaymentMethods.Card))
        Console.WriteLine(PaymentValidator.GetCardOrCrypto(str, ValidationRegistry.PaymentMethods.Card)); 
    else
        Console.WriteLine("Invalid payment method!");
}
catch (Exception e)
{
    Console.WriteLine(e);
}