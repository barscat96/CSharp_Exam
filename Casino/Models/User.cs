namespace CSharp_Exam.Casino.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime RegisteredAt { get; set; }
        public List<string> ActivityHistory { get; set; } = new List<string>();

        public User() { }

        public User(User other)
        {
            Id = other.Id;
            Username = other.Username;
            PasswordHash = other.PasswordHash;
            PhoneNumber = other.PhoneNumber;
            Balance = other.Balance;
            RegisteredAt = other.RegisteredAt;
            ActivityHistory = new List<string>(other.ActivityHistory);
        }
    }
}
