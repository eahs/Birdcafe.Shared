
using System;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Meta
{
    /// <summary>
    /// Contains global configuration and balancing constants.
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        /// <summary>
        /// Baseline customers per day at neutral popularity.
        /// </summary>
        public int BaseCustomersPerDay { get; set; } = 10;

        /// <summary>
        /// Multiplier determining how much Popularity affects customer count.
        /// </summary>
        public float PopularityToCustomerFactor { get; set; } = 0.5f;
        
        /// <summary>
        /// Initial coffee stock provided on Day 1.
        /// </summary>
        public int DefaultDay1Coffee { get; set; } = 20;
        
        /// <summary>
        /// Base probability (0.0 to 1.0) of a bird getting sick.
        /// </summary>
        public float BaselineSicknessChance { get; set; } = 0.05f; 

        /// <summary>
        /// Multiplier applied to sickness chance when hunger is low.
        /// </summary>
        public float LowHungerSicknessMultiplier { get; set; } = 2.0f;

        /// <summary>
        /// Multiplier applied to sickness chance when energy is low.
        /// </summary>
        public float LowEnergySicknessMultiplier { get; set; } = 1.5f;
        
        /// <summary>
        /// Standard sale price for a unit of Coffee.
        /// </summary>
        public decimal BasePriceCoffee { get; set; } = 3.00m;

        /// <summary>
        /// Standard sale price for a unit of Baked Goods.
        /// </summary>
        public decimal BasePriceBakedGoods { get; set; } = 4.50m;

        /// <summary>
        /// Standard sale price for a unit of Themed Merch.
        /// </summary>
        public decimal BasePriceThemedMerch { get; set; } = 15.00m;
        
        /// <summary>
        /// Standard cost for a single "Feed" action.
        /// </summary>
        public decimal BaselineBirdFoodCost { get; set; } = 5.00m;

        /// <summary>
        /// Standard cost for a single "Vet Visit".
        /// </summary>
        public decimal BaselineVetCost { get; set; } = 50.00m;
    }
}