using System;
using System.Collections.Generic;

namespace CSharp_Exam.Casino.Games
{
    public class Dice
    {
        public decimal Play(decimal balance, List<string> activityHistory, string username)
        {
            Console.Clear();
            Console.WriteLine("🎲 ЛАСКАВО ПРОСИМО ДО ГРИ В КУБИКИ 🎲");
            Console.WriteLine($"Ваш поточний баланс: {balance:C}");

            Console.Write("Введіть суму ставки: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal bet) || bet <= 0)
            {
                Console.WriteLine("❌ Некоректна сума ставки.");
                WaitForInput(); return balance;
            }

            if (bet > balance)
            {
                Console.WriteLine("❌ Недостатньо коштів на балансі.");
                WaitForInput(); return balance;
            }

            Console.WriteLine("\nОберіть тип ставки:");
            Console.WriteLine("1. Сума більше 7 (виграш 1:1)");
            Console.WriteLine("2. Сума менше 7 (виграш 1:1)");
            Console.WriteLine("3. Сума рівно 7 (виграш 4:1)");
            Console.WriteLine("4. Дубль (обидва кубики однакові) (виграш 5:1)");
            Console.Write("Ваш вибір (1-4): ");

            string choice = Console.ReadLine();
            balance -= bet;
            decimal winAmount = 0;

            var rng = new Random();
            int dice1 = rng.Next(1, 7);
            int dice2 = rng.Next(1, 7);
            int sum = dice1 + dice2;
            bool isDouble = dice1 == dice2;

            Console.WriteLine($"\nКидаємо кубики... 🎲 {dice1} + 🎲 {dice2} = Сума: {sum}");
            if (isDouble) Console.WriteLine("Це дубль!");

            if (choice == "1" && sum > 7) winAmount = bet * 2m;
            else if (choice == "2" && sum < 7) winAmount = bet * 2m;
            else if (choice == "3" && sum == 7) winAmount = bet * 5m;
            else if (choice == "4" && isDouble) winAmount = bet * 6m;

            Console.WriteLine(winAmount > 0 ? $"🎉 Вітаємо! Ви виграли {winAmount:C}!" : "😔 Ви програли. Щасти наступного разу!");

            balance += winAmount;
            activityHistory.Add($"{DateTime.Now}: Гра в Кубики. Ставка: {bet:C}, Виграш: {winAmount:C}");
            Console.WriteLine($"\nВаш новий баланс: {balance:C}");
            WaitForInput();
            return balance;
        }

        private void WaitForInput()
        {
            Console.WriteLine("Натисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }
    }
}