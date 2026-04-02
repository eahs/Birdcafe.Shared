using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Models.Economy
{
    /// <summary>
    /// Persistent inventory and unlock state for Pete's Pet Store purchases.
    /// </summary>
    [Serializable]
    public class PetStoreState
    {
        /// <summary>
        /// Legacy total bird food units (kept for compatibility, mapped to SeedMix).
        /// </summary>
        public int BirdFoodUnits
        {
            get => GetFoodUnits(BirdFoodType.SeedMix);
            set => BirdFoodByType[BirdFoodType.SeedMix] = Math.Max(0, value);
        }

        /// <summary>
        /// Owned bird food quantities by food type.
        /// </summary>
        public Dictionary<BirdFoodType, int> BirdFoodByType { get; set; } = new Dictionary<BirdFoodType, int>();

        /// <summary>
        /// Owned toy counts keyed by catalog item id.
        /// </summary>
        public Dictionary<string, int> OwnedToyQuantities { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Owned costume counts keyed by catalog item id.
        /// </summary>
        public Dictionary<string, int> OwnedCostumeQuantities { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of unopened special egg toys currently in storage.
        /// </summary>
        public int SpecialEggToysOwned { get; set; }

        /// <summary>
        /// History of deterministic rewards pulled from special egg toys.
        /// </summary>
        public List<EggRewardRecord> EggRewardHistory { get; set; } = new List<EggRewardRecord>();

        /// <summary>
        /// Count of stackable bird-buff rewards earned from egg openings.
        /// </summary>
        public int TotalBirdBuffStacks { get; set; }

        /// <summary>
        /// Returns the currently stored quantity for a specific food type.
        /// </summary>
        public int GetFoodUnits(BirdFoodType type)
        {
            return BirdFoodByType.TryGetValue(type, out var qty) ? qty : 0;
        }

        /// <summary>
        /// Returns total stored bird-food units across all food subtypes.
        /// </summary>
        public int GetTotalFoodUnits()
        {
            return BirdFoodByType.Values.Sum(v => Math.Max(0, v));
        }

        /// <summary>
        /// Adds owned food inventory for the supplied type.
        /// </summary>
        /// <remarks>
        /// Non-positive quantities are ignored so callers can pass user input safely.
        /// </remarks>
        public void AddFood(BirdFoodType type, int quantity)
        {
            if (quantity <= 0)
                return;

            BirdFoodByType[type] = GetFoodUnits(type) + quantity;
        }

        /// <summary>
        /// Attempts to consume owned food inventory for a care action.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when enough inventory exists and is consumed; otherwise <see langword="false"/>.
        /// </returns>
        public bool TryConsumeFood(BirdFoodType type, int quantity)
        {
            if (quantity <= 0)
                return false;

            var current = GetFoodUnits(type);
            if (current < quantity)
                return false;

            BirdFoodByType[type] = current - quantity;
            return true;
        }
    }

    /// <summary>
    /// A persisted record of one opened special egg toy reward.
    /// </summary>
    [Serializable]
    public class EggRewardRecord
    {
        /// <summary>
        /// In-game day when this reward was granted.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Reward category used to route the reward to the correct inventory/effect pipeline.
        /// </summary>
        public EggRewardType RewardType { get; set; }

        /// <summary>
        /// Stable reward id used by inventory storage and catalog lookups.
        /// </summary>
        public string RewardId { get; set; }

        /// <summary>
        /// Display name shown in reward history and popups.
        /// </summary>
        public string RewardName { get; set; }

        /// <summary>
        /// Player-facing explanation of what the reward grants.
        /// </summary>
        public string RewardDescription { get; set; }
    }
}
