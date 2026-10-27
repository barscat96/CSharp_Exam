namespace CSharp_Exam.Casino.Finance.Exceptions;

class PaymentMethodNotFound(string payment_method) : Exception
{
    public override string ToString()
        => this.Message;

    public override string Message 
        => $"No existing payment method found for '{payment_method}'";
}
