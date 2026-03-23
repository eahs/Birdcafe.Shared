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

        public Dictionary<string, int> OwnedToyQuantities { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> OwnedCostumeQuantities { get; set; } = new Dictionary<string, int>();

        public int SpecialEggToysOwned { get; set; }

        public List<EggRewardRecord> EggRewardHistory { get; set; } = new List<EggRewardRecord>();

        public int TotalBirdBuffStacks { get; set; }

        
        public int GetFoodUnits(BirdFoodType type)
        {
            return BirdFoodByType.TryGetValue(type, out var qty) ? qty : 0;
        }

        public int GetTotalFoodUnits()
        {
            return BirdFoodByType.Values.Sum(v => Math.Max(0, v));
        }

        public void AddFood(BirdFoodType type, int quantity)
        {
            if (quantity <= 0)
                return;

            BirdFoodByType[type] = GetFoodUnits(type) + quantity;
        }

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
        public int DayNumber { get; set; }

        public EggRewardType RewardType { get; set; }

        public string RewardId { get; set; }

        public string RewardName { get; set; }

        public string RewardDescription { get; set; }
    }
}
