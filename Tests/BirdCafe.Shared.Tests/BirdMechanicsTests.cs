using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BirdCafe.Shared.Tests
{
    public class BirdMechanicsTests
    {
        private BirdCafeController CreateEveningController()
        {
            var controller = new BirdCafeController();
            controller.Meta.StartNewGame("Tester", "Cafe");
            controller.Simulation.RunDaySimulation();
            controller.Simulation.AdvanceFromSimulation();
            controller.CurrentState.Economy.CurrentBalance = 5000m;
            return controller;
        }


        private GameSave CloneSave(GameSave save)
        {
            var json = JsonSerializer.Serialize(save);
            return JsonSerializer.Deserialize<GameSave>(json);
        }

        [Test]
        public void BuyFood_DeductsMoney_AndAddsStoredFoodInventory()
        {
            var controller = CreateEveningController();
            decimal startMoney = controller.CurrentState.Economy.CurrentBalance;

            var result = controller.PetStore.BuySupply(PetStoreCatalog.BirdFoodFruitBlendItemId, PetStoreSupplyType.BirdFood, 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(startMoney - (PetStoreCatalog.BirdFoodPrice * 2), controller.CurrentState.Economy.CurrentBalance);
            Assert.AreEqual(2, controller.CurrentState.PetStore.BirdFoodInventory[BirdFoodType.FruitBlend]);
            Assert.AreEqual(2, controller.CurrentState.PetStore.BirdFoodUnits);
        }

        [Test]
        public void Feed_ConsumesStoredFood_AndDoesNotChargeMoney()
        {
            var controller = CreateEveningController();
            var bird = controller.CurrentState.Birds.First();

            controller.PetStore.BuySupply(PetStoreCatalog.BirdFoodSeedMixItemId, PetStoreSupplyType.BirdFood, 1);
            decimal moneyAfterPurchase = controller.CurrentState.Economy.CurrentBalance;

            var feedResult = controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsTrue(feedResult.IsSuccess);
            Assert.AreEqual(moneyAfterPurchase, controller.CurrentState.Economy.CurrentBalance);
            Assert.AreEqual(0, controller.CurrentState.PetStore.BirdFoodUnits);
        }

        [Test]
        public void Feed_Fails_WhenNoStoredFoodExists()
        {
            var controller = CreateEveningController();
            var bird = controller.CurrentState.Birds.First();

            var feedResult = controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.IsFalse(feedResult.IsSuccess);
            Assert.AreEqual("NoStoredBirdFood", feedResult.ErrorCode);
        }

        [Test]
        public void FeedingPreferredFood_IncreasesTrust_AndTrustPersistsOnLoad()
        {
            var controller = CreateEveningController();
            var bird = controller.CurrentState.Birds.First();
            bird.PreferredFoods = new List<BirdFoodType> { BirdFoodType.SeedMix };

            controller.PetStore.BuySupply(PetStoreCatalog.BirdFoodSeedMixItemId, PetStoreSupplyType.BirdFood, 1);
            controller.Care.PerformCareAction(bird.Id, CareActionIds.Feed);

            Assert.GreaterOrEqual(bird.Trust, 8);

            var save = controller.CurrentState;
            var secondController = new BirdCafeController();
            secondController.Meta.LoadGame(save);

            Assert.AreEqual(bird.Trust, secondController.CurrentState.Birds.First(b => b.Id == bird.Id).Trust);
        }

        [Test]
        public void FriendshipData_PersistsAndImprovesSimulationRevenue()
        {
            var baseController = CreateEveningController();
            baseController.PetStore.BuyBird("Budgerigar");

            var lowTrustSave = CloneSave(baseController.CurrentState);
            var highTrustController = new BirdCafeController();
            highTrustController.Meta.LoadGame(CloneSave(lowTrustSave));

            foreach (var bird in highTrustController.CurrentState.Birds)
            {
                bird.Trust = 100;
            }

            var a = highTrustController.CurrentState.Birds[0];
            var b = highTrustController.CurrentState.Birds[1];
            a.GrowFriendship(b.Id, 100);
            b.GrowFriendship(a.Id, 100);

            var loadedSave = CloneSave(highTrustController.CurrentState);
            var verifyController = new BirdCafeController();
            verifyController.Meta.LoadGame(loadedSave);

            var verifyA = verifyController.CurrentState.Birds.First(x => x.Id == a.Id);
            Assert.AreEqual(100, verifyA.GetFriendshipScore(b.Id));

            // Low-trust baseline
            var baselineController = new BirdCafeController();
            baselineController.Meta.LoadGame(CloneSave(lowTrustSave));
            baselineController.SetPhaseForTests(GamePhase.DayLoop);
            highTrustController.SetPhaseForTests(GamePhase.DayLoop);

            baselineController.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 12345;
            highTrustController.CurrentState.CurrentDayState.CurrentPlan.DaySeed = 12345;
            baselineController.CurrentState.Cafe.Inventory.Coffee.QuantityOnHand = 200;
            baselineController.CurrentState.Cafe.Inventory.BakedGoods.QuantityOnHand = 200;
            baselineController.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand = 200;
            highTrustController.CurrentState.Cafe.Inventory.Coffee.QuantityOnHand = 200;
            highTrustController.CurrentState.Cafe.Inventory.BakedGoods.QuantityOnHand = 200;
            highTrustController.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand = 200;

            var baselineResult = (DaySimulationResult)baselineController.Simulation.RunDaySimulation().Payload;
            var boostedResult = (DaySimulationResult)highTrustController.Simulation.RunDaySimulation().Payload;

            Assert.Greater(boostedResult.Economy.TotalRevenue, baselineResult.Economy.TotalRevenue);
        }
    }
}
