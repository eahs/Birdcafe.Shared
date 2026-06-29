
using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Enums;
using NUnit.Framework;
using System.Linq;

namespace BirdCafe.Shared.Tests
{
    public class SimulationTests
    {
        private BirdCafeController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new BirdCafeController();
        }

        [Test]
        public void StartNewGame_SetsUpDefaults()
        {
            var res = _controller.Meta.StartNewGame("TestPlayer", "TestCafe");

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(1, _controller.CurrentState.CurrentDayNumber);
            Assert.AreEqual(GamePhase.DayLoop, _controller.CurrentPhase);
            Assert.AreEqual(1, _controller.CurrentState.Birds.Count);
            Assert.AreEqual("Peep", _controller.CurrentState.Birds[0].Name);
        }

        [Test]
        public void RunDaySimulation_GeneratesTimeline()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");

            var res = _controller.Simulation.RunDaySimulation();

            Assert.IsTrue(res.IsSuccess, "Simulation should succeed");

            var resultData = res.Payload as Models.Simulation.DaySimulationResult;
            Assert.IsNotNull(resultData);

            Assert.Greater(resultData.Timeline.Count, 0, "Should have timeline events");
            Assert.Greater(resultData.Customers.CustomersArrived, 0, "Customers should arrive");
            Assert.GreaterOrEqual(resultData.Economy.EndingMoney, resultData.Economy.StartingMoney);
        }

        [Test]
        public void RunDaySimulation_AttributesCustomersToBirds()
        {
            // THIS TEST would have caught the bug where Bird.CustomersServed was 0.

            // 1. Setup Day 1
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");

            // 2. Run Simulation
            var res = _controller.Simulation.RunDaySimulation();
            var resultData = res.Payload as Models.Simulation.DaySimulationResult;

            // 3. Validate Global Stats
            Assert.Greater(resultData.Customers.CustomersServed, 0, "Global served count should be > 0");

            // 4. Validate Individual Bird Stats
            var peepSummary = resultData.BirdSummaries.First(b => b.BirdName == "Peep");

            Assert.IsTrue(peepSummary.WorkedToday, "Peep should be marked as working");

            // The fix ensures this is now true:
            Assert.AreEqual(resultData.Customers.CustomersServed, peepSummary.CustomersServed,
                "Since Peep is the only worker, they must have served all the customers.");

            Assert.Greater(peepSummary.CustomersServed, 0, "Peep's individual served count must be > 0");
        }

        [Test]
        public void PurchasingInventory_DeductsMoney()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");

            _controller.Simulation.RunDaySimulation();
            _controller.Simulation.AdvanceFromSimulation(); // To EveningLoop

            decimal startMoney = _controller.CurrentState.Economy.CurrentBalance;

            _controller.Planning.SetInventoryOrder(ProductType.Coffee, 5);
            var res = _controller.Planning.FinalizeDay();

            Assert.IsTrue(res.IsSuccess);

            // Use Last() on Ledger to verify exact transaction
            var lastEntry = _controller.CurrentState.Economy.Ledger.Last();
            Assert.AreEqual(-5.0m, lastEntry.Amount);
            Assert.AreEqual("Inventory Restock: Coffee", lastEntry.Reason);
            Assert.AreEqual(ExpenseCategory.InventoryCoffee, lastEntry.Category);
        }


        [Test]
        public void TrustIncreasesSimulationCashOutput()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            var bird = _controller.CurrentState.Birds.First();
            bird.Trust = 0;

            var noTrust = _controller.Simulation.RunDaySimulation();
            var noTrustRevenue = ((Models.Simulation.DaySimulationResult)noTrust.Payload).Economy.TotalRevenue;

            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            bird = _controller.CurrentState.Birds.First();
            bird.Trust = 100;

            var highTrust = _controller.Simulation.RunDaySimulation();
            var highTrustRevenue = ((Models.Simulation.DaySimulationResult)highTrust.Payload).Economy.TotalRevenue;

            Assert.Greater(highTrustRevenue, noTrustRevenue);
        }


        [Test]
        public void RunDaySimulation_ServiceCompletedTimelinePopularity_MatchesCustomerTransactionsForMultiItemOrders()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");

            // Force deterministic multi-item demand and keep service throughput reliable.
            _controller.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 12345;
            _controller.CurrentState.Config.ChanceForSecondaryItem = 1f;
            _controller.CurrentState.Config.BaseCustomersPerDay = 2;
            _controller.CurrentState.Config.PopularityToCustomerFactor = 0f;
            _controller.CurrentState.Config.CustomerPatienceSeconds = 10000f;

            // Ensure inventory never blocks fulfillment for this regression scenario.
            _controller.CurrentState.Cafe.Inventory.Coffee.QuantityOnHand = 200;
            _controller.CurrentState.Cafe.Inventory.BakedGoods.QuantityOnHand = 200;
            _controller.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand = 200;

            var runResult = _controller.Simulation.RunDaySimulation();
            Assert.IsTrue(runResult.IsSuccess, "Simulation should succeed");

            var dayResult = runResult.Payload as Models.Simulation.DaySimulationResult;
            Assert.IsNotNull(dayResult, "Simulation payload should be a day result");

            var servedCustomers = dayResult.CustomerTransactions
                .Where(t => t.Outcome == CustomerOutcome.Served)
                .ToList();

            Assert.IsNotEmpty(servedCustomers, "Expected at least one served customer");

            var serviceCompletedEvents = dayResult.Timeline
                .Where(e => e.EventType == SimulationTimelineEventType.ServiceCompleted && e.CustomerId.HasValue)
                .ToList();

            Assert.IsTrue(
                servedCustomers.Any(c => serviceCompletedEvents.Count(e => e.CustomerId == c.CustomerId) > 1),
                "Expected at least one served customer to have more than one ServiceCompleted event.");

            const float tolerance = 0.00001f;
            foreach (var servedCustomer in servedCustomers)
            {
                float timelinePopularityDelta = serviceCompletedEvents
                    .Where(e => e.CustomerId == servedCustomer.CustomerId)
                    .Sum(e => e.PopularityDelta);

                Assert.That(
                    timelinePopularityDelta,
                    Is.EqualTo(servedCustomer.PopularityDelta).Within(tolerance),
                    $"Timeline popularity mismatch for served customer {servedCustomer.CustomerId}.");
            }

            float totalTimelinePopularityDelta = serviceCompletedEvents.Sum(e => e.PopularityDelta);
            float totalServedTransactionPopularityDelta = servedCustomers.Sum(c => c.PopularityDelta);

            Assert.That(
                totalTimelinePopularityDelta,
                Is.EqualTo(totalServedTransactionPopularityDelta).Within(tolerance),
                "Total successful ServiceCompleted timeline popularity should match served transaction popularity total.");
        }

        [Test]
        public void TrustAndFriendshipRevenueBonuses_AreDeterministicForSameSeed()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            _controller.SetPhaseForTests(GamePhase.EveningLoop);
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
            _controller.PetStore.BuyBird("budgie");
            var birdA = _controller.CurrentState.Birds[0];
            var birdB = _controller.CurrentState.Birds[1];
            birdA.Trust = 80;
            birdB.Trust = 60;
            birdA.AddFriend(birdB.Id);
            birdB.AddFriend(birdA.Id);
            var seed = _controller.CurrentState.CurrentDayState.CurrentPlan.DaySeed;
            _controller.CurrentState.CurrentDayState.CurrentPlan.BirdIdsWorking = new System.Collections.Generic.List<string> { birdA.Id, birdB.Id };

            _controller.SetPhaseForTests(GamePhase.DayLoop);
            var first = (Models.Simulation.DaySimulationResult)_controller.Simulation.RunDaySimulation().Payload;

            var secondController = new BirdCafeController();
            secondController.Meta.StartNewGame("TestPlayer", "TestCafe");
            secondController.SetPhaseForTests(GamePhase.EveningLoop);
            secondController.CurrentState.Economy.CurrentBalance = 5000m;
            secondController.PetStore.BuyBird("budgie");
            var secondBirdA = secondController.CurrentState.Birds[0];
            var secondBirdB = secondController.CurrentState.Birds[1];
            secondBirdA.Trust = 80;
            secondBirdB.Trust = 60;
            secondBirdA.AddFriend(secondBirdB.Id);
            secondBirdB.AddFriend(secondBirdA.Id);
            secondController.CurrentState.CurrentDayState.CurrentPlan.DaySeed = seed;
            secondController.CurrentState.CurrentDayState.CurrentPlan.BirdIdsWorking = new System.Collections.Generic.List<string> { secondBirdA.Id, secondBirdB.Id };
            secondController.SetPhaseForTests(GamePhase.DayLoop);
            var second = (Models.Simulation.DaySimulationResult)secondController.Simulation.RunDaySimulation().Payload;

            Assert.AreEqual(first.Economy.TotalRevenue, second.Economy.TotalRevenue);
        }
    }
}