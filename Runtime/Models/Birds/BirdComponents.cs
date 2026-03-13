
using System;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// A permanent characteristic modifier for a bird (e.g., "Fast Learner").
    /// </summary>
    [Serializable]
    public class BirdTrait
    {
        /// <summary>
        /// The unique code for the trait type.
        /// </summary>
        public string TraitType { get; set; }

        /// <summary>
        /// Human-readable name of the trait.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Flavor text description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Technical description of what the trait does to stats.
        /// </summary>
        public string GameplayImpactDescription { get; set; }
    }

    /// <summary>
    /// An item equipped by a bird that provides stat bonuses.
    /// </summary>
    [Serializable]
    public class CostumeStats
    {
        /// <summary>
        /// Unique identifier for the costume.
        /// </summary>
        public string CostumeId { get; set; }

        /// <summary>
        /// Display name of the costume.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Bonus applied to the bird's Productivity stat.
        /// </summary>
        public float ProductivityMultiplier { get; set; }

        /// <summary>
        /// Bonus applied to the bird's Friendliness stat.
        /// </summary>
        public float FriendlinessMultiplier { get; set; }

        /// <summary>
        /// Bonus applied to the bird's Reliability stat.
        /// </summary>
        public float ReliabilityMultiplier { get; set; }

        /// <summary>
        /// Bonus applied that effects daily hunger loss.
        /// </summary>
        public float HungerDecayMultiplier { get; set; }

        /// <summary>
        /// Bonus applied that effects daily mood loss 
        /// </summary>
        public float MoodDecayMultiplier { get; set; }

        /// <summary>
        /// Bonus applied to the amount of energy gained each night
        /// </summary>
        public float NightlyEnergyRecoveryMultiplier { get; set; }



        

    }

    /// <summary>
    /// Defines the parameters for a care action (Feed, Play, etc.).
    /// </summary>
    [Serializable]
    public class CareActionTemplate
    {
        /// <summary>
        /// Unique identifier for the action.
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// Name displayed in the UI.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Cost in game currency to perform this action.
        /// </summary>
        public decimal MoneyCost { get; set; }

        /// <summary>
        /// Amount added to Hunger (positive means less hungry).
        /// </summary>
        public float HungerChange { get; set; }

        /// <summary>
        /// Amount added to Mood.
        /// </summary>
        public float MoodChange { get; set; }

        /// <summary>
        /// Amount added to Health.
        /// </summary>
        public float HealthChange { get; set; }

        /// <summary>
        /// Amount added to Energy.
        /// </summary>
        public float EnergyChange { get; set; }

        /// <summary>
        /// Amount added to Stress (usually negative to reduce stress).
        /// </summary>
        public float StressChange { get; set; }

        /// <summary>
        /// Maximum times this can be used per day per bird (-1 for infinite).
        /// </summary>
        public int DailyUseLimit { get; set; } = -1;
    }
}