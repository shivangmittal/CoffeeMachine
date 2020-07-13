using CoffeeMachine.Models;
using System;
using System.Collections.Generic;

namespace CoffeeMachine.Services
{
    internal class ManagePantry : IManagePantry
    {
        private static readonly object _lockObject = new Object();
        private readonly Pantry _pantry;

        public ManagePantry()
        {
            _pantry = Pantry.GetInstance;
        }


        public ManagePantry(Pantry pantry)
        {
            _pantry = pantry;
        }

        /// <summary>
        ///  Add new ingredient/ Update quantity of ingredient in Pantry :  O(1) time
        /// </summary>
        /// <param name="ingredient"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool LoadIngredient(string ingredient, int quantity)
        {
            if (_pantry.Ingredients.ContainsKey(ingredient))
            {
                _pantry.Ingredients[ingredient] += quantity;
            }
            else
            {
                _pantry.Ingredients.Add(ingredient, quantity);
            }

            return true;
        }

        /// <summary>
        /// Try to get list of passed ingredients if they are present in the Pantry, error otherwise
        /// </summary>
        /// <param name="ingredients"></param>
        /// <returns></returns>
        public (bool canBePrepared, string inSufficientItem) TryGetAllIngredients(List<Ingredient> ingredients)
        {
            lock (_lockObject)
            {
                foreach (var ingredient in ingredients)
                {
                    if (!_pantry.Ingredients.ContainsKey(ingredient.IngredientName) || _pantry.Ingredients[ingredient.IngredientName] < ingredient.Quantity)
                    {
                        return (false, ingredient.IngredientName);
                    }
                }

                foreach (var ingredient in ingredients)
                {
                    _pantry.Ingredients[ingredient.IngredientName] = _pantry.Ingredients[ingredient.IngredientName] - ingredient.Quantity;
                }
                return (true, string.Empty);
            }
        }

        /// <summary>
        /// Check if pantry has all ingredients mentioned 
        /// </summary>
        /// <param name="ingredients"></param>
        /// <returns></returns>
        public bool HasAllIngredients(List<Ingredient> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                if (!_pantry.Ingredients.ContainsKey(ingredient.IngredientName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}