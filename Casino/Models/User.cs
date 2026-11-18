using CSharp_Exam.Casino.Finance.Configs;
using System.Text.Json.Serialization;

namespace CSharp_Exam.Casino.Models
{
    public class User
    {
        [JsonIgnore]
        public uint Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = "+380501234567";
        [JsonIgnore]
        public decimal Balance { get; set; }
        public DateTime RegisteredAt { get; set; }
        public List<string> ActivityHistory { get; set; } = new List<string>();

        [JsonIgnore]
        public PaymentMethod SelectedMethod { get; set; }

        public User() { }

        public User(uint id, string username = "")
        {
            Id = id;
            if (!string.IsNullOrEmpty(username))
                Username = username;
        }

        public User(User other)
        {
            Id = other.Id;
            Username = other.Username;
            PasswordHash = other.PasswordHash;
            PhoneNumber = other.PhoneNumber;
            Balance = other.Balance;
            RegisteredAt = other.RegisteredAt;
            ActivityHistory = [.. other.ActivityHistory ];
        }

        public override string ToString()
            => $"//-- {Username} --\\\\" +
                $"ID: {Id}\nНомер Телефону: {PhoneNumber}\nБаланс: {Balance}";
    }
}
