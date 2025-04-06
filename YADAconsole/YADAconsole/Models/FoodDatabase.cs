using System.Text.Json.Serialization;
using YADAconsole.Services;

namespace YADAconsole.Models
{
    public class FoodDatabase
    {
        public List<Food> Foods { get; set; } = new List<Food>();

        [JsonIgnore]
        private bool _isInitialized = false;

        public FoodDatabase()
        {
            // Only initialize sample foods if it's a new database
            if (Foods.Count == 0 && !_isInitialized)
            {
                InitializeSampleFoods();
                _isInitialized = true;
            }
        }

        private void InitializeSampleFoods()
        {
            // Try to load foods from basic_foods.json
            var persistenceService = new DataPersistenceService();
            var basicFoods = persistenceService.ConvertBasicFoodsDataToFoods();
            
            if (basicFoods.Count > 0)
            {
                // Use the loaded basic foods
                Foods.AddRange(basicFoods);
                Console.WriteLine($"Loaded {basicFoods.Count} foods from basic_foods.json");
            }
            else
            {
                // Fallback to hard-coded foods if loading fails
                var apple = new BasicFood("Apple")
                {
                    Keywords = new List<string> { "fruit", "fresh" },
                    Nutrients = new Nutrients
                    {
                        Calories = 52,
                        Carbohydrates = 14,
                        Protein = 0.3,
                        Fat = 0.2,
                        Micronutrients = new Dictionary<string, double>
                        {
                            { "Sugar", 10.4 },
                            { "Fiber", 2.4 }
                        }
                    },
                    ServingSize = 100
                };

                var chicken = new BasicFood("Chicken Breast")
                {
                    Keywords = new List<string> { "meat", "protein" },
                    Nutrients = new Nutrients
                    {
                        Calories = 165,
                        Carbohydrates = 0,
                        Protein = 31,
                        Fat = 3.6,
                        Micronutrients = new Dictionary<string, double>
                        {
                            { "Sugar", 0 },
                            { "Fiber", 0 }
                        }
                    },
                    ServingSize = 100
                };

                Foods.Add(apple);
                Foods.Add(chicken);
            }
        }

        public void AddFood(Food food)
        {
            Foods.Add(food);
        }

        public List<Food> SearchFood(string keyword)
        {
            string searchTerm = keyword.ToLower();
            return Foods.Where(f =>
                f.Name.ToLower().Contains(searchTerm) ||
                f.Keywords.Any(k => k.ToLower().Contains(searchTerm))
            ).ToList();
        }

        public void DisplayAllFoods()
        {
            Console.WriteLine("=== Food Database ===");
            for (int i = 0; i < Foods.Count; i++)
            {
                Console.WriteLine($"{i+1}. {Foods[i].Name}");
                // Show keywords to help users identify foods
                if (Foods[i].Keywords.Count > 0)
                {
                    Console.WriteLine($"   Keywords: {string.Join(", ", Foods[i].Keywords)}");
                }
            }
        }

        public void DisplayFoodList(List<Food> foods)
        {
            Console.WriteLine($"Found {foods.Count} items:");
            for (int i = 0; i < foods.Count; i++)
            {
                Console.WriteLine($"{i+1}. {foods[i].Name}");
                // Display keywords to help users see why this item matched
                if (foods[i].Keywords.Count > 0)
                {
                    Console.WriteLine($"   Keywords: {string.Join(", ", foods[i].Keywords)}");
                }
            }
        }
    }
}
