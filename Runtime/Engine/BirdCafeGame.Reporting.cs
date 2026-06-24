using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Reporting;
using BirdCafe.Shared.ViewModels;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains daily, expense, weekly, and game-over reporting operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Builds the evening daily report from the cached simulation result.
        /// </summary>
        /// <returns>A UI-ready daily report, or an empty report before simulation completes.</returns>
        public DailyReportViewModel GetDailyReport()
        {
            if (_cachedSimResult == null)
            {
                // An empty object is easier for simple front ends to bind than a null report.
                return new DailyReportViewModel();
            }

            var customerStats = _cachedSimResult.Customers;
            var economyStats = _cachedSimResult.Economy;

            var viewModel = new DailyReportViewModel
            {
                DayNumber = _cachedSimResult.DayNumber,
                DayName = _cachedSimResult.DayName,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,

                CustomersServed = customerStats.CustomersServed,
                CustomersLost = customerStats.CustomersLeftUnhappy + customerStats.CustomersLeftNoStock,
                LostWaitTooLong = customerStats.CustomersLeftUnhappy,
                LostNoStock = customerStats.CustomersLeftNoStock,

                TotalRevenue = economyStats.TotalRevenue,
                NetProfit = economyStats.NetProfit,

                CoffeeSold = customerStats.CoffeeSold,
                BakedSold = customerStats.BakedGoodsSold,
                MerchSold = customerStats.MerchSold,

                // Perishable totals equal sold plus wasted units. Merch is nonperishable, so its
                // total includes the remaining stock still available after the work day.
                CoffeeTotal = customerStats.CoffeeSold + customerStats.CoffeeWasted,
                BakedTotal = customerStats.BakedGoodsSold + customerStats.BakedGoodsWasted,
                MerchTotal = customerStats.MerchSold
                    + _controller.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand
            };

            float popularityDelta = _cachedSimResult.Popularity.PopularityDelta;
            if (popularityDelta > 2)
            {
                viewModel.PopularityNarrative = "Popularity is rising! People love the cafe.";
            }
            else if (popularityDelta < -2)
            {
                viewModel.PopularityNarrative = "Popularity is dropping. Customers are unhappy.";
            }
            else
            {
                viewModel.PopularityNarrative = "Popularity is stable.";
            }

            // Each bird receives a compact performance row. Detailed stat deltas remain available
            // in the simulation result but are intentionally not required by this screen contract.
            foreach (var bird in _cachedSimResult.BirdSummaries)
            {
                viewModel.Birds.Add(new BirdPerformanceModel
                {
                    BirdId = bird.BirdId,
                    Name = bird.BirdName,
                    Worked = bird.WorkedToday,
                    CustomersServed = bird.CustomersServed,
                    BecameSick = bird.BecameSick
                });
            }

            return viewModel;
        }

        /// <summary>
        /// Builds a customizable expense report using the shared reporting manager.
        /// </summary>
        /// <param name="request">Options controlling report scope, filters, and grouping.</param>
        /// <returns>A UI-ready expense report.</returns>
        public ExpenseReportViewModel GetExpenseReport(ExpenseReportRequest request)
        {
            // Null means "use the request model's defaults," not "no report."
            return _controller.Reporting.GenerateExpenseReport(request ?? new ExpenseReportRequest());
        }

        /// <summary>
        /// Builds a bird-specific expense report while preserving the shared facade boundary.
        /// </summary>
        /// <param name="birdId">The bird whose related expenses should be selected.</param>
        /// <param name="request">Optional report settings to augment with the bird filter.</param>
        /// <returns>A UI-ready expense report filtered to the requested bird.</returns>
        public ExpenseReportViewModel GetBirdExpenseReport(
            string birdId,
            ExpenseReportRequest request = null)
        {
            // The default emphasizes current-week transaction detail, which is the most useful view
            // when the UI opens reporting from an individual bird's care context.
            var effectiveRequest = request ?? new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CurrentWeek,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            };

            effectiveRequest.BirdId = birdId;
            return _controller.Reporting.GenerateExpenseReport(effectiveRequest);
        }

        /// <summary>
        /// Builds the unified Cost of Care modal payload for the selected time filter.
        /// </summary>
        /// <param name="timeFilter">The time scope to include in the report.</param>
        /// <returns>A view model containing all Cost of Care tab data.</returns>
        public CostOfCareReportViewModel GetCostOfCareReportViewModel(
            CostOfCareReportTimeFilter timeFilter)
        {
            return _controller.Reporting.GenerateCostOfCareReport(timeFilter);
        }

        /// <summary>
        /// Builds weekly report data for the most recently completed week.
        /// </summary>
        /// <returns>A UI-ready weekly report.</returns>
        public WeeklyReportViewModel GetWeeklyReport()
        {
            // CurrentWeekNumber points at the active/upcoming week after rollover, so reporting uses
            // the immediately preceding week number.
            int completedWeek = _controller.CurrentState.CurrentWeekNumber - 1;
            return _controller.Reporting.GenerateWeeklyReport(completedWeek);
        }

        /// <summary>
        /// Completes weekly report flow and resumes day-loop progression.
        /// </summary>
        public void CompleteWeek()
        {
            // Weekly reporting is a presentation pause. Returning to DayLoop allows the next day to
            // proceed through its normal introduction and simulation path.
            _controller.SetPhase(GamePhase.DayLoop);
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Builds final game-over details from the current save state.
        /// </summary>
        /// <returns>A UI-ready game-over summary.</returns>
        public GameOverViewModel GetGameOverDetails()
        {
            var state = _controller.CurrentState;

            // The existing rules distinguish popularity collapse from the economy-based failure
            // path. Reporting only describes the already-resolved terminal state.
            return new GameOverViewModel
            {
                Reason = state.Cafe.Popularity <= 0 ? "Popularity Collapse" : "Bankruptcy",
                DaysSurvived = state.CurrentDayNumber,
                FinalScore = state.Economy.CurrentBalance
            };
        }
    }
}
