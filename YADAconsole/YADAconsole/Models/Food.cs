namespace YADAconsole.Models
{
    public abstract class Food
    {
        public int Id { get; set; } // New property for tracking
        public string Name { get; set; }
        public List<string> Keywords { get; set; }
        public Nutrients Nutrients { get; set; }
        public double ServingSize { get; set; } // in grams or milliliters

        protected Food(string name)
        {
            Id = 0; // Default ID
            Name = name;
            Keywords = new List<string>();
            Nutrients = new Nutrients();
            ServingSize = 100; // Default serving size is 100g/ml
        }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Food: {Name}");
            Console.WriteLine($"Serving Size: {ServingSize}g");
            Console.WriteLine($"Nutrients (per serving): {Nutrients}");
            Console.WriteLine($"Keywords: {string.Join(", ", Keywords)}");
        }
    }
}
