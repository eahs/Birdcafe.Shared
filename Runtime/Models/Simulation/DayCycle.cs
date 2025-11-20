
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Simulation
{
    [Serializable]
    public class DayState
    {
        public int DayNumber { get; set; } = 1;
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
        public int WeekNumber { get; set; } = 1;
        
        public bool SimulationCompleted { get; set; }
        public DailyPlan CurrentPlan { get; set; } = new DailyPlan();
    }

    [Serializable]
    public class DailyPlan
    {
        public int TargetDayNumber { get; set; }
        
        /// <summary>
        /// Deterministic seed for the day's simulation.
        /// </summary>
        public int DaySeed { get; set; }

        // Purchasing
        public int PlannedCoffeePurchase { get; set; }
        public int PlannedBakedGoodsPurchase { get; set; }
        public int PlannedThemedMerchPurchase { get; set; }
        
        public decimal? DailyBudgetLimit { get; set; }
        
        // Staffing
        public List<string> BirdIdsWorking { get; set; } = new List<string>();
        public List<string> BirdIdsResting { get; set; } = new List<string>();
        
        public string Notes { get; set; }
    }
}