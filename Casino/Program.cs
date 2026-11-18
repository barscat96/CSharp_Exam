using CSharp_Exam.Casino.Finance.Classes;
using CSharp_Exam.Casino.Services;
using CSharp_Exam.Casino.Services.Types;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Bank bank = new("Central Cat Bank");

var account = bank.Menu();
Console.WriteLine(account?.user?.Username ?? "Idk");
account = bank.Menu(); // login
Console.WriteLine(account?.user?.Username ?? "Idk");