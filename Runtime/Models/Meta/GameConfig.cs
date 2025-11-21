
using System;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Meta
{
    /// <summary>
    /// Contains global configuration and balancing constants.
    /// Junior Dev Note: Tweak these values to balance the game. 
    /// Do not hardcode numbers in Managers!
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        // --- Customer Flow ---

        /// <summary>
        /// Baseline customers per day at neutral popularity.
        /// </summary>
        public int BaseCustomersPerDay { get; set; } = 10;

        /// <summary>
        /// Multiplier determining how much Popularity affects customer count.
        /// </summary>
        public float PopularityToCustomerFactor { get; set; } = 0.5f;

        // --- Simulation Settings ---

        /// <summary>
        /// How long a game day lasts in simulation seconds.
        /// </summary>
        public float DayDurationSeconds { get; set; } = 120f;

        /// <summary>
        /// How long a customer waits (in seconds) before leaving angry.
        /// </summary>
        public float CustomerPatienceSeconds { get; set; } = 5.0f;

        // --- Bird Stats & Decay ---

        /// <summary>
        /// Amount of Hunger lost automatically at end of day.
        /// </summary>
        public float DailyHungerDecay { get; set; } = 30f;

        /// <summary>
        /// Amount of Mood lost automatically at end of day due to stress/boredom.
        /// </summary>
        public float DailyMoodDecay { get; set; } = 10f;

        /// <summary>
        /// Base amount of Energy EVERY bird recovers overnight by sleeping.
        /// </summary>
        public float BaseNightlyEnergyRecovery { get; set; } = 15f;

        /// <summary>
        /// ADDITIONAL amount of Energy recovered if a bird is assigned a Rest Day.
        /// Total recovery = BaseNightlyEnergyRecovery + RestDayEnergyBonus.
        /// </summary>
        public float RestDayEnergyBonus { get; set; } = 40f;

        /// <summary>
        /// Amount of Energy lost per customer served.
        /// </summary>
        public float EnergyCostPerService { get; set; } = 2f;
        
        // --- Sickness ---

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
        
        // --- Economy ---

        /// <summary>
        /// Initial coffee stock provided on Day 1.
        /// </summary>
        public int DefaultDay1Coffee { get; set; } = 20;

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
        
        // --- Care Costs ---

        /// <summary>
        /// Standard cost for a single "Feed" action.
        /// </summary>
        public decimal BaselineBirdFoodCost { get; set; } = 5.00m;

        /// <summary>
        /// Standard cost for a single "Vet Visit".
        /// </summary>
        public decimal BaselineVetCost { get; set; } = 50.00m;
        
        /// <summary>
        /// Standard cost for playing with a bird (Toys, etc).
        /// </summary>
        public decimal BaselinePlayCost { get; set; } = 0.00m;
    }
}