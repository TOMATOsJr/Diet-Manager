namespace YADAconsole.Models
{
    public class CompositeFood : Food
    {
        public List<(Food Food, double Servings)> Ingredients { get; private set; }
        
        // Creator user ID to track ownership
        public string CreatorId { get; set; } = "";

        public CompositeFood(string name) : base(name)
        {
            Ingredients = new List<(Food, double)>();
        }

        public void AddIngredient(Food food, double servings = 1.0)
        {
            Ingredients.Add((food, servings));
            CalculateNutrients();
        }

        public void EditIngredient(int index, Food food, double servings)
        {
            if (index >= 0 && index < Ingredients.Count)
            {
                Ingredients[index] = (food, servings);
                CalculateNutrients();
            }
        }

        public void RemoveIngredient(int index)
        {
            if (index >= 0 && index < Ingredients.Count)
            {
                Ingredients.RemoveAt(index);
                CalculateNutrients();
            }
        }

        public void CalculateNutrients()
        {
            Nutrients = new Nutrients();

            foreach (var (ingredient, servings) in Ingredients)
            {
                // Scale the nutrients based on serving size
                Nutrients.Calories += ingredient.Nutrients.Calories * servings;
                Nutrients.Protein += ingredient.Nutrients.Protein * servings;
                Nutrients.Carbohydrates += ingredient.Nutrients.Carbohydrates * servings;
                Nutrients.Fat += ingredient.Nutrients.Fat * servings;

                foreach (var micronutrient in ingredient.Nutrients.Micronutrients)
                {
                    if (!Nutrients.Micronutrients.ContainsKey(micronutrient.Key))
                    {
                        Nutrients.Micronutrients[micronutrient.Key] = 0;
                    }
                    Nutrients.Micronutrients[micronutrient.Key] += micronutrient.Value * servings;
                }
            }
        }

        // Helper method to get recipe ID from keywords
        public int GetRecipeId()
        {
            foreach (var keyword in Keywords)
            {
                if (keyword.StartsWith("__RECIPE_ID_") && keyword.EndsWith("__"))
                {
                    string idStr = keyword.Replace("__RECIPE_ID_", "").Replace("__", "");
                    if (int.TryParse(idStr, out int id))
                        return id;
                }
            }
            return -1;
        }

        // Helper method to get creator user ID from keywords
        public string GetCreatorId()
        {
            if (!string.IsNullOrEmpty(CreatorId))
                return CreatorId;
                
            foreach (var keyword in Keywords)
            {
                if (keyword.StartsWith("__USERID_") && keyword.EndsWith("__"))
                {
                    CreatorId = keyword.Replace("__USERID_", "").Replace("__", "");
                    return CreatorId;
                }
            }
            return "";
        }

        // Helper to check if current user is the owner
        public bool IsOwnedBy(string userId)
        {
            return GetCreatorId() == userId;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("=== RECIPE ===");
            Console.WriteLine($"Recipe ID: {Id}");
            base.DisplayInfo();
            Console.WriteLine("Ingredients:");
            for (int i = 0; i < Ingredients.Count; i++)
            {
                Console.WriteLine($"{i+1}. {Ingredients[i].Food.Name} - {Ingredients[i].Servings} serving(s)");
            }

            // Don't display metadata keywords
            var displayKeywords = Keywords.Where(k => 
                !k.StartsWith("__RECIPE_ID_") && 
                !k.StartsWith("__USERID_")).ToList();

            if (displayKeywords.Any())
            {
                Console.WriteLine($"Keywords: {string.Join(", ", displayKeywords)}");
            }
        }
    }
}
