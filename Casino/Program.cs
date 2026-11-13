using CSharp_Exam.Casino.Finance.Classes;
using CSharp_Exam.Casino.Services;
using CSharp_Exam.Casino.Services.Types;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Bank bank = new("Central Cat Bank");

DataService.Register(bank);

DataService.ExecuteAll(ActionType.Load);