using System.Collections.Generic;

namespace CoffeeMachine.Models
{
    /// <summary>
    /// Singleton instance to keep the ingredients same across the machine
    /// </summary>
    public sealed class Pantry
    {
        public Dictionary<string, int> Ingredients { get; set; }

        private Pantry()
        {
        }

        public static Pantry GetInstance { get { return Nested.Ingredients; } }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly Pantry Ingredients = new Pantry { Ingredients = new Dictionary<string, int>() };
        }
    }
}