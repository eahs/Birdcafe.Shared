using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.PetStore;
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
            _controller.Meta.StartNewGame("Tester", "Cafe");
        }

        [Test]
        public void PurchaseEntertainerBird_DeductsMoney_AndWritesLedger()
        {
            MoveToEvening(_controller);
            var def = PetStoreCatalog.EntertainerBirds.First();
            decimal start = _controller.CurrentState.Economy.CurrentBalance;

            var res = _controller.Planning.PurchaseEntertainerBird(def.BirdId);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(start - def.Price, _controller.CurrentState.Economy.CurrentBalance);
            var ledger = _controller.CurrentState.Economy.Ledger.Last();
            Assert.AreEqual(-def.Price, ledger.Amount);
            Assert.IsTrue(ledger.Reason.Contains(def.SpeciesName));
        }

        [Test]
        public void PurchaseEntertainerBird_FailsWhenInsufficientFunds()
        {
            MoveToEvening(_controller);
            var expensive = PetStoreCatalog.EntertainerBirds.OrderByDescending(b => b.Price).First();
            _controller.CurrentState.Economy.CurrentBalance = 0;

            var res = _controller.Planning.PurchaseEntertainerBird(expensive.BirdId);

            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual("InsufficientFunds", res.ErrorCode);
        }

        [Test]
        public void PurchaseEntertainerBird_FailsOutsideEveningPhase()
        {
            var res = _controller.Planning.PurchaseEntertainerBird(PetStoreCatalog.EntertainerBirds.First().BirdId);
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual("InvalidPhase", res.ErrorCode);
        }

        [Test]
        public void PurchaseEntertainerBird_PersistsInSaveState()
        {
            MoveToEvening(_controller);
            var id = PetStoreCatalog.EntertainerBirds[1].BirdId;

            _controller.Planning.PurchaseEntertainerBird(id);

            Assert.Contains(id, _controller.CurrentState.PetStore.OwnedEntertainerBirdIds);
        }

        [Test]
        public void EntertainerBirds_IncreaseSimulationRevenue()
        {
            var baseController = new BirdCafeController();
            baseController.Meta.StartNewGame("A", "Cafe");
            var seeded = baseController.CurrentState.CurrentDayState.CurrentPlan.DaySeed;

            var boostedController = new BirdCafeController();
            boostedController.Meta.StartNewGame("B", "Cafe");
            boostedController.CurrentState.CurrentDayState.CurrentPlan.DaySeed = seeded;
            baseController.CurrentState.CurrentDayState.CurrentPlan.DaySeed = seeded;

            var macaw = PetStoreCatalog.EntertainerBirds.First(b => b.BirdId == "ent-hyacinth-macaw");
            boostedController.CurrentState.PetStore.OwnedEntertainerBirdIds.Add(macaw.BirdId);

            var baseResult = (BirdCafe.Shared.Models.Simulation.DaySimulationResult)baseController.Simulation.RunDaySimulation().Payload;
            var boostedResult = (BirdCafe.Shared.Models.Simulation.DaySimulationResult)boostedController.Simulation.RunDaySimulation().Payload;

            Assert.AreEqual(macaw.DailyRevenueBonus, boostedResult.Economy.PassiveBonusRevenue);
            Assert.AreEqual(baseResult.Economy.TotalRevenue + macaw.DailyRevenueBonus, boostedResult.Economy.TotalRevenue);
        }

        [Test]
        public void MysteryEggReward_IsDeterministic_AndPersists()
        {
            var c1 = new BirdCafeController();
            c1.Meta.StartNewGame("A", "Cafe");
            MoveToEvening(c1);

            var c2 = new BirdCafeController();
            c2.Meta.StartNewGame("B", "Cafe");
            MoveToEvening(c2);

            // align deterministic seed/day context
            c2.CurrentState.CurrentDayState.CurrentPlan.DaySeed = c1.CurrentState.CurrentDayState.CurrentPlan.DaySeed;
            c2.CurrentState.CurrentDayNumber = c1.CurrentState.CurrentDayNumber;

            var r1 = (EggRewardDefinition)c1.Planning.PurchaseMysteryEgg().Payload;
            var r2 = (EggRewardDefinition)c2.Planning.PurchaseMysteryEgg().Payload;

            Assert.AreEqual(r1.RewardId, r2.RewardId);
            Assert.Contains(r1.RewardId, c1.CurrentState.PetStore.UnlockedEggRewardIds);
            Assert.AreEqual(r1.RewardId, c1.CurrentState.PetStore.LastUnlockedEggRewardId);
        }

        private static void MoveToEvening(BirdCafeController controller)
        {
            controller.Simulation.RunDaySimulation();
            controller.Simulation.AdvanceFromSimulation();
        }
    }
}
