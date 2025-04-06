using YADAconsole.Models;
using YADAconsole.Services;

namespace YADAconsole.Managers
{
    public class AuthenticationManager
    {
        private readonly AuthenticationService _authService;
        private readonly DataPersistenceService _persistenceService;

        public AuthenticationManager(AuthenticationService authService, DataPersistenceService persistenceService)
        {
            _authService = authService;
            _persistenceService = persistenceService;
        }

        public User? LoginOrRegister()
        {
            while (true)
            {
                Console.WriteLine("\n1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        return Login();
                    case "2":
                        Register();
                        break;
                    case "3":
                        return null;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private User? Login()
        {
            Console.WriteLine("\n=== Login ===");
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";

            Console.Write("Password: ");
            string password = ReadPassword();

            var user = _authService.Login(email, password);

            if (user == null)
            {
                Console.WriteLine("Invalid email or password. Please try again.");
                return null;
            }

            Console.WriteLine($"Welcome back, {user.Name}!");
            return user;
        }

        private void Register()
        {
            Console.WriteLine("\n=== Register ===");

            Console.Write("Name: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";

            Console.Write("Password: ");
            string password = ReadPassword();

            Console.Write("Confirm Password: ");
            string confirmPassword = ReadPassword();

            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match. Registration failed.");
                return;
            }

            var (success, message) = _authService.Register(name, email, password);
            Console.WriteLine(message);

            if (success)
            {
                Console.WriteLine("Please complete your profile:");

                Console.Write("Age: ");
                int.TryParse(Console.ReadLine(), out int age);

                Console.Write("Gender (Male/Female): ");
                string gender = Console.ReadLine() ?? "";

                Console.Write("Height (cm): ");
                double.TryParse(Console.ReadLine(), out double height);

                Console.Write("Weight (kg): ");
                double.TryParse(Console.ReadLine(), out double weight);

                Console.WriteLine("Activity levels: sedentary, light, moderate, active, very active");
                Console.Write("Activity Level: ");
                string activityLevel = Console.ReadLine() ?? "moderate";

                // Update user profile
                var user = _authService.Login(email, password);
                if (user != null)
                {
                    user.Age = age;
                    user.Gender = gender;
                    user.Height = height;
                    user.Weight = weight;
                    user.ActivityLevel = activityLevel;
                    user.CalculateTargetCalories();

                    // Resave users
                    var users = _persistenceService.LoadUsers();
                    var index = users.FindIndex(u => u.Id == user.Id);
                    if (index >= 0)
                    {
                        users[index] = user;
                        _persistenceService.SaveUsers(users);
                    }
                }
            }
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password[0..^1];
                        Console.Write("\b \b");
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }
    }
}