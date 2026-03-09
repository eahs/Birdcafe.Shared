using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.PetStore
{
    /// <summary>
    /// Persistent state for Rick's Pet Store ownership and unlocks.
    /// </summary>
    [Serializable]
    public class PetStoreState
    {
        public List<string> OwnedEntertainerBirdIds { get; set; } = new List<string>();
        public SupplyInventory Supplies { get; set; } = new SupplyInventory();
        public List<string> UnlockedEggRewardIds { get; set; } = new List<string>();
        public int MysteryEggOpenCount { get; set; }
        public string LastUnlockedEggRewardId { get; set; }
    }

    [Serializable]
    public class SupplyInventory
    {
        public int BirdFoodCount { get; set; }
        public int ToyCount { get; set; }
        public int CostumeCount { get; set; }
    }

    /// <summary>
    /// Catalog definition for entertainer birds sold in Rick's Pet Store.
    /// </summary>
    [Serializable]
    public class PetBirdDefinition
    {
        public string BirdId { get; set; }
        public string SpeciesName { get; set; }
        public decimal Price { get; set; }
        public PetBirdRarity Rarity { get; set; }
        public string EffectDescription { get; set; }
        public decimal DailyRevenueBonus { get; set; }
        public float DailyPopularityBonus { get; set; }
    }

    /// <summary>
    /// Catalog definition for supplies sold in Rick's Pet Store.
    /// </summary>
    [Serializable]
    public class PetSupplyDefinition
    {
        public PetStoreSupplyType SupplyType { get; set; }
        public string DisplayName { get; set; }
        public decimal Price { get; set; }
        public string EffectDescription { get; set; }
    }

    /// <summary>
    /// Reward definition for the mystery egg table.
    /// </summary>
    [Serializable]
    public class EggRewardDefinition
    {
        public string RewardId { get; set; }
        public string DisplayName { get; set; }
        public EggRewardType RewardType { get; set; }
        public string Description { get; set; }
        public decimal DailyRevenueBonus { get; set; }
        public float DailyPopularityBonus { get; set; }
    }
}
