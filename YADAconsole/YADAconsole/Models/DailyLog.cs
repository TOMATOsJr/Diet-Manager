namespace YADAconsole.Models
{
    public class DailyLog
    {
        public DateOnly Date { get; set; }
        public List<(Food Food, double Servings)> ConsumedFoods { get; private set; }
        public double Weight { get; set; }
        public string ActivityLevel { get; set; }

        // For undo/redo functionality
        private Stack<List<(Food, double)>> UndoStack { get; set; } = new();
        private Stack<List<(Food, double)>> RedoStack { get; set; } = new();

        public DailyLog(DateOnly date)
        {
            Date = date;
            ConsumedFoods = new List<(Food, double)>();
            ActivityLevel = "moderate";
        }

        public void AddFood(Food food, double servings)
        {
            // Save current state for undo
            UndoStack.Push(new List<(Food, double)>(ConsumedFoods));
            RedoStack.Clear();

            ConsumedFoods.Add((food, servings));
        }

        public void DeleteFood(int index)
        {
            if (index >= 0 && index < ConsumedFoods.Count)
            {
                // Save current state for undo
                UndoStack.Push(new List<(Food, double)>(ConsumedFoods));
                RedoStack.Clear();

                ConsumedFoods.RemoveAt(index);
            }
        }

        public bool UndoAction()
        {
            if (UndoStack.Count > 0)
            {
                RedoStack.Push(new List<(Food, double)>(ConsumedFoods));
                ConsumedFoods = UndoStack.Pop();
                return true;
            }
            return false;
        }

        public bool RedoAction()
        {
            if (RedoStack.Count > 0)
            {
                UndoStack.Push(new List<(Food, double)>(ConsumedFoods));
                ConsumedFoods = RedoStack.Pop();
                return true;
            }
            return false;
        }

        public void DisplayLog()
        {
            Console.WriteLine($"=== Daily Log: {Date.ToString("yyyy-MM-dd")} ===");
            Console.WriteLine($"Activity Level: {ActivityLevel}");
            if (Weight > 0)
            {
                Console.WriteLine($"Recorded Weight: {Weight} kg");
            }

            if (ConsumedFoods.Count == 0)
            {
                Console.WriteLine("No foods logged for this day.");
                return;
            }

            Console.WriteLine("Consumed Foods:");
            int i = 1;
            Nutrients totalNutrients = new Nutrients();

            foreach (var (food, servings) in ConsumedFoods)
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
    }
}
