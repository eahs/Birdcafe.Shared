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
            var result = _controller.PetStore.BuyBird("cockatiel");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(startMoney - 260m, _controller.CurrentState.Economy.CurrentBalance);
            Assert.IsTrue(_controller.CurrentState.Birds.Any(b => b.SpeciesId == "cockatiel"));
            Assert.IsTrue(_controller.CurrentState.Economy.Ledger.Last().Reason.Contains("Cockatiel"));
        }

        [Test]
        public void BuyBird_FailsWhenInsufficientFunds()
        {
            _controller.CurrentState.Economy.CurrentBalance = 10m;
            var result = _controller.PetStore.BuyBird("kingfisher");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InsufficientFunds", result.ErrorCode);
        }

        [Test]
        public void SupportsMultipleOwnedBirds()
        {
            _controller.PetStore.BuyBird("budgie");
            _controller.PetStore.BuyBird("cockatiel");
            Assert.GreaterOrEqual(_controller.CurrentState.Birds.Count, 3);
        }

        [Test]
        public void SupplyCatalogLookup_ResolvesSharedDefinitions()
        {
            var fruitMedley = PetStoreCatalog.FindSupplyOffer(BirdFoodType.FruitMedley.ToString(), PetStoreSupplyType.BirdFood);
            var specialEggToy = PetStoreCatalog.FindSupplyOffer(PetStoreCatalog.SpecialEggToyItemId, PetStoreSupplyType.SpecialEggToy);

            Assert.NotNull(fruitMedley);
            Assert.AreEqual(BirdFoodType.FruitMedley, fruitMedley.BirdFoodType);
            Assert.AreEqual(ExpenseCategory.FoodAndSupplies, fruitMedley.ExpenseCategory);
            Assert.NotNull(specialEggToy);
            Assert.AreEqual(PetStoreSupplyType.SpecialEggToy, specialEggToy.SupplyType);
        }

        [Test]
        public void BuySupply_DoesNotDeductMoney_WhenBirdFoodValidationFails()
        {
            var startMoney = _controller.CurrentState.Economy.CurrentBalance;
            var startingLedgerCount = _controller.CurrentState.Economy.Ledger.Count;

            var result = _controller.PetStore.BuySupply(PetStoreCatalog.SpecialEggToyItemId, PetStoreSupplyType.BirdFood, 1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InvalidItem", result.ErrorCode);
            Assert.AreEqual(startMoney, _controller.CurrentState.Economy.CurrentBalance);
            Assert.AreEqual(startingLedgerCount, _controller.CurrentState.Economy.Ledger.Count);
        }

        [Test]
        public void BuyingBirdFood_DeductsMoney_AndAddsStoredFoodInventory()
        {
            var startMoney = _controller.CurrentState.Economy.CurrentBalance;

            var result = _controller.PetStore.BuySupply(BirdFoodType.SeedMix.ToString(), PetStoreSupplyType.BirdFood, 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(startMoney - (PetStoreCatalog.BirdFoodSeedMixPrice * 2), _controller.CurrentState.Economy.CurrentBalance);
            Assert.AreEqual(2, _controller.CurrentState.PetStore.GetFoodUnits(BirdFoodType.SeedMix));
            Assert.IsTrue(_controller.CurrentState.Economy.Ledger.Last().Reason.Contains("Supply:"));
        }

        [Test]
        public void BuyingNonFoodSupplies_GrantsCorrectOwnership()
        {
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.ToyFeatherWandId, PetStoreSupplyType.Toy).IsSuccess);
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.SpecialEggToyItemId, PetStoreSupplyType.SpecialEggToy).IsSuccess);

            Assert.AreEqual(1, _controller.CurrentState.PetStore.OwnedToyQuantities[PetStoreCatalog.ToyFeatherWandId]);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.OwnedCostumeQuantities[PetStoreCatalog.CostumeBandanaId]);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.SpecialEggToysOwned);
        }

        [Test]
        public void EquipCostume_FailsOutsideEveningPhase()
        {
            var bird = _controller.CurrentState.Birds.First();
            _controller.SetPhaseForTests(GamePhase.DayLoop);

            var result = _controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InvalidPhase", result.ErrorCode);
        }

        [Test]
        public void EquipCostume_FailsForMissingBird()
        {
            var result = _controller.PetStore.EquipCostume("missing-bird", PetStoreCatalog.CostumeBandanaId);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("InvalidBird", result.ErrorCode);
        }

        [Test]
        public void EquipCostume_FailsForUnownedCostume()
        {
            var bird = _controller.CurrentState.Birds.First();

            var result = _controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("CostumeNotOwned", result.ErrorCode);
        }

        [Test]
        public void EquipCostume_SucceedsForOwnedCostume()
        {
            var bird = _controller.CurrentState.Birds.First();
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);

            var result = _controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(PetStoreCatalog.CostumeBandanaId, bird.CostumeId);
        }

        [Test]
        public void EquipCostume_WithNull_RemovesCostume()
        {
            var bird = _controller.CurrentState.Birds.First();
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);
            Assert.IsTrue(_controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId).IsSuccess);

            var result = _controller.PetStore.EquipCostume(bird.Id, null);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(bird.CostumeId);
        }

        [Test]
        public void EquipCostume_DoesNotConsumeOwnedCostumeQuantity()
        {
            var bird = _controller.CurrentState.Birds.First();
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);
            var ownedBeforeEquip = _controller.CurrentState.PetStore.OwnedCostumeQuantities[PetStoreCatalog.CostumeBandanaId];

            var equipResult = _controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId);
            var unequipResult = _controller.PetStore.EquipCostume(bird.Id, null);

            Assert.IsTrue(equipResult.IsSuccess);
            Assert.IsTrue(unequipResult.IsSuccess);
            Assert.AreEqual(ownedBeforeEquip, _controller.CurrentState.PetStore.OwnedCostumeQuantities[PetStoreCatalog.CostumeBandanaId]);
        }

        [Test]
        public void EquipCostume_AllowsRewardOnlyOwnedCostume()
        {
            var bird = _controller.CurrentState.Birds.First();
            _controller.CurrentState.PetStore.OwnedCostumeQuantities["Costume_GoldenVest"] = 1;

            var result = _controller.PetStore.EquipCostume(bird.Id, "Costume_GoldenVest");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Costume_GoldenVest", bird.CostumeId);
        }

        [Test]
        public void EquipCostume_ReequippingSameCostumeIsSafe()
        {
            var bird = _controller.CurrentState.Birds.First();
            Assert.IsTrue(_controller.PetStore.BuySupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume).IsSuccess);
            Assert.IsTrue(_controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId).IsSuccess);

            var result = _controller.PetStore.EquipCostume(bird.Id, PetStoreCatalog.CostumeBandanaId);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(PetStoreCatalog.CostumeBandanaId, bird.CostumeId);
            Assert.AreEqual(1, _controller.CurrentState.PetStore.OwnedCostumeQuantities[PetStoreCatalog.CostumeBandanaId]);
        }

        [Test]
        public void SpecialEggToyOffer_UsesCorrectSupplyType()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Tester", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();

            var offer = game.GetPetStoreSupplyOffers().Single(o => o.ItemId == PetStoreCatalog.SpecialEggToyItemId);

            Assert.AreEqual(PetStoreSupplyType.SpecialEggToy, offer.SupplyType);
        }

        [Test]
        public void GetPetStoreSupplyOffers_OwnedCostumeProjectsBuyableFalse()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Tester", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;
            Assert.IsTrue(game.BuyPetStoreSupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume));

            var offer = game.GetPetStoreSupplyOffers().Single(o => o.ItemId == PetStoreCatalog.CostumeBandanaId);

            Assert.AreEqual(1, offer.OwnedQuantity);
            Assert.IsFalse(offer.Buyable);
        }

        [Test]
        public void GetPetStoreSupplyOffers_OwnedCostumeProjectsIsAffordableFalseEvenWhenMoneyIsSufficient()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Tester", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;
            Assert.IsTrue(game.BuyPetStoreSupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume));

            var offer = game.GetPetStoreSupplyOffers().Single(o => o.ItemId == PetStoreCatalog.CostumeBandanaId);

            Assert.IsFalse(offer.IsAffordable);
        }

        [Test]
        public void EggRewardResolution_IsDeterministicAndPersisted()
        {
            _controller.PetStore.BuySupply(PetStoreCatalog.SpecialEggToyItemId, PetStoreSupplyType.SpecialEggToy);
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
            fresh.PetStore.BuySupply(PetStoreCatalog.SpecialEggToyItemId, PetStoreSupplyType.SpecialEggToy);
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
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;

            Assert.IsTrue(game.BuyPetStoreBird("budgie"));
            var careVm = game.GetCareDashboard();
            Assert.IsTrue(careVm.Birds.Any(b => b.Name.Contains("Buddy")));
        }

        [Test]
        public void EquipBirdCostume_FacadeEquipsOwnedCostume_AndCareDashboardIncludesCostumeId()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("Tester", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;
            Assert.IsTrue(game.BuyPetStoreSupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume));

            var birdId = game.Controller.CurrentState.Birds.First().Id;
            var equipResult = game.EquipBirdCostume(birdId, PetStoreCatalog.CostumeBandanaId);
            var careVm = game.GetCareDashboard();
            var birdVm = careVm.Birds.First(b => b.Id == birdId);

            Assert.IsTrue(equipResult);
            Assert.AreEqual(PetStoreCatalog.CostumeBandanaId, birdVm.CostumeId);
        }

        [Test]
        public void FeedConsumesStoredFood_InsteadOfChargingMoney()
        {
            _controller.PetStore.BuySupply(BirdFoodType.SeedMix.ToString(), PetStoreSupplyType.BirdFood, 1);
            var bird = _controller.CurrentState.Birds.First();
            decimal moneyBeforeFeed = _controller.CurrentState.Economy.CurrentBalance;

            var feedResult = _controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsTrue(feedResult.IsSuccess);
            Assert.AreEqual(0, _controller.CurrentState.PetStore.GetTotalFoodUnits());
            Assert.AreEqual(moneyBeforeFeed, _controller.CurrentState.Economy.CurrentBalance);
        }

        [Test]
        public void FeedFailsWhenNoStoredFoodExists()
        {
            var bird = _controller.CurrentState.Birds.First();
            _controller.CurrentState.PetStore.BirdFoodByType.Clear();

            var feedResult = _controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsFalse(feedResult.IsSuccess);
            Assert.AreEqual("NoStoredFood", feedResult.ErrorCode);
        }

        [Test]
        public void FeedingPreferredFood_IncreasesTrust()
        {
            var bird = _controller.CurrentState.Birds.First();
            bird.PreferredFoods.Clear();
            bird.PreferredFoods.Add(BirdFoodType.FruitMedley);
            bird.Trust = 0;
            _controller.PetStore.BuySupply(BirdFoodType.FruitMedley.ToString(), PetStoreSupplyType.BirdFood, 1);

            var feedResult = _controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsTrue(feedResult.IsSuccess);
            Assert.AreEqual(10, bird.Trust);
        }

        [Test]
        public void TrustAndFriendshipPersistThroughLoad()
        {
            var birdA = _controller.CurrentState.Birds[0];
            _controller.PetStore.BuyBird("budgie");
            var birdB = _controller.CurrentState.Birds.Last();

            birdA.Trust = 33;
            birdA.AddFriend(birdB.Id);
            birdB.AddFriend(birdA.Id);

            var save = _controller.CurrentState;
            var fresh = new BirdCafeController();
            var loadResult = fresh.Meta.LoadGame(save);

            Assert.IsTrue(loadResult.IsSuccess);
            Assert.AreEqual(33, fresh.CurrentState.Birds.First(b => b.Id == birdA.Id).Trust);
            Assert.IsTrue(fresh.CurrentState.Birds.First(b => b.Id == birdA.Id).FriendBirdIds.Contains(birdB.Id));
        }

        [Test]
        public void MultipleBirdsCanMaintainFriendships()
        {
            _controller.PetStore.BuyBird("budgie");
            _controller.PetStore.BuyBird("cockatiel");

            var birds = _controller.CurrentState.Birds;
            birds[0].AddFriend(birds[1].Id);
            birds[1].AddFriend(birds[2].Id);
            birds[2].AddFriend(birds[0].Id);

            Assert.IsTrue(birds[0].FriendBirdIds.Contains(birds[1].Id));
            Assert.IsTrue(birds[1].FriendBirdIds.Contains(birds[2].Id));
            Assert.IsTrue(birds[2].FriendBirdIds.Contains(birds[0].Id));
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
