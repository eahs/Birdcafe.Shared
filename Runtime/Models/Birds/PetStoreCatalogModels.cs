using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// Definition for a purchasable bird species in Pete's Pet Store.
    /// </summary>
    [Serializable]
    public class BirdSpeciesOffer
    {
        /// <summary>
        /// Stable species identifier used for save persistence and catalog lookups.
        /// </summary>
        public string SpeciesId { get; set; }

        /// <summary>
        /// Player-facing name shown in store listings and purchase receipts.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Rarity bucket used by UI presentation to communicate how special this offer is.
        /// </summary>
        public BirdRarity Rarity { get; set; }

        /// <summary>
        /// Purchase cost deducted from the cafe balance when this bird is bought.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Flavor text displayed in pet-store screens to describe the species identity.
        /// </summary>
        public string FlavorDescription { get; set; }

        /// <summary>
        /// Base productivity stat assigned to newly created birds of this species.
        /// </summary>
        public float Productivity { get; set; }

        /// <summary>
        /// Base friendliness stat assigned to newly created birds of this species.
        /// </summary>
        public float Friendliness { get; set; }

        /// <summary>
        /// Base reliability stat assigned to newly created birds of this species.
        /// </summary>
        public float Reliability { get; set; }

        /// <summary>
        /// Preferred food types used by care systems to award higher trust when matched.
        /// </summary>
        public List<BirdFoodType> PreferredFoods { get; set; } = new List<BirdFoodType>();
    }

    /// <summary>
    /// Definition for a purchasable supply in Pete's Pet Store.
    /// </summary>
    [Serializable]
    public class PetStoreSupplyDefinition
    {
        /// <summary>
        /// Stable supply identifier recorded in ownership dictionaries and ledger entries.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Display name shown to players in store and inventory views.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Supply bucket that determines which inventory collection receives the purchase.
        /// </summary>
        public PetStoreSupplyType SupplyType { get; set; }

        /// <summary>
        /// UI category label used to group this offer in shopping and inventory screens.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Unit price charged per quantity during store checkout.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Short explanation of the item's gameplay effect shown to the player.
        /// </summary>
        public string EffectText { get; set; }

        /// <summary>
        /// Ledger expense category used when this item is purchased.
        /// </summary>
        public ExpenseCategory ExpenseCategory { get; set; }

        /// <summary>
        /// Optional food subtype, required when <see cref="SupplyType"/> is <see cref="PetStoreSupplyType.BirdFood"/>.
        /// </summary>
        public BirdFoodType? BirdFoodType { get; set; }

        /// <summary>
        /// Indicates whether the offer can be directly purchased, versus reward-only catalog entries.
        /// </summary>
        public bool Buyable { get; set; } = false;
    }
}
