using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachine.Models
{
    /// <summary>
    /// This model is to keep all the list of beverages
    /// </summary>
    public class BeverageMenu : IDisposable
    {
        public BeverageMenu()
        {
            Beverages = new Dictionary<string, List<Ingredient>>();
        }

        public Dictionary<string, List<Ingredient>> Beverages;

        public void Dispose()
        {
            if (Beverages != null && Beverages.Any())
                Beverages.Clear();
        }
    }
}