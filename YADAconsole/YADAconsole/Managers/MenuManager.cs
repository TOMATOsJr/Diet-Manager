using YADAconsole.Models;
using YADAconsole.Services;
using System;

namespace YADAconsole.Managers
{
    public class MenuManager
    {
        private readonly DietManager _dietManager;
        private readonly DataPersistenceService _persistenceService;
        private readonly FoodLogManager _foodLogManager;
        private readonly RecipeManager _recipeManager;
        private readonly NutrientAnalysisManager _nutrientAnalysisManager;
        private readonly UserProfileManager _userProfileManager;
        private DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Today);

        public MenuManager(DietManager dietManager, DataPersistenceService persistenceService)
        {
            _dietManager = dietManager;
            _persistenceService = persistenceService;

            // Initialize all sub-managers
            _foodLogManager = new FoodLogManager(dietManager);
            _recipeManager = new RecipeManager(dietManager);
            _nutrientAnalysisManager = new NutrientAnalysisManager(dietManager);
            _userProfileManager = new UserProfileManager(dietManager, persistenceService);
        }

        public void DisplayMainMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n=== Main Menu ===");
                Console.WriteLine("1. Manage Food Log");
                Console.WriteLine("2. Manage Recipes");
                Console.WriteLine("3. View Nutrient Analysis");
                Console.WriteLine("4. Manage User Profile");
                Console.WriteLine("5. Set Current Date");
                Console.WriteLine("6. Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        _foodLogManager.ManageFoodLog(_currentDate);
                        break;
                    case "2":
                        _recipeManager.ManageRecipes();
                        break;
                    case "3":
                        _nutrientAnalysisManager.ViewNutrientAnalysis(_currentDate);
                        break;
                    case "4":
                        _userProfileManager.ManageUserProfile();
                        break;
                    case "5":
                        SetCurrentDate();
                        break;
                    case "6":
                        // Save changes before exiting
                        _dietManager.SaveChanges();
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private void SetCurrentDate()
        {
            Console.WriteLine("\n=== Set Current Date ===");
            Console.Write("Enter the date (yyyy-MM-dd): ");
            if (DateOnly.TryParse(Console.ReadLine(), out DateOnly newDate))
            {
                _currentDate = newDate;
                Console.WriteLine($"Current date set to {_currentDate}");
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
        }
    }
}