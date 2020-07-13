using CoffeeMachine.Models;
using CoffeeMachine.Services;
using CoffeeMachine.Utility;
using Newtonsoft.Json;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace CoffeeMachine.Tests
{
    public class CoffeeMachineTest
    {
        private IManagePantry manageIngredients;
        private IManageBeverage manageBeverage;
        private BeverageMenu beverageMenu;

        public CoffeeMachineTest()
        {
            ResetPantry();
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SimpleExample.json")]
        public async Task ShouldGetBeveragesPossible_WhenIngredientsAre_NotSufficient(string settings)
        {
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            //Act
            Parallel.ForEach(beverageMenu.Beverages, beverage =>
            {
                result.Add(manageBeverage.MakeBeverage(beverage.Key).Result);
            });

            // Assert
            result.Count.ShouldBe(4);
            result.Where(t => t.IndexOf("is prepared", StringComparison.Ordinal) > -1).Count().ShouldBe(2);
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SufficientIngredients.json")]
        public async Task ShouldQueueAndTryGetBeverages_WhenOutletsAreLessButRequestsAreMore(string settings)
        {
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            //Act
            Parallel.ForEach(beverageMenu.Beverages, beverage =>
            {
                result.Add(manageBeverage.MakeBeverage(beverage.Key).Result);
            });

            // Assert
            result.Count.ShouldBe(10);
            result.Where(t => t.IndexOf("is prepared", StringComparison.Ordinal) > -1).Count().ShouldBe(8);
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SufficientIngredients.json")]
        public async Task AddBeverage_ShouldAddNewBeverage_WhenBeverageIsAddedAtRunTime(string settings)
        {
            var newBeverage = "ginger_allay";
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            manageBeverage.AddBeverage(newBeverage, new List<Ingredient>
            {
                new Ingredient {IngredientName = "ginger_syrup",Quantity = 50},
                new Ingredient {IngredientName = "Brown_Sugar",Quantity = 20}
            });

            var makeResult = manageBeverage.MakeBeverage(newBeverage).Result;

            makeResult.ShouldBe(newBeverage + " is prepared");
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SimpleExample.json")]
        public async Task AddBeverage_ShouldNOTBeAllowed_WhenIngredientsNeededAre_NotInPantry(string settings)
        {
            var newBeverage = "ginger_allay";
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            var addResult = manageBeverage.AddBeverage(newBeverage, new List<Ingredient>
            {
                new Ingredient {IngredientName = "ginger_syrup",Quantity = 50},
                new Ingredient {IngredientName = "Brown_Sugar",Quantity = 20}
            });

            addResult.error.ShouldStartWith("Pantry does not provide ");
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SimpleExample.json")]
        public async Task DeleteBeverage_ShouldRemoveFromMenu(string settings)
        {
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            var isRemoved = manageBeverage.RemoveBeverage("hot_tea");
            isRemoved.isSuccess.ShouldBe(true);

            var manageResult = await manageBeverage.MakeBeverage("hot_tea");

            manageResult.ShouldBe("This Beverage is not present at the moment");
        }

        [Theory]
        [JsonFileData(@"Tests\Resources\SimpleExample.json")]
        public async Task Pantry_ShouldUpdate_WhenAddedIngredientsAtRuntime(string settings)
        {
            var newBeverage = "ginger_allay";
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            //Arrange
            ConfigureMachineBasedOnInput(settings);

            manageIngredients.LoadIngredient("Brown_Sugar", 30);

            var addResult = manageBeverage.AddBeverage(newBeverage, new List<Ingredient>
            {
                new Ingredient {IngredientName = "ginger_syrup",Quantity = 50},
                new Ingredient {IngredientName = "Brown_Sugar",Quantity = 20}
            });

            addResult.isSuccess.ShouldBe(true);
        }

        /// <summary>
        /// This is to configure the initial ingredients and beverage menu of the coffee machine
        /// </summary>
        /// <param name="settings"></param>
        private void ConfigureMachineBasedOnInput(string settings)
        {
            try
            {
                var coffeeMachineConfig = JsonConvert.DeserializeObject<BootCoffeeMachine>(settings).machine;

                Outlet.NumOfOutlets = coffeeMachineConfig.Outlets.FirstOrDefault().Value;
                manageIngredients = new ManagePantry();

                foreach (var ingredients in coffeeMachineConfig.Total_items_quantity)
                {
                    var isSuccess = manageIngredients.LoadIngredient(ingredients.Key, ingredients.Value);
                    isSuccess.ShouldBe(true);
                }

                beverageMenu = new BeverageMenu();
                manageBeverage = new ManageBeverage(manageIngredients, beverageMenu);
                foreach (var beverage in coffeeMachineConfig.Beverages)
                {
                    var beverageResonse = manageBeverage.AddBeverage(beverage.Key,
                        beverage.Value.Select(t => new Ingredient
                        {
                            IngredientName = t.Key,
                            Quantity = t.Value
                        }).ToList(), false);
                    beverageResonse.isSuccess.ShouldBe(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Machine configurations are not correct", e.Message);
                throw;
            }
        }

        private void ResetPantry()
        {
            if (Pantry.GetInstance != null && Pantry.GetInstance.Ingredients != null)
                Pantry.GetInstance.Ingredients.Clear();
            if (beverageMenu != null)
                beverageMenu.Dispose();
        }

        private static string GetTestData(string jsonFilePath)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), jsonFilePath);

            //var path = Path.IsPathRooted(jsonFilePath)
            //    ? jsonFilePath
            //    : Path.GetRelativePath(Directory.GetCurrentDirectory(), jsonFilePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find specified file at the given path: {jsonFilePath}");
            }
            return File.ReadAllText(path);
        }
    }
}