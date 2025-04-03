namespace YADAconsole.Models
{
    public class CompositeFood : Food
    {
        public List<(Food Food, double Servings)> Ingredients { get; private set; }

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

        public override void DisplayInfo()
        {
            Console.WriteLine("=== COMPOSITE FOOD RECIPE ===");
            base.DisplayInfo();
            Console.WriteLine("Ingredients:");
            for (int i = 0; i < Ingredients.Count; i++)
            {
                Console.WriteLine($"{i+1}. {Ingredients[i].Food.Name} - {Ingredients[i].Servings} serving(s)");
            }
        }
    }
}
