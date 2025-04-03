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

        public double CalculateTargetCalories(string method = "Harris-Benedict")
        {
            // Harris-Benedict equation for BMR
            double bmr = 0;

            if (Gender.ToLower() == "male")
            {
                bmr = 88.362 + (13.397 * Weight) + (4.799 * Height) - (5.677 * Age);
            }
            else // Female
            {
                bmr = 447.593 + (9.247 * Weight) + (3.098 * Height) - (4.330 * Age);
            }

            // Apply activity multiplier
            double activityMultiplier = ActivityLevel.ToLower() switch
            {
                "sedentary" => 1.2,
                "light" => 1.375,
                "moderate" => 1.55,
                "active" => 1.725,
                "very active" => 1.9,
                _ => 1.2
            };

            TargetCalories = (int)(bmr * activityMultiplier);
            return TargetCalories;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"User Profile: {Name}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Age: {Age} | Gender: {Gender}");
            Console.WriteLine($"Height: {Height}cm | Weight: {Weight}kg");
            Console.WriteLine($"Activity Level: {ActivityLevel}");
            Console.WriteLine($"Target Daily Calories: {TargetCalories}");
        }
    }
}
