
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Economy
{
    /// <summary>
    /// Holds the current stock levels for all product categories.
    /// </summary>
    [Serializable]
    public class InventoryState
    {
        /// <summary>
        /// Inventory for Coffee products.
        /// </summary>
        public ProductInventoryEntry Coffee { get; set; } = new ProductInventoryEntry { ProductType = ProductType.Coffee, IsPerishable = true };

        /// <summary>
        /// Inventory for Baked Goods.
        /// </summary>
        public ProductInventoryEntry BakedGoods { get; set; } = new ProductInventoryEntry { ProductType = ProductType.BakedGoods, IsPerishable = true };

        /// <summary>
        /// Inventory for Themed Merchandise.
        /// </summary>
        public ProductInventoryEntry ThemedMerch { get; set; } = new ProductInventoryEntry { ProductType = ProductType.ThemedMerch, IsPerishable = false };
    }

    /// <summary>
    /// Represents the stock and pricing details for a specific product type.
    /// </summary>
    [Serializable]
    public class ProductInventoryEntry
    {
        /// <summary>
        /// The type of product this entry tracks.
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// The current total number of units available for sale.
        /// </summary>
        public int QuantityOnHand { get; set; }

        /// <summary>
        /// The price at which each unit is sold to customers.
        /// </summary>
        public decimal CurrentSalePricePerUnit { get; set; }

        /// <summary>
        /// The average cost paid per unit (for profit calculation).
        /// </summary>
        public decimal AverageCostBasisPerUnit { get; set; }

        /// <summary>
        /// If true, unsold stock is removed (wasted) at the end of the day.
        /// </summary>
        public bool IsPerishable { get; set; }
        
        /// <summary>
        /// Tracks quantity purchased specifically for the current day's plan.
        /// </summary>
        public int QuantityPurchasedForToday { get; set; }

        /// <summary>
        /// Tracks quantity carried over from previous days (for non-perishables).
        /// </summary>
        public int QuantityRolledOver { get; set; }
    }
}