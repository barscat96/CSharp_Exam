using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_Exam.Casino.Games
{
    public class Blackjack
    {
        private class Card
        {
            public string Rank { get; set; }
            public int Value { get; set; }
            public string Suit { get; set; }
            public override string ToString() => $"{Rank}{Suit}";
        }

        private class Deck
        {
            private List<Card> cards = new List<Card>();
            private Random rng = new Random();

            public Deck()
            {
                string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
                string[] suits = { "♠", "♥", "♦", "♣" };

                foreach (var suit in suits)
                {
                    foreach (var rank in ranks)
                    {
                        int value = rank switch { "J" or "Q" or "K" => 10, "A" => 11, _ => int.Parse(rank) };
                        cards.Add(new Card { Rank = rank, Suit = suit, Value = value });
                    }
                }
                Shuffle();
            }

            public void Shuffle() => cards = cards.OrderBy(c => rng.Next()).ToList();
            public Card Draw()
            {
                Card card = cards[0];
                cards.RemoveAt(0);
                return card;
            }
        }

        private int CalculateHandValue(List<Card> hand)
        {
            int value = hand.Sum(c => c.Value);
            int aces = hand.Count(c => c.Rank == "A");
            while (value > 21 && aces > 0) { value -= 10; aces--; }
            return value;
        }

        public decimal Play(decimal balance, List<string> activityHistory, string username)
        {
            Console.Clear();
            Console.WriteLine("🃏 ЛАСКАВО ПРОСИМО ДО BLACKJACK 🃏");
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
            var deck = new Deck();
            var playerHand = new List<Card> { deck.Draw(), deck.Draw() };
            var dealerHand = new List<Card> { deck.Draw(), deck.Draw() };

            Console.WriteLine($"\nВаші карти: {string.Join(", ", playerHand)} (Сума: {CalculateHandValue(playerHand)})");
            Console.WriteLine($"Карти дилера: {dealerHand[0]} та [Прихована]");

            bool playerBusted = false;
            while (true)
            {
                Console.WriteLine("\n1. Взяти карту (Hit)");
                Console.WriteLine("2. Залишитися (Stand)");
                Console.Write("Ваш вибір (1-2): ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    playerHand.Add(deck.Draw());
                    int pValue = CalculateHandValue(playerHand);
                    Console.WriteLine($"\nВаші карти: {string.Join(", ", playerHand)} (Сума: {pValue})");
                    if (pValue > 21) { Console.WriteLine("💥 Перебір! Ви програли."); playerBusted = true; break; }
                }
                else if (choice == "2") { break; }
            }

            if (!playerBusted)
            {
                Console.WriteLine($"\nДилер відкриває карту: {string.Join(", ", dealerHand)} (Сума: {CalculateHandValue(dealerHand)})");
                while (CalculateHandValue(dealerHand) < 17)
                {
                    dealerHand.Add(deck.Draw());
                    Console.WriteLine($"Дилер бере карту: {string.Join(", ", dealerHand)} (Сума: {CalculateHandValue(dealerHand)})");
                }

                int pValue = CalculateHandValue(playerHand);
                int dValue = CalculateHandValue(dealerHand);

                if (dValue > 21 || pValue > dValue)
                {
                    decimal winAmount = bet * 2m;
                    Console.WriteLine($"🎉 Вітаємо! Ви виграли {winAmount:C}!");
                    balance += winAmount;
                }
                else if (pValue == dValue)
                {
                    Console.WriteLine("🤝 Нічия! Ставку повернуто.");
                    balance += bet;
                }
                else { Console.WriteLine("😔 Дилер виграв. Ви програли."); }
            }

            activityHistory.Add($"{DateTime.Now}: Гра в Blackjack. Ставка: {bet:C}");
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