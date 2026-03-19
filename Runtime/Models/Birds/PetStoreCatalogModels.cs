using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// Definition for a purchasable bird species in Rick's Pet Store.
    /// </summary>
    [Serializable]
    public class BirdSpeciesOffer
    {
        public string SpeciesId { get; set; }

        public string DisplayName { get; set; }

        public BirdRarity Rarity { get; set; }

        public decimal Price { get; set; }

        public string FlavorDescription { get; set; }

        public float Productivity { get; set; }

        public float Friendliness { get; set; }

        public float Reliability { get; set; }

        public List<BirdFoodType> PreferredFoods { get; set; } = new List<BirdFoodType>();
    }

    /// <summary>
    /// Definition for a purchasable supply in Rick's Pet Store.
    /// </summary>
    [Serializable]
    public class PetStoreSupplyDefinition
    {
        public string ItemId { get; set; }

        public string DisplayName { get; set; }

        public PetStoreSupplyType SupplyType { get; set; }

        public string CategoryText { get; set; }

        public decimal Price { get; set; }

        public string EffectText { get; set; }

        public ExpenseCategory ExpenseCategory { get; set; }

        public BirdFoodType? BirdFoodType { get; set; }
    }
}
