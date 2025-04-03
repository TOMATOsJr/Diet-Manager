namespace YADAconsole.Models
{
    public class Nutrients
    {
        public double Calories { get; set; }
        public double Protein { get; set; } // in grams
        public double Carbohydrates { get; set; } // in grams
        public double Fat { get; set; } // in grams
        public Dictionary<string, double> Micronutrients { get; set; } = new();

        public Nutrients()
        {
            Calories = 0;
            Protein = 0;
            Carbohydrates = 0;
            Fat = 0;
        }

        public static Nutrients SumNutrients(Nutrients foodA, Nutrients foodB)
        {
            Nutrients result = new Nutrients
            {
                Calories = foodA.Calories + foodB.Calories,
                Protein = foodA.Protein + foodB.Protein,
                Carbohydrates = foodA.Carbohydrates + foodB.Carbohydrates,
                Fat = foodA.Fat + foodB.Fat
            };

            // Combine micronutrients
            foreach (var nutrient in foodA.Micronutrients)
            {
                result.Micronutrients[nutrient.Key] = nutrient.Value;
            }

            foreach (var nutrient in foodB.Micronutrients)
            {
                if (result.Micronutrients.ContainsKey(nutrient.Key))
                {
                    result.Micronutrients[nutrient.Key] += nutrient.Value;
                }
                else
                {
                    result.Micronutrients[nutrient.Key] = nutrient.Value;
                }
            }

            return result;
        }

        public override string ToString()
        {
            return $"Calories: {Calories}kcal, Protein: {Protein}g, Carbs: {Carbohydrates}g, Fat: {Fat}g";
        }
    }
}
