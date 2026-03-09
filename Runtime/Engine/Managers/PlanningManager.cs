
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Cafe;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles evening planning features: inventory/staffing and Rick's Pet Store purchases.
    /// </summary>
    public class PlanningManager
    {
        private readonly BirdCafeController _controller;

        public PlanningManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public EngineResult SetInventoryOrder(ProductType type, int quantity)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;
            switch (type)
            {
                case ProductType.Coffee: plan.PlannedCoffeePurchase = quantity; break;
                case ProductType.BakedGoods: plan.PlannedBakedGoodsPurchase = quantity; break;
                case ProductType.ThemedMerch: plan.PlannedThemedMerchPurchase = quantity; break;
            }

            return EngineResult.Success();
        }

        public EngineResult SetStaffRoster(string birdId, bool isWorking)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;
            if (isWorking && !plan.BirdIdsWorking.Contains(birdId))
            {
                plan.BirdIdsWorking.Add(birdId);
                plan.BirdIdsResting.Remove(birdId);
            }
            else if (!isWorking && !plan.BirdIdsResting.Contains(birdId))
            {
                plan.BirdIdsResting.Add(birdId);
                plan.BirdIdsWorking.Remove(birdId);
            }

            return EngineResult.Success();
        }

        public EngineResult PurchasePetBird(string catalogId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            var state = _controller.CurrentState;
            var entry = PetStoreCatalog.Birds.FirstOrDefault(x => x.Id == catalogId);
            if (entry == null) return EngineResult.Failure("InvalidBird", "That bird is not available.");

            if (state.Cafe.PetStore.OwnedEntertainerBirds.Any(x => x.CatalogId == catalogId))
                return EngineResult.Failure("AlreadyOwned", "You already own this entertainer bird.");

            if (state.Economy.CurrentBalance < entry.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for that bird.");

            state.Economy.CurrentBalance -= entry.Price;
            state.Cafe.PetStore.OwnedEntertainerBirds.Add(new OwnedEntertainerBird
            {
                CatalogId = entry.Id,
                Name = entry.Name,
                Rarity = entry.Rarity,
                PurchasePrice = entry.Price,
                CustomerBonus = entry.CustomerBonus,
                FlatRevenueBonus = entry.FlatRevenueBonus,
                EffectDescription = entry.EffectDescription
            });

            AddLedger(state, -entry.Price, $"Rick's Pet Store Bird: {entry.Name}", ExpenseCategory.ToysAndActivities, "Bought entertainer bird");
            return EngineResult.Success();
        }

        public EngineResult PurchasePetSupply(PetStoreSupplyType supplyType)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            if (supplyType == PetStoreSupplyType.MysteryEgg)
                return PurchaseMysteryEgg();

            var state = _controller.CurrentState;
            var entry = PetStoreCatalog.Supplies.First(x => x.Type == supplyType);
            if (state.Economy.CurrentBalance < entry.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for that item.");

            state.Economy.CurrentBalance -= entry.Price;
            var inv = state.Cafe.PetStore.Supplies;
            if (supplyType == PetStoreSupplyType.BirdFood) inv.BirdFoodOwned++;
            else if (supplyType == PetStoreSupplyType.Toys) inv.ToysOwned++;
            else if (supplyType == PetStoreSupplyType.Costumes) inv.CostumesOwned++;

            AddLedger(state, -entry.Price, $"Rick's Pet Store Supply: {entry.Name}", ExpenseCategory.FoodAndSupplies, "Bought pet store supply");
            return EngineResult.Success();
        }

        public EngineResult PurchaseMysteryEgg()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            var state = _controller.CurrentState;
            var eggEntry = PetStoreCatalog.Supplies.First(x => x.Type == PetStoreSupplyType.MysteryEgg);
            if (state.Economy.CurrentBalance < eggEntry.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for the mystery egg.");

            state.Economy.CurrentBalance -= eggEntry.Price;
            AddLedger(state, -eggEntry.Price, "Rick's Mystery Egg", ExpenseCategory.ToysAndActivities, "Opened mystery egg");

            int indexSeed = state.CurrentDayState.CurrentPlan.DaySeed ^ (state.Cafe.PetStore.MysteryEggsOpened + 1) * 397 ^ state.CurrentDayNumber * 97;
            var rng = new Random(indexSeed);
            var reward = PetStoreCatalog.EggRewards[rng.Next(PetStoreCatalog.EggRewards.Count)];

            state.Cafe.PetStore.MysteryEggsOpened++;
            state.Cafe.PetStore.RewardHistory.Add(reward.Name);
            if (!state.Cafe.PetStore.UnlockedRewardIds.Contains(reward.Id))
            {
                state.Cafe.PetStore.UnlockedRewardIds.Add(reward.Id);
            }

            if (reward.UnlockUniqueToy) state.Cafe.PetStore.Supplies.ToysOwned += 1;
            if (reward.UnlockRareCostume) state.Cafe.PetStore.Supplies.CostumesOwned += 1;

            AddLedger(state, 0m, $"Egg Reward: {reward.Name}", ExpenseCategory.Miscellaneous, reward.Description);
            return EngineResult.Success(reward);
        }

        public EngineResult FinalizeDay()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            decimal totalCost = EconomyHelper.CalculateTotalPlanCost(
                plan.PlannedCoffeePurchase,
                plan.PlannedBakedGoodsPurchase,
                plan.PlannedThemedMerchPurchase
            );

            if (state.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Cannot afford inventory order.");

            ProcessRestockPayment(state, totalCost, plan);
            AdvanceCalendar(state);
            PrepareNextDayPlan(state, plan);
            TransitionPhase(state);
            return EngineResult.Success();
        }

        private void ProcessRestockPayment(GameSave state, decimal cost, DailyPlan plan)
        {
            state.Economy.CurrentBalance -= cost;
            state.Cafe.Inventory.Coffee.QuantityOnHand += plan.PlannedCoffeePurchase;
            state.Cafe.Inventory.BakedGoods.QuantityOnHand += plan.PlannedBakedGoodsPurchase;
            state.Cafe.Inventory.ThemedMerch.QuantityOnHand += plan.PlannedThemedMerchPurchase;

            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -cost,
                Reason = "Inventory Restock",
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.Miscellaneous
            });
        }

        private void AddLedger(GameSave state, decimal amount, string reason, ExpenseCategory category, string shortDescription)
        {
            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = amount,
                Reason = reason,
                Category = category,
                Timestamp = DateTime.Now,
                ShortDescription = shortDescription
            });
        }

        private void AdvanceCalendar(GameSave state)
        {
            state.CurrentDayNumber++;
            var nextDay = state.CurrentDayName + 1;
            if ((int)nextDay > 6) nextDay = DayOfWeek.Sunday;
            state.CurrentDayName = nextDay;
            if (state.CurrentDayName == DayOfWeek.Sunday)
                state.CurrentWeekNumber++;
        }

        private void PrepareNextDayPlan(GameSave state, DailyPlan previousPlan)
        {
            var r = new Random();
            state.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = state.CurrentDayNumber,
                DaySeed = r.Next(),
                BirdIdsWorking = new List<string>(previousPlan.BirdIdsWorking)
            };
        }

        private void TransitionPhase(GameSave state)
        {
            if (state.CurrentDayName == DayOfWeek.Sunday && state.CurrentDayNumber > 1)
            {
                _controller.SetPhase(GamePhase.Reporting);
            }
            else
            {
                _controller.SetPhase(GamePhase.DayLoop);
            }
        }
    }
}
