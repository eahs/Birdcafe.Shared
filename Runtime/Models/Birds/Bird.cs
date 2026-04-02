
using BirdCafe.Shared.Enums;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// Represents a single bird entity, including its stats, customization, and state.
    /// Contains logic for safely updating its own stats.
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
        /// </summary>
        public float Energy { get; set; } = 100f;

        /// <summary>
        /// Mental pressure. 1 = Calm, 100 = Panic/High Stress.
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

        #region Customization & Items

        /// <summary>
        /// Hex code for the primary feather color.
        /// </summary>
        public string PrimaryColorHex { get; set; }

        /// <summary>
        /// List of permanent traits affecting the bird's behavior.
        /// </summary>
        public List<BirdTrait> Traits { get; set; } = new List<BirdTrait>();

        /// <summary>
        /// Equipped costume item id, or <see langword="null"/> when no costume is equipped.
        /// </summary>
        public string CostumeId { get; set; } = null;

        #endregion

        #region Social & Preference Data

        /// <summary>
        /// Ongoing trust the bird has in the cafe team (0-100).
        /// </summary>
        public float Trust { get; set; } = 0f;

        /// <summary>
        /// Preferred food types that provide stronger trust gains when fed.
        /// </summary>
        public List<BirdFoodType> PreferredFoods { get; set; } = new List<BirdFoodType>();

        /// <summary>
        /// Other bird IDs this bird has bonded friendships with.
        /// </summary>
        public List<string> FriendBirdIds { get; set; } = new List<string>();

        #endregion

        #region Domain Logic


        /// <summary>
        /// Increases trust by a value while clamping to 0..100.
        /// </summary>
        public void IncreaseTrust(float amount)
        {
            Trust = ClampStat(Trust + amount);
        }

        /// <summary>
        /// Returns true if this bird prefers the specified food type.
        /// </summary>
        public bool PrefersFood(BirdFoodType foodType)
        {
            return PreferredFoods != null && PreferredFoods.Contains(foodType);
        }

        /// <summary>
        /// Creates a mutual friendship relationship between this bird and another bird ID.
        /// </summary>
        public void AddFriend(string otherBirdId)
        {
            if (string.IsNullOrWhiteSpace(otherBirdId) || otherBirdId == Id)
                return;

            if (!FriendBirdIds.Contains(otherBirdId))
                FriendBirdIds.Add(otherBirdId);
        }

        /// <summary>
        /// Applies a care template to this bird.
        /// Handles all stat clamping logic (0-100) internally to prevent bugs.
        /// </summary>
        /// <param name="template">The care action effects to apply.</param>
        public void ApplyCareEffect(CareActionTemplate template)
        {
            if (template == null) return;

            // Math.Min ensures we never go above 100.

            // Hunger (Add, clamp to 100)
            Hunger = ClampStat(Hunger + template.HungerChange);

            // Mood (Add, clamp to 100)
            Mood = ClampStat(Mood + template.MoodChange);

            // Health (Add, clamp to 100)
            Health = ClampStat(Health + template.HealthChange);

            // Energy (Add, clamp to 100)
            Energy = ClampStat(Energy + template.EnergyChange);

            // Stress (Add [usually negative], clamp to 0 minimum)
            // Math.Max ensures we never go below 0.
            Stress = ClampStat(Stress + template.StressChange);
        }

        /// <summary>
        /// Consumes energy for performing a task.
        /// </summary>
        /// <param name="amount">Amount of energy to reduce.</param>
        public void ConsumeEnergy(float amount)
        {
            // Reduce energy but ensure it doesn't drop below 0.
            Energy = ClampStat(Energy - amount);
        }

        /// <summary>
        /// Applies daily decay stats (Hunger, Mood).
        /// </summary>
        /// <param name="hungerDecay">Amount to reduce hunger by.</param>
        /// <param name="moodDecay">Amount to reduce mood by.</param>
        public void ApplyDailyDecay(float hungerDecay, float moodDecay)
        {
            // Reduce stats but clamp to 0.
            Hunger = ClampStat(Hunger - hungerDecay);
            Mood = ClampStat(Mood - moodDecay);
        }

        private static float ClampStat(float value)
        {
            return Math.Max(0, Math.Min(100, value));
        }

        /// <summary>
        /// Recovers energy during a rest day.
        /// </summary>
        /// <param name="amount">Amount of energy to restore.</param>
        public void RecoverEnergy(float amount)
        {
            // Increase energy but clamp to 100.
            Energy = Math.Min(100, Energy + amount);
            // Reduce stress, clamping to 0.
            Stress = Math.Max(0, Stress - 30); // Hardcoded stress relief for resting
        }

        #endregion
    }
}
