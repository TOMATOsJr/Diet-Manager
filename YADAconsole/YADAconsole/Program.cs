using YADAconsole.Models;
using YADAconsole.Services;

namespace YADAconsole
{
    class Program
    {
        private static DietManager? _dietManager;
        private static DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Today);
        private static DataPersistenceService _persistenceService = new DataPersistenceService();
        private static AuthenticationService _authService = new AuthenticationService(_persistenceService);

        static void Main(string[] args)
        {
            Console.WriteLine("=== YADA Diet Manager ===");
            Console.WriteLine("Welcome to Your Adaptive Diet Assistant");

            // Login/registration with proper authentication
            User? user = LoginOrRegister();
            if (user == null)
            {
                Console.WriteLine("Login failed. Exiting application.");
                return;
            }

            _dietManager = new DietManager(user, _persistenceService);

            bool exit = false;
            while (!exit)
            {
                DisplayMainMenu();
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        ManageFoodLog();
                        break;
                    case "2":
                        ManageRecipes();
                        break;
                    case "3":
                        ViewNutrientAnalysis();
                        break;
                    case "4":
                        ManageUserProfile();
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

            Console.WriteLine("Thank you for using YADA Diet Manager. Goodbye!");
        }

        static User? LoginOrRegister()
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

        static User? Login()
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

        static void Register()
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

        static string ReadPassword()
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

        static void DisplayMainMenu()
        {
            Console.WriteLine("\n=== Main Menu ===");
            Console.WriteLine("1. Manage Food Log");
            Console.WriteLine("2. Manage Recipes");
            Console.WriteLine("3. View Nutrient Analysis");
            Console.WriteLine("4. Manage User Profile");
            Console.WriteLine("5. Set Current Date");
            Console.WriteLine("6. Exit");
            Console.Write("Enter your choice: ");
        }

        static void ManageFoodLog()
        {
            Console.WriteLine("\n=== Manage Food Log ===");
            if (_dietManager == null) return;

            DailyLog log = _dietManager.GetOrCreateDailyLog(_currentDate);
            log.DisplayLog();

            Console.WriteLine("\n1. Add Food");
            Console.WriteLine("2. Remove Food");
            Console.WriteLine("3. Undo Last Action");
            Console.WriteLine("4. Redo Last Action");
            Console.WriteLine("5. Back to Main Menu");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    AddFoodToLog(log);
                    break;
                case "2":
                    RemoveFoodFromLog(log);
                    break;
                case "3":
                    if (log.UndoAction()) Console.WriteLine("Undo successful.");
                    else Console.WriteLine("Nothing to undo.");
                    break;
                case "4":
                    if (log.RedoAction()) Console.WriteLine("Redo successful.");
                    else Console.WriteLine("Nothing to redo.");
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            // Save changes
            _dietManager?.SaveChanges();
        }

        static void AddFoodToLog(DailyLog log)
        {
            if (_dietManager == null) return;

            Console.WriteLine("\n=== Add Food to Log ===");
            _dietManager.Database.DisplayAllFoods();
            Console.Write("Enter the number of the food to add: ");
            if (int.TryParse(Console.ReadLine(), out int foodIndex) && foodIndex > 0 && foodIndex <= _dietManager.Database.Foods.Count)
            {
                Food food = _dietManager.Database.Foods[foodIndex - 1];
                Console.Write("Enter the number of servings: ");
                if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                {
                    log.AddFood(food, servings);
                    Console.WriteLine("Food added successfully.");
                }
                else
                {
                    Console.WriteLine("Invalid servings.");
                }
            }
            else
            {
                Console.WriteLine("Invalid food selection.");
            }
        }

        static void RemoveFoodFromLog(DailyLog log)
        {
            Console.WriteLine("\n=== Remove Food from Log ===");
            log.DisplayLog();
            Console.Write("Enter the number of the food to remove: ");
            if (int.TryParse(Console.ReadLine(), out int foodIndex) && foodIndex > 0 && foodIndex <= log.ConsumedFoods.Count)
            {
                log.DeleteFood(foodIndex - 1);
                Console.WriteLine("Food removed successfully.");
            }
            else
            {
                Console.WriteLine("Invalid food selection.");
            }
        }

        static void ManageRecipes()
        {
            Console.WriteLine("\n=== Manage Recipes ===");
            if (_dietManager == null) return;

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("1. Create New Recipe");
                Console.WriteLine("2. View Existing Recipes");
                Console.WriteLine("3. Edit Recipe");
                Console.WriteLine("4. Delete Recipe");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        CreateNewRecipe();
                        break;
                    case "2":
                        ViewRecipes();
                        break;
                    case "3":
                        EditRecipe();
                        break;
                    case "4":
                        DeleteRecipe();
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }

            // Save changes when exiting
            _dietManager?.SaveChanges();
        }

