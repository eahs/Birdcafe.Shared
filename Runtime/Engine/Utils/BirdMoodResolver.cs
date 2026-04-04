using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Resolves a high-level animation mood from the bird's live gameplay stats.
    /// </summary>
    public static class BirdMoodResolver
    {
        // Health at or below this threshold is treated as critical and should read as sick.
        private const float CriticalHealthThreshold = 20f;

        // Low energy should visibly bias toward sleepy states.
        private const float SleepyEnergyThreshold = 30f;

        // Low hunger should visibly bias toward hungry states.
        private const float HungryThreshold = 30f;

        // Very high mood without negative overrides should read as excited.
        private const float ExcitedMoodThreshold = 80f;

        // Healthy baseline for generally positive idle behavior.
        private const float HappyMoodThreshold = 60f;
        private const float HappyHealthThreshold = 60f;
        private const float HappyEnergyThreshold = 45f;
        private const float HappyHungerThreshold = 45f;

        /// <summary>
        /// Derives the visual mood with fixed priority: Sick, Sleepy, Hungry, Excited, Happy, Neutral.
        /// </summary>
        public static BirdAnimationMood Resolve(Bird bird)
        {
            if (bird == null)
                return BirdAnimationMood.Neutral;

            if (bird.IsSick || bird.IsSeverelySick || bird.Health <= CriticalHealthThreshold)
                return BirdAnimationMood.Sick;

            if (bird.Energy <= SleepyEnergyThreshold)
                return BirdAnimationMood.Sleepy;

            if (bird.Hunger <= HungryThreshold)
                return BirdAnimationMood.Hungry;

            if (bird.Mood >= ExcitedMoodThreshold)
                return BirdAnimationMood.Excited;

            if (bird.Mood >= HappyMoodThreshold
                && bird.Health >= HappyHealthThreshold
                && bird.Energy >= HappyEnergyThreshold
                && bird.Hunger >= HappyHungerThreshold)
            {
                return BirdAnimationMood.Happy;
            }

            return BirdAnimationMood.Neutral;
        }
    }
}
