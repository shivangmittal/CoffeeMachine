using CoffeeMachine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeMachine.Services
{
    /// <summary>
    /// This is the contract to Manage the beverage
    /// </summary>
    public interface IManageBeverage
    {
        Task<string> MakeBeverage(string beverage, int makingTimeInMilliseconds = 0);

        (bool isSuccess, string error) AddBeverage(string beverageName, List<Ingredient> ingredients, bool checkIngredientsBeforeAdd = true);

        (bool isSuccess, string) RemoveBeverage(string beverageName);
    }
}