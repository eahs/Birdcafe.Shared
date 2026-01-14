using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Centralized logic for calculating costs and prices.
    /// Ensures both the UI and the Engine use the same math.
    /// </summary>
    public static class EconomyHelper
    {
        /// <summary>
        /// Calculates the total cost to purchase the specified quantity of inventory.
        /// </summary>
        /// <param name="type">Product type.</param>
        /// <param name="quantity">Number of units.</param>
        /// <returns>Total cost in currency.</returns>
        public static decimal CalculateRestockCost(ProductType type, int quantity)
        {
            // Junior Dev Note: 
            // Ideally these unit costs should come from GameConfig, 
            // but we hardcode them here to centralize the magic numbers 
            // if we haven't added them to config yet.

            decimal unitCost = type switch
            {
                ProductType.Coffee => 1.0m,
                ProductType.BakedGoods => 2.0m,
                ProductType.ThemedMerch => 8.0m,
                _ => 0m
            };

            // Multiply unit cost by quantity.
            return unitCost * quantity;
        }

        /// <summary>
        /// Calculates total cost for a mixed cart of items.
        /// </summary>
        /// <param name="coffeeQty">Amount of coffee.</param>
        /// <param name="bakedQty">Amount of baked goods.</param>
        /// <param name="merchQty">Amount of merch.</param>
        /// <returns>Sum total cost.</returns>
        public static decimal CalculateTotalPlanCost(int coffeeQty, int bakedQty, int merchQty)
        {
            // Sum up the cost of each category.
            return CalculateRestockCost(ProductType.Coffee, coffeeQty) +
                   CalculateRestockCost(ProductType.BakedGoods, bakedQty) +
                   CalculateRestockCost(ProductType.ThemedMerch, merchQty);
        }
    }
}