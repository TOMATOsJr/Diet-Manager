using YADAconsole.Models;
using YADAconsole.Services;
using YADAconsole.Managers;

namespace YADAconsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== YADA Diet Manager ===");
            Console.WriteLine("Welcome to Your Adaptive Diet Assistant");

            // Initialize services
            var persistenceService = new DataPersistenceService();
            var authService = new AuthenticationService(persistenceService);

            // Authenticate user
            var authenticationManager = new AuthenticationManager(authService, persistenceService);
            var user = authenticationManager.LoginOrRegister();
            if (user == null)
            {
                Console.WriteLine("Login failed. Exiting application.");
                return;
            }

            // Initialize DietManager
            var dietManager = new DietManager(user, persistenceService);

            // Start the main menu
            var menuManager = new MenuManager(dietManager, persistenceService);
            menuManager.DisplayMainMenu();

            Console.WriteLine("Thank you for using YADA Diet Manager. Goodbye!");
        }
    }
}
