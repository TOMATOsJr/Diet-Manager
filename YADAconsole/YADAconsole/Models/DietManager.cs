using YADAconsole.Services;

namespace YADAconsole.Models
{
    public class DietManager
    {
        public User User { get; private set; }
        public FoodDatabase Database { get; private set; }
        public Dictionary<DateOnly, DailyLog> UserLogs { get; set; }
        private readonly DataPersistenceService _persistenceService;

        public DietManager(User user, DataPersistenceService persistenceService)
        {
            User = user;
            _persistenceService = persistenceService;
            Database = _persistenceService.LoadFoodDatabase();
            UserLogs = _persistenceService.LoadUserLogs(user.Id);
        }

        public DailyLog GetOrCreateDailyLog(DateOnly date)
        {
            if (!UserLogs.ContainsKey(date))
            {
                var log = new DailyLog(date);
                // Do not initialize with current user stats automatically
                UserLogs[date] = log;
            }
            return UserLogs[date];
        }

        // This new method just checks if a log exists without creating one
        public bool HasLogForDate(DateOnly date)
        {
            return UserLogs.ContainsKey(date);
        }

        // Get an existing log without creating a new one
        public DailyLog GetExistingDailyLog(DateOnly date)
        {
            return UserLogs.TryGetValue(date, out var log) ? log : null;
        }

        public void AddFoodToLog(Food food, double servings, DateOnly date)
        {
            DailyLog log = GetOrCreateDailyLog(date);
            log.AddFood(food, servings);

            // Save changes
            _persistenceService.SaveUserLogs(User.Id, UserLogs);
        }

        public Dictionary<string, double> CalculateCalorieBalance(DateOnly date)
        {
            var result = new Dictionary<string, double>();

            if (!UserLogs.ContainsKey(date))
            {
                result["consumed"] = 0;
                result["target"] = User.TargetCalories;
                result["balance"] = -User.TargetCalories;
                return result;
            }

            DailyLog log = UserLogs[date];
            double totalCalories = 0;

            foreach (var (food, servings) in log.ConsumedFoodItems)
            {
                totalCalories += food.Nutrients.Calories * servings;
            }

            result["consumed"] = totalCalories;
            result["target"] = User.TargetCalories;
            result["balance"] = totalCalories - User.TargetCalories;

            return result;
        }

        public Dictionary<string, double> AnalyzeNutrients(DateOnly date)
        {
            var result = new Dictionary<string, double>();

            if (!UserLogs.ContainsKey(date))
            {
                return result;
            }

            DailyLog log = UserLogs[date];
            double totalCalories = 0;
            double totalProtein = 0;
            double totalCarbs = 0;
            double totalFat = 0;

            foreach (var (food, servings) in log.ConsumedFoodItems)
            {
                totalCalories += food.Nutrients.Calories * servings;
                totalProtein += food.Nutrients.Protein * servings;
                totalCarbs += food.Nutrients.Carbohydrates * servings;
                totalFat += food.Nutrients.Fat * servings;
            }

            // Calculate macronutrient percentages
            result["calories"] = totalCalories;
            result["protein"] = totalProtein;
            result["carbs"] = totalCarbs;
            result["fat"] = totalFat;

            if (totalCalories > 0)
            {
                result["protein_pct"] = (totalProtein * 4) / totalCalories * 100;
                result["carbs_pct"] = (totalCarbs * 4) / totalCalories * 100;
                result["fat_pct"] = (totalFat * 9) / totalCalories * 100;
            }

            return result;
        }

        public Dictionary<string, double> GetNutrientDeficiencies()
        {
            // This is a simplified version - would normally compare with recommended daily values
            var deficiencies = new Dictionary<string, double>();

            // Example: Check if protein is below 15% of calories
            var nutrients = AnalyzeNutrients(DateOnly.FromDateTime(DateTime.Today));
            if (nutrients.ContainsKey("protein_pct") && nutrients["protein_pct"] < 15)
            {
                deficiencies["protein"] = 15 - nutrients["protein_pct"];
            }

            return deficiencies;
        }

        public List<Food> GetFoodSuggestions()
        {
            // This would typically analyze nutrient deficiencies and suggest foods
            // For simplicity, just return some healthy options from the database
            return Database.Foods.Take(3).ToList();
        }

        public void SaveChanges()
        {
            // Save food database and user logs
            _persistenceService.SaveFoodDatabase(Database);
            _persistenceService.SaveUserLogs(User.Id, UserLogs);
        }

        // Calculate calories based on the daily log's recorded metrics and user's method
        public int CalculateCaloriesForDate(DateOnly date)
        {
            // Get log if it exists, otherwise use user's current stats
            if (!UserLogs.TryGetValue(date, out DailyLog log))
            {
                return User.TargetCalories;
            }
            
            // Create a temporary user with the day's metrics but use user's calculation method
            var tempUser = new User
            {
                Gender = User.Gender,
                Age = log.Age > 0 ? log.Age : User.Age,
                Weight = log.Weight > 0 ? log.Weight : User.Weight,
                Height = log.Height > 0 ? log.Height : User.Height,
                ActivityLevel = !string.IsNullOrEmpty(log.ActivityLevel) ? log.ActivityLevel : User.ActivityLevel,
                BodyFatPercentage = User.BodyFatPercentage,
                CalorieCalculationMethod = User.CalorieCalculationMethod
            };
            
            return (int)tempUser.CalculateTargetCalories();
        }
    }
}
