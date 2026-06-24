using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Reporting;
using BirdCafe.Shared.ViewModels;
using System;
using System.Collections.Generic;
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
            var days = state.PastDayResults.Where(d => d.WeekNumber == weekNumber).ToList();

            decimal totalRevenue = days.Sum(d => d.Economy.TotalRevenue);
            decimal totalExpenses = GetLedgerEntriesForExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = ((weekNumber - 1) * 7) + 1,
                EndDayNumber = ((weekNumber - 1) * 7) + 7,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            }).Sum(GetExpenseAmount);

            decimal weeklyProfit = totalRevenue - totalExpenses;
            float avgHealth = state.Birds.Count > 0 ? state.Birds.Average(b => b.Health) : 0;

            string narrative = "The cafe ran smoothly.";
            if (weeklyProfit < 0) narrative = "We lost money this week. We need to cut costs.";
            else if (avgHealth < 40) narrative = "Profits are okay, but the birds are exhausted.";
            else if (weeklyProfit > 500) narrative = "An outstanding week! The birds are happy and rich.";

            return new WeeklyReportViewModel
            {
                WeekNumber = weekNumber,
                TotalProfit = weeklyProfit,
                AvgBirdHealth = (int)avgHealth,
                Narrative = narrative
            };
        }

        /// <summary>
        /// Builds a customizable expense report using ledger entries as the source of truth.
        /// </summary>
        /// <param name="request">Report options that control scope, grouping, and filters.</param>
        /// <returns>A UI-ready expense report.</returns>
        public ExpenseReportViewModel GenerateExpenseReport(ExpenseReportRequest request)
        {
            var effectiveRequest = request ?? new ExpenseReportRequest();
            var report = new ExpenseReportViewModel
            {
                Title = BuildTitle(effectiveRequest),
                ScopeText = BuildScopeText(effectiveRequest)
            };

            if (!effectiveRequest.IncludeCareExpenses && !effectiveRequest.IncludeInventoryExpenses)
            {
                report.Warnings.Add("Both care and inventory expense filters are disabled, so no categorized expenses can be shown.");
            }

            var entries = GetLedgerEntriesForExpenseReport(effectiveRequest).ToList();
            report.TotalCareExpenses = entries.Where(IsCareExpense).Sum(GetExpenseAmount);
            report.TotalBirdExpenses = entries.Where(e => !string.IsNullOrEmpty(e.RelatedBirdId)).Sum(GetExpenseAmount);
            report.TotalCafeExpenses = entries.Where(e => string.IsNullOrEmpty(e.RelatedBirdId)).Sum(GetExpenseAmount);
            report.GrandTotalExpenses = entries.Sum(GetExpenseAmount);

            report.Rows = effectiveRequest.GroupBy == ExpenseReportGroupBy.ByTransaction
                ? BuildTransactionRows(entries)
                : BuildGroupedRows(entries, effectiveRequest.GroupBy);

            if (effectiveRequest.IncludeRunningTotal)
            {
                ApplyRunningTotals(report.Rows);
            }

            if (effectiveRequest.Scope == ExpenseReportScope.CustomDayRange &&
                (!effectiveRequest.StartDayNumber.HasValue || !effectiveRequest.EndDayNumber.HasValue))
            {
                report.Warnings.Add("Custom day-range reports require both a start day and an end day.");
            }

            if (effectiveRequest.Scope == ExpenseReportScope.CustomDayRange &&
                effectiveRequest.StartDayNumber.HasValue &&
                effectiveRequest.EndDayNumber.HasValue &&
                effectiveRequest.StartDayNumber.Value > effectiveRequest.EndDayNumber.Value)
            {
                report.Warnings.Add("The custom day range start is after the end, so no expenses matched the request.");
            }

            if (entries.Count == 0)
            {
                report.Warnings.Add("No expense entries matched the current report filters.");
            }

            return report;
        }

        /// <summary>
        /// Builds the unified "Cost of Care Report" payload used by the UI modal tabs.
        /// </summary>
        /// <param name="timeFilter">The selected report time filter.</param>
        /// <returns>A single view model that contains overview, care costs, bird breakdown, and cafe sales data.</returns>
        public CostOfCareReportViewModel GenerateCostOfCareReport(CostOfCareReportTimeFilter timeFilter)
        {
            var state = _controller.CurrentState;
            var (startDay, endDay) = ResolveDayRangeForCostOfCareReport(timeFilter);

            var scopedLedgerEntries = state.Economy.Ledger
                .Where(entry => entry.DayNumber >= startDay && entry.DayNumber <= endDay)
                .OrderBy(entry => entry.DayNumber)
                .ThenBy(entry => entry.Timestamp)
                .ToList();

            var expenseEntries = scopedLedgerEntries.Where(entry => entry.Amount < 0m).ToList();
            var directBirdExpenseEntries = expenseEntries.Where(entry => !string.IsNullOrEmpty(entry.RelatedBirdId)).ToList();
            var cafeWideExpenseEntries = expenseEntries.Where(entry => string.IsNullOrEmpty(entry.RelatedBirdId)).ToList();
            var usageEntries = scopedLedgerEntries
                .Where(entry => entry.Amount == 0m && !string.IsNullOrEmpty(entry.RelatedBirdId) && !string.IsNullOrEmpty(entry.ItemId))
                .ToList();

            var birdRows = BuildCostOfCareBirdRows(state, directBirdExpenseEntries, usageEntries);
            var birdAttributedTotal = birdRows.Sum(row => row.TotalCost);

            var report = new CostOfCareReportViewModel
            {
                TimeFilter = timeFilter,
                ScopeText = BuildCostOfCareScopeText(timeFilter, startDay, endDay),
                Overview = new CostOfCareOverviewViewModel
                {
                    CurrentBalance = state.Economy.CurrentBalance,
                    TotalSales = scopedLedgerEntries.Where(entry => entry.Amount > 0m).Sum(entry => entry.Amount),
                    TotalExpenses = expenseEntries.Sum(GetExpenseAmount),
                    NetProfit = scopedLedgerEntries.Sum(entry => entry.Amount),
                    CareExpensesTotal = expenseEntries.Where(IsCareCostCategory).Sum(GetExpenseAmount),
                    InventoryExpensesTotal = expenseEntries.Where(IsInventoryExpense).Sum(GetExpenseAmount),
                    BirdAttributedExpensesTotal = birdAttributedTotal,
                    CafeWideExpensesTotal = cafeWideExpenseEntries.Sum(GetExpenseAmount)
                },
                CareCosts = BuildCostOfCareCategoryBreakdown(expenseEntries),
                BirdBreakdown = new CostOfCareBirdBreakdownViewModel
                {
                    Birds = birdRows,
                    Total = birdAttributedTotal
                },
                CafeSales = BuildCostOfCareCafeSales(state, startDay, endDay)
            };

            return report;
        }

        /// <summary>
        /// Checks if the player has met any failure conditions (Bankruptcy or bad Popularity).
        /// </summary>
        /// <returns>True if the game is over; otherwise, false.</returns>
        public bool CheckGameOver()
        {
            var state = _controller.CurrentState;
            decimal minCost = state.Config.BasePriceCoffee + state.Config.BaselineBirdFoodCost;
            if (state.Economy.CurrentBalance < minCost && state.Cafe.Inventory.Coffee.QuantityOnHand == 0)
            {
                return true;
            }

            if (state.Cafe.Popularity <= 0)
            {
                return true;
            }

            return false;
        }

        private IEnumerable<LedgerEntry> GetLedgerEntriesForExpenseReport(ExpenseReportRequest request)
        {
            var state = _controller.CurrentState;
            var range = ResolveDayRange(request);

            return state.Economy.Ledger
                .Where(entry => entry.Amount < 0)
                .Where(entry => entry.DayNumber >= range.startDay && entry.DayNumber <= range.endDay)
                .Where(entry => MatchesRequestFilters(entry, request))
                .OrderBy(entry => entry.DayNumber)
                .ThenBy(entry => entry.Timestamp)
                .ThenBy(entry => entry.ShortDescription ?? entry.Reason ?? string.Empty)
                .ToList();
        }

        private bool MatchesRequestFilters(LedgerEntry entry, ExpenseReportRequest request)
        {
            if (request.ExpenseCategory.HasValue && entry.Category != request.ExpenseCategory.Value)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(request.BirdId) && entry.RelatedBirdId != request.BirdId)
            {
                return false;
            }

            bool isCareExpense = IsCareExpense(entry);
            bool isInventoryExpense = IsInventoryExpense(entry);

            if (!request.IncludeCareExpenses && !request.IncludeInventoryExpenses)
            {
                return false;
            }

            if (request.IncludeCareExpenses && request.IncludeInventoryExpenses)
            {
                return true;
            }

            if (request.IncludeCareExpenses)
            {
                return isCareExpense;
            }

            return isInventoryExpense;
        }

        private List<ExpenseReportRowViewModel> BuildTransactionRows(List<LedgerEntry> entries)
        {
            return entries
                .Select(entry => new ExpenseReportRowViewModel
                {
                    Label = entry.ShortDescription ?? entry.Reason ?? "Expense",
                    SecondaryLabel = $"Day {entry.DayNumber} / Week {entry.WeekNumber}",
                    Amount = GetExpenseAmount(entry),
                    CategoryText = entry.Category.ToString(),
                    BirdName = GetBirdName(entry.RelatedBirdId),
                    DayNumber = entry.DayNumber
                })
                .ToList();
        }

        private List<ExpenseReportRowViewModel> BuildGroupedRows(List<LedgerEntry> entries, ExpenseReportGroupBy groupBy)
        {
            switch (groupBy)
            {
                case ExpenseReportGroupBy.ByDay:
                    return entries
                        .GroupBy(e => e.DayNumber)
                        .OrderBy(group => group.Key)
                        .Select(group => new ExpenseReportRowViewModel
                        {
                            Label = $"Day {group.Key}",
                            SecondaryLabel = $"{group.Count()} expense entries",
                            Amount = group.Sum(GetExpenseAmount),
                            CategoryText = "Mixed",
                            DayNumber = group.Key
                        })
                        .ToList();

                case ExpenseReportGroupBy.ByCategory:
                    return entries
                        .GroupBy(e => e.Category)
                        .OrderBy(group => group.Key.ToString())
                        .Select(group => new ExpenseReportRowViewModel
                        {
                            Label = group.Key.ToString(),
                            SecondaryLabel = $"{group.Count()} expense entries",
                            Amount = group.Sum(GetExpenseAmount),
                            CategoryText = group.Key.ToString(),
                            DayNumber = group.Min(e => e.DayNumber)
                        })
                        .ToList();

                case ExpenseReportGroupBy.ByBird:
                    return entries
                        .GroupBy(e => string.IsNullOrEmpty(e.RelatedBirdId) ? string.Empty : e.RelatedBirdId)
                        .OrderBy(group => GetBirdName(group.Key))
                        .Select(group => new ExpenseReportRowViewModel
                        {
                            Label = string.IsNullOrEmpty(group.Key) ? "Cafe-wide" : GetBirdName(group.Key),
                            SecondaryLabel = $"{group.Count()} expense entries",
                            Amount = group.Sum(GetExpenseAmount),
                            CategoryText = string.IsNullOrEmpty(group.Key) ? "General" : "Bird-linked",
                            BirdName = string.IsNullOrEmpty(group.Key) ? null : GetBirdName(group.Key),
                            DayNumber = group.Min(e => e.DayNumber)
                        })
                        .ToList();

                default:
                    return BuildTransactionRows(entries);
            }
        }

        private void ApplyRunningTotals(List<ExpenseReportRowViewModel> rows)
        {
            decimal runningTotal = 0m;
            foreach (var row in rows)
            {
                runningTotal += row.Amount;
                row.RunningTotal = runningTotal;
            }
        }

        private bool IsCareExpense(LedgerEntry entry)
        {
            return !string.IsNullOrEmpty(entry.RelatedBirdId) &&
                   (entry.Category == ExpenseCategory.FoodAndSupplies ||
                    entry.Category == ExpenseCategory.VetCare ||
                    entry.Category == ExpenseCategory.ToysAndActivities);
        }

        private bool IsInventoryExpense(LedgerEntry entry)
        {
            return entry.Category == ExpenseCategory.InventoryCoffee ||
                   entry.Category == ExpenseCategory.InventoryBakedGoods ||
                   entry.Category == ExpenseCategory.InventoryThemedMerch;
        }

        private bool IsCareCostCategory(LedgerEntry entry)
        {
            return entry.Category == ExpenseCategory.FoodAndSupplies ||
                   entry.Category == ExpenseCategory.VetCare ||
                   entry.Category == ExpenseCategory.ToysAndActivities ||
                   entry.Category == ExpenseCategory.UpgradesAndCustomization ||
                   entry.Category == ExpenseCategory.Miscellaneous;
        }

        private List<CostOfCareBirdRowViewModel> BuildCostOfCareBirdRows(
            GameSave state,
            List<LedgerEntry> directBirdExpenseEntries,
            List<LedgerEntry> usageEntries)
        {
            var birdCostMap = state.Birds.ToDictionary(bird => bird.Id, bird => new CostOfCareBirdRowViewModel
            {
                BirdId = bird.Id,
                BirdName = bird.Name
            });

            foreach (var entry in directBirdExpenseEntries)
            {
                if (!birdCostMap.TryGetValue(entry.RelatedBirdId, out var row))
                {
                    continue;
                }

                ApplyBirdCost(row, entry.Category, GetExpenseAmount(entry));
            }

            foreach (var entry in usageEntries)
            {
                if (!birdCostMap.TryGetValue(entry.RelatedBirdId, out var row))
                {
                    continue;
                }

                var catalogValue = ResolveSupplyValueFromCatalog(entry.ItemId, entry.Category);
                if (catalogValue <= 0m)
                {
                    continue;
                }

                ApplyBirdCost(row, entry.Category, catalogValue);
            }

            foreach (var row in birdCostMap.Values)
            {
                row.TotalCost = row.FoodAndSuppliesCost + row.VetCareCost + row.ToysAndActivitiesCost + row.AcquisitionCost + row.OtherCosts;
            }

            return birdCostMap.Values
                .OrderBy(row => row.BirdName)
                .ToList();
        }

        private void ApplyBirdCost(CostOfCareBirdRowViewModel row, ExpenseCategory category, decimal amount)
        {
            if (amount <= 0m)
            {
                return;
            }

            switch (category)
            {
                case ExpenseCategory.FoodAndSupplies:
                    row.FoodAndSuppliesCost += amount;
                    return;
                case ExpenseCategory.VetCare:
                    row.VetCareCost += amount;
                    return;
                case ExpenseCategory.ToysAndActivities:
                    row.ToysAndActivitiesCost += amount;
                    return;
                case ExpenseCategory.UpgradesAndCustomization:
                    // Bird purchases are logged as upgrades/customization with a bird id.
                    row.AcquisitionCost += amount;
                    return;
                default:
                    row.OtherCosts += amount;
                    return;
            }
        }

        private decimal ResolveSupplyValueFromCatalog(string itemId, ExpenseCategory expenseCategory)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0m;
            }

            var match = PetStoreCatalog.GetSupplyOffers()
                .FirstOrDefault(supply => supply.ItemId == itemId && supply.ExpenseCategory == expenseCategory)
                ?? PetStoreCatalog.GetSupplyOffers().FirstOrDefault(supply => supply.ItemId == itemId);

            return match?.Price ?? 0m;
        }

        private CostOfCareCategoryBreakdownViewModel BuildCostOfCareCategoryBreakdown(List<LedgerEntry> expenseEntries)
        {
            var rows = expenseEntries
                .Where(IsCareCostCategory)
                .GroupBy(entry => entry.Category)
                .OrderBy(group => group.Key.ToString())
                .Select(group => new CostOfCareCategoryRowViewModel
                {
                    Category = group.Key,
                    CategoryText = group.Key.ToString(),
                    TotalCost = group.Sum(GetExpenseAmount)
                })
                .ToList();

            return new CostOfCareCategoryBreakdownViewModel
            {
                Categories = rows,
                Total = rows.Sum(row => row.TotalCost)
            };
        }

        private CostOfCareCafeSalesViewModel BuildCostOfCareCafeSales(GameSave state, int startDay, int endDay)
        {
            var scopedResults = state.PastDayResults
                .Where(result => result.DayNumber >= startDay && result.DayNumber <= endDay)
                .OrderBy(result => result.DayNumber)
                .ToList();

            decimal coffeeSales = 0m;
            decimal bakedGoodsSales = 0m;
            decimal merchSales = 0m;

            foreach (var result in scopedResults)
            {
                foreach (var eventItem in result.Timeline.Where(item => item.EventType == SimulationTimelineEventType.ServiceCompleted && item.Product.HasValue))
                {
                    if (eventItem.Product == ProductType.Coffee)
                    {
                        coffeeSales += eventItem.MoneyDelta;
                    }
                    else if (eventItem.Product == ProductType.BakedGoods)
                    {
                        bakedGoodsSales += eventItem.MoneyDelta;
                    }
                    else if (eventItem.Product == ProductType.ThemedMerch)
                    {
                        merchSales += eventItem.MoneyDelta;
                    }
                }
            }

            return new CostOfCareCafeSalesViewModel
            {
                TotalSales = scopedResults.Sum(result => result.Economy.TotalRevenue),
                CoffeeSales = coffeeSales,
                BakedGoodsSales = bakedGoodsSales,
                MerchSales = merchSales,
                CoffeeUnitsSold = scopedResults.Sum(result => result.Customers.CoffeeSold),
                BakedGoodsUnitsSold = scopedResults.Sum(result => result.Customers.BakedGoodsSold),
                MerchUnitsSold = scopedResults.Sum(result => result.Customers.MerchSold),
                CustomersServed = scopedResults.Sum(result => result.Customers.CustomersServed),
                CustomersLost = scopedResults.Sum(result => result.Customers.CustomersLeftUnhappy + result.Customers.CustomersLeftNoStock)
            };
        }

        private (int startDay, int endDay) ResolveDayRangeForCostOfCareReport(CostOfCareReportTimeFilter timeFilter)
        {
            var state = _controller.CurrentState;
            int currentDay = state.CurrentDayNumber;
            int currentWeek = GetCurrentWeekNumberForReporting();
            int weekStartDay = ((currentWeek - 1) * 7) + 1;
            int weekEndDay = ((currentWeek - 1) * 7) + 7;

            switch (timeFilter)
            {
                case CostOfCareReportTimeFilter.ThisWeek:
                    return (weekStartDay, weekEndDay);
                case CostOfCareReportTimeFilter.AllTime:
                    return (1, currentDay);
                default:
                    return (currentDay, currentDay);
            }
        }

        private string BuildCostOfCareScopeText(CostOfCareReportTimeFilter timeFilter, int startDay, int endDay)
        {
            switch (timeFilter)
            {
                case CostOfCareReportTimeFilter.ThisWeek:
                    return $"Week {GetCurrentWeekNumberForReporting()} (Days {startDay}-{endDay})";
                case CostOfCareReportTimeFilter.AllTime:
                    return $"All Time (Days {startDay}-{endDay})";
                default:
                    return $"Day {startDay}";
            }
        }

        private (int startDay, int endDay) ResolveDayRange(ExpenseReportRequest request)
        {
            var state = _controller.CurrentState;
            switch (request.Scope)
            {
                case ExpenseReportScope.CurrentWeek:
                    int weekNumber = GetCurrentWeekNumberForReporting();
                    return (((weekNumber - 1) * 7) + 1, ((weekNumber - 1) * 7) + 7);

                case ExpenseReportScope.CustomDayRange:
                    int startDay = request.StartDayNumber ?? int.MaxValue;
                    int endDay = request.EndDayNumber ?? int.MinValue;
                    return (startDay, endDay);

                default:
                    return (state.CurrentDayNumber, state.CurrentDayNumber);
            }
        }

        private int GetCurrentWeekNumberForReporting()
        {
            var state = _controller.CurrentState;
            if (_controller.CurrentPhase == GamePhase.Reporting && state.CurrentWeekNumber > 1)
            {
                return state.CurrentWeekNumber - 1;
            }

            return state.CurrentWeekNumber;
        }

        private decimal GetExpenseAmount(LedgerEntry entry)
        {
            return Math.Abs(entry.Amount);
        }

        private string GetBirdName(string birdId)
        {
            if (string.IsNullOrEmpty(birdId))
            {
                return null;
            }

            return _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId)?.Name ?? "Unknown Bird";
        }

        private string BuildTitle(ExpenseReportRequest request)
        {
            if (!string.IsNullOrEmpty(request.BirdId))
            {
                return $"Expense Report - {GetBirdName(request.BirdId)}";
            }

            return "Expense Report";
        }

        private string BuildScopeText(ExpenseReportRequest request)
        {
            switch (request.Scope)
            {
                case ExpenseReportScope.CurrentWeek:
                    return $"Week {GetCurrentWeekNumberForReporting()}";
                case ExpenseReportScope.CustomDayRange:
                    if (request.StartDayNumber.HasValue && request.EndDayNumber.HasValue)
                    {
                        return $"Days {request.StartDayNumber.Value}-{request.EndDayNumber.Value}";
                    }
                    return "Custom Day Range";
                default:
                    return $"Day {_controller.CurrentState.CurrentDayNumber}";
            }
        }
    }
}
