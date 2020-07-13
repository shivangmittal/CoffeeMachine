using CoffeeMachine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoffeeMachine.Services
{
    /// <summary>
    /// This class Manages the making of beverages
    /// </summary>
    public class ManageBeverage : IManageBeverage, IDisposable
    {
        /// <summary>
        /// It manages to limit the concurrent making to the limit of outlets and keep other request in a queue
        /// </summary>
        private static Semaphore semaphoreObject = new Semaphore(initialCount: Outlet.NumOfOutlets, maximumCount: Outlet.NumOfOutlets);

        private static readonly object LockObject = new Object();

        private readonly BeverageMenu _beverageMenu;

        private readonly IManagePantry _manageIngredients;

        public ManageBeverage(IManagePantry manageIngredients, BeverageMenu beverageMenu)
        {
            _manageIngredients = manageIngredients;
            _beverageMenu = beverageMenu;
        }

        /// <summary>
        /// Make Beverage Handler which will make a beverage in O(I) linear where I is the count of ingredients in the beverage
        /// </summary>
        /// <param name="beverage"></param>
        /// <param name="makingTime">ADDED TO TEST SCENARIOS WHEN ALL THE OUTLETS ARE BUSY</param>
        /// <returns></returns>
        public async Task<string> MakeBeverage(string beverage, int makingTimeInMilliseconds = 0)
        {
            if (!_beverageMenu.Beverages.ContainsKey(beverage))
            {
                return await Task.FromResult("This Beverage is not present at the moment");
            }

            var ingredients = _beverageMenu.Beverages[beverage];

            // Semaphore section with maximum coffee request allowed in parallel is equal to the outlets
            // This section will keep the other requests in Queue
            var acquired = semaphoreObject.WaitOne();

            //// Uncomment this section when the requests need NOT to be queued but declined
            //if (!acquired)
            //{
            //    return await Task.FromResult("All outlets are busy, kindly wait for your turn");
            //}
            var checkIngredientsInPantry = _manageIngredients.TryGetAllIngredients(ingredients);

            semaphoreObject.Release(1);

            if (!checkIngredientsInPantry.canBePrepared)
            {
                return await Task.FromResult($"{beverage} cannot be prepared because item {checkIngredientsInPantry.inSufficientItem} is not sufficient");
            }

            if (makingTimeInMilliseconds > 0)
                await Task.Delay(makingTimeInMilliseconds);

            return await Task.FromResult($"{beverage} is prepared");
        }

        /// <summary>
        /// Add a new beverage at runtime in O(1) time
        /// </summary>
        /// <param name="beverageName"></param>
        /// <param name="ingredients"></param>
        /// <param name="checkIngredientsBeforeAdd"> additional check if enabled, check the ingredients in O(I) time I = New beverage Ingredients</param>
        /// <returns></returns>
        public (bool isSuccess, string error) AddBeverage(string beverageName, List<Ingredient> ingredients, bool checkIngredientsBeforeAdd = true)
        {
            if (_beverageMenu.Beverages.ContainsKey(beverageName))
            {
                return (false, "This Beverage is already present");
            }

            if (checkIngredientsBeforeAdd && !_manageIngredients.HasAllIngredients(ingredients))
            {
                return (false, "Pantry does not provide the specified ingredients");
            }

            lock (LockObject)
            {
                _beverageMenu.Beverages.Add(beverageName, ingredients);
            }

            return (true, "Beverage added successfully");
        }

        /// <summary>
        /// Remove any beverage on runtime  (example in case of a part malfunction like coffee grinder is not working)
        /// </summary>
        /// <param name="beverageName"></param>
        /// <returns></returns>
        public (bool isSuccess, string) RemoveBeverage(string beverageName)
        {
            if (!_beverageMenu.Beverages.ContainsKey(beverageName))
            {
                return (false, "This Beverage is not present");
            }

            lock (LockObject)
            {
                _beverageMenu.Beverages.Remove(beverageName);
            }

            return (true, "Beverage added successfully");
        }

        public void Dispose()
        {
            _beverageMenu.Dispose();
        }
    }
}