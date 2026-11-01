namespace CSharp_Exam.Models
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
    }
}
