using System.Security.Cryptography;
using System.Text;
using YADAconsole.Models;

namespace YADAconsole.Services
{
    public class AuthenticationService
    {
        private readonly DataPersistenceService _persistenceService;
        private List<User> _users;

        public AuthenticationService(DataPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            _users = _persistenceService.LoadUsers();
        }

        public User? Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public (bool Success, string Message) Register(string name, string email, string password)
        {
            // Check if email already exists
            if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Email is already registered");
            }

            // Create password hash and salt
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Create new user
            var user = new User(name, email)
            {
                Id = Guid.NewGuid().ToString(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _users.Add(user);
            _persistenceService.SaveUsers(_users);

            return (true, "Registration successful");
        }

        public bool ResetPassword(string email, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return false;

            // In a real application, you would send a verification email first
            // Here we'll just reset the password directly for simplicity

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _persistenceService.SaveUsers(_users);

            return true;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                    return false;
            }

            return true;
        }
    }
}
