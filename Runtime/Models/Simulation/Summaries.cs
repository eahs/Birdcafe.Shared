
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Simulation
{
    /// <summary>
    /// A lightweight summary of a day used for historical reporting.
    /// </summary>
    [Serializable]
    public class DaySummary
    {
        public int DayNumber { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int WeekNumber { get; set; }
        
        /// <summary>
        /// Reference ID to the detailed SimulationResult.
        /// </summary>
        public string DayResultId { get; set; } 
        
        public decimal TotalCareExpenses { get; set; }
        public decimal TotalInventoryExpenses { get; set; }
        public decimal NetProfit { get; set; }
    }

    /// <summary>
    /// Aggregated data for a full week (Sunday-Saturday).
    /// </summary>
    [Serializable]
    public class WeeklySummary
    {
        public int WeekNumber { get; set; }
        public int StartDayNumber { get; set; }
        public int EndDayNumber { get; set; }
        
        public decimal TotalIncome { get; set; }
        public decimal TotalCareExpenses { get; set; }
        public decimal TotalInventoryExpenses { get; set; }
        public decimal NetProfit { get; set; }
        
        /// <summary>
        /// Average health of the flock across the week (1-100).
        /// </summary>
        public float AvgBirdHealth { get; set; }

        /// <summary>
        /// Average mood of the flock across the week (1-100).
        /// </summary>
        public float AvgBirdMood { get; set; }
        
        public float TotalPopularityChange { get; set; }
        public float AvgCustomersPerDay { get; set; }
        
        /// <summary>
        /// Generated narrative text describing the week's performance.
        /// </summary>
        public string NarrativeSummary { get; set; }

        /// <summary>
        /// Key bullet points from the week.
        /// </summary>
        public List<string> Highlights { get; set; } = new List<string>();
    }
}