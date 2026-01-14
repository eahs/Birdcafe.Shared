
using BirdCafe.Shared.ViewModels;
using System;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Manages the generation of weekly reports and checking for game-over states.
    /// </summary>
    public class ReportingManager
    {
        /// <summary>
        /// Reference to the main controller.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingManager"/> class.
        /// </summary>
        /// <param name="controller">The main game controller.</param>
        public ReportingManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Generates a summary report for a specific week.
        /// </summary>
        /// <param name="weekNumber">The week number to report on.</param>
        /// <returns>A view model containing the report data.</returns>
        public WeeklyReportViewModel GenerateWeeklyReport(int weekNumber)
        {
            var state = _controller.CurrentState;

            // Define Time Window: Assuming Week 1 = Days 1-7, Week 2 = 8-14.
            int startDay = (weekNumber - 1) * 7 + 1;
            int endDay = startDay + 6;

            // Note: In a robust system, we would filter the Ledger by date here.
            // For now, we will calculate based on the PastDayResults which are keyed by Day/Week.

            // Using LINQ to filter the past results to just the days in the requested week.
            var days = state.PastDayResults.Where(d => d.WeekNumber == weekNumber).ToList();

            // Sum up the total revenue from all days in this list.
            decimal totalRevenue = days.Sum(d => d.Economy.TotalRevenue);

            // Calculate approximate profit by taking revenue and subtracting waste and inventory costs recorded in the day results.
            decimal approxProfit = totalRevenue - days.Sum(d => d.Economy.WasteCost + d.Economy.InventoryCost);

            // Calculate Bird Welfare Stats.
            float avgHealth = 0;
            if (state.Birds.Count > 0)
                avgHealth = state.Birds.Average(b => b.Health); // LINQ Average calculation.

            // Generate a simple narrative string based on performance metrics.
            string narrative = "The cafe ran smoothly.";
            if (approxProfit < 0) narrative = "We lost money this week. We need to cut costs.";
            else if (avgHealth < 40) narrative = "Profits are okay, but the birds are exhausted.";
            else if (approxProfit > 500) narrative = "An outstanding week! The birds are happy and rich.";

            return new WeeklyReportViewModel
            {
                WeekNumber = weekNumber,
                TotalProfit = approxProfit,
                AvgBirdHealth = (int)avgHealth,
                Narrative = narrative
            };
        }

        /// <summary>
        /// Checks if the player has met any failure conditions (Bankruptcy or bad Popularity).
        /// </summary>
        /// <returns>True if the game is over; otherwise, false.</returns>
        public bool CheckGameOver()
        {
            var state = _controller.CurrentState;

            // Condition 1: Bankruptcy 
            // Calculated as: Balance is less than cost of (1 Coffee + 1 Food) AND we have no Coffee left to sell.
            decimal minCost = state.Config.BasePriceCoffee + state.Config.BaselineBirdFoodCost;
            if (state.Economy.CurrentBalance < minCost && state.Cafe.Inventory.Coffee.QuantityOnHand == 0)
            {
                return true;
            }

            // Condition 2: Popularity Collapse
            // If popularity hits 0, no customers will come.
            if (state.Cafe.Popularity <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper to map real time dates to game day numbers.
        /// </summary>
        /// <param name="start">The start date of the game.</param>
        /// <param name="current">The current transaction date.</param>
        /// <returns>The calculated day number.</returns>
        private int GetDayFromDate(DateTime start, DateTime current)
        {
            return (current - start).Days + 1;
        }
    }
}