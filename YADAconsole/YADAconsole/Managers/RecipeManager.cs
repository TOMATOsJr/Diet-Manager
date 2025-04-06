using YADAconsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YADAconsole.Managers
{
    public class RecipeManager
    {
        private readonly DietManager _dietManager;

        public RecipeManager(DietManager dietManager)
        {
            _dietManager = dietManager;
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
                Console.WriteLine("\nAvailable ingredients:");
                _dietManager.Database.DisplayAllFoods();

                Console.Write("Enter ingredient number (or 0 to finish): ");
                if (int.TryParse(Console.ReadLine(), out int ingredientIndex))
                {
                    if (ingredientIndex == 0)
                    {
                        addingIngredients = false;
                    }
                    else if (ingredientIndex > 0 && ingredientIndex <= _dietManager.Database.Foods.Count)
                    {
                        Console.Write("Enter number of servings: ");
                        if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                        {
                            recipe.AddIngredient(_dietManager.Database.Foods[ingredientIndex - 1], servings);
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

            _dietManager.Database.AddFood(recipe);
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

            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to view details (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                recipes[recipeIndex - 1].DisplayInfo();
            }
        }

        private void EditRecipe()
        {
            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes to edit.");
                return;
            }

            Console.WriteLine("\n=== Edit Recipe ===");
            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to edit (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                CompositeFood recipe = recipes[recipeIndex - 1];
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
                        Console.WriteLine("\nAvailable ingredients:");
                        _dietManager.Database.DisplayAllFoods();
                        Console.Write("Enter ingredient number: ");

                        if (int.TryParse(Console.ReadLine(), out int ingredientIndex) &&
                            ingredientIndex > 0 &&
                            ingredientIndex <= _dietManager.Database.Foods.Count)
                        {
                            Console.Write("Enter servings: ");
                            if (double.TryParse(Console.ReadLine(), out double servings) && servings > 0)
                            {
                                recipe.AddIngredient(_dietManager.Database.Foods[ingredientIndex - 1], servings);
                                Console.WriteLine("Ingredient added.");
                            }
                        }
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
                        break;
                    case "3":
                        Console.Write("Enter new name: ");
                        string newName = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            recipe.Name = newName;
                            Console.WriteLine("Name updated.");
                        }
                        break;
                }
            }
        }

        private void DeleteRecipe()
        {
            List<CompositeFood> recipes = _dietManager.Database.Foods.OfType<CompositeFood>().ToList();
            if (recipes.Count == 0)
            {
                Console.WriteLine("No recipes to delete.");
                return;
            }

            Console.WriteLine("\n=== Delete Recipe ===");
            for (int i = 0; i < recipes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recipes[i].Name}");
            }

            Console.Write("Enter recipe number to delete (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int recipeIndex) && recipeIndex > 0 && recipeIndex <= recipes.Count)
            {
                CompositeFood recipe = recipes[recipeIndex - 1];
                Console.Write($"Are you sure you want to delete '{recipe.Name}'? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    int index = _dietManager.Database.Foods.FindIndex(f => f == recipe);
                    if (index >= 0)
                    {
                        _dietManager.Database.Foods.RemoveAt(index);
                        Console.WriteLine("Recipe deleted successfully.");
                    }
                }
            }
        }
    }
}