using YADAconsole.Models;
using YADAconsole.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YADAconsole.Managers
{
    public class RecipeManager
    {
        private readonly DietManager _dietManager;
        private readonly DataPersistenceService _persistenceService;

        public RecipeManager(DietManager dietManager)
        {
            _dietManager = dietManager;
            _persistenceService = new DataPersistenceService(); // Direct instance for recipe management
        }

        public void ManageRecipes()
        {
            Console.WriteLine("\n=== Manage Recipes ===");

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("1. Create New Recipe");
                Console.WriteLine("2. View Existing Recipes");
                Console.WriteLine("3. Edit Recipe");
                Console.WriteLine("4. Delete Recipe");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        CreateNewRecipe();
                        break;
                    case "2":
                        ViewRecipes();
                        break;
                    case "3":
                        EditRecipe();
                        break;
                    case "4":
                        DeleteRecipe();
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }

            // Save changes when exiting
            _dietManager.SaveChanges();
        }

        private void CreateNewRecipe()
        {
            Console.Write("Enter recipe name: ");
            string name = Console.ReadLine() ?? "New Recipe";

            var recipe = new CompositeFood(name);

            bool addingIngredients = true;
            while (addingIngredients)
            {
                // Add keyword search option
                Console.WriteLine("\nSearch for ingredients:");
                Console.WriteLine("1. Show all foods");
                Console.WriteLine("2. Search by keyword");
                Console.Write("Enter choice: ");
                
                string choice = Console.ReadLine() ?? "1";
                
                List<Food> foodOptions;
                
                if (choice == "2")
                {
                    Console.Write("Enter search keyword: ");
                    string keyword = Console.ReadLine() ?? "";
                    foodOptions = _dietManager.Database.SearchFood(keyword);
                    
                    if (foodOptions.Count == 0)
                    {
                        Console.WriteLine("No foods found matching that keyword.");
                        continue;
                    }
                    
                    Console.WriteLine("\nSearch results:");
                    _dietManager.Database.DisplayFoodList(foodOptions);
                }
                else
                {
                    // Show all foods
                    Console.WriteLine("\nAvailable ingredients:");
                    _dietManager.Database.DisplayAllFoods();
                    foodOptions = _dietManager.Database.Foods;
                }

                Console.Write("Enter ingredient number (or 0 to finish): ");
                if (int.TryParse(Console.ReadLine(), out int ingredientIndex))
                {
                    if (ingredientIndex == 0)
                    {
                        addingIngredients = false;
                    }
                    else if (ingredientIndex > 0 && ingredientIndex <= foodOptions.Count)
                    {
                        Console.Write("Enter number of servings: ");
                        if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                        {
                            recipe.AddIngredient(foodOptions[ingredientIndex - 1], servings);
                            Console.WriteLine("Ingredient added successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid servings. Please enter a positive number.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }

            Console.Write("Enter keywords (comma separated): ");
            string keywordInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(keywordInput))
            {
                recipe.Keywords = keywordInput.Split(',').Select(k => k.Trim()).ToList();
            }

            // Add recipe to both in-memory database and recipes.json
            _dietManager.Database.AddFood(recipe);
            _persistenceService.AddRecipeToDatabase(recipe, _dietManager.User.Id);
            
            Console.WriteLine("Recipe created and added to food database.");
            recipe.DisplayInfo();
        }

        private void ViewRecipes()
        {
            Console.WriteLine("\n=== Recipes ===");
            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();

            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes found.");
                return;
            }

            Console.WriteLine("All Available Recipes:");
            int i = 1;
            foreach (var recipe in recipes)
            {
                string ownershipStatus = recipe.IsOwnedBy(_dietManager.User.Id) ? "[Your Recipe]" : "[Shared Recipe]";
                Console.WriteLine($"{i}. {recipe.Name} {ownershipStatus}");
                i++;
            }

            Console.Write("Enter recipe number to view details (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                var selectedRecipe = recipes[recipeIndex - 1];
                selectedRecipe.DisplayInfo();
                
                // Show ownership information
                bool isOwner = selectedRecipe.IsOwnedBy(_dietManager.User.Id);
                Console.WriteLine($"Recipe created by: {(isOwner ? "You" : "Another user")}");
                Console.WriteLine("Note: All recipes can be added to your food log regardless of creator");
            }
        }

        private void EditRecipe()
        {
            List<CompositeFood> allRecipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            
            // Filter to show only recipes the user owns
            var editableRecipes = allRecipes.Where(r => r.IsOwnedBy(_dietManager.User.Id)).ToList();
            
            if (editableRecipes.Count == 0)
            {
                Console.WriteLine("You don't have any recipes that you can edit.");
                return;
            }

            Console.WriteLine("\n=== Edit Recipe ===");
            Console.WriteLine("Your Recipes:");
            
            for (int i = 0; i < editableRecipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {editableRecipes[i].Name}");
            }

            Console.Write("Enter recipe number to edit (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= editableRecipes.Count)
            {
                CompositeFood recipe = editableRecipes[recipeIndex - 1];
                recipe.DisplayInfo();

                Console.WriteLine("\n1. Add ingredient");
                Console.WriteLine("2. Remove ingredient");
                Console.WriteLine("3. Edit name");
                Console.WriteLine("4. Return");
                Console.Write("Enter choice: ");

                string choice = Console.ReadLine() ?? "";
                switch (choice)
                {
                    case "1":
                        // Add keyword search option when adding ingredient
                        Console.WriteLine("\nSearch for ingredients:");
                        Console.WriteLine("1. Show all foods");
                        Console.WriteLine("2. Search by keyword");
                        Console.Write("Enter choice: ");
                        
                        string searchChoice = Console.ReadLine() ?? "1";
                        
                        List<Food> foodOptions;
                        
                        if (searchChoice == "2")
                        {
                            Console.Write("Enter search keyword: ");
                            string keyword = Console.ReadLine() ?? "";
                            foodOptions = _dietManager.Database.SearchFood(keyword);
                            
                            if (foodOptions.Count == 0)
                            {
                                Console.WriteLine("No foods found matching that keyword.");
                                return;
                            }
                            
                            Console.WriteLine("\nSearch results:");
                            _dietManager.Database.DisplayFoodList(foodOptions);
                        }
                        else
                        {
                            Console.WriteLine("\nAvailable ingredients:");
                            _dietManager.Database.DisplayAllFoods();
                            foodOptions = _dietManager.Database.Foods;
                        }

                        Console.Write("Enter ingredient number: ");
                        if (int.TryParse(Console.ReadLine(), out int ingredientIndex) &&
                            ingredientIndex > 0 &&
                            ingredientIndex <= foodOptions.Count)
                        {
                            Console.Write("Enter servings: ");
                            if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                            {
                                recipe.AddIngredient(foodOptions[ingredientIndex - 1], servings);
                                Console.WriteLine("Ingredient added.");
                            }
                        }
                        
                        // Update recipe in recipes.json after modification
                        _persistenceService.UpdateRecipe(recipe, recipe.Id, _dietManager.User.Id);
                        break;
                    case "2":
                        if (recipe.Ingredients.Count == 0)
                        {
                            Console.WriteLine("No ingredients to remove.");
                            return;
                        }

                        Console.Write("Enter ingredient number to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int removeIndex) &&
                            removeIndex > 0 &&
                            removeIndex <= recipe.Ingredients.Count)
                        {
                            recipe.RemoveIngredient(removeIndex - 1);
                            Console.WriteLine("Ingredient removed.");
                        }
                        
                        // Update recipe in recipes.json after modification
                        _persistenceService.UpdateRecipe(recipe, recipe.Id, _dietManager.User.Id);
                        break;
                    case "3":
                        Console.Write("Enter new name: ");
                        string newName = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            recipe.Name = newName;
                            Console.WriteLine("Name updated.");
                            
                            // Update recipe in recipes.json after modification
                            _persistenceService.UpdateRecipe(recipe, recipe.Id, _dietManager.User.Id);
                        }
                        break;
                }
            }
        }

        private void DeleteRecipe()
        {
            List<CompositeFood> allRecipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            
            // Filter to show only recipes the user owns
            var deletableRecipes = allRecipes.Where(r => r.IsOwnedBy(_dietManager.User.Id)).ToList();
            
            if (deletableRecipes.Count == 0)
            {
                Console.WriteLine("You don't have any recipes that you can delete.");
                return;
            }

            Console.WriteLine("\n=== Delete Recipe ===");
            Console.WriteLine("Your Recipes:");
            
            for (int i = 0; i < deletableRecipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {deletableRecipes[i].Name}");
            }

            Console.Write("Enter recipe number to delete (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= deletableRecipes.Count)
            {
                CompositeFood recipe = deletableRecipes[recipeIndex - 1];
                Console.Write($"Are you sure you want to delete '{recipe.Name}'? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    // Remove from in-memory database
                    int index = _dietManager.Database.Foods.FindIndex(f => f == recipe);
                    if (index >= 0)
                    {
                        _dietManager.Database.Foods.RemoveAt(index);
                        
                        // Also remove from recipes.json using the recipe's ID
                        _persistenceService.DeleteRecipe(recipe.Id, _dietManager.User.Id);
                        
                        Console.WriteLine("Recipe deleted successfully.");
                    }
                }
            }
        }
    }
}