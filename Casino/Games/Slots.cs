using System;
using System.Collections.Generic;

namespace CSharp_Exam.Casino.Games
{
    public class Slots
    {
        private static readonly string[] Symbols = { "🍒", "🍋", "🍇", "💎", "7️⃣" };
        private static readonly Dictionary<string, decimal> Payouts = new()
        {
            { "7️⃣", 50m }, { "💎", 20m }, { "🍇", 10m }, { "🍋", 5m }, { "🍒", 3m }
        };

        public decimal Play(decimal balance, List<string> activityHistory, string username)
        {
            Console.Clear();
            Console.WriteLine("🎰 ЛАСКАВО ПРОСИМО ДО ГРИ В СЛОТИ 🎰");
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

            balance -= bet;
            Console.WriteLine("\nКрутимо барабани...");
            System.Threading.Thread.Sleep(1000);

            var rng = new Random();
            string reel1 = Symbols[rng.Next(Symbols.Length)];
            string reel2 = Symbols[rng.Next(Symbols.Length)];
            string reel3 = Symbols[rng.Next(Symbols.Length)];

            Console.WriteLine($"\n[ {reel1} ] [ {reel2} ] [ {reel3} ]");

            decimal winAmount = 0;
            if (reel1 == reel2 && reel2 == reel3)
            {
                winAmount = bet * Payouts[reel1];
                Console.WriteLine($"🎉 ДЖЕКПОТ! Ви виграли {winAmount:C}!");
            }
            else if (reel1 == reel2 || reel2 == reel3 || reel1 == reel3)
            {
                winAmount = bet * 1.5m;
                Console.WriteLine($"✨ Невеликий виграш! Ви отримали {winAmount:C}!");
            }
            else
            {
                Console.WriteLine("😔 На жаль, ви програли. Спробуйте ще раз!");
            }

            balance += winAmount;
            activityHistory.Add($"{DateTime.Now}: Гра в Слоти. Ставка: {bet:C}, Виграш: {winAmount:C}");

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