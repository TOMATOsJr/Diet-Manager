using YADAconsole.Models;

namespace YADAconsole.Managers
{
    public class FoodLogManager
    {
        private readonly DietManager _dietManager;

        public FoodLogManager(DietManager dietManager)
        {
            _dietManager = dietManager;
        }

        public void ManageFoodLog(DateOnly currentDate)
        {
            Console.WriteLine("\n=== Manage Food Log ===");

            DailyLog log = _dietManager.GetOrCreateDailyLog(currentDate);
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
            _dietManager.SaveChanges();
        }

        private void AddFoodToLog(DailyLog log)
        {
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

        private void RemoveFoodFromLog(DailyLog log)
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
    }
}