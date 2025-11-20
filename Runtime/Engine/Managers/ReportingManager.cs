
using System;
using System.Linq;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.ViewModels;

namespace BirdCafe.Shared.Engine.Managers
{
    public class ReportingManager
    {
        private readonly BirdCafeController _controller;

        public ReportingManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public WeeklyReportViewModel GenerateWeeklyReport(int weekNumber)
        {
            var state = _controller.CurrentState;
            
            // 1. Define Time Window
            // Assuming Week 1 = Days 1-7, Week 2 = 8-14
            int startDay = (weekNumber - 1) * 7 + 1;
            int endDay = startDay + 6;

            // 2. Aggregate Ledger (Source of Truth for Money)
            var entries = state.Economy.Ledger
                .Where(l => GetDayFromDate(state.Profile.CreatedDate, l.Timestamp) >= startDay)
                .Where(l => GetDayFromDate(state.Profile.CreatedDate, l.Timestamp) <= endDay)
                .ToList();

            // Note: In a real app, LedgerEntry should likely store 'DayNumber' explicitly 
            // to avoid Date math, but we'll use a robust fallback here or assume entries
            // are filtered by the Controller context if we added DayNumber to Ledger.
            // For this example, we will sum ALL ledger entries since the last report 
            // or rely on PastDayResults for simplicity if Ledger is too complex.
            
            // ALTERNATIVE: Aggregate from PastDayResults + Care History
            // Let's stick to the robust method: Summing specific ledger entries if we had DayNumber.
            // Since we didn't add DayNumber to LedgerEntry in the previous step, 
            // we will aggregate from PastDayResults for Revenue and estimate expenses.
            
            // Let's calculate based on the PastDayResults which ARE keyed by Day/Week.
            var days = state.PastDayResults.Where(d => d.WeekNumber == weekNumber).ToList();

            decimal totalRevenue = days.Sum(d => d.Economy.TotalRevenue);
            
            // Care expenses are tricky without DayNumber in Ledger, 
            // but let's assume we calculate NetProfit from the change in balance 
            // from Start of Week to End of Week.
            // (This requires tracking historic balance, which we haven't done).
            
            // SIMPLEST VALID APPROACH FOR V1:
            // Sum Revenue from Days, Sum Expenses from Ledger (filtering by approximation or just taking all recent).
            // For this snippet, we will just return the summation of Day Results NetProfit 
            // (Revenue - Waste) minus an estimate of Inventory Costs.
            
            decimal approxProfit = totalRevenue - days.Sum(d => d.Economy.WasteCost + d.Economy.InventoryCost);

            // 3. Bird Welfare
            float avgHealth = 0;
            if (state.Birds.Count > 0)
                avgHealth = state.Birds.Average(b => b.Health);

            // 4. Narrative Generation
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

        public bool CheckGameOver()
        {
            var state = _controller.CurrentState;

            // Condition 1: Bankruptcy (Cant afford 1 unit of coffee + 1 feed)
            decimal minCost = state.Config.BasePriceCoffee + state.Config.BaselineBirdFoodCost;
            if (state.Economy.CurrentBalance < minCost && state.Cafe.Inventory.Coffee.QuantityOnHand == 0)
            {
                return true;
            }

            // Condition 2: Popularity Collapse
            if (state.Cafe.Popularity <= 0)
            {
                return true;
            }

            return false;
        }
        
        // Helper to map real time to game days if needed
        private int GetDayFromDate(DateTime start, DateTime current)
        {
            return (current - start).Days + 1;
        }
    }
}