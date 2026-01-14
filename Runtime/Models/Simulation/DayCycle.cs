
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Simulation
{
    /// <summary>
    /// Tracks the current status of the day.
    /// </summary>
    [Serializable]
    public class DayState
    {
        /// <summary>
        /// Current day number (1-based).
        /// </summary>
        public int DayNumber { get; set; } = 1;

        /// <summary>
        /// Current day of week.
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

        /// <summary>
        /// Current week number.
        /// </summary>
        public int WeekNumber { get; set; } = 1;

        /// <summary>
        /// True if simulation has run for this day.
        /// </summary>
        public bool SimulationCompleted { get; set; }

        /// <summary>
        /// The plan governing today's simulation.
        /// </summary>
        public DailyPlan CurrentPlan { get; set; } = new DailyPlan();
    }

    /// <summary>
    /// Defines decisions made for a specific day.
    /// </summary>
    [Serializable]
    public class DailyPlan
    {
        /// <summary>
        /// The day number this plan is for.
        /// </summary>
        public int TargetDayNumber { get; set; }

        /// <summary>
        /// Deterministic seed for the day's simulation.
        /// </summary>
        public int DaySeed { get; set; }

        // Purchasing

        /// <summary>
        /// Amount of coffee to buy.
        /// </summary>
        public int PlannedCoffeePurchase { get; set; }

        /// <summary>
        /// Amount of baked goods to buy.
        /// </summary>
        public int PlannedBakedGoodsPurchase { get; set; }

        /// <summary>
        /// Amount of merch to buy.
        /// </summary>
        public int PlannedThemedMerchPurchase { get; set; }

        /// <summary>
        /// Optional spending limit.
        /// </summary>
        public decimal? DailyBudgetLimit { get; set; }

        // Staffing

        /// <summary>
        /// List of IDs of birds working today.
        /// </summary>
        public List<string> BirdIdsWorking { get; set; } = new List<string>();

        /// <summary>
        /// List of IDs of birds resting today.
        /// </summary>
        public List<string> BirdIdsResting { get; set; } = new List<string>();

        /// <summary>
        /// Player notes for the day.
        /// </summary>
        public string Notes { get; set; }
    }
}