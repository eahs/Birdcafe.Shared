
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
        /// <summary>
        /// Day number.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Day of week.
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Week number.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Reference ID to the detailed SimulationResult.
        /// </summary>
        public string DayResultId { get; set; }

        /// <summary>
        /// Care expenses sum.
        /// </summary>
        public decimal TotalCareExpenses { get; set; }

        /// <summary>
        /// Inventory expenses sum.
        /// </summary>
        public decimal TotalInventoryExpenses { get; set; }

        /// <summary>
        /// Profit.
        /// </summary>
        public decimal NetProfit { get; set; }
    }

    /// <summary>
    /// Aggregated data for a full week (Sunday-Saturday).
    /// </summary>
    [Serializable]
    public class WeeklySummary
    {
        /// <summary>
        /// Week number.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Start day number.
        /// </summary>
        public int StartDayNumber { get; set; }

        /// <summary>
        /// End day number.
        /// </summary>
        public int EndDayNumber { get; set; }

        /// <summary>
        /// Total income.
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// Total care cost.
        /// </summary>
        public decimal TotalCareExpenses { get; set; }

        /// <summary>
        /// Total inv cost.
        /// </summary>
        public decimal TotalInventoryExpenses { get; set; }

        /// <summary>
        /// Profit.
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// Average health of the flock across the week (1-100).
        /// </summary>
        public float AvgBirdHealth { get; set; }

        /// <summary>
        /// Average mood of the flock across the week (1-100).
        /// </summary>
        public float AvgBirdMood { get; set; }

        /// <summary>
        /// Pop change.
        /// </summary>
        public float TotalPopularityChange { get; set; }

        /// <summary>
        /// Avg daily customers.
        /// </summary>
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