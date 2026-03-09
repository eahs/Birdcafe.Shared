using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Economy
{
    /// <summary>
    /// Persistent inventory and unlock state for Rick's Pet Store purchases.
    /// </summary>
    [Serializable]
    public class PetStoreState
    {
        public int BirdFoodUnits { get; set; }

        public Dictionary<string, int> OwnedToyQuantities { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> OwnedCostumeQuantities { get; set; } = new Dictionary<string, int>();

        public int SpecialEggToysOwned { get; set; }

        public List<EggRewardRecord> EggRewardHistory { get; set; } = new List<EggRewardRecord>();

        public int TotalBirdBuffStacks { get; set; }
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
