using System.Text.Json;
using YADAconsole.Models;

namespace YADAconsole.Services
{
    public class DataPersistenceService
    {
        private readonly string _basePath;
        private readonly string _usersPath;
        private readonly string _foodDatabasePath;
        private readonly string _userLogsFolder;

        public DataPersistenceService()
        {
            // Change the base path to use a Data directory in the YADAconsole project folder
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            _basePath = Path.Combine(projectDirectory, "Data");
            _usersPath = Path.Combine(_basePath, "users.json");
            _foodDatabasePath = Path.Combine(_basePath, "fooddatabase.json");
            _userLogsFolder = Path.Combine(_basePath, "userlogs");

            // Ensure directories exist
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(_userLogsFolder);
        }

        #region User Management

        public List<User> LoadUsers()
        {
            if (!File.Exists(_usersPath))
                return new List<User>();

            try
            {
                string json = File.ReadAllText(_usersPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<User>>(json, options) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        public void SaveUsers(List<User> users)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(users, options);
                File.WriteAllText(_usersPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
            }
        }

        #endregion

        #region Food Database

        public FoodDatabase LoadFoodDatabase()
        {
            var database = new FoodDatabase();

            if (!File.Exists(_foodDatabasePath))
                return database;

            try
            {
                string json = File.ReadAllText(_foodDatabasePath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                // For proper deserialization of polymorphic types, we need custom handling
                // This is a simplified approach - consider using System.Text.Json.Serialization.JsonPolymorphic
                // in .NET 7+ or a more robust solution for production code
                database = JsonSerializer.Deserialize<FoodDatabase>(json, options) ?? new FoodDatabase();
                return database;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading food database: {ex.Message}");
                return database;
            }
        }

        public void SaveFoodDatabase(FoodDatabase database)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(database, options);
                File.WriteAllText(_foodDatabasePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving food database: {ex.Message}");
            }
        }

        #endregion

        #region User Logs

        public Dictionary<DateOnly, DailyLog> LoadUserLogs(string userId)
        {
            var logs = new Dictionary<DateOnly, DailyLog>();
            string userLogsPath = Path.Combine(_userLogsFolder, $"{userId}.json");

            if (!File.Exists(userLogsPath))
                return logs;

            try
            {
                string json = File.ReadAllText(userLogsPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                // Deserialize as a list first, then convert to dictionary
                var logsList = JsonSerializer.Deserialize<List<DailyLog>>(json, options) ?? new List<DailyLog>();
                
                // Load the food database for reference
                var foodDatabase = LoadFoodDatabase();
                
                foreach (var log in logsList)
                {
                    log.SyncFoodLists(foodDatabase); // Rebuild the food lists from IDs
                    logs[log.Date] = log;
                }

                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user logs: {ex.Message}");
                return logs;
            }
        }

        public void SaveUserLogs(string userId, Dictionary<DateOnly, DailyLog> logs)
        {
            try
            {
                string userLogsPath = Path.Combine(_userLogsFolder, $"{userId}.json");
                var options = new JsonSerializerOptions { WriteIndented = true };

                // Convert dictionary to list for serialization
                var logsList = logs.Values.ToList();
                string json = JsonSerializer.Serialize(logsList, options);
                File.WriteAllText(userLogsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user logs: {ex.Message}");
            }
        }

        #endregion
    }
}
