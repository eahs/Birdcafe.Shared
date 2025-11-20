
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// Represents a single bird entity, including its stats, customization, and state.
    /// </summary>
    [Serializable]
    public class Bird
    {
        #region Identity & Progression

        /// <summary>
        /// Unique identifier for the bird.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The display name given to the bird by the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identifier for the species definition.
        /// </summary>
        public string SpeciesId { get; set; }

        /// <summary>
        /// Current life stage of the bird (e.g., Hatchling, Adult).
        /// </summary>
        public BirdAgeStage AgeStage { get; set; } = BirdAgeStage.Hatchling;

        /// <summary>
        /// Current experience level.
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Accumulated experience points towards the next level.
        /// </summary>
        public float ExperiencePoints { get; set; }

        #endregion

        #region State Attributes (Scales 1-100)

        /// <summary>
        /// Current emotional state. 1 = Depressed/Angry, 100 = Ecstatic.
        /// Influenced by care, stress, and hunger.
        /// </summary>
        public float Mood { get; set; } = 50f;

        /// <summary>
        /// Physical condition. 1 = Critical condition, 100 = Perfect health.
        /// </summary>
        public float Health { get; set; } = 100f;

        /// <summary>
        /// Satiety level. 1 = Starving, 100 = Fully fed.
        /// </summary>
        public float Hunger { get; set; } = 100f;

        /// <summary>
        /// Stamina level. 1 = Exhausted, 100 = Fully rested.
        /// Decreases as the bird works.
        /// </summary>
        public float Energy { get; set; } = 100f;

        /// <summary>
        /// Mental pressure. 1 = Calm, 100 = Panic/High Stress.
        /// High stress negatively impacts Mood and Reliability.
        /// </summary>
        public float Stress { get; set; } = 0f;

        #endregion

        #region Work Stats (Scales 1-100)

        /// <summary>
        /// Represents how many customers the bird can serve efficiently.
        /// </summary>
        public float Productivity { get; set; } = 10f;

        /// <summary>
        /// Represents the positive impact on Popularity per interaction.
        /// </summary>
        public float Friendliness { get; set; } = 10f;

        /// <summary>
        /// Represents the bird's ability to avoid mistakes or spills.
        /// </summary>
        public float Reliability { get; set; } = 10f;

        #endregion

        #region Flags & Work Assignment

        /// <summary>
        /// Indicates if the bird currently has a minor illness.
        /// </summary>
        public bool IsSick { get; set; }

        /// <summary>
        /// Indicates if the bird is too sick to work.
        /// </summary>
        public bool IsSeverelySick { get; set; }

        /// <summary>
        /// If true, the bird is scheduled to rest during the next simulation day.
        /// </summary>
        public bool AssignedDayOffNextDay { get; set; }

        /// <summary>
        /// Tracks if the bird participated in the most recent day's work.
        /// </summary>
        public bool WorkedLastSimulation { get; set; }

        #endregion

        #region Customization

        /// <summary>
        /// Hex code for the primary feather color.
        /// </summary>
        public string PrimaryColorHex { get; set; }

        /// <summary>
        /// Hex code for the secondary feather color.
        /// </summary>
        public string SecondaryColorHex { get; set; }

        /// <summary>
        /// Hex code for the beak color.
        /// </summary>
        public string BeakColorHex { get; set; }

        /// <summary>
        /// Identifier for the currently worn costume.
        /// </summary>
        public string CostumeId { get; set; }

        /// <summary>
        /// List of identifiers for equipped accessories (glasses, hats, etc.).
        /// </summary>
        public List<string> AccessoryIds { get; set; } = new List<string>();

        #endregion

        #region Traits & Equipment

        /// <summary>
        /// List of permanent traits affecting the bird's behavior.
        /// </summary>
        public List<BirdTrait> Traits { get; set; } = new List<BirdTrait>();

        /// <summary>
        /// List of functional items currently equipped to the bird.
        /// </summary>
        public List<EquippedItem> EquippedItems { get; set; } = new List<EquippedItem>();

        #endregion
    }
}