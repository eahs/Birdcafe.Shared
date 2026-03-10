using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles Rick's Pet Store purchases and deterministic egg rewards.
    /// </summary>
    public class PetStoreManager
    {
        private readonly BirdCafeController _controller;

        public PetStoreManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public EngineResult BuyBird(string speciesId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            var offer = PetStoreCatalog.FindBirdOffer(speciesId);
            if (offer == null)
                return EngineResult.Failure("InvalidBird", "That bird is not sold in Rick's Pet Store.");

            if (_controller.CurrentState.Economy.CurrentBalance < offer.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money to buy this bird.");

            SpendMoney(offer.Price, $"Rick's Pet Store Bird Purchase: {offer.DisplayName}", ExpenseCategory.UpgradesAndCustomization);

            var state = _controller.CurrentState;
            var createdBird = new Bird
            {
                Name = PetStoreCatalog.BuildBirdName(offer.DisplayName, state.CurrentDayNumber, state.Birds.Count),
                SpeciesId = offer.SpeciesId,
                PrimaryColorHex = "#A8D8FF",
                Productivity = offer.Productivity,
                Friendliness = offer.Friendliness,
                Reliability = offer.Reliability,
                Mood = 75,
                Hunger = 100,
                Energy = 100,
                Health = 100
            };

            state.Birds.Add(createdBird);
            state.CurrentDayState.CurrentPlan.BirdIdsWorking.Add(createdBird.Id);

            return EngineResult.Success(createdBird);
        }

        public EngineResult BuySupply(string itemId, PetStoreSupplyType supplyType, int quantity = 1)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            if (quantity <= 0)
                return EngineResult.Failure("InvalidQuantity", "Quantity must be at least 1.");

            decimal unitPrice = GetSupplyUnitPrice(itemId, supplyType);
            if (unitPrice <= 0)
                return EngineResult.Failure("InvalidItem", "That store item is unavailable.");

            decimal totalCost = unitPrice * quantity;
            if (_controller.CurrentState.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Not enough money to buy that item.");

            SpendMoney(totalCost, $"Rick's Pet Store Supply Purchase: {itemId} x{quantity}", PetStoreCatalog.GetCategoryForSupply(supplyType));

            var store = _controller.CurrentState.PetStore;
            if (supplyType == PetStoreSupplyType.BirdFood)
            {
                store.BirdFoodUnits += quantity;
            }
            else if (supplyType == PetStoreSupplyType.Toy)
            {
                AddQuantity(store.OwnedToyQuantities, itemId, quantity);
            }
            else if (supplyType == PetStoreSupplyType.Costume)
            {
                AddQuantity(store.OwnedCostumeQuantities, itemId, quantity);
            }
            else
            {
                store.SpecialEggToysOwned += quantity;
            }

            return EngineResult.Success();
        }

        public EngineResult OpenSpecialEggToy()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open in the evening.");

            var state = _controller.CurrentState;
            if (state.PetStore.SpecialEggToysOwned <= 0)
                return EngineResult.Failure("NoEggToy", "You do not own any special egg toys.");

            state.PetStore.SpecialEggToysOwned--;

            // Deterministic: seed is a stable function of day and prior reward count.
            int seed = state.CurrentDayState.CurrentPlan.DaySeed ^ (state.PetStore.EggRewardHistory.Count + 1) ^ (state.CurrentDayNumber * 97);
            var rng = new Random(seed);

            var rewards = BuildRewardTable();
            var reward = rewards[rng.Next(rewards.Count)];

            ApplyEggReward(state.PetStore, reward);
            state.PetStore.EggRewardHistory.Add(reward);

            return EngineResult.Success(reward);
        }

        private List<EggRewardRecord> BuildRewardTable()
        {
            return new List<EggRewardRecord>
            {
                new EggRewardRecord { RewardType = EggRewardType.BirdBuff, RewardId = "Buff_Charm", RewardName = "Aura Buff", RewardDescription = "All birds gain +2 friendliness value in tomorrow's service flow." },
                new EggRewardRecord { RewardType = EggRewardType.UniqueToy, RewardId = "Toy_StarlightSpinner", RewardName = "Starlight Spinner", RewardDescription = "Unique toy added to your collection." },
                new EggRewardRecord { RewardType = EggRewardType.RareCostume, RewardId = "Costume_GoldenVest", RewardName = "Golden Vest", RewardDescription = "Rare costume added to wardrobe." }
            };
        }

        private void ApplyEggReward(PetStoreState store, EggRewardRecord reward)
        {
            reward.DayNumber = _controller.CurrentState.CurrentDayNumber;

            if (reward.RewardType == EggRewardType.BirdBuff)
            {
                store.TotalBirdBuffStacks += 1;
                return;
            }

            if (reward.RewardType == EggRewardType.UniqueToy)
            {
                AddQuantity(store.OwnedToyQuantities, reward.RewardId, 1);
                return;
            }

            AddQuantity(store.OwnedCostumeQuantities, reward.RewardId, 1);
        }

        private decimal GetSupplyUnitPrice(string itemId, PetStoreSupplyType supplyType)
        {
            return (itemId, supplyType) switch
            {
                (_, PetStoreSupplyType.BirdFood) => PetStoreCatalog.BirdFoodPrice,
                (var s, PetStoreSupplyType.Toy) when s == PetStoreCatalog.ToyFeatherWandId => PetStoreCatalog.FeatherWandPrice,
                (var s, PetStoreSupplyType.Toy) when s == PetStoreCatalog.ToyBellOrbId => PetStoreCatalog.BellOrbPrice,
                (var s, PetStoreSupplyType.Costume) when s == PetStoreCatalog.CostumeBandanaId => PetStoreCatalog.BandanaPrice,
                (var s, PetStoreSupplyType.Costume) when s == PetStoreCatalog.CostumeRoyalCapeId => PetStoreCatalog.RoyalCapePrice,
                (_, PetStoreSupplyType.SpecialEggToy) => PetStoreCatalog.SpecialEggToyPrice,
                _ => 0
            };
        }

        private void SpendMoney(decimal amount, string reason, ExpenseCategory category)
        {
            var economy = _controller.CurrentState.Economy;
            economy.CurrentBalance -= amount;
            economy.Ledger.Add(new LedgerEntry
            {
                Amount = -amount,
                Reason = reason,
                Timestamp = DateTime.Now,
                Category = category,
                ShortDescription = reason
            });
        }

        private void AddQuantity(Dictionary<string, int> map, string key, int amount)
        {
            if (!map.ContainsKey(key))
            {
                map[key] = 0;
            }

            map[key] += amount;
        }
    }
}
