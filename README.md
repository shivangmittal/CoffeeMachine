# CoffeeMachine


## Design Principle: KISS 

## Models
	- BeverageMenu : POCO for beverages
	- BootCoffeeMachine : This is the virtual model to read the input and configure the machine
	- Ingredient
	- Outlet : Static 
	- Pantry : Singleton instance to keep the ingredients same across the machine.

## Services
  - ManagePantry 
	- LoadIngredient : Add new ingredient/ Update quantity of ingredient in Pantry :  O(1) time
	- HasAllIngredients : Check if pantry has all ingredients mentioned
	- TryGetAllIngredients : Try to get list of passed ingredients if they are present in the Pantry, error otherwise

  - ManageBeverage 
	- MakeBeverage : makes sure to get upto N beverages at a time, keep more requests in queue and process them once any outlet is free
                 Using SemaPhore to keep track of concurrent requests and manage the outlet limit.  Time Complexity :  O(I) linear Where I = Number of ingredients in a beverage
	- AddBeverage : Add a new beverage at runtime. Time Complexity :  O(1) time
	- RemoveBeverage : Remove any beverage on runtime  (example in case of a part malfunction like coffee grinder is not working) Time Complexity :  O(1) time


## Tests
	- CoffeeMachineTest