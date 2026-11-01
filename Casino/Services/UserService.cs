using System.Security.Cryptography;
using System.Text;
using CSharp_Exam.Models;

namespace CSharp_Exam.Services
{
    public class UserService
    {
        private List<User> _users = new List<User>();
        private int _nextId = 1;

        public User? CurrentUser { get; private set; }

        private User? _pendingUser;
        private string _code = "";
        private DateTime _codeExpires;
        private int _attempts;

        public bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public User? FindByUsername(string username)
        {
            foreach (var user in _users)
            {
                if (user.Username.ToLower() == username.ToLower())
                    return user;
            }
            return null;
        }

        public User? FindByPhone(string phone)
        {
            foreach (var user in _users)
            {
                if (user.PhoneNumber == phone.Replace(" ", ""))
                    return user;
            }
            return null;
        }

        public (bool Success, string Message) Register(string username, string password, string phone)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                return (false, "Логін має бути не коротшим за 3 символи.");

            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return (false, "Пароль має бути не коротшим за 6 символів.");

            if (!IsValidPhone(phone))
                return (false, "Невірний номер телефону. Приклад: +380501234567");

            if (FindByUsername(username) != null)
                return (false, "Такий логін вже зайнятий.");

            if (FindByPhone(phone) != null)
                return (false, "Цей номер вже зареєстровано.");

            User user = new User();
            user.Id = _nextId;
            user.Username = username.Trim();
            user.PasswordHash = HashPassword(password);
            user.PhoneNumber = phone.Replace(" ", "");
            user.Balance = 0;
            user.RegisteredAt = DateTime.Now;
            user.ActivityHistory.Add(DateTime.Now + ": реєстрація");

            _nextId++;
            _users.Add(user);

            return (true, "Реєстрація пройшла успішно.");
        }

        public (bool Success, string Message) Login(string username, string password)
        {
            if (IsLoggedIn)
                return (false, "Ви вже увійшли. Спочатку вийдіть з акаунта.");

            User? user = FindByUsername(username);

            if (user == null || user.PasswordHash != HashPassword(password))
                return (false, "Невірний логін або пароль.");

            Random random = new Random();
            _code = random.Next(100000, 999999).ToString();
            _codeExpires = DateTime.Now.AddMinutes(5);
            _attempts = 0;
            _pendingUser = user;

            Console.WriteLine("[SMS на " + user.PhoneNumber + "] Ваш код: " + _code);

            string hidden = "***" + user.PhoneNumber.Substring(user.PhoneNumber.Length - 4);
            return (true, "Код надіслано на номер " + hidden);
        }

        public (bool Success, string Message) VerifyCode(string code)
        {
            if (_pendingUser == null)
                return (false, "Спочатку введіть логін і пароль.");

            if (DateTime.Now > _codeExpires)
            {
                _pendingUser = null;
                return (false, "Код застарів. Увійдіть знову.");
            }

            _attempts++;

            if (code.Trim() != _code)
            {
                if (_attempts >= 3)
                {
                    _pendingUser.ActivityHistory.Add(DateTime.Now + ": 3 невірні коди поспіль");
                    _pendingUser = null;
                    return (false, "Забагато спроб. Увійдіть знову.");
                }
                return (false, "Невірний код. Залишилось спроб: " + (3 - _attempts));
            }

            CurrentUser = _pendingUser;
            CurrentUser.ActivityHistory.Add(DateTime.Now + ": вхід в акаунт");
            _pendingUser = null;

            return (true, "Ласкаво просимо, " + CurrentUser.Username + "!");
        }

        public (bool Success, string Message) Logout()
        {
            if (CurrentUser == null)
                return (false, "Ви не увійшли в акаунт.");

            CurrentUser.ActivityHistory.Add(DateTime.Now + ": вихід з акаунта");
            CurrentUser = null;

            return (true, "Ви вийшли з акаунта.");
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = phone.Replace(" ", "");

            if (!phone.StartsWith("+"))
                return false;

            string digits = phone.Substring(1);

            if (digits.Length < 10 || digits.Length > 15)
                return false;

            foreach (char c in digits)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
