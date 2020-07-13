using System.Collections.Generic;
using CoffeeMachine.Models;

namespace CoffeeMachine.Services
{
    /// <summary>
    /// Contract to Manage the ingredients
    /// </summary>
    public interface IManagePantry
    {
        bool LoadIngredient(string ingredient, int quantity);

        (bool canBePrepared, string inSufficientItem) TryGetAllIngredients(List<Ingredient> ingredients);

        bool HasAllIngredients(List<Ingredient> ingredients);
    }
}