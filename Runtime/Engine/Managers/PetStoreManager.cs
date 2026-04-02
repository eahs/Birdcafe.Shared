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
    /// Handles Pete's Pet Store purchases and deterministic egg rewards.
    /// </summary>
    public class PetStoreManager
    {
        private const string InvalidPhaseCode = "InvalidPhase";
        private const string InvalidItemCode = "InvalidItem";
        private const string InvalidBirdFoodTypeCode = "InvalidBirdFoodType";
        private const string InvalidBirdCode = "InvalidBird";
        private const string InsufficientFundsCode = "InsufficientFunds";
        private const string InvalidQuantityCode = "InvalidQuantity";
        private const string CostumeNotOwnedCode = "CostumeNotOwned";

        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes the pet-store manager bound to a shared controller instance.
        /// </summary>
        public PetStoreManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Purchases a bird offer from Pete's Pet Store and adds the created bird to the roster.
        /// </summary>
        /// <param name="speciesId">Catalog species id from <see cref="PetStoreCatalog.BirdOffers"/>.</param>
        /// <returns>
        /// Success with the created bird payload, or failure when phase, funds, or species selection is invalid.
        /// </returns>
        public EngineResult BuyBird(string speciesId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure(InvalidPhaseCode, "Pete's Pet Store is only open in the evening.");

            var offer = PetStoreCatalog.FindBirdOffer(speciesId);
            if (offer == null)
                return EngineResult.Failure("InvalidBird", "That bird is not sold in Pete's Pet Store.");

            if (_controller.CurrentState.Economy.CurrentBalance < offer.Price)
                return EngineResult.Failure(InsufficientFundsCode, "Not enough money to buy this bird.");

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
                Health = 100,
                Trust = 0,
                PreferredFoods = offer.PreferredFoods.ToList()
            };

            SpendMoney(offer.Price, 1, $"Bird: {offer.DisplayName}", ExpenseCategory.UpgradesAndCustomization, speciesId, createdBird.Id);

            var existingFriend = state.Birds.FirstOrDefault();
            if (existingFriend != null)
            {
                existingFriend.AddFriend(createdBird.Id);
                createdBird.AddFriend(existingFriend.Id);
            }

            state.Birds.Add(createdBird);
            state.CurrentDayState.CurrentPlan.BirdIdsWorking.Add(createdBird.Id);

            return EngineResult.Success(createdBird);
        }

        /// <summary>
        /// Purchases a quantity of a store supply and applies it to persistent pet-store inventory state.
        /// </summary>
        /// <param name="itemId">Supply item id from the shared pet-store catalog.</param>
        /// <param name="supplyType">Supply category used to disambiguate overlapping item ids.</param>
        /// <param name="quantity">Quantity to purchase; must be greater than zero.</param>
        /// <returns>
        /// Success when funds are deducted and inventory is updated, otherwise a validation failure result.
        /// </returns>
        public EngineResult BuySupply(string itemId, PetStoreSupplyType supplyType, int quantity = 1)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure(InvalidPhaseCode, "Pete's Pet Store is only open in the evening.");

            if (quantity <= 0)
                return EngineResult.Failure(InvalidQuantityCode, "Quantity must be at least 1.");

            var supply = PetStoreCatalog.FindSupplyOffer(itemId, supplyType);
            if (supply == null)
                return EngineResult.Failure(InvalidItemCode, "That store item is unavailable.");

            if (supply.SupplyType == PetStoreSupplyType.BirdFood && supply.BirdFoodType == null)
                return EngineResult.Failure(InvalidBirdFoodTypeCode, "That food type is unavailable.");

            var totalCost = supply.Price * quantity;
            if (_controller.CurrentState.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure(InsufficientFundsCode, "Not enough money to buy that item.");

            SpendMoney(totalCost, quantity, $"Supply: {itemId} x{quantity}", supply.ExpenseCategory, supply.ItemId);
            ApplySupplyPurchase(supply, quantity);

            return EngineResult.Success();
        }

        /// <summary>
        /// Opens one owned special egg toy and resolves its deterministic reward.
        /// </summary>
        /// <returns>
        /// Success with the generated <see cref="EggRewardRecord"/> payload, or failure when the call is invalid.
        /// </returns>
        public EngineResult OpenSpecialEggToy()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure(InvalidPhaseCode, "Pete's Pet Store is only open in the evening.");

            var state = _controller.CurrentState;
            if (state.PetStore.SpecialEggToysOwned <= 0)
                return EngineResult.Failure("NoEggToy", "You do not own any special egg toys.");

            state.PetStore.SpecialEggToysOwned--;

            // Seed intentionally uses only persisted progression inputs so repeated runs of the same
            // save/day/reward-history state produce the same reward sequence across front ends.
            int seed = state.CurrentDayState.CurrentPlan.DaySeed ^ (state.PetStore.EggRewardHistory.Count + 1) ^ (state.CurrentDayNumber * 97);
            var rng = new Random(seed);

            var rewards = BuildRewardTable();
            var reward = rewards[rng.Next(rewards.Count)];

            ApplyEggReward(state.PetStore, reward);
            state.PetStore.EggRewardHistory.Add(reward);

            return EngineResult.Success(reward);
        }

        /// <summary>
        /// Equips or unequips a costume on a specific bird.
        /// </summary>
        /// <param name="birdId">Target bird identifier.</param>
        /// <param name="costumeId">
        /// Costume item id to equip, or null to unequip.
        /// Ownership is treated as an unlock and is not consumed.
        /// </param>
        public EngineResult EquipCostume(string birdId, string costumeId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure(InvalidPhaseCode, "Pete's Pet Store is only open in the evening.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null)
                return EngineResult.Failure(InvalidBirdCode, "That bird does not exist.");

            if (costumeId == null)
            {
                bird.CostumeId = null;
                return EngineResult.Success();
            }

            var costume = PetStoreCatalog.FindSupplyOffer(costumeId, PetStoreSupplyType.Costume);
            if (costume == null || costume.SupplyType != PetStoreSupplyType.Costume)
                return EngineResult.Failure(InvalidItemCode, "That costume is unavailable.");

            bool isOwned = _controller.CurrentState.PetStore.OwnedCostumeQuantities.TryGetValue(costumeId, out var ownedQuantity) && ownedQuantity > 0;
            if (!isOwned)
                return EngineResult.Failure(CostumeNotOwnedCode, "You do not own that costume yet.");

            bird.CostumeId = costumeId;
            return EngineResult.Success();
        }

        private void ApplySupplyPurchase(PetStoreSupplyDefinition supply, int quantity)
        {
            var store = _controller.CurrentState.PetStore;

            if (supply.SupplyType == PetStoreSupplyType.BirdFood)
            {
                store.AddFood(supply.BirdFoodType.Value, quantity);
                return;
            }

            if (supply.SupplyType == PetStoreSupplyType.Toy)
            {
                AddQuantity(store.OwnedToyQuantities, supply.ItemId, quantity);
                return;
            }

            if (supply.SupplyType == PetStoreSupplyType.Costume)
            {
                AddQuantity(store.OwnedCostumeQuantities, supply.ItemId, quantity);
                return;
            }

            store.SpecialEggToysOwned += quantity;
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

        private void SpendMoney(decimal amount, int quantity, string reason, ExpenseCategory category, string itemId = null, string relatedBirdId = null)
        {
            var economy = _controller.CurrentState.Economy;
            economy.CurrentBalance -= amount;
            economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = _controller.CurrentState.CurrentDayNumber,
                WeekNumber = _controller.CurrentState.CurrentWeekNumber,
                Amount = -amount,
                Quantity = quantity,
                Reason = reason,
                Timestamp = DateTime.Now,
                Category = category,
                ShortDescription = reason,
                ItemId = itemId,
                RelatedBirdId = relatedBirdId
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
