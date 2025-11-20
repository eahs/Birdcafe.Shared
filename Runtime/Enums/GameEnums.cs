
using System;

namespace BirdCafe.Shared.Enums
{
    [Serializable]
    public enum ProductType
    {
        Coffee = 0,
        BakedGoods = 1,
        ThemedMerch = 2
    }

    [Serializable]
    public enum BirdAgeStage
    {
        Hatchling = 0,
        Juvenile = 1,
        Adult = 2,
        Elder = 3
    }

    [Serializable]
    public enum ExpenseCategory
    {
        FoodAndSupplies = 0,
        VetCare = 1,
        ToysAndActivities = 2,
        UpgradesAndCustomization = 3,
        InventoryCoffee = 4,
        InventoryBakedGoods = 5,
        InventoryThemedMerch = 6,
        Miscellaneous = 7
    }

    [Serializable]
    public enum CareActionType
    {
        Feed = 0,
        Play = 1,
        Rest = 2,
        VetVisit = 3
    }

    [Serializable]
    public enum EventType
    {
        // General Meta events
        DayStarted,
        DayEnded,
        BirdSick,
        BirdRecovered,
        ProductSold,
        ProductWasted,
        CustomerServed,
        PopularityChanged,
        GameOver,
        StoryTriggered
    }

    /// <summary>
    /// Represents the distinct states of the engine state machine.
    /// </summary>
    [Serializable]
    public enum GamePhase
    {
        Meta,            // Main Menu / Loading
        DayLoop,         // Ready to run simulation or currently simulating
        EveningLoop,     // Care, Summary, Planning
        Reporting        // Weekly Summary or Game Over
    }

    /// <summary>
    /// Possible outcomes for a customer interaction.
    /// </summary>
    [Serializable]
    public enum CustomerOutcome
    {
        Served,
        LeftUnhappy,      // Low patience or bad service
        LeftNoStock       // Product unavailable
    }

    /// <summary>
    /// Types of events strictly for the visual timeline playback.
    /// </summary>
    [Serializable]
    public enum SimulationTimelineEventType
    {
        CustomerArrived,
        CustomerQueued,
        ServiceStarted,
        ServiceCompleted,
        ServiceFailed,       // Includes LeftUnhappy, LeftNoStock
        ItemPerishedAtEndOfDay,
        BirdStateChanged     // E.g. exhaustion
    }
}