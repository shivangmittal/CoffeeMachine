using System.Collections.Generic;

namespace CoffeeMachine.Models
{
    /// <summary>
    ///  This is the virtual model to read the input and configure the machine
    /// </summary>
    public class BootCoffeeMachine
    {
        public Machine machine { get; set; }
    }

    public class Machine
    {
        public Dictionary<string, int> Outlets { get; set; }
        public Dictionary<string, int> Total_items_quantity { get; set; }
        public Dictionary<string, Dictionary<string, int>> Beverages { get; set; }
    }
}