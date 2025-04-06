using System.Text.Json;
using YADAconsole.Models;

namespace YADAconsole.Services
{
    public class DataPersistenceService
    {
        private readonly string _basePath;
        private readonly string _usersPath;
        private readonly string _basicFoodsPath;
        private readonly string _recipesPath;
        private readonly string _userLogsFolder;

        public DataPersistenceService()
        {
            // Change the base path to use a Data directory in the YADAconsole project folder
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            _basePath = Path.Combine(projectDirectory, "Data");
            _usersPath = Path.Combine(_basePath, "users.json");
            _basicFoodsPath = Path.Combine(_basePath, "basic_foods.json");
            _recipesPath = Path.Combine(_basePath, "recipes.json");
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

            try
            {
                // Use basic_foods.json instead of separate fooddatabase.json
                var foods = ConvertBasicFoodsDataToFoods();
                
                // Also load all recipes from recipes.json
                var recipeData = LoadRecipes();
                var recipeFoods = ConvertRecipeDataToFoods();
                
                // Add both basic foods and recipes to the database
                database.Foods = foods;
                database.Foods.AddRange(recipeFoods);
                
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
                // We don't need to save the entire database structure
                // Instead, extract basic food data and save it
                var basicFoodsData = new List<BasicFoodData>();
                
                foreach (var food in database.Foods)
                {
                    // Handle only BasicFood objects for now
                    if (food is BasicFood basicFood)
                    {
                        var foodData = new BasicFoodData
                        {
                            // Use the last food's ID + 1 or start from 1
                            Id = basicFoodsData.Count > 0 ? basicFoodsData.Max(f => f.Id) + 1 : 1,
                            Name = food.Name,
                            Category = food.Keywords.FirstOrDefault() ?? "Miscellaneous",
                            Carbs = food.Nutrients.Carbohydrates,
                            Fats = food.Nutrients.Fat,
                            Protein = food.Nutrients.Protein,
                            Sugar = food.Nutrients.Micronutrients.TryGetValue("Sugar", out double sugar) ? sugar : 0,
                            Fiber = food.Nutrients.Micronutrients.TryGetValue("Fiber", out double fiber) ? fiber : 0,
                            Keywords = food.Keywords.ToList()
                        };
                        basicFoodsData.Add(foodData);
                    }
                    // Handle composite foods separately if needed
                }
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(basicFoodsData, options);
                File.WriteAllText(_basicFoodsPath, json);
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

        #region Basic Foods

        public List<BasicFoodData> LoadBasicFoodsData()
        {
            if (!File.Exists(_basicFoodsPath))
                return new List<BasicFoodData>();

            try
            {
                string json = File.ReadAllText(_basicFoodsPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<BasicFoodData>>(json, options) ?? new List<BasicFoodData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading basic foods data: {ex.Message}");
                return new List<BasicFoodData>();
            }
        }

        // Update this method to set the ID when converting basic foods data
        public List<Food> ConvertBasicFoodsDataToFoods()
        {
            var basicFoodsData = LoadBasicFoodsData();
            var foods = new List<Food>();

            foreach (var foodData in basicFoodsData)
            {
                var basicFood = new BasicFood(foodData.Name);
                
                // Set ID from the data
                basicFood.Id = foodData.Id;
                
                // Use keywords from data or fall back to category
                basicFood.Keywords = foodData.Keywords != null && foodData.Keywords.Count > 0 
                    ? foodData.Keywords 
                    : new List<string> { foodData.Category };
                
                // Set nutrients
                basicFood.Nutrients = new Nutrients
                {
                    Carbohydrates = foodData.Carbs,
                    Fat = foodData.Fats,
                    Protein = foodData.Protein,
                    // Calculate calories: 4 calories per gram of protein and carbs, 9 calories per gram of fat
                    Calories = (foodData.Carbs * 4) + (foodData.Protein * 4) + (foodData.Fats * 9)
                };
                
                // Add sugar and fiber as micronutrients
                basicFood.Nutrients.Micronutrients["Sugar"] = foodData.Sugar;
                basicFood.Nutrients.Micronutrients["Fiber"] = foodData.Fiber;
                
                // Set default serving size
                basicFood.ServingSize = 100; // Default serving size in grams
                
                foods.Add(basicFood);
            }

            return foods;
        }

        #endregion

        #region Recipes

        public List<RecipeData> LoadRecipes()
        {
            if (!File.Exists(_recipesPath))
                return new List<RecipeData>();

            try
            {
                string json = File.ReadAllText(_recipesPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<RecipeData>>(json, options) ?? new List<RecipeData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipes: {ex.Message}");
                return new List<RecipeData>();
            }
        }

        public void SaveRecipes(List<RecipeData> recipes)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(recipes, options);
                File.WriteAllText(_recipesPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving recipes: {ex.Message}");
            }
        }

        // Update AddRecipeToDatabase to set ID
        public void AddRecipeToDatabase(CompositeFood recipe, string userId)
        {
            var recipes = LoadRecipes();
            
            // Get the next available ID
            int nextId = recipes.Count > 0 ? recipes.Max(r => r.Id) + 1 : 1001;
            
            // Set the recipe ID
            recipe.Id = nextId;
            
            var recipeData = new RecipeData
            {
                Id = nextId,
                Name = recipe.Name,
                Category = recipe.Keywords.FirstOrDefault() ?? "recipe",
                Carbs = recipe.Nutrients.Carbohydrates,
                Fats = recipe.Nutrients.Fat,
                Protein = recipe.Nutrients.Protein,
                Sugar = recipe.Nutrients.Micronutrients.TryGetValue("Sugar", out double sugar) ? sugar : 0,
                Fiber = recipe.Nutrients.Micronutrients.TryGetValue("Fiber", out double fiber) ? fiber : 0,
                Keywords = recipe.Keywords.ToList(),
                UserId = userId,
                Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientData 
                { 
                    FoodId = i.Food.GetType().Name + "-" + i.Food.Name,
                    Name = i.Food.Name, 
                    Quantity = i.Servings 
                }).ToList()
            };
            
            recipes.Add(recipeData);
            SaveRecipes(recipes);
        }

        public void UpdateRecipe(CompositeFood recipe, int recipeId, string userId)
        {
            var recipes = LoadRecipes();
            var existingRecipe = recipes.FirstOrDefault(r => r.Id == recipeId);
            
            if (existingRecipe == null || existingRecipe.UserId != userId)
                return;
                
            existingRecipe.Name = recipe.Name;
            existingRecipe.Category = recipe.Keywords.FirstOrDefault() ?? "recipe";
            existingRecipe.Carbs = recipe.Nutrients.Carbohydrates;
            existingRecipe.Fats = recipe.Nutrients.Fat;
            existingRecipe.Protein = recipe.Nutrients.Protein;
            existingRecipe.Sugar = recipe.Nutrients.Micronutrients.TryGetValue("Sugar", out double sugar) ? sugar : 0;
            existingRecipe.Fiber = recipe.Nutrients.Micronutrients.TryGetValue("Fiber", out double fiber) ? fiber : 0;
            existingRecipe.Keywords = recipe.Keywords.ToList();
            existingRecipe.Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientData 
            { 
                FoodId = i.Food.GetType().Name + "-" + i.Food.Name,
                Name = i.Food.Name, 
                Quantity = i.Servings 
            }).ToList();
            
            SaveRecipes(recipes);
        }

        public bool DeleteRecipe(int recipeId, string userId)
        {
            var recipes = LoadRecipes();
            var recipeToDelete = recipes.FirstOrDefault(r => r.Id == recipeId);
            
            if (recipeToDelete == null || recipeToDelete.UserId != userId)
                return false;
                
            recipes.Remove(recipeToDelete);
            SaveRecipes(recipes);
            return true;
        }

        // Add a method to convert recipe data to CompositeFood objects
        // Update this method to set recipe IDs
        public List<Food> ConvertRecipeDataToFoods()
        {
            var recipes = LoadRecipes();
            var foods = new List<Food>();
            var basicFoods = ConvertBasicFoodsDataToFoods(); // Need basic foods to resolve ingredients

            foreach (var recipeData in recipes)
            {
                var recipe = new CompositeFood(recipeData.Name);
                
                // Set the recipe ID from the data
                recipe.Id = recipeData.Id;
                
                // Set keywords from data
                recipe.Keywords = recipeData.Keywords ?? new List<string> { recipeData.Category };
                
                // Add metadata for ownership tracking
                recipe.Keywords.Add($"__USERID_{recipeData.UserId}__");

                // Add ingredients
                foreach (var ingredient in recipeData.Ingredients)
                {
                    // Find the ingredient in basic foods
                    var food = basicFoods.FirstOrDefault(f => 
                        f.GetType().Name + "-" + f.Name == ingredient.FoodId);
                        
                    if (food != null)
                    {
                        recipe.AddIngredient(food, ingredient.Quantity);
                    }
                    else
                    {
                        // Create a placeholder if ingredient not found
                        var placeholderFood = new BasicFood(ingredient.Name);
                        recipe.AddIngredient(placeholderFood, ingredient.Quantity);
                        Console.WriteLine($"Warning: Ingredient '{ingredient.Name}' not found in database.");
                    }
                }
                
                foods.Add(recipe);
            }

            return foods;
        }

        #endregion
    }

    // Model to match the structure in basic_foods.json
    public class BasicFoodData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double Protein { get; set; }
        public double Sugar { get; set; }
        public double Fiber { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    // Model to match the structure in recipes.json
    public class RecipeData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double Protein { get; set; }
        public double Sugar { get; set; }
        public double Fiber { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public string UserId { get; set; } = "";
        public List<RecipeIngredientData> Ingredients { get; set; } = new List<RecipeIngredientData>();
    }

    public class RecipeIngredientData
    {
        public string FoodId { get; set; } = "";
        public string Name { get; set; } = "";
        public double Quantity { get; set; }
    }
}
