using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Cafe;
using BirdCafe.Shared.Models.Simulation;
using NUnit.Framework;
using System.Linq;

namespace BirdCafe.Shared.Tests
{
    public class PetStoreTests
    {
        private BirdCafeController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new BirdCafeController();
            _controller.Meta.StartNewGame("PetStoreTester", "Cafe");
        }

        private void MoveToEvening()
        {
            _controller.Simulation.RunDaySimulation();
            _controller.Simulation.AdvanceFromSimulation();
        }

        [Test]
        public void PurchasePetBird_DeductsMoney_AndRecordsLedger()
        {
            MoveToEvening();
            _controller.CurrentState.Economy.CurrentBalance = 10000m;

            var before = _controller.CurrentState.Economy.CurrentBalance;
            var entry = Engine.Utils.PetStoreCatalog.Birds.First();

            var result = _controller.Planning.PurchasePetBird(entry.Id);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(before - entry.Price, _controller.CurrentState.Economy.CurrentBalance);
            Assert.AreEqual(entry.Id, _controller.CurrentState.Cafe.PetStore.OwnedEntertainerBirds.Last().CatalogId);
            Assert.IsTrue(_controller.CurrentState.Economy.Ledger.Last().Reason.Contains("Rick's Pet Store Bird"));
        }

        [Test]
        public void PurchasePetBird_Fails_WhenInsufficientFunds()
        {
            MoveToEvening();
            _controller.CurrentState.Economy.CurrentBalance = 0m;

            var result = _controller.Planning.PurchasePetBird("kakapo-ancient");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InsufficientFunds", result.ErrorCode);
        }

        [Test]
        public void PurchasePetBird_Fails_InWrongPhase()
        {
            var result = _controller.Planning.PurchasePetBird("budgie-blue");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InvalidPhase", result.ErrorCode);
        }

        [Test]
        public void PurchasedBird_PersistsInSaveState()
        {
            MoveToEvening();
            _controller.CurrentState.Economy.CurrentBalance = 10000m;
            _controller.Planning.PurchasePetBird("budgie-blue");

            Assert.AreEqual(1, _controller.CurrentState.Cafe.PetStore.OwnedEntertainerBirds.Count);
            Assert.AreEqual("budgie-blue", _controller.CurrentState.Cafe.PetStore.OwnedEntertainerBirds[0].CatalogId);
        }

        [Test]
        public void EntertainerBird_IncreasesNextDaySimulationValue()
        {
            MoveToEvening();
            _controller.CurrentState.Economy.CurrentBalance = 10000m;
            _controller.Planning.PurchasePetBird("kakapo-ancient");
            _controller.Planning.FinalizeDay();
            _controller.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 777;

            var sim = _controller.Simulation.RunDaySimulation();
            var day2 = sim.Payload as DaySimulationResult;

            Assert.IsTrue(sim.IsSuccess);
            Assert.Greater(day2.Economy.PetStoreBonusRevenue, 0m);
            Assert.Greater(day2.Economy.PetStoreBonusCustomers, 0);
            Assert.Greater(day2.Customers.CustomersArrived, _controller.CurrentState.Config.BaseCustomersPerDay);
        }

        [Test]
        public void MysteryEggReward_IsDeterministic_AndPersists()
        {
            MoveToEvening();
            _controller.CurrentState.Economy.CurrentBalance = 10000m;
            _controller.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 444;

            var a = _controller.Planning.PurchaseMysteryEgg();
            Assert.IsTrue(a.IsSuccess);
            var rewardA = a.Payload as PetEggRewardEntry;

            var mirror = new BirdCafeController();
            mirror.Meta.StartNewGame("Mirror", "Cafe");
            mirror.Simulation.RunDaySimulation();
            mirror.Simulation.AdvanceFromSimulation();
            mirror.CurrentState.Economy.CurrentBalance = 10000m;
            mirror.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 444;
            var b = mirror.Planning.PurchaseMysteryEgg();
            var rewardB = b.Payload as PetEggRewardEntry;

            Assert.AreEqual(rewardA.Id, rewardB.Id);
            Assert.IsTrue(_controller.CurrentState.Cafe.PetStore.UnlockedRewardIds.Contains(rewardA.Id));
            Assert.AreEqual(rewardA.Name, _controller.CurrentState.Cafe.PetStore.RewardHistory.Last());
        }
    }
}
