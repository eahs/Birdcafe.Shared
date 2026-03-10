using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
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
            _controller.Simulation.RunDaySimulation();
            _controller.Simulation.AdvanceFromSimulation();
            _controller.CurrentState.Economy.CurrentBalance = 5000m;
        }

        [Test]
        public void BuyBird_FailsOutsideEveningPhase()
        {
            _controller.SetPhaseForTests(GamePhase.DayLoop);
            var result = _controller.PetStore.BuyBird(PetStoreCatalog.BirdOffers[0].SpeciesId);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InvalidPhase", result.ErrorCode);
        }

        [Test]
        public void BuyBird_DeductsMoney_RecordsLedger_AndPersistsInRoster()
        {
            var startMoney = _controller.CurrentState.Economy.CurrentBalance;
            var result = _controller.PetStore.BuyBird("Cockatiel");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(startMoney - 260m, _controller.CurrentState.Economy.CurrentBalance);
            Assert.IsTrue(_controller.CurrentState.Birds.Any(b => b.SpeciesId == "Cockatiel"));
            Assert.IsTrue(_controller.CurrentState.Economy.Ledger.Last().Reason.Contains("Cockatiel"));
        }

        [Test]
        public void BuyBird_FailsWhenInsufficientFunds()
        {
            _controller.CurrentState.Economy.CurrentBalance = 10m;
            var result = _controller.PetStore.BuyBird("HyacinthMacaw");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InsufficientFunds", result.ErrorCode);
        }

        [Test]
        public void SupportsMultipleOwnedBirds()
        {
            _controller.PetStore.BuyBird("Budgerigar");
            _controller.PetStore.BuyBird("Cockatiel");
            Assert.GreaterOrEqual(_controller.CurrentState.Birds.Count, 3);
        }

        [Test]
        public void SupplyPurchasesPersist()
        {
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.BirdFoodSeedMixItemId, PetStoreSupplyType.BirdFood).IsSuccess);
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.ToyFeatherWandId, PetStoreSupplyType.Toy).IsSuccess);
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);

            Assert.AreEqual(1, _controller.CurrentState.PetStore.BirdFoodUnits);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.OwnedToyQuantities[PetStoreCatalog.ToyFeatherWandId]);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.OwnedCostumeQuantities[PetStoreCatalog.CostumeBandanaId]);
        }

        [Test]
        public void EggRewardResolution_IsDeterministicAndPersisted()
        {
            _controller.PetStore.BuySupply("SpecialEggToy", PetStoreSupplyType.SpecialEggToy);
            var first = _controller.PetStore.OpenSpecialEggToy();

            Assert.IsTrue(first.IsSuccess);
            var rewardA = first.Payload as EggRewardRecord;
            Assert.NotNull(rewardA);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.EggRewardHistory.Count);

            var fresh = new BirdCafeController();
            fresh.Meta.StartNewGame("PetStoreTester", "Cafe");
            fresh.Simulation.RunDaySimulation();
            fresh.Simulation.AdvanceFromSimulation();
            fresh.CurrentState.Economy.CurrentBalance = 5000m;
            fresh.CurrentState.CurrentDayState.CurrentPlan.DaySeed = _controller.CurrentState.CurrentDayState.CurrentPlan.DaySeed;
            fresh.PetStore.BuySupply("SpecialEggToy", PetStoreSupplyType.SpecialEggToy);
            var second = fresh.PetStore.OpenSpecialEggToy();
            var rewardB = second.Payload as EggRewardRecord;

            Assert.AreEqual(rewardA.RewardId, rewardB.RewardId);
        }

        [Test]
        public void BirdCatalog_HigherPriceGeneratesHigherStats()
        {
            var ordered = PetStoreCatalog.BirdOffers.OrderBy(b => b.Price).ToList();
            for (int i = 1; i < ordered.Count; i++)
            {
                Assert.GreaterOrEqual(ordered[i].Productivity, ordered[i - 1].Productivity);
                Assert.GreaterOrEqual(ordered[i].Friendliness, ordered[i - 1].Friendliness);
            }

            Assert.AreEqual(BirdRarity.Exotic, ordered.Last().Rarity);
        }

        [Test]
        public void PurchasedBirdAppearsInCareDashboard()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Tester", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();

            Assert.IsTrue(game.BuyPetStoreBird("Budgerigar"));
            var careVm = game.GetCareDashboard();
            Assert.IsTrue(careVm.Birds.Any(b => b.Name.Contains("Budgerigar")));
        }
    }

    internal static class ControllerTestExtensions
    {
        internal static void SetPhaseForTests(this BirdCafeController controller, GamePhase phase)
        {
            typeof(BirdCafeController)
                .GetMethod("SetPhase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(controller, new object[] { phase });
        }
    }
}
