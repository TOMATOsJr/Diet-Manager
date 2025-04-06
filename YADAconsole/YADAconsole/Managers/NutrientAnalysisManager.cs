using YADAconsole.Models;
using System;
using System.Collections.Generic;

namespace YADAconsole.Managers
{
    public class NutrientAnalysisManager
    {
        private readonly DietManager _dietManager;

        public NutrientAnalysisManager(DietManager dietManager)
        {
            _dietManager = dietManager;
        }

        public void ViewNutrientAnalysis(DateOnly currentDate)
        {
            Console.WriteLine("\n=== Nutrient Analysis ===");
            Console.WriteLine($"Analysis for: {currentDate.ToString("yyyy-MM-dd")}");

            // Check if a log exists for this date without automatically creating one
            bool logExists = _dietManager.HasLogForDate(currentDate);
            DailyLog log = logExists ? _dietManager.GetExistingDailyLog(currentDate) : null;
            
            // Calculate calorie target based on the metrics for that specific date if available
            string calculationMethod = _dietManager.User.CalorieCalculationMethod;
            int calculatedTarget;
            
            if (logExists && log != null)
            {
                calculatedTarget = CalculateTargetForDate(log, calculationMethod);
            }
            else
            {
                // Use current user profile if no log for this date
                calculatedTarget = _dietManager.User.TargetCalories;
            }
            
            // Calculate actual consumption
            var balance = _dietManager.CalculateCalorieBalance(currentDate);
            Console.WriteLine($"Calories Consumed: {balance["consumed"]} kcal");
            Console.WriteLine($"Target Calories: {calculatedTarget} kcal");
            Console.WriteLine($"Calorie Balance: {balance["consumed"] - calculatedTarget} kcal");

            // Display which calculation method was used (from user profile)
            Console.WriteLine($"Calculation Method: {calculationMethod}");
            
            // Display the user metrics used for this day
            ShowUserMetricsForDate(log);

            // Check if under/over target and provide feedback
            if (balance["consumed"] < calculatedTarget)
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
            else if (balance["consumed"] > calculatedTarget)
            {
                Console.WriteLine("You are over your calorie target. Consider eating less.");
            }
            else
            {
                Console.WriteLine("You are on track with your calorie target.");
            }

            // Show macro distribution
            if (_dietManager.UserLogs.ContainsKey(currentDate) &&
                _dietManager.UserLogs[currentDate].ConsumedFoodItems.Count > 0)
            {
                double totalProtein = 0;
                double totalCarbs = 0;
                double totalFat = 0;

                // Use ConsumedFoodItems which returns tuples that can be deconstructed
                foreach (var (food, servings) in log.ConsumedFoodItems)
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
        
        private int CalculateTargetForDate(DailyLog log, string method)
        {
            // Create a temporary user with the day's metrics but user's calculation method
            var tempUser = new User
            {
                Gender = _dietManager.User.Gender,
                Age = log.Age > 0 ? log.Age : _dietManager.User.Age,
                Weight = log.Weight > 0 ? log.Weight : _dietManager.User.Weight,
                Height = log.Height > 0 ? log.Height : _dietManager.User.Height,
                ActivityLevel = !string.IsNullOrEmpty(log.ActivityLevel) ? log.ActivityLevel : _dietManager.User.ActivityLevel,
                BodyFatPercentage = _dietManager.User.BodyFatPercentage,
                CalorieCalculationMethod = method // Always use user's method
            };
            
            // Explicitly pass the method to ensure it's used
            return (int)tempUser.CalculateTargetCalories(method);
        }
        
        private void ShowUserMetricsForDate(DailyLog log)
        {
            Console.WriteLine("\nUser Metrics Used for Calculation:");
            
            // Check if we have a log at all
            if (log == null)
            {
                Console.WriteLine("Weight: data not available");
                Console.WriteLine("Height: data not available");
                Console.WriteLine("Age: data not available");
                Console.WriteLine("Activity level: data not available");
                return;
            }
            
            // Display the metrics that exist in the log, or "data not available" if missing
            if (log.Weight > 0)
                Console.WriteLine($"Weight: {log.Weight} kg");
            else
                Console.WriteLine("Weight: data not available");
                
            if (log.Height > 0)
                Console.WriteLine($"Height: {log.Height} cm");
            else
                Console.WriteLine("Height: data not available");
                
            if (log.Age > 0)
                Console.WriteLine($"Age: {log.Age}");
            else
                Console.WriteLine("Age: data not available");
                
            if (!string.IsNullOrEmpty(log.ActivityLevel))
                Console.WriteLine($"Activity level: {log.ActivityLevel}");
            else
                Console.WriteLine("Activity level: data not available");
        }
    }
}