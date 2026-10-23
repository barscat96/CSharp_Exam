using CSharp_Exam.Models;

namespace CSharp_Exam.Services
{
    public class UserService
    {
        private readonly List<User> _users = new List<User>();
        private int _nextId = 1;

        public User? CurrentUser { get; private set; }

        public List<User> GetAllUsers() => _users;

        public User? FindByUsername(string username)
            => _users.FirstOrDefault(u => u.Username == username);
    }
}
