namespace CSharp_Exam.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public DateTime RegisteredAt { get; set; }
        public List<string> ActivityHistory { get; set; } = new List<string>();

        public void AddActivity(string text)
        {
            ActivityHistory.Add(DateTime.Now + ": " + text);
        }

        public bool Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сума має бути більшою за 0.");
                return false;
            }

            Balance = Balance + amount;
            AddActivity("поповнення балансу на " + amount);
            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сума має бути більшою за 0.");
                return false;
            }

            if (amount > Balance)
            {
                Console.WriteLine("Недостатньо коштів на балансі.");
                return false;
            }

            Balance = Balance - amount;
            AddActivity("списання з балансу " + amount);
            return true;
        }

        public void ShowBalance()
        {
            Console.WriteLine("Баланс: " + Balance);
        }

        public void ShowProfile()
        {
            string phone = PhoneNumber;

            if (phone.Length > 4)
            {
                phone = "***" + phone.Substring(phone.Length - 4);
            }

            Console.WriteLine("Логін: " + Username);
            Console.WriteLine("Телефон: " + phone);
            Console.WriteLine("Баланс: " + Balance);
            Console.WriteLine("Дата реєстрації: " + RegisteredAt);
        }

        public void ShowHistory(int count)
        {
            if (ActivityHistory.Count == 0)
            {
                Console.WriteLine("Історія порожня.");
                return;
            }

            int start = ActivityHistory.Count - count;
            if (start < 0)
            {
                start = 0;
            }

            for (int i = start; i < ActivityHistory.Count; i++)
            {
                Console.WriteLine(ActivityHistory[i]);
            }
        }
    }
}
