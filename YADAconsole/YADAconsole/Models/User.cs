namespace YADAconsole.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public string Gender { get; set; } = "";
        public int Age { get; set; }
        public double Height { get; set; } // in cm
        public double Weight { get; set; } // in kg
        public int TargetCalories { get; set; }
        public string ActivityLevel { get; set; } = "moderate";
        public string CalorieCalculationMethod { get; set; } = "Harris-Benedict"; // Default method
        public double BodyFatPercentage { get; set; } = 20; // Default value, used for Katch-McArdle

        // For JSON deserialization
        public User()
        {
            Name = "";
            Email = "";
        }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
            TargetCalories = 2000; // Default target
        }

        public bool VerifyPassword(string password)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != PasswordHash[i]) return false;
                }
            }
            return true;
        }

        public double CalculateTargetCalories(string method = null)
        {
            // Use specified method or the user's default method
            method ??= CalorieCalculationMethod;
            
            double bmr = 0;

            switch (method)
            {
                case "Mifflin-St Jeor":
                    bmr = CalculateMifflinStJeorBMR();
                    break;
                case "Katch-McArdle":
                    bmr = CalculateKatchMcArdleBMR();
                    break;
                case "Harris-Benedict":
                default:
                    bmr = CalculateHarrisBenedictBMR();
                    break;
            }

            // Apply activity multiplier
            double activityMultiplier = GetActivityMultiplier();

            TargetCalories = (int)(bmr * activityMultiplier);
            return TargetCalories;
        }
        
        private double CalculateHarrisBenedictBMR()
        {
            if (Gender.ToLower() == "male")
            {
                return 88.362 + (13.397 * Weight) + (4.799 * Height) - (5.677 * Age);
            }
            else // Female
            {
                return 447.593 + (9.247 * Weight) + (3.098 * Height) - (4.330 * Age);
            }
        }
        
        private double CalculateMifflinStJeorBMR()
        {
            if (Gender.ToLower() == "male")
            {
                return (10 * Weight) + (6.25 * Height) - (5 * Age) + 5;
            }
            else // Female
            {
                return (10 * Weight) + (6.25 * Height) - (5 * Age) - 161;
            }
        }
        
        private double CalculateKatchMcArdleBMR()
        {
            // Calculate lean body mass based on body fat percentage
            double leanBodyMass = Weight * (1 - (BodyFatPercentage / 100));
            
            // Katch-McArdle formula uses lean body mass
            return 370 + (21.6 * leanBodyMass);
        }
        
        private double GetActivityMultiplier()
        {
            return ActivityLevel.ToLower() switch
            {
                "sedentary" => 1.2,
                "light" => 1.375,
                "moderate" => 1.55,
                "active" => 1.725,
                "very active" => 1.9,
                _ => 1.2
            };
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"User Profile: {Name}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Age: {Age} | Gender: {Gender}");
            Console.WriteLine($"Height: {Height}cm | Weight: {Weight}kg");
            Console.WriteLine($"Activity Level: {ActivityLevel}");
            Console.WriteLine($"Calorie Calculation Method: {CalorieCalculationMethod}");
            
            if (CalorieCalculationMethod == "Katch-McArdle")
                Console.WriteLine($"Body Fat Percentage: {BodyFatPercentage}%");
                
            Console.WriteLine($"Target Daily Calories: {TargetCalories}");
        }
        
        public string GetCalorieCalculationMethodDescription()
        {
            return CalorieCalculationMethod switch
            {
                "Harris-Benedict" => "Classic method that uses weight, height, and age",
                "Mifflin-St Jeor" => "More modern and accurate for most people",
                "Katch-McArdle" => "Uses lean body mass for more accurate results if you know your body fat %",
                _ => "Unknown method"
            };
        }
    }
}
