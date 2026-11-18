using System;
using System.Collections.Generic;

namespace CSharp_Exam.Casino.Games
{
    public class Roulette
    {
        private static readonly int[] RedNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        public decimal Play(decimal balance, List<string> activityHistory, string username)
        {
            Console.Clear();
            Console.WriteLine("🎡 ЛАСКАВО ПРОСИМО ДО РУЛЕТКИ 🎡");
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
            Console.WriteLine("1. Конкретне число (0-36) - виграш 35:1");
            Console.WriteLine("2. Колір (Червоне/Чорне) - виграш 1:1");
            Console.WriteLine("3. Парність (Парне/Непарне) - виграш 1:1");
            Console.Write("Ваш вибір (1-3): ");

            string choice = Console.ReadLine();
            balance -= bet;
            decimal winAmount = 0;

            var rng = new Random();
            int winningNumber = rng.Next(0, 37);
            bool isRed = Array.Exists(RedNumbers, n => n == winningNumber);
            string color = winningNumber == 0 ? "Зелене" : (isRed ? "Червоне" : "Чорне");
            string parity = winningNumber == 0 ? "Ні" : (winningNumber % 2 == 0 ? "Парне" : "Непарне");

            Console.WriteLine($"\nКулька крутиться... Випало число: {winningNumber} ({color}, {parity})");

            if (choice == "1")
            {
                Console.Write("Введіть число від 0 до 36: ");
                if (int.TryParse(Console.ReadLine(), out int number) && number >= 0 && number <= 36)
                {
                    if (number == winningNumber) winAmount = bet * 36m;
                }
            }
            else if (choice == "2")
            {
                Console.Write("Введіть колір (Червоне/Чорне): ");
                string colorChoice = Console.ReadLine()?.Trim().ToLower();
                if (colorChoice == "червоне" && isRed) winAmount = bet * 2m;
                else if (colorChoice == "чорне" && !isRed && winningNumber != 0) winAmount = bet * 2m;
            }
            else if (choice == "3")
            {
                Console.Write("Введіть парність (Парне/Непарне): ");
                string parityChoice = Console.ReadLine()?.Trim().ToLower();
                if (parityChoice == "парне" && winningNumber % 2 == 0 && winningNumber != 0) winAmount = bet * 2m;
                else if (parityChoice == "непарне" && winningNumber % 2 != 0) winAmount = bet * 2m;
            }

            Console.WriteLine(winAmount > 0 ? $"🎉 Вітаємо! Ви виграли {winAmount:C}!" : "😔 Ви програли. Щасти наступного разу!");

            balance += winAmount;
            activityHistory.Add($"{DateTime.Now}: Гра в Рулетку. Ставка: {bet:C}, Виграш: {winAmount:C}");
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