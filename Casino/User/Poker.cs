using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_Exam.Casino.Games
{
    // 4 покерні масті
    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    // Ранги карт. Двійка починається з 2, тому Туз автоматично стає 14.
    // Це потрібно для легкого порівняння карт через математичні знаки > або <.
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    // Клас одної карти. Має масть і номінал.
    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        // Перетворює об'єкт карти на красивий текст для консолі (наприклад, A♠ або 10♥)
        public override string ToString()
        {
            char suitSymbol = Suit switch
            {
                Suit.Diamonds => '♦',
                Suit.Hearts => '♥',
                Suit.Clubs => '♣',
                Suit.Spades => '♠',
                _ => ' '
            };

            string rankStr = Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)Rank).ToString()
            };

            return $"{rankStr}{suitSymbol}";
        }
    }

    // Клас колоди карт (52 штуки)
    public class Deck
    {
        private List<Card> cards = new List<Card>();
        private Random random = new Random();

        // Автоматично створює всі 52 карти, коли пишемо new Deck()
        public Deck()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }
            Shuffle(); // Одразу тасуємо її
        }

        // Кожній карті присвоюється випадковий номер за яким і сортується колода
        public void Shuffle()
        {
            cards = cards.OrderBy(c => random.Next()).ToList();
        }

        // Верхню карту витягуєм з деки і видаляєм зі списка шоб не повторювалась
        public Card Draw()
        {
            if (cards.Count == 0) throw new InvalidOperationException("Колода порожня!");
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        // Механіка казіка - викинути куда подальше верхню карту перед роздачею щоб ніхто раптом не підглянув XD
        public void BurnCard()
        {
            if (cards.Count > 0) cards.RemoveAt(0);
        }
    }

    // Все що є в гравця (ім'я, фішки, ставка, карти в руці)
    public class Player
    {
        public string Name { get; }
        public int Balance { get; set; }
        public int CurrentBet { get; set; }
        public bool IsFolded { get; set; } // якщо тру - гравець пасанув і вилетів з роздачі
        public List<Card> Hand { get; } = new List<Card>();

        public Player(string name, int startbalance)
        {
            Name = name;
            Balance = startbalance;
        }
    }
    // Сила комбінацій від 0 до 9
    public enum HandStrength
    {
        HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush
    }

    // Фінальний результат гравця, який вміє порівнювати себе через IComparable
    public class HandValue : IComparable<HandValue>
    {
        public HandStrength HandStrength { get; set; }
        public List<int> CardValues { get; set; }

        // Порівняння резу гравця з резом суперника
        public int CompareTo(HandValue other)
        {
            if (other == null) return 1;

            // Спочатку дивимося в кого найкрутіша комбнація
            if (this.HandStrength != other.HandStrength)
                return this.HandStrength.CompareTo(other.HandStrength);

            // Якщо однакові комбінації — по черзі порівнюємо старші карти
            for (int i = 0; i < this.CardValues.Count; i++)
            {
                if (this.CardValues[i] != other.CardValues[i])
                    return this.CardValues[i].CompareTo(other.CardValues[i]);
            }
            return 0; // Якщо все однакове то нічия
        }
    }

    // Математичний аналізатор карт
    public static class PokerEvaluator
    {
        // Приймає 7 карт, генерує 21 варіант по 5 карт і знаходить найкращий результат
        public static HandValue Evaluate7Cards(List<Card> sevenCards)
        {
            var allCombos = GetCombinations(sevenCards, 5);
            HandValue bestHand = null;

            // Перебірка всіх комбінацій
            foreach (var combo in allCombos)
            {
                var currentHandValue = Evaluate5Cards(combo);
                if (bestHand == null || currentHandValue.CompareTo(bestHand) > 0)
                {
                    bestHand = currentHandValue;
                }
            }
            return bestHand;
        }

        // Оцінка конкретних 5 карт столу/руки
        private static HandValue Evaluate5Cards(List<Card> hand)
        {
            var sorted = hand.OrderByDescending(c => (int)c.Rank).ToList();

            bool isFlush = sorted.All(c => c.Suit == sorted[0].Suit);
            bool isStraight = true;

            for (int i = 0; i < 4; i++)
            {
                if ((int)sorted[i].Rank - (int)sorted[i + 1].Rank != 1)
                {
                    isStraight = false;
                    break;
                }
            }

            // Групування однакових карт (використав Linq)
            var groups = sorted.GroupBy(c => (int)c.Rank)
                               .OrderByDescending(g => g.Count()) // Спочатку групи, де найбільше карт
                               .ThenByDescending(g => g.Key)      // Потім за старшинством карт всередині груп
                               .ToList();

            // Перевірка комбінацій від Роял флешу вниз до старшої карти
            if (isStraight && isFlush)
            {
                bool isRoyal = sorted[0].Rank == Rank.Ace;
                return new HandValue { HandStrength = isRoyal ? HandStrength.RoyalFlush : HandStrength.StraightFlush, CardValues = sorted.Select(c => (int)c.Rank).ToList() };
            }

            // Каре - якщо у першій групі 4 однакові карти
            if (groups.Count == 2 && groups[0].Count() == 4)
                return new HandValue { HandStrength = HandStrength.FourOfAKind, CardValues = groups.Select(g => g.Key).ToList() };

            // Фул Хаус - якщо є дві групи — в одній 3 карти, в іншій 2
            if (groups.Count == 2 && groups[0].Count() == 3 && groups[1].Count() == 2)
                return new HandValue { HandStrength = HandStrength.FullHouse, CardValues = groups.Select(g => g.Key).ToList() };

            if (isFlush)
                return new HandValue { HandStrength = HandStrength.Flush, CardValues = sorted.Select(c => (int)c.Rank).ToList() };

            if (isStraight)
                return new HandValue { HandStrength = HandStrength.Straight, CardValues = sorted.Select(c => (int)c.Rank).ToList() };

            // Трійка - якщо в першій групі 3 карти
            if (groups.Count == 3 && groups[0].Count() == 3)
                return new HandValue { HandStrength = HandStrength.ThreeOfAKind, CardValues = groups.Select(g => g.Key).ToList() };

            // Дві пари - дві групи по 2 карти
            if (groups.Count == 3 && groups[0].Count() == 2 && groups[1].Count() == 2)
                return new HandValue { HandStrength = HandStrength.TwoPair, CardValues = groups.Select(g => g.Key).ToList() };

            // Одна пара - одна група з 2 карт
            if (groups.Count == 4 && groups[0].Count() == 2)
                return new HandValue { HandStrength = HandStrength.Pair, CardValues = groups.Select(g => g.Key).ToList() };

            // Якщо взагалі нічо нема - просто старша карта
            return new HandValue { HandStrength = HandStrength.HighCard, CardValues = sorted.Select(c => (int)c.Rank).ToList() };
        }

        // Рекурсивна математична функція для комбінаторики 5 з 7
        private static List<List<T>> GetCombinations<T>(List<T> list, int length)
        {
            if (length == 1) return list.Select(item => new List<T> { item }).ToList();
            var result = new List<List<T>>();
            for (int i = 0; i <= list.Count - length; i++)
            {
                var subCombos = GetCombinations(list.Skip(i + 1).ToList(), length - 1);
                foreach (var subCombo in subCombos)
                {
                    subCombo.Insert(0, list[i]);
                    result.Add(subCombo);
                }
            }
            return result;
        }
    }

    public class PokerGame
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("🎲       КОНСОЛЬНИЙ СВІТОВИЙ ТУРНІР З ПОКЕРУ       🎲");
            Console.WriteLine("======================================================\n");

            Console.Write("Введіть ваше ім'я для гри: ");
            string userName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = "Player";
            }

            // Створюємо гравців разок тут, щоб фішки не скидалися кожен раунд
            Player user = new Player(userName, 10000);
            Player bot1 = new Player("Брін Кенні (США)", 10000);
            Player bot2 = new Player("Джастін Бономо (США)", 10000);
            Player bot3 = new Player("Стівен Чідвік (Англія)", 10000);
            Player bot4 = new Player("Даніель Негреану (Канада)", 10000);
            Player bot5 = new Player("Адріан Матеос (Іспанія)", 10000);
            List<Player> players = new List<Player> { user, bot1, bot2, bot3, bot4, bot5 };

            // ГОЛОВНИЙ ЦИКЛ ГРИ - граємо скільки хочемо
            while (true)
            {
                // Перевірка: якщо у тебе залишилось менше $100 — ти банкрут
                if (user.Balance < 100)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n💀 Упс! У вас залишилось менше $100. Ви збанкрутували! Казино закривається.");
                    Console.ResetColor();
                    break;
                }

                // Скидаємо карти та ставки перед кожною новою роздачею
                foreach (var p in players)
                {
                    p.Hand.Clear();
                    p.CurrentBet = 0;
                    p.IsFolded = p.Balance < 100; // Збанкрутілі боти пропускають раунд
                }

                // Перевірка: чи лишився хоч один живий бот із грошима
                if (players.Count(p => !p.IsFolded && p != user) == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n🎉 НЕЙМОВІРНО! Ви повністю обнулили всіх мільйонерів за столом! Ви бос цього казино!");
                    Console.ResetColor();
                    break;
                }

                Deck deck = new Deck(); // Нова потасована колода на кожну роздачу
                int pot = 0;

                Console.Clear();
                Console.WriteLine("================================================================");
                Console.WriteLine("🃏             НОВА РОЗДАЧА (КАРТИ ПЕРЕТАСОВАНО)             🃏");
                Console.WriteLine("================================================================");

                int ante = 100;
                Console.WriteLine($"\n[АНТЕ] Всі активні гравці скидаються по ${ante} в банк...");
                foreach (var p in players.Where(p => !p.IsFolded))
                {
                    p.Balance -= ante;
                    p.CurrentBet = ante;
                    pot += ante;
                }

                for (int i = 0; i < 2; i++)
                {
                    foreach (var p in players.Where(p => !p.IsFolded)) p.Hand.Add(deck.Draw());
                }

                deck.BurnCard();
                List<Card> tableCards = new List<Card> { deck.Draw(), deck.Draw(), deck.Draw() };

                Console.WriteLine($"\n💰 Стартовий банк чемпіонату: ${pot}");
                Console.WriteLine($"🃏 Карти на столі (Флоп): {string.Join(" ", tableCards)} | [Приховано] [Приховано]");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n👉 Твої карти на руках: {string.Join(" ", user.Hand)} (Твій баланс: ${user.Balance})");
                Console.ResetColor();

                int currentMaxBet = ante;

                Console.WriteLine($"\n--- ХІД ГРАВЦЯ {user.Name.ToUpper()} ---");
                Console.WriteLine("1. Check (Чек — передати хід без підвищення)");
                Console.WriteLine("2. Raise (Рейз — підняти ставку, докинувши +$500)");
                Console.WriteLine("3. Fold (Фолд — пасанути і викинути карти)");
                Console.Write("Введіть цифру (1-3): ");

                string input = Console.ReadLine();
                if (input == "2")
                {
                    int raise = 500;
                    user.Balance -= raise;
                    user.CurrentBet += raise;
                    pot += raise;
                    currentMaxBet = user.CurrentBet;
                    Console.WriteLine($"\n[!] Ви зробили Рейз! Ваша загальна ставка: ${user.CurrentBet}");
                }
                else if (input == "3")
                {
                    user.IsFolded = true;
                    Console.WriteLine("\n[!] Ви скинули карти. Раунд для вас закінчено.");
                }
                else
                {
                    Console.WriteLine("\n[!] Ви сказали Чек.");
                }

                Random rng = new Random();
                foreach (var bot in players.Where(p => p != user && !p.IsFolded))
                {
                    var current5Cards = bot.Hand.Concat(tableCards).ToList();
                    HandValue botHandValue = PokerEvaluator.Evaluate7Cards(current5Cards);
                    HandStrength strength = botHandValue.HandStrength;

                    bool isAggressivePro = bot.Name.Contains("Бономо") || bot.Name.Contains("Матеос");

                    // Ситуація: Ніхто не піднімав ставку (всі сказали Чек)
                    if (currentMaxBet == bot.CurrentBet)
                    {
                        if (isAggressivePro && (strength >= HandStrength.Pair || rng.Next(0, 100) < 30))
                        {
                            int proRaise = 500;
                            if (bot.Balance >= proRaise)
                            {
                                bot.Balance -= proRaise;
                                bot.CurrentBet += proRaise;
                                pot += proRaise;
                                currentMaxBet = bot.CurrentBet;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"🚨 🤖 {bot.Name} перехоплює ініціативу і робить РЕЙЗ на +${proRaise}!");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"🤖 {bot.Name} каже: CHECK");
                        }
                    }
                    // Ситуація: Ставка зросла
                    else
                    {
                        bool shouldCall = false;

                        if (strength >= HandStrength.TwoPair) shouldCall = true;
                        else if (strength == HandStrength.Pair) shouldCall = rng.Next(0, 100) < (isAggressivePro ? 85 : 65);
                        else shouldCall = isAggressivePro && (rng.Next(0, 100) < 30);

                        int callAmount = currentMaxBet - bot.CurrentBet;
                        if (shouldCall && bot.Balance >= callAmount)
                        {
                            bot.Balance -= callAmount;
                            bot.CurrentBet += callAmount;
                            pot += callAmount;
                            Console.WriteLine($"🤖 {bot.Name} каже: CALL (доставив ${callAmount})");
                        }
                        else
                        {
                            bot.IsFolded = true;
                            Console.WriteLine($"🤖 {bot.Name} каже: FOLD (викинув карти)");
                        }
                    }
                }

                if (currentMaxBet > user.CurrentBet)
                {
                    user.IsFolded = true;
                }

                if (!user.IsFolded)
                {
                    deck.BurnCard(); tableCards.Add(deck.Draw());
                    deck.BurnCard(); tableCards.Add(deck.Draw());

                    Console.WriteLine("\n======================================================");
                    Console.WriteLine($"\n🃏 Фінальні карти на столі: {string.Join(" ", tableCards)}\n");

                    Player winner = null;
                    HandValue bestResult = null;

                    foreach (var p in players.Where(p => !p.IsFolded))
                    {
                        var full7Cards = p.Hand.Concat(tableCards).ToList();
                        HandValue result = PokerEvaluator.Evaluate7Cards(full7Cards);

                        Console.WriteLine($"👤 {p.Name} [{string.Join(" ", p.Hand)}] -> {result.HandStrength}");

                        if (bestResult == null || result.CompareTo(bestResult) > 0)
                        {
                            bestResult = result;
                            winner = p;
                        }
                    }
                    Console.WriteLine("\n------------------------------------------------------");
                    if (winner == user)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"🎉 НЕЙМОВІРНО, {user.Name.ToUpper()}! Ви обіграли найкращих хайроллерів планети і забрали куш: ${pot}!");
                        user.Balance += pot;
                    }
                    else
                    {
                        // Якщо ти пасанув, боти швиденько розігрують банк між собою
                        var activeBots = players.Where(p => !p.IsFolded && p != user).ToList();
                        if (activeBots.Count > 0)
                        {
                            Player botWinner = activeBots[rng.Next(activeBots.Count)];
                            botWinner.Balance += pot;
                            Console.WriteLine($"\n💀 Ви скинули карти. Фінальний банк у ${pot} розіграли між собою боти. Виграв {botWinner.Name}.");
                        }
                    }
                    Console.WriteLine("\nНатисніть Enter, щоб продовжити...");
                    while (Console.KeyAvailable) Console.ReadKey(true);
                    Console.ReadLine();


                    // ВИВІД ПОТОЧНОГО БАЛАНСУ ТА ЗАПИТ НА НАСТУПНИЙ РАУНД
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n💰 Твій поточний баланс фішок: ${user.Balance}");
                    Console.ResetColor();

                    Console.Write("\nБажаєте зіграти наступну роздачу? (y/n): ");
                    string answer = Console.ReadLine()?.ToLower();
                    if (answer != "y" && answer != "н")
                    {
                        Console.WriteLine("\nДякуємо за гру! Ви зняли гроші та вийшли з казино.");
                        break;
                    }
                }
            }
        }
    }
}
