using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Top-level view model used to drive the "Cost of Care Report" modal.
    /// </summary>
    [Serializable]
    public class CostOfCareReportViewModel
    {
        /// <summary>
        /// Selected time filter currently applied to this report payload.
        /// </summary>
        public CostOfCareReportTimeFilter TimeFilter { get; set; }

        /// <summary>
        /// Human-readable label for the selected report range.
        /// </summary>
        public string ScopeText { get; set; }

        /// <summary>
        /// Data for the Overview tab.
        /// </summary>
        public CostOfCareOverviewViewModel Overview { get; set; } = new CostOfCareOverviewViewModel();

        /// <summary>
        /// Data for the Care Costs tab.
        /// </summary>
        public CostOfCareCategoryBreakdownViewModel CareCosts { get; set; } = new CostOfCareCategoryBreakdownViewModel();

        /// <summary>
        /// Data for the Bird Breakdown tab.
        /// </summary>
        public CostOfCareBirdBreakdownViewModel BirdBreakdown { get; set; } = new CostOfCareBirdBreakdownViewModel();

        /// <summary>
        /// Data for the Cafe Sales tab.
        /// </summary>
        public CostOfCareCafeSalesViewModel CafeSales { get; set; } = new CostOfCareCafeSalesViewModel();
    }

    /// <summary>
    /// Overview totals shown in the first tab of the Cost of Care modal.
    /// </summary>
    [Serializable]
    public class CostOfCareOverviewViewModel
    {
        /// <summary>
        /// Current money balance from the save-state economy model.
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Positive sales total for the selected scope.
        /// </summary>
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Total expenses (absolute value) from ledger entries with negative amounts.
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Net result for the selected scope (sales minus expenses).
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// Total of care-category expenses.
        /// </summary>
        public decimal CareExpensesTotal { get; set; }

        /// <summary>
        /// Total of inventory restock expenses.
        /// </summary>
        public decimal InventoryExpensesTotal { get; set; }

        /// <summary>
        /// Total expenses attributed to individual birds, including valued usage rows.
        /// </summary>
        public decimal BirdAttributedExpensesTotal { get; set; }

        /// <summary>
        /// Total expenses that are cafe-wide and not tied to a specific bird.
        /// </summary>
        public decimal CafeWideExpensesTotal { get; set; }
    }

    /// <summary>
    /// Category-grouped cost data for the Care Costs tab.
    /// </summary>
    [Serializable]
    public class CostOfCareCategoryBreakdownViewModel
    {
        /// <summary>
        /// Grouped category rows for display.
        /// </summary>
        public List<CostOfCareCategoryRowViewModel> Categories { get; set; } = new List<CostOfCareCategoryRowViewModel>();

        /// <summary>
        /// Sum of all category rows.
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// One category row in the Care Costs tab.
    /// </summary>
    [Serializable]
    public class CostOfCareCategoryRowViewModel
    {
        /// <summary>
        /// Expense category represented by this row.
        /// </summary>
        public ExpenseCategory Category { get; set; }

        /// <summary>
        /// UI-friendly category text.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Total amount for this category.
        /// </summary>
        public decimal TotalCost { get; set; }
    }

    /// <summary>
    /// Per-bird cost table for the Bird Breakdown tab.
    /// </summary>
    [Serializable]
    public class CostOfCareBirdBreakdownViewModel
    {
        /// <summary>
        /// One row per bird.
        /// </summary>
        public List<CostOfCareBirdRowViewModel> Birds { get; set; } = new List<CostOfCareBirdRowViewModel>();

        /// <summary>
        /// Total of all bird rows.
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Cost data for a single bird in the Bird Breakdown tab.
    /// </summary>
    [Serializable]
    public class CostOfCareBirdRowViewModel
    {
        /// <summary>
        /// Persistent bird identifier.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Display name of the bird.
        /// </summary>
        public string BirdName { get; set; }

        /// <summary>
        /// Food and supplies costs attributed to this bird.
        /// </summary>
        public decimal FoodAndSuppliesCost { get; set; }

        /// <summary>
        /// Vet care costs attributed to this bird.
        /// </summary>
        public decimal VetCareCost { get; set; }

        /// <summary>
        /// Toys and activities costs attributed to this bird.
        /// </summary>
        public decimal ToysAndActivitiesCost { get; set; }

        /// <summary>
        /// Acquisition or purchase costs attributed to this bird.
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Additional bird-attributed costs not covered by primary buckets.
        /// </summary>
        public decimal OtherCosts { get; set; }

        /// <summary>
        /// Total bird-attributed cost.
        /// </summary>
        public decimal TotalCost { get; set; }
    }

    /// <summary>
    /// Cafe sales and throughput data for the Cafe Sales tab.
    /// </summary>
    [Serializable]
    public class CostOfCareCafeSalesViewModel
    {
        /// <summary>
        /// Total sales for the selected scope.
        /// </summary>
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Coffee sales amount.
        /// </summary>
        public decimal CoffeeSales { get; set; }

        /// <summary>
        /// Baked goods sales amount.
        /// </summary>
        public decimal BakedGoodsSales { get; set; }

        /// <summary>
        /// Merchandise sales amount.
        /// </summary>
        public decimal MerchSales { get; set; }

        /// <summary>
        /// Coffee units sold.
        /// </summary>
        public int CoffeeUnitsSold { get; set; }

        /// <summary>
        /// Baked goods units sold.
        /// </summary>
        public int BakedGoodsUnitsSold { get; set; }

        /// <summary>
        /// Merchandise units sold.
        /// </summary>
        public int MerchUnitsSold { get; set; }

        /// <summary>
        /// Number of customers served.
        /// </summary>
        public int CustomersServed { get; set; }

        /// <summary>
        /// Number of customers lost due to wait/no-stock outcomes.
        /// </summary>
        public int CustomersLost { get; set; }
    }

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
