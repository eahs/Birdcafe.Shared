
using System;

namespace BirdCafe.Shared.Enums
{
    /// <summary>
    /// Defines the types of products the cafe sells.
    /// </summary>
    [Serializable]
    public enum ProductType
    {
        Coffee = 0,
        BakedGoods = 1,
        ThemedMerch = 2
    }

    /// <summary>
    /// Represents the life stage of a bird.
    /// </summary>
    [Serializable]
    public enum BirdAgeStage
    {
        Hatchling = 0,
        Juvenile = 1,
        Adult = 2,
        Elder = 3
    }

    /// <summary>
    /// Categories for financial tracking in the Ledger.
    /// </summary>
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

    /// <summary>
    /// Defines generic types of care actions.
    /// </summary>
    [Serializable]
    public enum CareActionType
    {
        Feed = 0,
        Play = 1,
        Rest = 2,
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
        ServiceFailed,       
        ItemPerishedAtEndOfDay,
        BirdStateChanged    
    }

    /// <summary>
    /// Represents the distinct states of the engine state machine.
    /// </summary>
    [Serializable]
    public enum GamePhase
    {
        Meta,            
        DayLoop,         
        EveningLoop,     
        Reporting        
    }

    /// <summary>
    /// Possible outcomes for a customer interaction.
    /// </summary>
    [Serializable]
    public enum CustomerOutcome
    {
        Served,
        LeftUnhappy,      
        LeftNoStock       
    }
}