
using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    // =================================================================================
    // EVENING SUMMARY / REPORT
    // =================================================================================

    /// <summary>
    /// ViewModel representing the data needed to display the "End of Day" report.
    /// Aggregates financial totals, customer stats, and individual bird performance 
    /// for the user to review before moving to the Care phase.
    /// </summary>
    [Serializable]
    public class DailyReportViewModel
    {
        /// <summary>
        /// The integer number of the day just completed (e.g., 1).
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// A formatted string combining day and name (e.g., "Day 1 - Monday").
        /// </summary>
        public string DayName { get; set; }

        /// <summary>
        /// The current popularity score of the cafe (0-100) after the day's events.
        /// </summary>
        public int CurrentPopularity { get; set; }

        /// <summary>
        /// Total number of customers successfully served.
        /// </summary>
        public int CustomersServed { get; set; }

        /// <summary>
        /// Total number of customers who left without purchasing (sum of WaitTooLong and NoStock).
        /// </summary>
        public int CustomersLost { get; set; }

        /// <summary>
        /// Count of customers who left because service took too long.
        /// </summary>
        public int LostWaitTooLong { get; set; }

        /// <summary>
        /// Count of customers who left because the requested item was out of stock.
        /// </summary>
        public int LostNoStock { get; set; }

        /// <summary>
        /// Total gross income generated during the day.
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Operational profit (Revenue minus COGS and Waste).
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// The player's total liquid cash balance after all daily calculations.
        /// </summary>
        public decimal CurrentMoney { get; set; }

        // --- Sales Breakdown ---

        /// <summary>
        /// Units of Coffee sold.
        /// </summary>
        public int CoffeeSold { get; set; }

        /// <summary>
        /// Units of Baked Goods sold.
        /// </summary>
        public int BakedSold { get; set; }

        /// <summary>
        /// Units of Themed Merchandise sold.
        /// </summary>
        public int MerchSold { get; set; }

        // --- Inventory Context (for UI Progress Bars) ---

        /// <summary>
        /// Total coffee stock available at the start of the day (Sold + Wasted).
        /// Used as the denominator for sales progress bars.
        /// </summary>
        public int CoffeeTotal { get; set; }

        /// <summary>
        /// Total baked goods stock available at the start of the day (Sold + Wasted).
        /// Used as the denominator for sales progress bars.
        /// </summary>
        public int BakedTotal { get; set; }

        /// <summary>
        /// Total merchandise available (Sold + Remaining).
        /// </summary>
        public int MerchTotal { get; set; }

        /// <summary>
        /// A pre-generated narrative string describing the change in popularity 
        /// (e.g., "Popularity is rising! People love the cafe.").
        /// </summary>
        public string PopularityNarrative { get; set; }

        /// <summary>
        /// List of performance summaries for every bird owned by the player.
        /// </summary>
        public List<BirdPerformanceModel> Birds { get; set; } = new List<BirdPerformanceModel>();
    }

    /// <summary>
    /// Represents a single bird's contribution to the day's work.
    /// Used within the <see cref="DailyReportViewModel"/>.
    /// </summary>
    [Serializable]
    public class BirdPerformanceModel
    {
        /// <summary>
        /// Unique ID of the bird.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Display name of the bird.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if the bird was assigned to the roster for this day.
        /// </summary>
        public bool Worked { get; set; }

        /// <summary>
        /// The number of transactions this bird handled.
        /// </summary>
        public int CustomersServed { get; set; }

        /// <summary>
        /// True if the bird fell sick during the shift.
        /// </summary>
        public bool BecameSick { get; set; }
    }

    // =================================================================================
    // EVENING CARE INTERACTION
    // =================================================================================

    /// <summary>
    /// ViewModel for the "Tamagotchi-style" care screen where players feed and heal birds.
    /// </summary>
    [Serializable]
    public class CareDashboardViewModel
    {
        /// <summary>
        /// The player's current available funds. 
        /// UI should update this as actions are performed.
        /// </summary>
        public decimal CurrentMoney { get; set; }

        /// <summary>
        /// The cafe's current popularity score.
        /// </summary>
        public int CurrentPopularity { get; set; }

        /// <summary>
        /// Total bird food currently stored from Pete's Pet Store purchases.
        /// </summary>
        public int StoredBirdFoodUnits { get; set; }

        /// <summary>
        /// A list of all birds owned by the player, formatted for the care UI cards.
        /// </summary>
        public List<BirdCareViewModel> Birds { get; set; } = new List<BirdCareViewModel>();
    }

    /// <summary>
    /// Represents the state of a single bird on the Care Dashboard.
    /// Stats are normalized to integers (0-100) for simple UI sliders.
    /// </summary>
    [Serializable]
    public class BirdCareViewModel
    {
        /// <summary>
        /// Unique ID of the bird.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier for the species definition.
        /// </summary>
        public string SpeciesId { get; set; }

        /// <summary>
        /// Display name of the bird.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Satiety level (0-100).
        /// </summary>
        public int Hunger { get; set; }

        /// <summary>
        /// Happiness level (0-100).
        /// </summary>
        public int Mood { get; set; }

        /// <summary>
        /// Stamina level (0-100).
        /// </summary>
        public int Energy { get; set; }

        /// <summary>
        /// Physical health level (0-100).
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Trust level (0-100) built by feeding preferred foods.
        /// </summary>
        public int Trust { get; set; }

        /// <summary>
        /// Equipped costume item id, or null when no costume is equipped.
        /// </summary>
        public string CostumeId { get; set; }

        /// <summary>
        /// Display-friendly preferred food list for this bird.
        /// </summary>
        public string PreferredFoodsText { get; set; }

        /// <summary>
        /// Number of friendship bonds this bird currently has.
        /// </summary>
        public int FriendshipCount { get; set; }

        /// <summary>
        /// Indicates if the bird has a minor illness requiring Vet attention.
        /// </summary>
        public bool IsSick { get; set; }

        /// <summary>
        /// Indicates if the player has toggled this bird to take a day off tomorrow.
        /// </summary>
        public bool WillRestTomorrow { get; set; }
    }

    /// <summary>
    /// Represents an interactive button for a care action (e.g., "Feed").
    /// </summary>
    [Serializable]
    public class CareActionViewModel
    {
        /// <summary>
        /// The internal ID used to execute the command (e.g., "Feed", "Vet").
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// The text to display on the button (e.g., "Feed Snack").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The cost in currency to perform this action.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Checks player's money to see if action is affordable.
        /// UI should disable the button if this is false.
        /// </summary>
        public bool IsAffordable { get; set; }
    }

    // =================================================================================
    // EVENING PLANNING / SHOP
    // =================================================================================

    /// <summary>
    /// ViewModel for the Planning phase, combining Inventory Purchasing and Staff Rostering.
    /// </summary>
    [Serializable]
    public class PlanningDashboardViewModel
    {
        /// <summary>
        /// The player's current available balance.
        /// </summary>
        public decimal CurrentMoney { get; set; }

        /// <summary>
        /// The cafe's current popularity.
        /// </summary>
        public int CurrentPopularity { get; set; }

        /// <summary>
        /// The calculated total cost of all currently planned purchases.
        /// Must be re-calculated whenever the user changes an order quantity.
        /// </summary>
        public decimal ProjectedCost { get; set; }

        /// <summary>
        /// A list of recent daily results to help the player decide how much to buy.
        /// </summary>
        public List<DailySalesHistoryModel> RecentHistory { get; set; } = new List<DailySalesHistoryModel>();

        /// <summary>
        /// The list of products available for purchase/restock.
        /// </summary>
        public List<InventoryItemModel> Inventory { get; set; } = new List<InventoryItemModel>();

        /// <summary>
        /// The list of staff members available for rostering.
        /// </summary>
        public List<StaffModel> Roster { get; set; } = new List<StaffModel>();

        /// <summary>
        /// A list of validation warnings (e.g., "Not enough money!") to display to the user.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// A single row of historical data for the planning graphs/tables.
    /// </summary>
    [Serializable]
    public class DailySalesHistoryModel
    {
        /// <summary>
        /// The day number this record represents.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Total customers who entered the cafe that day.
        /// </summary>
        public int CustomersArrived { get; set; }

        /// <summary>
        /// Quantity of Coffee sold.
        /// </summary>
        public int CoffeeSold { get; set; }

        /// <summary>
        /// Quantity of Coffee that perished unsold.
        /// </summary>
        public int CoffeeWasted { get; set; }

        /// <summary>
        /// Quantity of Baked Goods sold.
        /// </summary>
        public int BakedSold { get; set; }

        /// <summary>
        /// Quantity of Baked Goods that perished unsold.
        /// </summary>
        public int BakedWasted { get; set; }

        /// <summary>
        /// Quantity of Merchandise sold.
        /// </summary>
        public int MerchSold { get; set; }
    }

    /// <summary>
    /// Represents a product in the purchasing interface.
    /// </summary>
    [Serializable]
    public class InventoryItemModel
    {
        /// <summary>
        /// The type of product (Coffee, BakedGoods, etc.).
        /// </summary>
        public ProductType Type { get; set; }

        /// <summary>
        /// Display name of the product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The quantity currently in stock (before new purchases).
        /// </summary>
        public int CurrentQuantity { get; set; }

        /// <summary>
        /// The amount the user intends to buy for the next day.
        /// </summary>
        public int PlannedPurchase { get; set; }

        /// <summary>
        /// The cost per single unit.
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// The total cost (PlannedPurchase * UnitCost).
        /// </summary>
        public decimal TotalCost { get; set; }
    }

    /// <summary>
    /// Represents a bird in the Roster selection interface.
    /// </summary>
    [Serializable]
    public class StaffModel
    {
        /// <summary>
        /// Unique ID of the bird.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Display name of the bird.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if the bird is currently selected to work the next shift.
        /// </summary>
        public bool IsWorking { get; set; }

        /// <summary>
        /// Human-readable status (e.g., "Working", "Resting", "Sick (Cannot Work)").
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Logic-driven flag indicating if the bird is physically capable of working.
        /// Should disable the toggle in the UI if false.
        /// </summary>
        public bool CanWork { get; set; }
    }

    /// <summary>
    /// Lightweight summary model shown on the evening hub landing screen.
    /// </summary>
    [Serializable]
    public class EveningHubViewModel
    {
        /// <summary>
        /// Current in-game day number shown in the hub header.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Current available balance after day simulation and evening actions.
        /// </summary>
        public decimal CurrentMoney { get; set; }

        /// <summary>
        /// Current cafe popularity used to communicate progression health.
        /// </summary>
        public int CurrentPopularity { get; set; }
    }

    /// <summary>
    /// Top-level snapshot for the pet-store dashboard before selecting specific offers.
    /// </summary>
    [Serializable]
    public class PetStoreDashboardViewModel
    {
        /// <summary>
        /// Current available balance while browsing the pet store.
        /// </summary>
        public decimal CurrentMoney { get; set; }

        /// <summary>
        /// Current popularity value shown for progression context.
        /// </summary>
        public int CurrentPopularity { get; set; }

        /// <summary>
        /// Number of birds currently owned in the save roster.
        /// </summary>
        public int OwnedBirdCount { get; set; }

        /// <summary>
        /// Total stored bird-food units across all food types.
        /// </summary>
        public int BirdFoodUnits { get; set; }

        /// <summary>
        /// Number of unopened special egg toys currently owned.
        /// </summary>
        public int SpecialEggToysOwned { get; set; }

        /// <summary>
        /// Narrative summary of the last opened egg reward, if any.
        /// </summary>
        public string LastEggRewardText { get; set; }
    }

    /// <summary>
    /// UI projection of a purchasable bird offer from the shared pet-store catalog.
    /// </summary>
    [Serializable]
    public class PetStoreBirdOfferViewModel
    {
        /// <summary>
        /// Stable species id used when purchasing this offer.
        /// </summary>
        public string SpeciesId { get; set; }

        /// <summary>
        /// Display name shown in the bird offer card.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Formatted rarity text for presentation layers.
        /// </summary>
        public string RarityText { get; set; }

        /// <summary>
        /// Purchase price for the bird offer.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Descriptive effect/flavor text displayed in store UI.
        /// </summary>
        public string EffectText { get; set; }

        /// <summary>
        /// Whether the current balance can afford this offer right now.
        /// </summary>
        public bool IsAffordable { get; set; }
    }

    /// <summary>
    /// UI projection of a supply offer with ownership and affordability information.
    /// </summary>
    [Serializable]
    public class PetStoreSupplyOfferViewModel
    {
        /// <summary>
        /// Stable item id passed to the purchase action.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Display name shown for this supply offer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human-readable category label used for grouping in UI.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Supply bucket indicating where ownership is tracked.
        /// </summary>
        public PetStoreSupplyType SupplyType { get; set; }

        /// <summary>
        /// Price charged per purchased unit.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Current owned quantity for this specific item.
        /// </summary>
        public int OwnedQuantity { get; set; }

        /// <summary>
        /// Player-facing explanation of what this item does.
        /// </summary>
        public string EffectText { get; set; }

        /// <summary>
        /// Whether the current balance can buy one unit right now.
        /// </summary>
        public bool IsAffordable { get; set; }

        /// <summary>
        /// Indicates whether this offer is directly purchasable rather than reward-only.
        /// </summary>
        public bool Buyable { get; set; }
    }

    /// <summary>
    /// Result payload returned after opening a special egg toy.
    /// </summary>
    [Serializable]
    public class EggRewardResultViewModel
    {
        /// <summary>
        /// True when an egg opening produced a reward payload.
        /// </summary>
        public bool HasReward { get; set; }

        /// <summary>
        /// Display text for the reward category.
        /// </summary>
        public string RewardTypeText { get; set; }

        /// <summary>
        /// Reward name shown to the player.
        /// </summary>
        public string RewardName { get; set; }

        /// <summary>
        /// Reward description shown to explain effect or unlock.
        /// </summary>
        public string RewardDescription { get; set; }
    }

    /// <summary>
    /// UI-friendly representation of one owned pet-store inventory entry.
    /// </summary>
    [Serializable]
    public class OwnedInventoryItem
    {
        /// <summary>
        /// Stable item id for this owned inventory entry.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Display name shown in inventory listing UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category label used for grouped inventory sections.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Supply type used for downstream inventory actions.
        /// </summary>
        public PetStoreSupplyType SupplyType { get; set; }

        /// <summary>
        /// Owned quantity for this inventory item.
        /// </summary>
        public int OwnedQuantity { get; set; }

        /// <summary>
        /// Effect text copied from catalog metadata for quick display.
        /// </summary>
        public string EffectText { get; set; }
    }

    /// <summary>
    /// Grouped inventory collections for rendering the player's pet-store ownership.
    /// </summary>
    [Serializable]
    public class InventoryViewModel
    {
        /// <summary>
        /// Owned food entries grouped for inventory rendering.
        /// </summary>
        public List<OwnedInventoryItem> OwnedFood { get; set; }
        /// <summary>
        /// Owned toy entries grouped for inventory rendering.
        /// </summary>
        public List<OwnedInventoryItem> OwnedToys { get; set; }
        /// <summary>
        /// Owned costume entries grouped for inventory rendering.
        /// </summary>
        public List<OwnedInventoryItem> OwnedCostumes { get; set; }
        /// <summary>
        /// Owned egg-related entries grouped for inventory rendering.
        /// </summary>
        public List<OwnedInventoryItem> OwnedEggs { get; set; }
    }
}
