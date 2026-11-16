===================================================
YADA - YOUR ADAPTIVE DIET ASSISTANT
===================================================

TABLE OF CONTENTS
------------------
1. Overview
2. System Requirements
3. How to Run the Application
4. Class Structure
   4.1 Models
   4.2 Services
   4.3 Managers
5. Core Features
6. Data Storage
7. Troubleshooting
8. Version Information

===================================================
1. OVERVIEW
===================================================

YADA (Your Adaptive Diet Assistant) is a C# console application that helps users track their daily food intake, manage recipes, analyze nutritional information, and monitor caloric balance. The application allows users to create accounts, log meals, create custom recipes, and receive personalized nutritional guidance.

===================================================
2. SYSTEM REQUIREMENTS
===================================================

- .NET 9.0 runtime
- Windows, macOS, or Linux operating system
- Minimum 100MB of disk space
- Internet connection (optional, only for updates)

===================================================
3. HOW TO RUN THE APPLICATION
===================================================

1. Navigate to the YADAconsole directory
2. Open a command prompt or terminal
3. Run the following command:
   dotnet run

Alternatively, run the compiled executable:
1. Navigate to YADAconsole\YADAconsole\bin\Debug\net9.0\
2. Run YADAconsole.exe

On first launch, YADA will create a Data folder with initial files:
- users.json
- basic_foods.json
- recipes.json
- userlogs/ (directory)

===================================================
4. CLASS STRUCTURE
===================================================

4.1 MODELS
----------

User
----
- Properties: Id, Name, Email, PasswordHash, PasswordSalt, Gender, Age, Height, Weight, TargetCalories, ActivityLevel, CalorieCalculationMethod, BodyFatPercentage
- Methods:
  * VerifyPassword(string): Validates user password
  * CalculateTargetCalories(string): Calculates target calories based on various formulas
  * CalculateHarrisBenedictBMR(): Calculates BMR using Harris-Benedict equation
  * CalculateMifflinStJeorBMR(): Calculates BMR using Mifflin-St Jeor equation
  * CalculateKatchMcArdleBMR(): Calculates BMR using Katch-McArdle formula
  * GetActivityMultiplier(): Returns activity multiplier for calorie calculations
  * DisplayInfo(): Prints user profile information
  * GetCalorieCalculationMethodDescription(): Returns description of selected calculation method

Food (Abstract)
----
- Properties: Id, Name, Keywords, Nutrients, ServingSize
- Methods:
  * DisplayInfo(): Shows food information

BasicFood (Inherits Food)
---------
- Represents single-ingredient foods
- Methods:
  * DisplayInfo(): Overrides base method to show basic food information

CompositeFood (Inherits Food)
------------
- Properties: Ingredients (List of Food and serving pairs), CreatorId
- Methods:
  * AddIngredient(Food, double): Adds a food ingredient with servings
  * EditIngredient(int, Food, double): Modifies an existing ingredient
  * RemoveIngredient(int): Removes an ingredient from the recipe
  * CalculateNutrients(): Calculates combined nutrients from all ingredients
  * GetRecipeId(): Retrieves recipe ID from keywords
  * GetCreatorId(): Retrieves creator user ID
  * IsOwnedBy(string): Checks if current user owns this recipe
  * DisplayInfo(): Overrides base method to show composite food information

DailyLog
--------
- Properties: Date, ConsumedFoods, ConsumedFoodItems, Weight, Height, Age, ActivityLevel
- Methods:
  * AddFood(Food, double): Adds food to the daily log
  * DeleteFood(int): Removes food from the log
  * UndoAction(): Reverts the last change made
  * RedoAction(): Re-applies a previously undone action
  * DisplayLog(): Shows log information
  * InitializeWithUserInfo(User): Sets metrics from user profile
  * SyncFoodLists(FoodDatabase): Rebuilds food references after deserialization

Nutrients
--------
- Properties: Calories, Protein, Carbohydrates, Fat, Micronutrients
- Methods:
  * SumNutrients(Nutrients, Nutrients): Combines two nutrient profiles
  * ToString(): Returns a string representation of nutrient values

FoodDatabase
-----------
- Properties: Foods (List of all available food items)
- Methods:
  * InitializeSampleFoods(): Prepares default food database
  * AddFood(Food): Adds new food to database
  * SearchFood(string): Searches for foods by keyword
  * DisplayAllFoods(): Shows all available foods
  * DisplayFoodList(List<Food>): Shows a subset of foods

FoodConsumption
--------------
- Properties: FoodId, FoodName, Servings, ServingSize
- Used for serialization of consumed food items

