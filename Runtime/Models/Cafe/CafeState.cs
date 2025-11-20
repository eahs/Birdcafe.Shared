
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Models.Economy;

namespace BirdCafe.Shared.Models.Cafe
{
    /// <summary>
    /// Represents the state of the physical cafe business.
    /// </summary>
    [Serializable]
    public class CafeState
    {
        /// <summary>
        /// The custom name of the cafe.
        /// </summary>
        public string CafeName { get; set; } = "Bird Cafe";

        /// <summary>
        /// The reputation score (1-100). Higher popularity attracts more customers.
        /// </summary>
        public float Popularity { get; set; } = 10f; 
        
        /// <summary>
        /// The current inventory holdings.
        /// </summary>
        public InventoryState Inventory { get; set; } = new InventoryState();
        
        /// <summary>
        /// IDs of unlocked special product variants.
        /// </summary>
        public List<string> UnlockedProductVariantIds { get; set; } = new List<string>();

        /// <summary>
        /// IDs of unlocked cafe decorations.
        /// </summary>
        public List<string> UnlockedDecorationIds { get; set; } = new List<string>();
    }
}