namespace YADAconsole.Models
{
    public class FoodConsumption
    {
        // Stores just the ID or identifier of the food, not the entire food data
        public string FoodId { get; set; } = "";
        public string FoodName { get; set; } = "";
        public double Servings { get; set; }
        public double ServingSize { get; set; }
        
        // For serialization
        public FoodConsumption() { }
        
        // Constructor to create from Food and servings
        public FoodConsumption(Food food, double servings)
        {
            FoodId = food.GetType().Name + "-" + food.Name;
            FoodName = food.Name;
            Servings = servings;
            ServingSize = food.ServingSize;
        }
    }
}
