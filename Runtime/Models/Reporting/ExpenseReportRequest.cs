using BirdCafe.Shared.Enums;
using System;

namespace BirdCafe.Shared.Models.Reporting
{
    /// <summary>
    /// Request data for building a customizable expense report from ledger history.
    /// </summary>
    [Serializable]
    public class ExpenseReportRequest
    {
        /// <summary>
        /// Which in-game time window should be reported.
        /// </summary>
        public ExpenseReportScope Scope { get; set; } = ExpenseReportScope.CurrentDay;

        /// <summary>
        /// How the matching expense rows should be grouped.
        /// </summary>
        public ExpenseReportGroupBy GroupBy { get; set; } = ExpenseReportGroupBy.ByTransaction;

        /// <summary>
        /// Inclusive start day for custom day-range reports.
        /// </summary>
        public int? StartDayNumber { get; set; }

        /// <summary>
        /// Inclusive end day for custom day-range reports.
        /// </summary>
        public int? EndDayNumber { get; set; }

        /// <summary>
        /// Whether care expenses should be included.
        /// </summary>
        public bool IncludeCareExpenses { get; set; } = true;

        /// <summary>
        /// Whether planning inventory expenses should be included.
        /// </summary>
        public bool IncludeInventoryExpenses { get; set; } = true;

        /// <summary>
        /// Optional bird filter for bird-specific reporting.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Optional explicit category filter.
        /// </summary>
        public ExpenseCategory? ExpenseCategory { get; set; }

        /// <summary>
        /// Whether to compute running totals in display order.
        /// </summary>
        public bool IncludeRunningTotal { get; set; }
    }
}
