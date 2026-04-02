
using System;

namespace BirdCafe.Shared.Enums
{
    /// <summary>
    /// Defines the types of products the cafe sells.
    /// </summary>
    [Serializable]
    public enum ProductType
    {
        /// <summary>
        /// Standard hot coffee. Perishable.
        /// </summary>
        Coffee = 0,

        /// <summary>
        /// Muffins, croissants, etc. Perishable.
        /// </summary>
        BakedGoods = 1,

        /// <summary>
        /// Mugs, shirts, etc. Non-perishable.
        /// </summary>
        ThemedMerch = 2
    }

    /// <summary>
    /// Represents the life stage of a bird.
    /// </summary>
    [Serializable]
    public enum BirdAgeStage
    {
        /// <summary>
        /// A baby bird.
        /// </summary>
        Hatchling = 0,

        /// <summary>
        /// A young bird.
        /// </summary>
        Juvenile = 1,

        /// <summary>
        /// A fully grown bird.
        /// </summary>
        Adult = 2,

        /// <summary>
        /// An old bird.
        /// </summary>
        Elder = 3
    }

    /// <summary>
    /// Categories for financial tracking in the Ledger.
    /// </summary>
    [Serializable]
    public enum ExpenseCategory
    {
        /// <summary>
        /// Costs related to bird feed and basics.
        /// </summary>
        FoodAndSupplies = 0,

        /// <summary>
        /// Medical expenses.
        /// </summary>
        VetCare = 1,

        /// <summary>
        /// Entertainment expenses.
        /// </summary>
        ToysAndActivities = 2,

        /// <summary>
        /// Expenses for bird aesthetics.
        /// </summary>
        UpgradesAndCustomization = 3,

        /// <summary>
        /// Cost of buying Coffee stock.
        /// </summary>
        InventoryCoffee = 4,

        /// <summary>
        /// Cost of buying Baked Goods stock.
        /// </summary>
        InventoryBakedGoods = 5,

        /// <summary>
        /// Cost of buying Merch stock.
        /// </summary>
        InventoryThemedMerch = 6,

        /// <summary>
        /// Other expenses.
        /// </summary>
        Miscellaneous = 7
    }



    /// <summary>
    /// Defines the supported in-game time windows for expense reports.
    /// </summary>
    [Serializable]
    public enum ExpenseReportScope
    {
        /// <summary>
        /// Use only the current in-game day.
        /// </summary>
        CurrentDay = 0,

        /// <summary>
        /// Use the current in-game week.
        /// </summary>
        CurrentWeek = 1,

        /// <summary>
        /// Use an explicit inclusive day-number range.
        /// </summary>
        CustomDayRange = 2
    }

    /// <summary>
    /// Defines how expense report rows should be grouped.
    /// </summary>
    [Serializable]
    public enum ExpenseReportGroupBy
    {
        /// <summary>
        /// One row per in-game day.
        /// </summary>
        ByDay = 0,

        /// <summary>
        /// One row per expense category.
        /// </summary>
        ByCategory = 1,

        /// <summary>
        /// One row per related bird.
        /// </summary>
        ByBird = 2,

        /// <summary>
        /// One row per ledger transaction.
        /// </summary>
        ByTransaction = 3
    }

    /// <summary>
    /// Time filters supported by the Cost of Care modal report.
    /// </summary>
    [Serializable]
    public enum CostOfCareReportTimeFilter
    {
        /// <summary>
        /// Include only the current in-game day.
        /// </summary>
        Today = 0,

        /// <summary>
        /// Include only the current in-game week.
        /// </summary>
        ThisWeek = 1,

        /// <summary>
        /// Include all completed and current in-game days.
        /// </summary>
        AllTime = 2
    }

    /// <summary>
    /// Defines generic types of care actions.
    /// </summary>
    [Serializable]
    public enum CareActionType
    {
        /// <summary>
        /// Give food.
        /// </summary>
        Feed = 0,

        /// <summary>
        /// Play with the bird.
        /// </summary>
        Play = 1,

        /// <summary>
        /// Toggle rest status.
        /// </summary>
        Rest = 2,

        /// <summary>
        /// Take to vet.
        /// </summary>
        VetVisit = 3
    }

    /// <summary>
    /// Constants for Care Action IDs to avoid "Magic Strings" in the codebase.
    /// </summary>
    public static class CareActionIds
    {
        /// <summary>
        /// ID for the Feed action.
        /// </summary>
        public const string Feed = "Feed";

        /// <summary>
        /// ID for the Veterinary Visit action.
        /// </summary>
        public const string Vet = "Vet";

        /// <summary>
        /// ID for playing with the bird.
        /// </summary>
        public const string Play = "Play";
    }

    /// <summary>
    /// Types of events strictly for the visual timeline playback.
    /// </summary>
    [Serializable]
    public enum SimulationTimelineEventType
    {
        /// <summary>
        /// A customer enters the shop.
        /// </summary>
        CustomerArrived,

        /// <summary>
        /// A customer waits in line.
        /// </summary>
        CustomerQueued,

        /// <summary>
        /// A bird begins serving the customer.
        /// </summary>
        ServiceStarted,

        /// <summary>
        /// Service finished successfully.
        /// </summary>
        ServiceCompleted,

        /// <summary>
        /// Service failed (walk out, no stock).
        /// </summary>
        ServiceFailed,

        /// <summary>
        /// Item spoiled at the end of day.
        /// </summary>
        ItemPerishedAtEndOfDay,

        /// <summary>
        /// Bird state update (e.g. got sick).
        /// </summary>
        BirdStateChanged
    }

    /// <summary>
    /// Represents the distinct phases of the engine state machine.
    /// </summary>
    [Serializable]
    public enum GamePhase
    {
        /// <summary>
        /// Main Menu or Loading.
        /// </summary>
        Meta,

        /// <summary>
        /// The active working day.
        /// </summary>
        DayLoop,

        /// <summary>
        /// The evening planning/care time.
        /// </summary>
        EveningLoop,

        /// <summary>
        /// Weekly report screen.
        /// </summary>
        Reporting
    }

    /// <summary>
    /// Possible outcomes for a customer interaction.
    /// </summary>
    [Serializable]
    public enum CustomerOutcome
    {
        /// <summary>
        /// Successfully bought items.
        /// </summary>
        Served,

        /// <summary>
        /// Left because service was too slow.
        /// </summary>
        LeftUnhappy,

        /// <summary>
        /// Left because items were out of stock.
        /// </summary>
        LeftNoStock
    }

    /// <summary>
    /// Rarity bands for birds sold in Pete's Pet Store.
    /// </summary>
    [Serializable]
    public enum BirdRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Exotic = 3
    }

    /// <summary>
    /// Categories for Pete's Pet Store supplies.
    /// </summary>
    [Serializable]
    public enum PetStoreSupplyType
    {
        BirdFood = 0,
        Toy = 1,
        Costume = 2,
        SpecialEggToy = 3
    }

    /// <summary>
    /// Food categories birds can consume during evening care.
    /// </summary>
    [Serializable]
    public enum BirdFoodType
    {
        SeedMix = 0,
        FruitMedley = 1,
        NutriPellets = 2
    }

    /// <summary>
    /// Reward categories granted from the special egg toy.
    /// </summary>
    [Serializable]
    public enum EggRewardType
    {
        BirdBuff = 0,
        UniqueToy = 1,
        RareCostume = 2
    }
}