        static void CreateNewRecipe()
        {
            if (_dietManager == null) return;

            Console.Write("Enter recipe name: ");
            string name = Console.ReadLine() ?? "New Recipe";

            var recipe = new CompositeFood(name);

            bool addingIngredients = true;
            while (addingIngredients)
            {
                Console.WriteLine("\nAvailable ingredients:");
                _dietManager.Database.DisplayAllFoods();

                Console.Write("Enter ingredient number (or 0 to finish): ");
                if (int.TryParse(Console.ReadLine(), out int ingredientIndex))
                {
                    if (ingredientIndex == 0)
                    {
                        addingIngredients = false;
                    }
                    else if (ingredientIndex > 0 && ingredientIndex <= _dietManager.Database.Foods.Count)
                    {
                        Console.Write("Enter number of servings: ");
                        if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                        {
                            recipe.AddIngredient(_dietManager.Database.Foods[ingredientIndex - 1], servings);
                            Console.WriteLine("Ingredient added successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid servings. Please enter a positive number.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }

            Console.Write("Enter keywords (comma separated): ");
            string keywordInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(keywordInput))
            {
                recipe.Keywords = keywordInput.Split(',').Select(k => k.Trim()).ToList();
            }

            _dietManager.Database.AddFood(recipe);
            Console.WriteLine("Recipe created and added to food database.");
            recipe.DisplayInfo();
        }

        static void ViewRecipes()
        {
            if (_dietManager == null) return;

            Console.WriteLine("\n=== Recipes ===");
            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();

            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes found.");
                return;
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to view details (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                recipes[recipeIndex - 1].DisplayInfo();
            }
        }

        static void EditRecipe()
        {
            if (_dietManager == null) return;

            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes to edit.");
                return;
            }

            Console.WriteLine("\n=== Edit Recipe ===");
            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to edit (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                CompositeFood recipe = recipes[recipeIndex - 1];
                recipe.DisplayInfo();

                Console.WriteLine("\n1. Add ingredient");
                Console.WriteLine("2. Remove ingredient");
                Console.WriteLine("3. Edit name");
                Console.WriteLine("4. Return");
                Console.Write("Enter choice: ");

                string choice = Console.ReadLine() ?? "";
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nAvailable ingredients:");
                        _dietManager.Database.DisplayAllFoods();
                        Console.Write("Enter ingredient number: ");

                        if (int.TryParse(Console.ReadLine(), out int ingredientIndex) &&
                            ingredientIndex > 0 &&
                            ingredientIndex <= _dietManager.Database.Foods.Count)
                        {
                            Console.Write("Enter servings: ");
                            if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                            {
                                recipe.AddIngredient(_dietManager.Database.Foods[ingredientIndex - 1], servings);
                                Console.WriteLine("Ingredient added.");
                            }
                        }
                        break;
                    case "2":
                        if (recipe.Ingredients.Count == 0)
                        {
                            Console.WriteLine("No ingredients to remove.");
                            return;
                        }

                        Console.Write("Enter ingredient number to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int removeIndex) &&
                            removeIndex > 0 &&
                            removeIndex <= recipe.Ingredients.Count)
                        {
                            recipe.RemoveIngredient(removeIndex - 1);
                            Console.WriteLine("Ingredient removed.");
                        }
                        break;
                    case "3":
                        Console.Write("Enter new name: ");
                        string newName = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            recipe.Name = newName;
                            Console.WriteLine("Name updated.");
                        }
                        break;
                }
            }
        }

        static void DeleteRecipe()
        {
            if (_dietManager == null) return;

            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes to delete.");
                return;
            }

            Console.WriteLine("\n=== Delete Recipe ===");
            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to delete (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                CompositeFood recipe = recipes[recipeIndex - 1];
                Console.Write($"Are you sure you want to delete '{recipe.Name}'? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    int index = _dietManager.Database.Foods.FindIndex(f => f == recipe);
                    if (index >= 0)
                    {
                        _dietManager.Database.Foods.RemoveAt(index);
                        Console.WriteLine("Recipe deleted successfully.");
                    }
                }
            }
        }

