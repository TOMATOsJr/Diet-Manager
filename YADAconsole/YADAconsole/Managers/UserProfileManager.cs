using YADAconsole.Models;
using YADAconsole.Services;
using System;
using System.Linq;

namespace YADAconsole.Managers
{
    public class UserProfileManager
    {
        private readonly DietManager _dietManager;
        private readonly DataPersistenceService _persistenceService;

        public UserProfileManager(DietManager dietManager, DataPersistenceService persistenceService)
        {
            _dietManager = dietManager;
            _persistenceService = persistenceService;
        }

        public void ManageUserProfile()
        {
            Console.WriteLine("\n=== Manage User Profile ===");

            _dietManager.User.DisplayInfo();

            Console.WriteLine("\n1. Edit Name");
            Console.WriteLine("2. Edit Age");
            Console.WriteLine("3. Edit Gender");
            Console.WriteLine("4. Edit Height");
            Console.WriteLine("5. Edit Weight");
            Console.WriteLine("6. Edit Activity Level");
            Console.WriteLine("7. Select Calorie Calculation Method");
            if (_dietManager.User.CalorieCalculationMethod == "Katch-McArdle")
            {
                Console.WriteLine("8. Set Body Fat Percentage");
            }
            Console.WriteLine("9. Recalculate Target Calories");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1":
                    Console.Write("Enter new name: ");
                    string name = Console.ReadLine() ?? "";
                    if (!string.IsNullOrWhiteSpace(name))
                        _dietManager.User.Name = name;
                    break;
                case "2":
                    Console.Write("Enter age: ");
                    if (int.TryParse(Console.ReadLine(), out int age) && age > 0)
                        _dietManager.User.Age = age;
                    break;
                case "3":
                    Console.Write("Enter gender (Male/Female): ");
                    string gender = Console.ReadLine() ?? "";
                    if (gender.ToLower() == "male" || gender.ToLower() == "female")
                        _dietManager.User.Gender = gender;
                    break;
                case "4":
                    Console.Write("Enter height in cm: ");
                    if (double.TryParse(Console.ReadLine(), out double height) && height > 0)
                        _dietManager.User.Height = height;
                    break;
                case "5":
                    Console.Write("Enter weight in kg: ");
                    if (double.TryParse(Console.ReadLine(), out double weight) && weight > 0)
                        _dietManager.User.Weight = weight;
                    break;
                case "6":
                    Console.WriteLine("Activity levels: sedentary, light, moderate, active, very active");
                    Console.Write("Enter activity level: ");
                    string level = Console.ReadLine()?.ToLower() ?? "";
                    if (new[] { "sedentary", "light", "moderate", "active", "very active" }.Contains(level))
                        _dietManager.User.ActivityLevel = level;
                    break;
                case "7":
                    SelectCalorieCalculationMethod();
                    break;
                case "8":
                    if (_dietManager.User.CalorieCalculationMethod == "Katch-McArdle")
                    {
                        Console.Write("Enter body fat percentage (5-50): ");
                        if (double.TryParse(Console.ReadLine(), out double bodyFat) && bodyFat > 5 && bodyFat < 50)
                            _dietManager.User.BodyFatPercentage = bodyFat;
                    }
                    break;
                case "9":
                    _dietManager.User.CalculateTargetCalories();
                    Console.WriteLine($"Target calories recalculated: {_dietManager.User.TargetCalories} kcal using {_dietManager.User.CalorieCalculationMethod} method");
                    break;
                case "0":
                    // Return to main menu
                    return;
            }

            // Save user changes
            SaveUserChanges();
            
            // Show the profile manager menu again
            ManageUserProfile();
        }
        
        private void SelectCalorieCalculationMethod()
        {
            Console.WriteLine("\n=== Calorie Calculation Methods ===");
            Console.WriteLine("1. Harris-Benedict: Classic method that uses weight, height, and age");
            Console.WriteLine("2. Mifflin-St Jeor: More modern and accurate for most people");
            Console.WriteLine("3. Katch-McArdle: Uses lean body mass for more accurate results (requires body fat %)");
            Console.Write("Enter your choice: ");
            
            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1":
                    _dietManager.User.CalorieCalculationMethod = "Harris-Benedict";
                    Console.WriteLine("Set calculation method to Harris-Benedict");
                    break;
                case "2":
                    _dietManager.User.CalorieCalculationMethod = "Mifflin-St Jeor";
                    Console.WriteLine("Set calculation method to Mifflin-St Jeor");
                    break;
                case "3":
                    _dietManager.User.CalorieCalculationMethod = "Katch-McArdle";
                    Console.WriteLine("Set calculation method to Katch-McArdle");
                    Console.WriteLine("Note: This method requires your body fat percentage for accurate results.");
                    Console.Write("Enter your body fat percentage (5-50): ");
                    if (double.TryParse(Console.ReadLine(), out double bodyFat) && bodyFat > 5 && bodyFat < 50)
                        _dietManager.User.BodyFatPercentage = bodyFat;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Keeping current method.");
                    break;
            }
        }

        private void SaveUserChanges()
        {
            var users = _persistenceService.LoadUsers();
            var index = users.FindIndex(u => u.Id == _dietManager.User.Id);
            if (index >= 0)
            {
                users[index] = _dietManager.User;
                _persistenceService.SaveUsers(users);
            }
        }
    }
}