using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Reporting;
using BirdCafe.Shared.Models.Simulation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void ExpenseReport_GroupedByDay_HasCorrectTotals()
        {
            SeedExpenseLedger(includeMiscExpense: true);

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByDay,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual(3, report.Rows.Count);
            Assert.AreEqual(10m, report.Rows.Single(r => r.DayNumber == 1).Amount);
            Assert.AreEqual(30m, report.Rows.Single(r => r.DayNumber == 2).Amount);
            Assert.AreEqual(20m, report.Rows.Single(r => r.DayNumber == 3).Amount);
        }

        [Test]
        public void ExpenseReport_TransactionRows_AreOrderedDeterministically()
        {
            SeedExpenseLedger();

            var report = _controller.Reporting.GenerateExpenseReport(new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CustomDayRange,
                StartDayNumber = 1,
                EndDayNumber = 3,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            });

            Assert.AreEqual("Vet Visit for Peep", report.Rows[0].Label);
            Assert.AreEqual("Restocked coffee beans x20", report.Rows[1].Label);
            Assert.AreEqual("Restocked coffee beans x10", report.Rows[2].Label);
            Assert.AreEqual("Play Time for Peep", report.Rows[3].Label);
        }

        [Test]
        public void ExpenseReport_CareOnlyFilter_DoesNotLeakOtherExpenseTypes()
        {
            SeedExpenseLedger(includeMiscExpense: true);

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
            Assert.IsTrue(report.Rows.All(r => r.BirdName != null));
            Assert.AreEqual(25m, report.GrandTotalExpenses);
        }

        [Test]
        public void ExpenseReport_InventoryOnlyFilter_DoesNotLeakOtherExpenseTypes()
        {
            SeedExpenseLedger(includeMiscExpense: true);

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
            Assert.IsTrue(report.Rows.All(r => r.CategoryText == ExpenseCategory.InventoryCoffee.ToString()));
            Assert.AreEqual(30m, report.GrandTotalExpenses);
        }

        [Test]
        public void BuyingBirdFood_StaysInFoodAndSuppliesCategory()
        {
            MoveToEveningPhase();

            var result = _controller.PetStore.BuySupply(BirdFoodType.SeedMix.ToString(), PetStoreSupplyType.BirdFood, 1);

            Assert.IsTrue(result.IsSuccess);
            var entry = _controller.CurrentState.Economy.Ledger.Last();
            Assert.AreEqual(ExpenseCategory.FoodAndSupplies, entry.Category);
            Assert.AreEqual(1, entry.DayNumber);
            Assert.AreEqual(1, entry.WeekNumber);
        }

        [Test]
        public void BuyingPetStoreSupply_StoresItemIdOnLedgerEntry()
        {
            MoveToEveningPhase();

            var result = _controller.PetStore.BuySupply(BirdFoodType.SeedMix.ToString(), PetStoreSupplyType.BirdFood, 1);

            Assert.IsTrue(result.IsSuccess);
            var entry = _controller.CurrentState.Economy.Ledger.Last();
            Assert.AreEqual(BirdFoodType.SeedMix.ToString(), entry.ItemId);
            Assert.AreEqual(ExpenseCategory.FoodAndSupplies, entry.Category);
            Assert.IsNull(entry.RelatedBirdId);
        }

        [Test]
        public void FeedAction_AddsBirdLinkedConsumptionLedgerEntry_WithItemId()
        {
            MoveToEveningPhase();
            Assert.IsTrue(_controller.PetStore.BuySupply(BirdFoodType.SeedMix.ToString(), PetStoreSupplyType.BirdFood, 1).IsSuccess);

            var bird = _controller.CurrentState.Birds.First();
            var result = _controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsTrue(result.IsSuccess);

            var ledger = _controller.CurrentState.Economy.Ledger;
            Assert.AreEqual(2, ledger.Count);

            var consumptionEntry = ledger.Last();
            Assert.AreEqual(ExpenseCategory.FoodAndSupplies, consumptionEntry.Category);
            Assert.AreEqual(0m, consumptionEntry.Amount);
            Assert.AreEqual(BirdFoodType.SeedMix.ToString(), consumptionEntry.ItemId);
            Assert.AreEqual(bird.Id, consumptionEntry.RelatedBirdId);
            StringAssert.Contains(bird.Name, consumptionEntry.ShortDescription);
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

        private void SeedExpenseLedger(bool includeMiscExpense = false)
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
            if (includeMiscExpense)
            {
                _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
                {
                    DayNumber = 3,
                    WeekNumber = 1,
                    Timestamp = new DateTime(2026, 1, 3, 9, 0, 0, DateTimeKind.Utc),
                    Amount = -5m,
                    Category = ExpenseCategory.Miscellaneous,
                    Reason = "Cage Cleaning",
                    ShortDescription = "Cage cleaning supplies"
                });
            }

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

        private void SeedCostOfCareReportData()
        {
            var state = _controller.CurrentState;
            var bird = state.Birds.First();
            state.CurrentDayNumber = 3;
            state.CurrentWeekNumber = 1;
            state.Economy.CurrentBalance = 900m;
            state.Economy.Ledger.Clear();
            state.PastDayResults.Clear();

            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 1,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                Amount = -120m,
                Category = ExpenseCategory.UpgradesAndCustomization,
                ItemId = "budgie",
                RelatedBirdId = bird.Id,
                Reason = "Bird: Buddy",
                ShortDescription = "Bird: Buddy"
            });
            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 2,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc),
                Amount = -18m,
                Category = ExpenseCategory.FoodAndSupplies,
                ItemId = BirdFoodType.SeedMix.ToString(),
                Reason = "Supply: SeedMix x1",
                ShortDescription = "Supply: SeedMix x1"
            });
            state.Economy.Ledger.Add(new LedgerEntry
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
            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 9, 0, 0, DateTimeKind.Utc),
                Amount = -10m,
                Category = ExpenseCategory.VetCare,
                RelatedBirdId = bird.Id,
                Reason = "Vet Visit",
                ShortDescription = "Vet Visit for Peep"
            });
            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                Amount = -15m,
                Category = ExpenseCategory.ToysAndActivities,
                RelatedBirdId = bird.Id,
                Reason = "Play Time",
                ShortDescription = "Play Time for Peep"
            });
            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 11, 0, 0, DateTimeKind.Utc),
                Amount = 0m,
                Category = ExpenseCategory.FoodAndSupplies,
                ItemId = BirdFoodType.SeedMix.ToString(),
                RelatedBirdId = bird.Id,
                Reason = "Bird Food Consumed",
                ShortDescription = "Peep ate Seed Mix"
            });
            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = 3,
                WeekNumber = 1,
                Timestamp = new DateTime(2026, 1, 3, 11, 30, 0, DateTimeKind.Utc),
                Amount = 40m,
                Category = ExpenseCategory.Miscellaneous,
                Reason = "Revenue",
                ShortDescription = "Income"
            });

            state.PastDayResults.Add(BuildDayResult(1, 20m, 10m, 5m, 5m, 1, 1, 1, 2, 1));
            state.PastDayResults.Add(BuildDayResult(2, 35m, 15m, 10m, 10m, 1, 1, 1, 3, 2));
            state.PastDayResults.Add(BuildDayResult(3, 40m, 10m, 15m, 15m, 1, 1, 0, 6, 1));
        }

        private DaySimulationResult BuildDayResult(
            int dayNumber,
            decimal totalRevenue,
            decimal coffeeSales,
            decimal bakedSales,
            decimal merchSales,
            int coffeeUnits,
            int bakedUnits,
            int merchUnits,
            int customersServed,
            int customersLost)
        {
            var timeline = new List<SimulationTimelineEvent>();
            AddProductEvents(timeline, ProductType.Coffee, coffeeUnits, coffeeSales);
            AddProductEvents(timeline, ProductType.BakedGoods, bakedUnits, bakedSales);
            AddProductEvents(timeline, ProductType.ThemedMerch, merchUnits, merchSales);

            return new DaySimulationResult
            {
                DayNumber = dayNumber,
                WeekNumber = 1,
                DayName = DayOfWeek.Monday.ToString(),
                Economy = new DayEconomySummary { TotalRevenue = totalRevenue },
                Customers = new DayCustomerSummary
                {
                    CoffeeSold = coffeeUnits,
                    BakedGoodsSold = bakedUnits,
                    MerchSold = merchUnits,
                    CustomersServed = customersServed,
                    CustomersLeftUnhappy = customersLost
                },
                Timeline = timeline
            };
        }

        private void AddProductEvents(List<SimulationTimelineEvent> timeline, ProductType product, int units, decimal totalRevenue)
        {
            if (units <= 0 || totalRevenue <= 0m)
            {
                return;
            }

            var unitValue = decimal.Round(totalRevenue / units, 2, MidpointRounding.AwayFromZero);
            decimal runningRevenue = 0m;
            for (int i = 0; i < units; i++)
            {
                bool isLast = i == units - 1;
                var eventValue = isLast ? totalRevenue - runningRevenue : unitValue;
                runningRevenue += eventValue;
                timeline.Add(new SimulationTimelineEvent
                {
                    EventType = SimulationTimelineEventType.ServiceCompleted,
                    Product = product,
                    MoneyDelta = eventValue
                });
            }
        }
    }
}
