namespace YADAconsole.Models
{
    public class BasicFood : Food
    {
        public BasicFood(string name, List<string> keywords, Nutrients nutrients) : base(name)
        {
            Keywords = keywords ?? new List<string>();
            Nutrients = nutrients ?? new Nutrients();
        }

        public BasicFood(string name) : base(name)
        {
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("=== BASIC FOOD ITEM ===");
            base.DisplayInfo();
        }
    }
}