DietManager
----------
- Properties: User, Database, UserLogs
- Methods:
  * GetOrCreateDailyLog(DateOnly): Retrieves or creates a daily log
  * HasLogForDate(DateOnly): Checks if log exists for date
  * GetExistingDailyLog(DateOnly): Gets log without creating if missing
  * AddFoodToLog(Food, double, DateOnly): Records food consumption
  * CalculateCalorieBalance(DateOnly): Calculates calories consumed vs target
  * AnalyzeNutrients(DateOnly): Analyzes nutrient intake
  * GetNutrientDeficiencies(): Identifies potential deficiencies
  * GetFoodSuggestions(): Recommends foods based on deficiencies
  * SaveChanges(): Persists changes to disk
  * CalculateCaloriesForDate(DateOnly): Calculates target calories for specific date

4.2 SERVICES
------------

AuthenticationService
-------------------
- Methods:
  * Login(string, string): Authenticates user
  * Register(string, string, string): Creates new user account
  * ResetPassword(string, string): Resets user password
  * CreatePasswordHash(string, out byte[], out byte[]): Creates secure password hash
  * VerifyPassword(string, byte[], byte[]): Validates password against stored hash

DataPersistenceService
--------------------
- Methods:
  * LoadUsers(): Loads user data from disk
  * SaveUsers(List<User>): Saves user data to disk
  * LoadFoodDatabase(): Loads all food data
  * SaveFoodDatabase(FoodDatabase): Saves food database
  * LoadUserLogs(string): Loads logs for specific user
  * SaveUserLogs(string, Dictionary<DateOnly, DailyLog>): Saves user logs
  * LoadBasicFoodsData(): Loads basic food data
  * ConvertBasicFoodsDataToFoods(): Converts data to Food objects
  * LoadRecipes(): Loads recipe data
  * SaveRecipes(List<RecipeData>): Saves recipe data
  * AddRecipeToDatabase(CompositeFood, string): Adds new recipe
  * UpdateRecipe(CompositeFood, int, string): Updates existing recipe
  * DeleteRecipe(int, string): Removes recipe from database
  * ConvertRecipeDataToFoods(): Converts recipe data to Food objects

4.3 MANAGERS
-----------

AuthenticationManager
------------------
- Methods:
  * LoginOrRegister(): Manages authentication flow
  * Login(): Handles user login
  * Register(): Handles user registration
  * ReadPassword(): Securely reads password input

UserProfileManager
----------------
- Methods:
  * ManageUserProfile(): Manages user profile options
  * SelectCalorieCalculationMethod(): Changes BMR calculation method
  * SaveUserChanges(): Persists user profile changes

MenuManager
---------
- Methods:
  * DisplayMainMenu(): Shows the main application menu
  * SetCurrentDate(): Changes the active date

FoodLogManager
------------
- Methods:
  * ManageFoodLog(DateOnly): Manages food logging
  * ContinueFoodLogManagement(DateOnly): Continues food log interface
  * AddFoodToLog(DailyLog): Adds food to log
  * RemoveFoodFromLog(DailyLog): Removes food from log
  * UpdateDailyMetrics(DailyLog): Updates weight, height, etc.
  * SaveChangesToDatabase(): Persists changes to disk

RecipeManager
-----------
- Methods:
  * ManageRecipes(): Manages recipe operations
  * CreateNewRecipe(): Creates a new composite food
  * ViewRecipes(): Displays available recipes
  * EditRecipe(): Modifies existing recipes
  * DeleteRecipe(): Removes recipes from database

NutrientAnalysisManager
---------------------
- Methods:
  * ViewNutrientAnalysis(DateOnly): Shows nutritional analysis
  * CalculateTargetForDate(DailyLog, string): Calculates target calories
  * ShowUserMetricsForDate(DailyLog): Displays metrics used for calculations

===================================================
5. CORE FEATURES
===================================================

1. User Authentication and Profile Management
   - Register new accounts
   - Log in to existing accounts
   - Update profile information
   - Choose different calorie calculation methods

2. Food Logging
   - Add foods to daily log
   - Remove foods from log
   - Update daily metrics (weight, activity level)
   - Undo/redo log changes

3. Recipe Management
   - Create new recipes (composite foods)
   - View existing recipes
   - Edit recipes (owner only)
   - Delete recipes (owner only)

4. Nutrient Analysis
   - View calorie balance
   - Analyze macronutrient distribution
   - Compare with dietary goals
   - Get food suggestions based on nutrient needs

===================================================
6. DATA STORAGE
===================================================

YADA stores data in JSON files located in the "Data" folder:

- users.json: User accounts and profile information
- basic_foods.json: Basic food items database
- recipes.json: User-created recipe data
- userlogs/[userId].json: Individual user daily logs

===================================================
7. TROUBLESHOOTING
===================================================

Issue: Application fails to start
Solution: Ensure .NET 9.0 runtime is installed

Issue: Login fails
Solution: Verify email/password or register a new account

Issue: Food database is empty
Solution: Check basic_foods.json exists in Data folder

Issue: Changes not saved
Solution: Ensure you clicked "Save Changes" option

Issue: Cannot access certain recipes
Solution: You can only edit/delete recipes you created

===================================================
8. VERSION INFORMATION
===================================================

YADA Diet Manager v1.0.0
Built with .NET 9.0