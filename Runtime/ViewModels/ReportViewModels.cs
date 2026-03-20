using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Data for the Weekly Report screen.
    /// </summary>
    [Serializable]
    public class WeeklyReportViewModel
    {
        /// <summary>
        /// The week number being reported.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Net profit for the week.
        /// </summary>
        public decimal TotalProfit { get; set; }

        /// <summary>
        /// Average health of the flock.
        /// </summary>
        public int AvgBirdHealth { get; set; }

        /// <summary>
        /// Text summary of performance.
        /// </summary>
        public string Narrative { get; set; }
    }

    /// <summary>
    /// View model for the additive customizable expense report feature.
    /// </summary>
    [Serializable]
    public class ExpenseReportViewModel
    {
        /// <summary>
        /// Report title suitable for screen headers.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Human-readable explanation of the active report scope.
        /// </summary>
        public string ScopeText { get; set; }

        /// <summary>
        /// Sum of care-related expenses included in this report.
        /// </summary>
        public decimal TotalCareExpenses { get; set; }

        /// <summary>
        /// Sum of expenses linked to a specific bird.
        /// </summary>
        public decimal TotalBirdExpenses { get; set; }

        /// <summary>
        /// Sum of cafe-wide expenses not linked to a bird.
        /// </summary>
        public decimal TotalCafeExpenses { get; set; }

        /// <summary>
        /// Sum of all included expenses.
        /// </summary>
        public decimal GrandTotalExpenses { get; set; }

        /// <summary>
        /// UI-ready report rows in display order.
        /// </summary>
        public List<ExpenseReportRowViewModel> Rows { get; set; } = new List<ExpenseReportRowViewModel>();

        /// <summary>
        /// Non-fatal notes for incomplete or overly restrictive requests.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// A single row in an expense report.
    /// </summary>
    [Serializable]
    public class ExpenseReportRowViewModel
    {
        /// <summary>
        /// Primary row label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Secondary supporting text.
        /// </summary>
        public string SecondaryLabel { get; set; }

        /// <summary>
        /// Positive display amount for this row.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Running total through this row when requested.
        /// </summary>
        public decimal RunningTotal { get; set; }

        /// <summary>
        /// Friendly category name for rendering.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Related bird display name when applicable.
        /// </summary>
        public string BirdName { get; set; }

        /// <summary>
        /// Related day number when applicable.
        /// </summary>
        public int DayNumber { get; set; }
    }

    /// <summary>
    /// Data for the Game Over screen.
    /// </summary>
    [Serializable]
    public class GameOverViewModel
    {
        /// <summary>
        /// Why the game ended (Bankruptcy/Popularity).
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// How many days the player lasted.
        /// </summary>
        public int DaysSurvived { get; set; }

        /// <summary>
        /// Final money score.
        /// </summary>
        public decimal FinalScore { get; set; }
    }
}
