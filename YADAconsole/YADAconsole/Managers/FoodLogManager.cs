using YADAconsole.Models;

namespace YADAconsole.Managers
{
    public class FoodLogManager
    {
        private readonly DietManager _dietManager;
        private bool _hasUnsavedChanges = false;

        public FoodLogManager(DietManager dietManager)
        {
            _dietManager = dietManager;
        }

        public void ManageFoodLog(DateOnly currentDate)
        {
            // Reset unsaved changes flag when first entering food log management
            _hasUnsavedChanges = false;

            Console.WriteLine("\n=== Manage Food Log ===");

            // Check if we have a log for this date without creating one
            DailyLog log;
            bool logExists = _dietManager.HasLogForDate(currentDate);
            
            if (logExists)
            {
                log = _dietManager.GetExistingDailyLog(currentDate);
                log.DisplayLog();
            }
            else
            {
                Console.WriteLine($"No log exists for {currentDate.ToString("yyyy-MM-dd")}.");
                Console.WriteLine("Weight: data not available");
                Console.WriteLine("Height: data not available");
                Console.WriteLine("Age: data not available");
                Console.WriteLine("Activity Level: data not available");
                Console.WriteLine("No foods logged for this day.");
                
                // Create log only if user wants to add data
                log = new DailyLog(currentDate);
            }

            Console.WriteLine("\n1. Add Food");
            Console.WriteLine("2. Remove Food");
            Console.WriteLine("3. Update Daily Metrics (Weight, Activity)");
            Console.WriteLine("4. Undo Last Action");
            Console.WriteLine("5. Redo Last Action");
            Console.WriteLine("6. Save Changes to Database");
            Console.WriteLine("7. Back to Main Menu");
            
            if (_hasUnsavedChanges)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[*] You have unsaved changes");
                Console.ResetColor();
            }
            
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
                    UpdateDailyMetrics(log);
                    break;
                case "4":
                    if (log.UndoAction()) 
                    {
                        Console.WriteLine("Undo successful.");
                        _hasUnsavedChanges = true;
                    }
                    else Console.WriteLine("Nothing to undo.");
                    break;
                case "5":
                    if (log.RedoAction()) 
                    {
                        Console.WriteLine("Redo successful.");
                        _hasUnsavedChanges = true;
                    }
                    else Console.WriteLine("Nothing to redo.");
                    break;
                case "6":
                    SaveChangesToDatabase();
                    break;
                case "7":
                    if (_hasUnsavedChanges)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("You have unsaved changes. Save before exiting? (y/n): ");
                        Console.ResetColor();
                        if ((Console.ReadLine() ?? "").ToLower() == "y")
                        {
                            SaveChangesToDatabase();
                        }
                        else
                        {
                            Console.WriteLine("Changes will not be saved to database.");
                        }
                    }
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            // Continue showing the menu
            ContinueFoodLogManagement(currentDate);
        }

        // Separate method to continue food log management without resetting the unsaved changes flag
        private void ContinueFoodLogManagement(DateOnly currentDate)
        {
            Console.WriteLine("\n=== Manage Food Log ===");

            DailyLog log = _dietManager.GetOrCreateDailyLog(currentDate);
            log.DisplayLog();

            Console.WriteLine("\n1. Add Food");
            Console.WriteLine("2. Remove Food");
            Console.WriteLine("3. Update Daily Metrics (Weight, Activity)");
            Console.WriteLine("4. Undo Last Action");
            Console.WriteLine("5. Redo Last Action");
            Console.WriteLine("6. Save Changes to Database");
            Console.WriteLine("7. Back to Main Menu");
            
            if (_hasUnsavedChanges)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[*] You have unsaved changes");
                Console.ResetColor();
            }
            
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
                    UpdateDailyMetrics(log);
                    break;
                case "4":
                    if (log.UndoAction()) 
                    {
                        Console.WriteLine("Undo successful.");
                        _hasUnsavedChanges = true;
                    }
                    else Console.WriteLine("Nothing to undo.");
                    break;
                case "5":
                    if (log.RedoAction()) 
                    {
                        Console.WriteLine("Redo successful.");
                        _hasUnsavedChanges = true;
                    }
                    else Console.WriteLine("Nothing to redo.");
                    break;
                case "6":
                    SaveChangesToDatabase();
                    break;
                case "7":
                    if (_hasUnsavedChanges)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("You have unsaved changes. Save before exiting? (y/n): ");
                        Console.ResetColor();
                        if ((Console.ReadLine() ?? "").ToLower() == "y")
                        {
                            SaveChangesToDatabase();
                        }
                        else
                        {
                            Console.WriteLine("Changes will not be saved to database.");
                        }
                    }
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            // Continue showing the menu
            ContinueFoodLogManagement(currentDate);
        }

        private void AddFoodToLog(DailyLog log)
        {
            Console.WriteLine("\n=== Add Food to Log ===");
            
            // Add keyword search options
            Console.WriteLine("1. Show all foods");
            Console.WriteLine("2. Search by keyword");
            Console.Write("Enter choice: ");
            
            string choice = Console.ReadLine() ?? "1";
            
            List<Food> foodOptions;
            
            if (choice == "2")
            {
                Console.Write("Enter search keyword: ");
                string keyword = Console.ReadLine() ?? "";
                foodOptions = _dietManager.Database.SearchFood(keyword);
                
                if (foodOptions.Count == 0)
                {
                    Console.WriteLine("No foods found matching that keyword.");
                    return;
                }
                
                Console.WriteLine("\nSearch results:");
                _dietManager.Database.DisplayFoodList(foodOptions);
            }
            else
            {
                // Show all foods
                _dietManager.Database.DisplayAllFoods();
                foodOptions = _dietManager.Database.Foods;
            }
            
            Console.Write("Enter the number of the food to add: ");
            if (int.TryParse(Console.ReadLine(), out int foodIndex) && foodIndex > 0 && foodIndex <= foodOptions.Count)
            {
                Food food = foodOptions[foodIndex - 1];
                Console.Write("Enter the number of servings: ");
                if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                {
                    log.AddFood(food, servings);
                    Console.WriteLine("Food added successfully to in-memory log.");
                    _hasUnsavedChanges = true;
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

        private void RemoveFoodFromLog(DailyLog log)
        {
            Console.WriteLine("\n=== Remove Food from Log ===");
            log.DisplayLog();
            Console.Write("Enter the number of the food to remove: ");
            if (int.TryParse(Console.ReadLine(), out int foodIndex) && foodIndex > 0 && foodIndex <= log.ConsumedFoodItems.Count)
            {
                log.DeleteFood(foodIndex - 1);
                Console.WriteLine("Food removed successfully from in-memory log.");
                _hasUnsavedChanges = true;
            }
            else
            {
                Console.WriteLine("Invalid food selection.");
            }
        }

        private void UpdateDailyMetrics(DailyLog log)
        {
            Console.WriteLine("\n=== Update Daily Metrics ===");
            
            Console.Write("Enter weight for this day (kg) or leave blank to skip: ");
            string weightInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(weightInput) && double.TryParse(weightInput, out double weight) && weight > 0)
            {
                log.Weight = weight;
                _hasUnsavedChanges = true;
                Console.WriteLine($"Weight updated to {weight} kg for {log.Date}");
            }
            else if (!string.IsNullOrWhiteSpace(weightInput))
            {
                Console.WriteLine("Invalid weight value. Please enter a positive number.");
            }
            
            // Similar pattern for other metrics
            Console.Write("Enter height for this day (cm) or leave blank to skip: ");
            string heightInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(heightInput) && double.TryParse(heightInput, out double height) && height > 0)
            {
                log.Height = height;
                _hasUnsavedChanges = true;
                Console.WriteLine($"Height updated to {height} cm for {log.Date}");
            }
            else if (!string.IsNullOrWhiteSpace(heightInput))
            {
                Console.WriteLine("Invalid height value. Please enter a positive number.");
            }
            
            Console.Write("Enter age for this day or leave blank to skip: ");
            string ageInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(ageInput) && int.TryParse(ageInput, out int age) && age > 0)
            {
                log.Age = age;
                _hasUnsavedChanges = true;
                Console.WriteLine($"Age updated to {age} for {log.Date}");
            }
            else if (!string.IsNullOrWhiteSpace(ageInput))
            {
                Console.WriteLine("Invalid age value. Please enter a positive number.");
            }
            
            Console.WriteLine("Activity levels: sedentary, light, moderate, active, very active");
            Console.Write("Enter activity level for this day or leave blank to skip: ");
            string activityLevel = Console.ReadLine()?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(activityLevel) && 
                new[] { "sedentary", "light", "moderate", "active", "very active" }.Contains(activityLevel))
            {
                log.ActivityLevel = activityLevel;
                _hasUnsavedChanges = true;
                Console.WriteLine($"Activity level updated to {activityLevel} for {log.Date}");
            }
            else if (!string.IsNullOrWhiteSpace(activityLevel))
            {
                Console.WriteLine("Invalid activity level. Please choose from the provided options.");
            }
            
            // Add this log to the UserLogs if it doesn't exist yet
            if (!_dietManager.HasLogForDate(log.Date))
            {
                _dietManager.UserLogs[log.Date] = log;
            }
        }

        private void SaveChangesToDatabase()
        {
            _dietManager.SaveChanges();
            _hasUnsavedChanges = false;
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Changes successfully saved to database.");
            Console.ResetColor();
        }
    }
}