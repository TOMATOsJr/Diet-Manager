using System.Text.Json.Serialization;

namespace YADAconsole.Models
{
    public class DailyLog
    {
        public DateOnly Date { get; set; }
        
        [JsonIgnore]
        private List<(Food Food, double Servings)> _consumedFoods = new();
        
        public List<FoodConsumption> ConsumedFoods { get; set; } = new();
        
        // User metrics for this specific day
        public double Weight { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public string ActivityLevel { get; set; } = "";
        // CalorieCalculationMethod removed - will always use the user's method

        // For undo/redo functionality
        [JsonIgnore]
        private Stack<List<(Food, double)>> UndoStack { get; set; } = new();
        
        [JsonIgnore]
        private Stack<List<(Food, double)>> RedoStack { get; set; } = new();

        public DailyLog(DateOnly date)
        {
            Date = date;
            // Don't set default ActivityLevel
        }
        
        [JsonIgnore]
        public IReadOnlyList<(Food Food, double Servings)> ConsumedFoodItems => _consumedFoods;

        public void AddFood(Food food, double servings)
        {
            // Save current state for undo
            UndoStack.Push(new List<(Food, double)>(_consumedFoods));
            RedoStack.Clear();

            _consumedFoods.Add((food, servings));
            
            // Update the serializable list - only store ID and servings
            ConsumedFoods.Add(new FoodConsumption(food, servings));
        }

        public void DeleteFood(int index)
        {
            if (index >= 0 && index < _consumedFoods.Count)
            {
                // Save current state for undo
                UndoStack.Push(new List<(Food, double)>(_consumedFoods));
                RedoStack.Clear();

                _consumedFoods.RemoveAt(index);
                if (index < ConsumedFoods.Count)
                    ConsumedFoods.RemoveAt(index);
            }
        }

        public bool UndoAction()
        {
            if (UndoStack.Count > 0)
            {
                RedoStack.Push(new List<(Food, double)>(_consumedFoods));
                _consumedFoods = UndoStack.Pop();
                
                // Update the serializable list
                ConsumedFoods.Clear();
                foreach (var (food, servings) in _consumedFoods)
                {
                    ConsumedFoods.Add(new FoodConsumption(food, servings));
                }
                
                return true;
            }
            return false;
        }

        public bool RedoAction()
        {
            if (RedoStack.Count > 0)
            {
                UndoStack.Push(new List<(Food, double)>(_consumedFoods));
                _consumedFoods = RedoStack.Pop();
                
                // Update the serializable list
                ConsumedFoods.Clear();
                foreach (var (food, servings) in _consumedFoods)
                {
                    ConsumedFoods.Add(new FoodConsumption(food, servings));
                }
                
                return true;
            }
            return false;
        }

        public void DisplayLog()
        {
            Console.WriteLine($"=== Daily Log: {Date.ToString("yyyy-MM-dd")} ===");
            
            // Display metrics only if they are available
            if (!string.IsNullOrEmpty(ActivityLevel))
                Console.WriteLine($"Activity Level: {ActivityLevel}");
            else
                Console.WriteLine("Activity Level: data not available");
            
            if (Weight > 0)
                Console.WriteLine($"Weight: {Weight} kg");
            else
                Console.WriteLine("Weight: data not available");
            
            if (Height > 0)
                Console.WriteLine($"Height: {Height} cm");
            else
                Console.WriteLine("Height: data not available");
            
            if (Age > 0)
                Console.WriteLine($"Age: {Age}");
            else
                Console.WriteLine("Age: data not available");

            if (_consumedFoods.Count == 0)
            {
                Console.WriteLine("No foods logged for this day.");
                return;
            }

            Console.WriteLine("Consumed Foods:");
            int i = 1;
            Nutrients totalNutrients = new Nutrients();

            foreach (var (food, servings) in _consumedFoods)
            {
                Console.WriteLine($"{i}. {food.Name} - {servings} serving(s)");

                // Sum up nutrients
                totalNutrients.Calories += food.Nutrients.Calories * servings;
                totalNutrients.Protein += food.Nutrients.Protein * servings;
                totalNutrients.Carbohydrates += food.Nutrients.Carbohydrates * servings;
                totalNutrients.Fat += food.Nutrients.Fat * servings;

                i++;
            }

            Console.WriteLine("\nDaily Totals:");
            Console.WriteLine(totalNutrients.ToString());
        }
        
        // Initialize this DailyLog with the current user's information - now only used when explicitly adding metrics
        public void InitializeWithUserInfo(User user)
        {
            // No default values are set - only explicit assignments
        }
        
        // Called after deserialization to rebuild the in-memory food list from IDs
        public void SyncFoodLists(FoodDatabase foodDatabase) 
        {
            _consumedFoods.Clear();
            
            foreach (var item in ConsumedFoods)
            {
                // Find the food in the database by ID
                var food = foodDatabase.Foods.FirstOrDefault(f => 
                    GetFoodIdentifier(f) == item.FoodId);
                
                if (food != null)
                {
                    _consumedFoods.Add((food, item.Servings));
                }
                else
                {
                    // If food not found, create a placeholder
                    string foodName = item.FoodId.Contains("-") ? 
                        item.FoodId.Split('-')[1] : item.FoodId;
                    
                    var placeholderFood = new BasicFood(foodName)
                    {
                        ServingSize = item.ServingSize
                    };
                    _consumedFoods.Add((placeholderFood, item.Servings));
                    
                    Console.WriteLine($"Warning: Food '{foodName}' not found in database.");
                }
            }
            
            // Clear both stacks since they might contain references to foods that no longer exist
            UndoStack.Clear();
            RedoStack.Clear();
        }
        
        // Helper to generate consistent food identifiers
        private string GetFoodIdentifier(Food food)
        {
            // Use type name and food name as identifier
            return food.GetType().Name + "-" + food.Name;
        }
    }
}
