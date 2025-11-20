
using NUnit.Framework;
using BirdCafe.Shared;
using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.ViewModels;
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
            Assert.AreEqual("Inventory Restock", lastEntry.Reason);
        }
    }
}