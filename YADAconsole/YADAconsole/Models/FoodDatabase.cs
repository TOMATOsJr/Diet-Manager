using System.Text.Json.Serialization;

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
            // Add some sample basic foods
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
                        { "Vitamin C", 4.6 },
                        { "Potassium", 107 }
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
                        { "Vitamin B6", 0.6 },
                        { "Niacin", 13.7 }
                    }
                },
                ServingSize = 100
            };

            Foods.Add(apple);
            Foods.Add(chicken);
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
            }
        }
    }
}
