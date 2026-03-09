using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Cafe
{
    [Serializable]
    public class PetStoreState
    {
        public List<OwnedEntertainerBird> OwnedEntertainerBirds { get; set; } = new List<OwnedEntertainerBird>();
        public PetStoreSupplyInventory Supplies { get; set; } = new PetStoreSupplyInventory();
        public List<string> UnlockedRewardIds { get; set; } = new List<string>();
        public List<string> RewardHistory { get; set; } = new List<string>();
        public int MysteryEggsOpened { get; set; }
    }

    [Serializable]
    public class OwnedEntertainerBird
    {
        public string CatalogId { get; set; }
        public string Name { get; set; }
        public PetBirdRarityTier Rarity { get; set; }
        public decimal PurchasePrice { get; set; }
        public int CustomerBonus { get; set; }
        public decimal FlatRevenueBonus { get; set; }
        public string EffectDescription { get; set; }
    }

    [Serializable]
    public class PetStoreSupplyInventory
    {
        public int BirdFoodOwned { get; set; }
        public int ToysOwned { get; set; }
        public int CostumesOwned { get; set; }
    }

    [Serializable]
    public class PetBirdCatalogEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PetBirdRarityTier Rarity { get; set; }
        public decimal Price { get; set; }
        public int CustomerBonus { get; set; }
        public decimal FlatRevenueBonus { get; set; }
        public string EffectDescription { get; set; }
    }

    [Serializable]
    public class PetSupplyCatalogEntry
    {
        public PetStoreSupplyType Type { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string EffectDescription { get; set; }
    }

    [Serializable]
    public class PetEggRewardEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RewardType { get; set; }
        public string Description { get; set; }
        public int BonusCustomers { get; set; }
        public decimal FlatRevenueBonus { get; set; }
        public bool UnlockUniqueToy { get; set; }
        public bool UnlockRareCostume { get; set; }
    }
}
