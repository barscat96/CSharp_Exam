using CSharp_Exam.Casino;
using CSharp_Exam.Casino.Finance.Classes;
using CSharp_Exam.Casino.Services;
using CSharp_Exam.Casino.Services.Types;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Dictionary<string, Bank> banks = new()
{
    ["Cat Bank"] = new("Cat Bank"),
    ["PrivatBank"] = new("PrivatBank")
};

foreach (Bank bank in banks.Values)
    DataService.Register(bank);

DataService.ExecuteAll(ActionType.Load);

Casino casino = new("Cat Casino");

casino.Start(banks);

DataService.ExecuteAll(ActionType.Save);