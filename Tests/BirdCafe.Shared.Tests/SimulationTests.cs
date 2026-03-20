
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
        public void FriendshipIncreasesSimulationCashOutput()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            _controller.SetPhaseForTests(GamePhase.EveningLoop);
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
            _controller.PetStore.BuyBird("Budgerigar");

            var birdA = _controller.CurrentState.Birds[0];
            var birdB = _controller.CurrentState.Birds[1];
            birdA.FriendBirdIds.Clear();
            birdB.FriendBirdIds.Clear();
            _controller.CurrentState.CurrentDayState.CurrentPlan.BirdIdsWorking = new System.Collections.Generic.List<string> { birdA.Id, birdB.Id };

            _controller.SetPhaseForTests(GamePhase.DayLoop);
            var noFriend = _controller.Simulation.RunDaySimulation();
            var noFriendRevenue = ((Models.Simulation.DaySimulationResult)noFriend.Payload).Economy.TotalRevenue;

            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            _controller.SetPhaseForTests(GamePhase.EveningLoop);
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
            _controller.PetStore.BuyBird("Budgerigar");

            birdA = _controller.CurrentState.Birds[0];
            birdB = _controller.CurrentState.Birds[1];
            birdA.AddFriend(birdB.Id);
            birdB.AddFriend(birdA.Id);
            _controller.CurrentState.CurrentDayState.CurrentPlan.BirdIdsWorking = new System.Collections.Generic.List<string> { birdA.Id, birdB.Id };

            _controller.SetPhaseForTests(GamePhase.DayLoop);
            var withFriend = _controller.Simulation.RunDaySimulation();
            var withFriendRevenue = ((Models.Simulation.DaySimulationResult)withFriend.Payload).Economy.TotalRevenue;

            Assert.Greater(withFriendRevenue, noFriendRevenue);
        }

        [Test]
        public void TrustAndFriendshipRevenueBonuses_AreDeterministicForSameSeed()
        {
            _controller.Meta.StartNewGame("TestPlayer", "TestCafe");
            _controller.SetPhaseForTests(GamePhase.EveningLoop);
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
            _controller.PetStore.BuyBird("Budgerigar");
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
            secondController.PetStore.BuyBird("Budgerigar");
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