        static void ViewNutrientAnalysis()
        {
            Console.WriteLine("\n=== Nutrient Analysis ===");
            if (_dietManager == null) return;

            var balance = _dietManager.CalculateCalorieBalance(_currentDate);
            Console.WriteLine($"Calories Consumed: {balance["consumed"]} kcal");
            Console.WriteLine($"Target Calories: {balance["target"]} kcal");
            Console.WriteLine($"Calorie Balance: {balance["balance"]} kcal");

            if (balance["balance"] < 0)
            {
                Console.WriteLine("You are under your calorie target. Consider eating more.");

                // Show food suggestions
                Console.WriteLine("\nSuggested foods to increase intake:");
                List<Food> suggestions = _dietManager.GetFoodSuggestions();
                foreach (var food in suggestions)
                {
                    Console.WriteLine($"- {food.Name}: {food.Nutrients.Calories} kcal per serving");
                }
            }
            else if (balance["balance"] > 0)
            {
                Console.WriteLine("You are over your calorie target. Consider eating less.");
            }
            else
            {
                Console.WriteLine("You are on track with your calorie target.");
            }

            // Show macro distribution
            if (_dietManager.UserLogs.ContainsKey(_currentDate) &&
                _dietManager.UserLogs[_currentDate].ConsumedFoods.Count > 0)
            {
                DailyLog log = _dietManager.UserLogs[_currentDate];
                double totalProtein = 0;
                double totalCarbs = 0;
                double totalFat = 0;

                foreach (var (food, servings) in log.ConsumedFoods)
                {
                    totalProtein += food.Nutrients.Protein * servings;
                    totalCarbs += food.Nutrients.Carbohydrates * servings;
                    totalFat += food.Nutrients.Fat * servings;
                }

                double totalCalories = balance["consumed"];

                if (totalCalories > 0)
                {
                    double proteinPercentage = (totalProtein * 4) / totalCalories * 100;
                    double carbPercentage = (totalCarbs * 4) / totalCalories * 100;
                    double fatPercentage = (totalFat * 9) / totalCalories * 100;

                    Console.WriteLine("\nMacronutrient Distribution:");
                    Console.WriteLine($"Protein: {proteinPercentage:F1}% ({totalProtein:F1}g)");
                    Console.WriteLine($"Carbohydrates: {carbPercentage:F1}% ({totalCarbs:F1}g)");
                    Console.WriteLine($"Fat: {fatPercentage:F1}% ({totalFat:F1}g)");
                }
            }
        }

        static void ManageUserProfile()
        {
            Console.WriteLine("\n=== Manage User Profile ===");
            if (_dietManager == null) return;

            _dietManager.User.DisplayInfo();

            Console.WriteLine("\n1. Edit Name");
            Console.WriteLine("2. Edit Age");
            Console.WriteLine("3. Edit Gender");
            Console.WriteLine("4. Edit Height");
            Console.WriteLine("5. Edit Weight");
            Console.WriteLine("6. Edit Activity Level");
            Console.WriteLine("7. Recalculate Target Calories");
            Console.WriteLine("8. Back to Main Menu");
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
                    _dietManager.User.CalculateTargetCalories();
                    Console.WriteLine($"Target calories recalculated: {_dietManager.User.TargetCalories} kcal");
                    break;
            }

            // Save user changes
            var users = _persistenceService.LoadUsers();
            var index = users.FindIndex(u => u.Id == _dietManager?.User.Id);
            if (index >= 0)
            {
                users[index] = _dietManager!.User;
                _persistenceService.SaveUsers(users);
            }
        }

        static void SetCurrentDate()
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
