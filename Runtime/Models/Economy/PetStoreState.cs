using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Models.Economy
{
    /// <summary>
    /// Persistent inventory and unlock state for Rick's Pet Store purchases.
    /// </summary>
    [Serializable]
    public class PetStoreState
    {
        public Dictionary<BirdFoodType, int> BirdFoodInventory { get; set; } = new Dictionary<BirdFoodType, int>();

        public int BirdFoodUnits => BirdFoodInventory.Values.Sum();

        public Dictionary<string, int> OwnedToyQuantities { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> OwnedCostumeQuantities { get; set; } = new Dictionary<string, int>();

        public int SpecialEggToysOwned { get; set; }

        public List<EggRewardRecord> EggRewardHistory { get; set; } = new List<EggRewardRecord>();

        public int TotalBirdBuffStacks { get; set; }

        public void AddBirdFood(BirdFoodType foodType, int quantity)
        {
            if (!BirdFoodInventory.ContainsKey(foodType))
            {
                BirdFoodInventory[foodType] = 0;
            }

            BirdFoodInventory[foodType] += quantity;
        }

        public bool TryConsumeBirdFood(BirdFoodType foodType)
        {
            if (!BirdFoodInventory.TryGetValue(foodType, out var quantity) || quantity <= 0)
            {
                return false;
            }

            BirdFoodInventory[foodType] = quantity - 1;
            return true;
        }

        public bool TryConsumeAnyBirdFood(out BirdFoodType consumedType)
        {
            foreach (var entry in BirdFoodInventory.Where(kvp => kvp.Value > 0).OrderBy(kvp => kvp.Key))
            {
                BirdFoodInventory[entry.Key] = entry.Value - 1;
                consumedType = entry.Key;
                return true;
            }

            consumedType = BirdFoodType.SeedMix;
            return false;
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
