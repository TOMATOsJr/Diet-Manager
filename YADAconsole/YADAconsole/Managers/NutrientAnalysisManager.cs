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

            var balance = _dietManager.CalculateCalorieBalance(currentDate);
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
            if (_dietManager.UserLogs.ContainsKey(currentDate) &&
                _dietManager.UserLogs[currentDate].ConsumedFoods.Count > 0)
            {
                DailyLog log = _dietManager.UserLogs[currentDate];
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
    }
}