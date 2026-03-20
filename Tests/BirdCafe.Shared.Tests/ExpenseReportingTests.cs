using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Reporting;
using NUnit.Framework;
using System;
using System.Linq;

namespace BirdCafe.Shared.Tests
{
    public class ExpenseReportingTests
    {
        private BirdCafeController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new BirdCafeController();
            _controller.Meta.StartNewGame("ExpenseTester", "Cafe");
        }

        [Test]
        public void ExpenseReport_CustomDayRange_ReturnsOnlyMatchingEntries()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 2,
                EndDayNumber = 2,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(2, report.Rows.Count);
            Assert.IsTrue(report.Rows.All(r => r.DayNumber == 2));
            Assert.AreEqual(30m, report.GrandTotalExpenses);
        }

        [Test]
        public void ExpenseReport_BirdFilter_ReturnsOnlyThatBirdsCosts()
        {
            SeedExpenseLedger();
            var bird = _controller.CurrentState.Birds.First();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                BirdId = bird.Id,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(2, report.Rows.Count);
            Assert.AreEqual(25m, report.TotalBirdExpenses);
            Assert.IsTrue(report.Rows.All(r => r.BirdName == bird.Name));
        }

        [Test]
        public void ExpenseReport_CareOnlyFilter_ExcludesInventoryEntries()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = false
            });

            Assert.AreEqual(2, report.Rows.Count);
            Assert.IsTrue(report.Rows.All(r => r.CategoryText != ExpenseCategory.InventoryCoffee.ToString()));
            Assert.AreEqual(25m, report.TotalCareExpenses);
            Assert.AreEqual(25m, report.GrandTotalExpenses);
        }

        [Test]
        public void ExpenseReport_InventoryOnlyFilter_ExcludesCareEntries()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = false,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(2, report.Rows.Count);
            Assert.IsTrue(report.Rows.All(r => r.BirdName == null));
            Assert.AreEqual(30m, report.TotalCafeExpenses);
            Assert.AreEqual(30m, report.GrandTotalExpenses);
        }

        [Test]
        public void ExpenseReport_GroupedByCategory_HasCorrectTotals()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByCategory,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(3, report.Rows.Count);
            Assert.AreEqual(10m, report.Rows.Single(r => r.CategoryText == ExpenseCategory.VetCare.ToString()).Amount);
            Assert.AreEqual(15m, report.Rows.Single(r => r.CategoryText == ExpenseCategory.ToysAndActivities.ToString()).Amount);
            Assert.AreEqual(30m, report.Rows.Single(r => r.CategoryText == ExpenseCategory.InventoryCoffee.ToString()).Amount);
        }

        [Test]
        public void ExpenseReport_RunningTotal_IsCalculatedInDisplayOrder()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true,
                IncludeRunningTotal = true
            });

            Assert.AreEqual(10m, report.Rows[0].RunningTotal);
            Assert.AreEqual(30m, report.Rows[1].RunningTotal);
            Assert.AreEqual(40m, report.Rows[2].RunningTotal);
            Assert.AreEqual(55m, report.Rows[3].RunningTotal);
        }

        [Test]
        public void FinalizeDay_CreatesSeparateLedgerEntriesPerInventoryType()
        {
            MoveToEveningPhase();

            _controller.Planning.SetInventoryOrder(ProductType.Coffee, 3);
            _controller.Planning.SetInventoryOrder(ProductType.BakedGoods, 2);
            var result = _controller.Planning.FinalizeDay();

            Assert.IsTrue(result.IsSuccess);
            var entries = _controller.CurrentState.Economy.Ledger.Where(e => e.DayNumber == 1).ToList();
            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue(entries.Any(e => e.Category == ExpenseCategory.InventoryCoffee && e.RelatedProduct == ProductType.Coffee && e.Amount == -3m));
            Assert.IsTrue(entries.Any(e => e.Category == ExpenseCategory.InventoryBakedGoods && e.RelatedProduct == ProductType.BakedGoods && e.Amount == -4m));
            Assert.IsFalse(entries.Any(e => e.Category == ExpenseCategory.Miscellaneous));
        }

        [Test]
        public void CareActions_UseSpecificExpenseCategories()
        {
            MoveToEveningPhase();
            _controller.CurrentState.Config.BaselinePlayCost = 15m;
            var bird = _controller.CurrentState.Birds.First();

            Assert.IsTrue(_controller.Care.PerformCareAction(bird.Id, CareActionIds.Vet).IsSuccess);
            Assert.IsTrue(_controller.Care.PerformCareAction(bird.Id, CareActionIds.Play).IsSuccess);

            var ledger = _controller.CurrentState.Economy.Ledger;
            Assert.AreEqual(2, ledger.Count);
            Assert.IsTrue(ledger.Any(entry => entry.Category == ExpenseCategory.VetCare));
            Assert.IsTrue(ledger.Any(entry => entry.Category == ExpenseCategory.ToysAndActivities));
            Assert.IsTrue(ledger.All(entry => entry.DayNumber == 1 && entry.WeekNumber == 1));
        }

        [Test]
        public void GetDailyReport_Behavior_RemainsUsableAfterExpenseReportingChanges()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Reporter", "Cafe");

            Assert.IsTrue(game.StartSimulationPlayback());
            var report = game.GetDailyReport();

            Assert.AreEqual(1, report.DayNumber);
            Assert.GreaterOrEqual(report.TotalRevenue, 0m);
            Assert.NotNull(report.Birds);
        }

        [Test]
        public void BirdCafeGame_BirdExpenseReport_UsesBirdFilter()
        {
            SeedExpenseLedger();
            BirdCafeGame.Instance.Controller.Meta.LoadGame(_controller.CurrentState);
            var bird = _controller.CurrentState.Birds.First();

            var report = BirdCafeGame.Instance.GetBirdExpenseReport(bird.Id, new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(25m, report.GrandTotalExpenses);
            Assert.IsTrue(report.Rows.All(r => r.BirdName == bird.Name));
        }

        private void MoveToEveningPhase()
        {
            _controller.Simulation.RunDaySimulation();
            _controller.Simulation.AdvanceFromSimulation();
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
        }

        private void SeedExpenseLedger()
        {
            var bird = _controller.CurrentState.Birds.First();
            _controller.CurrentState.Economy.Ledger.Clear();
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 1,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                Amount = -10m,
                Category = ExpenseCategory.VetCare,
                RelatedBirdId = bird.Id,
                Reason = "Vet Visit",
                ShortDescription = "Vet Visit for Peep"
            });
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 2,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc),
                Amount = -20m,
                Category = ExpenseCategory.InventoryCoffee,
                RelatedProduct = ProductType.Coffee,
                Reason = "Inventory Restock: Coffee",
                ShortDescription = "Restocked coffee beans x20"
            });
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 2,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                Amount = -10m,
                Category = ExpenseCategory.InventoryCoffee,
                RelatedProduct = ProductType.Coffee,
                Reason = "Inventory Restock: Coffee",
                ShortDescription = "Restocked coffee beans x10"
            });
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 8, 0, 0, DateTimeKind.Utc),
                Amount = -15m,
                Category = ExpenseCategory.ToysAndActivities,
                RelatedBirdId = bird.Id,
                Reason = "Play Time",
                ShortDescription = "Play Time for Peep"
            });
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc),
                Amount = 100m,
                Category = ExpenseCategory.Miscellaneous,
                Reason = "Revenue",
                ShortDescription = "Income"
            });
        }
    }
}
