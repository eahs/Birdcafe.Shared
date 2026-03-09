using BirdCafe.Shared.Enums;
using System;

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
    }
}
