using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains pet-store catalog projection, purchase, reward, and wardrobe operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Builds pet-store dashboard data, including ownership and latest reward context.
        /// </summary>
        /// <returns>A UI-ready overview of the player's pet-store status.</returns>
        public PetStoreDashboardViewModel GetPetStoreDashboard()
        {
            var state = _controller.CurrentState;
            var lastReward = state.PetStore.EggRewardHistory.LastOrDefault();

            // The dashboard intentionally uses aggregate values. Detailed offers are requested from
            // the dedicated bird and supply methods only when those screens are opened.
            return new PetStoreDashboardViewModel
            {
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity,
                OwnedBirdCount = state.Birds.Count,
                BirdFoodUnits = state.PetStore.GetTotalFoodUnits(),
                SpecialEggToysOwned = state.PetStore.SpecialEggToysOwned,
                LastEggRewardText = lastReward == null
                    ? "No egg reward opened yet."
                    : $"Last egg reward: {lastReward.RewardName}"
            };
        }

        /// <summary>
        /// Returns unowned bird offers with affordability precomputed for the store UI.
        /// </summary>
        /// <returns>A list of bird offers the player is currently eligible to purchase.</returns>
        public List<PetStoreBirdOfferViewModel> GetPetStoreBirdOffers()
        {
            var state = _controller.CurrentState;
            decimal money = state.Economy.CurrentBalance;

            // Species already owned by the player are filtered out because the current catalog
            // treats each offered species as a one-time roster addition.
            var ownedBirdSpecies = state.Birds.Select(bird => bird.SpeciesId);

            return PetStoreCatalog.BirdOffers
                .Where(offer => !ownedBirdSpecies.Contains(offer.SpeciesId))
                .Select(offer => new PetStoreBirdOfferViewModel
                {
                    SpeciesId = offer.SpeciesId,
                    Name = offer.DisplayName,
                    RarityText = offer.Rarity.ToString(),
                    Price = offer.Price,
                    EffectText = offer.FlavorDescription,
                    IsAffordable = money >= offer.Price
                })
                .ToList();
        }

        /// <summary>
        /// Returns supply offers with ownership, buyability, and affordability precomputed.
        /// </summary>
        /// <returns>A list of UI-ready supply offers.</returns>
        public List<PetStoreSupplyOfferViewModel> GetPetStoreSupplyOffers()
        {
            decimal money = _controller.CurrentState.Economy.CurrentBalance;
            PetStoreState store = _controller.CurrentState.PetStore;
            var offers = new List<PetStoreSupplyOfferViewModel>();

            foreach (var supply in PetStoreCatalog.GetSupplyOffers())
            {
                int ownedQuantity = GetOwnedSupplyQuantity(store, supply);
                bool projectedBuyable = supply.Buyable;

                // Costumes are treated as unique unlocks. Once owned, the same catalog entry should
                // remain visible but must not present another purchase action.
                if (supply.SupplyType == PetStoreSupplyType.Costume && ownedQuantity > 0)
                {
                    projectedBuyable = false;
                }

                offers.Add(new PetStoreSupplyOfferViewModel
                {
                    ItemId = supply.ItemId,
                    Name = supply.DisplayName,
                    CategoryText = supply.CategoryText,
                    SupplyType = supply.SupplyType,
                    Price = supply.Price,
                    OwnedQuantity = ownedQuantity,
                    EffectText = supply.EffectText,
                    IsAffordable = projectedBuyable && money >= supply.Price,
                    Buyable = projectedBuyable
                });
            }

            return offers;
        }

        /// <summary>
        /// Resolves the number of units owned for a catalog supply definition.
        /// </summary>
        /// <param name="store">The player's persistent pet-store state.</param>
        /// <param name="supply">The catalog definition whose quantity should be resolved.</param>
        /// <returns>The number of matching units currently owned.</returns>
        private static int GetOwnedSupplyQuantity(PetStoreState store, PetStoreSupplyDefinition supply)
        {
            // Food is keyed by BirdFoodType rather than by catalog item id.
            if (supply.SupplyType == PetStoreSupplyType.BirdFood && supply.BirdFoodType.HasValue)
            {
                return store.GetFoodUnits(supply.BirdFoodType.Value);
            }

            // Toys and costumes use independent dictionaries because their consumption and
            // ownership rules differ.
            if (supply.SupplyType == PetStoreSupplyType.Toy)
            {
                return store.OwnedToyQuantities.TryGetValue(supply.ItemId, out int toyCount)
                    ? toyCount
                    : 0;
            }

            if (supply.SupplyType == PetStoreSupplyType.Costume)
            {
                return store.OwnedCostumeQuantities.TryGetValue(supply.ItemId, out int costumeCount)
                    ? costumeCount
                    : 0;
            }

            // The remaining supported supply category is the special egg toy, which has a single
            // aggregate counter in save-state.
            return store.SpecialEggToysOwned;
        }

        /// <summary>
        /// Attempts to purchase a bird offer and publishes UI events when needed.
        /// </summary>
        /// <param name="speciesId">The catalog species identifier to purchase.</param>
        /// <returns><see langword="true"/> when the purchase succeeds; otherwise <see langword="false"/>.</returns>
        public bool BuyPetStoreBird(string speciesId)
        {
            decimal oldAmount = _controller.CurrentState.Economy.CurrentBalance;

            // PetStoreManager performs availability checks, charges the economy, adds the bird,
            // and records any associated persistent state.
            var result = _controller.PetStore.BuyBird(speciesId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            decimal newAmount = _controller.CurrentState.Economy.CurrentBalance;
            if (newAmount != oldAmount)
            {
                // Front ends can update money displays without polling the entire save object.
                OnMoneyChanged?.Invoke(oldAmount, newAmount);
            }

            return true;
        }

        /// <summary>
        /// Attempts to purchase one supply offer and publishes UI events when needed.
        /// </summary>
        /// <param name="itemId">The catalog item identifier to purchase.</param>
        /// <param name="supplyType">The category used to route the purchase in the manager.</param>
        /// <returns><see langword="true"/> when the purchase succeeds; otherwise <see langword="false"/>.</returns>
        public bool BuyPetStoreSupply(string itemId, PetStoreSupplyType supplyType)
        {
            decimal oldAmount = _controller.CurrentState.Economy.CurrentBalance;

            // The manager is authoritative for cost, inventory mutation, and validation.
            var result = _controller.PetStore.BuySupply(itemId, supplyType);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            decimal newAmount = _controller.CurrentState.Economy.CurrentBalance;
            if (newAmount != oldAmount)
            {
                OnMoneyChanged?.Invoke(oldAmount, newAmount);
            }

            return true;
        }

        /// <summary>
        /// Equips a costume on a bird, or unequips the current costume when
        /// <paramref name="costumeId"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="birdId">The identifier of the bird being customized.</param>
        /// <param name="costumeId">The owned costume identifier, or <see langword="null"/> to unequip.</param>
        /// <returns><see langword="true"/> when the equipment change succeeds; otherwise <see langword="false"/>.</returns>
        public bool EquipBirdCostume(string birdId, string costumeId)
        {
            // Ownership and bird validation remain inside PetStoreManager so every front end follows
            // identical customization rules.
            var result = _controller.PetStore.EquipCostume(birdId, costumeId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens a special egg toy and maps the resolved reward for UI display.
        /// </summary>
        /// <returns>A reward result indicating whether a reward was successfully produced.</returns>
        public EggRewardResultViewModel OpenSpecialEggToy()
        {
            // The manager consumes the toy and chooses/persists the reward as one atomic workflow.
            var result = _controller.PetStore.OpenSpecialEggToy();
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return new EggRewardResultViewModel { HasReward = false };
            }

            var reward = (EggRewardRecord)result.Payload;
            return new EggRewardResultViewModel
            {
                HasReward = true,
                RewardTypeText = reward.RewardType.ToString(),
                RewardName = reward.RewardName,
                RewardDescription = reward.RewardDescription
            };
        }

        /// <summary>
        /// Opens the wardrobe interaction for a specific bird in integrations that use this hook.
        /// </summary>
        /// <param name="birdId">The identifier of the bird whose wardrobe should be opened.</param>
        /// <remarks>
        /// This method preserves the original placeholder behavior. A future UI-specific wardrobe
        /// event can replace the diagnostic output without moving equipment rules out of the manager.
        /// </remarks>
        public void OpenWardrobe(string birdId)
        {
            var bird = _controller.CurrentState.Birds.FirstOrDefault(candidate => candidate.Id == birdId);

            // Keep the facade hook presentation-neutral for now while retaining the console
            // diagnostic used by the existing implementation.
            Console.WriteLine($"Opening wardrobe for {bird.Name}...");
        }
    }
}
