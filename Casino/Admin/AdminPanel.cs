using System;
using System.Collections.Generic;
using System.Linq;
using CSharp_Exam.Casino.Games;
using CSharp_Exam.Casino.Stats;

namespace CSharp_Exam.Casino.Admin
{
    public class AdminPanel
    {
        // Метод для запуску адмінки
        public static void ShowPanel(Player currentUser, List<Player> allPlayers, StatisticsManager? statsManager = null)
        {
            if (currentUser.Role != UserRole.Admin)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n🚨 ПОМИЛКА ДОСТУПУ: У вас немає прав адміністратора! Повертайтеся за ігрові столи.");
                Console.ResetColor();
                return;
            }

            // Якщо менеджер статистики не передали ззовні — створюєм дефолтний
            statsManager ??= new StatisticsManager();

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("==========================================================");
                Console.WriteLine($"👑  АДМІНІСТРАТИВНА ПАНЕЛЬ КAЗІНО | Адмін: {currentUser.Name} 👑");
                Console.WriteLine("==========================================================");
                Console.ResetColor();
                Console.WriteLine("1. Перегляд усіх користувачів казино");
                Console.WriteLine("2. Блокування користувача");
                Console.WriteLine("3. Розблокування користувача");
                Console.WriteLine("4. Перегляд статистики казино");
                Console.WriteLine("0. Вийти з адмін-панелі");
                Console.Write("\nОберіть дію: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowAllUsers(allPlayers);
                        break;
                    case "2":
                        SetBlockStatus(allPlayers, true);
                        break;
                    case "3":
                        SetBlockStatus(allPlayers, false);
                        break;
                    case "4":
                        ShowCasinoStatistics(statsManager);
                        break;
                    case "0":
                        Console.WriteLine("\nВихід з адмін-панелі...");
                        return;
                    default:
                        Console.WriteLine("[-] Неправильний вибір. Спробуйте ще раз.");
                        break;
                }

                Console.WriteLine("\nНатисніть Enter для продовження...");
                Console.ReadLine();
            }
        }

        // Перегляд користувачів
        private static void ShowAllUsers(List<Player> allPlayers)
        {
            Console.Clear();
            Console.WriteLine("======================================================================");
            Console.WriteLine("👤                   СПИСОК КОРИСТУВАЧІВ КАЗИНО                     👤");
            Console.WriteLine("======================================================================");
            Console.WriteLine($"{"ID",-5} | {"Ім'я гравця",-25} | {"Баланс",-12} | {"Роль",-10} | {"Статус",-12}");
            Console.WriteLine("----------------------------------------------------------------------");

            foreach (var p in allPlayers.OrderBy(p => p.Id))
            {
                if (p.IsBanned) Console.ForegroundColor = ConsoleColor.Red;
                else if (p.Role == UserRole.Admin) Console.ForegroundColor = ConsoleColor.Yellow;

                string statusText = p.IsBanned ? "❌ БАН" : "✅ Активний";
                Console.WriteLine($"{p.Id,-5} | {p.Name,-25} | ${p.Balance,-11} | {p.Role,-10} | {statusText,-12}");

                Console.ResetColor();
            }
            Console.WriteLine("======================================================================");
        }

        // Блокування\розблокування користувача
        private static void SetBlockStatus(List<Player> allPlayers, bool block)
        {
            ShowAllUsers(allPlayers);
            string actionText = block ? "БЛОКУВАННЯ" : "РОЗБЛОКУВАННЯ";
            Console.Write($"\nВведіть ID користувача для {actionText}: ");

            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                var targetPlayer = allPlayers.SingleOrDefault(p => p.Id == targetId);

                if (targetPlayer == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Помилка: Користувача з таким ID не знайдено!");
                    Console.ResetColor();
                    return;
                }

                if (targetPlayer.Role == UserRole.Admin)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Помилка: Не можна заблокувати іншого адміністратора!");
                    Console.ResetColor();
                    return;
                }

                targetPlayer.IsBanned = block;

                Console.ForegroundColor = block ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"\n[+] Успішно! Гравець {targetPlayer.Name} " + (block ? "ЗАБЛОКОВАНИЙ! ❌" : "розблокований! ✅"));
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("[-] Некоректний ID.");
            }
        }

        // Перегляд статистики казино
        private static void ShowCasinoStatistics(StatisticsManager statsManager)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================================");
            Console.WriteLine("📊              ЗАГАЛЬНА СТАТИСТИКА КАЗИНО              📊");
            Console.WriteLine("==========================================================");
            Console.ResetColor();
            Console.WriteLine($"Всього зіграно ставок:      {statsManager.TotalBets}");
            Console.WriteLine($"Загальна сума ставок:      ${statsManager.TotalBetAmount}");
            Console.WriteLine($"Загальна сума виплат:      ${statsManager.TotalPayouts}");

            Console.ForegroundColor = statsManager.NetResult >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Чистий прибуток казино:    ${statsManager.NetResult}");
            Console.ResetColor();

            var pokerStats = statsManager.GetGameStatistics("Poker");
            Console.WriteLine("\n--- Статистика по грі: POKER ---");
            Console.WriteLine($"Раундів: {pokerStats.TotalBets} | Ставок: ${pokerStats.TotalBetAmount} | Виплат: ${pokerStats.TotalPayouts}");
            Console.WriteLine($"Перемог гравців: {pokerStats.TotalWins} | Поразок: {pokerStats.TotalLosses} | Нічиїх: {pokerStats.TotalDraws}");
            Console.WriteLine("==========================================================");
        }
    }
}
