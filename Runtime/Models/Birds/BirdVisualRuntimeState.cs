using BirdCafe.Shared.Enums;
using System;

namespace BirdCafe.Shared.Models.Birds
{
    /// <summary>
    /// Persistent per-bird runtime visual state used by the shared animation-state subsystem.
    /// </summary>
    [Serializable]
    public class BirdVisualRuntimeState
    {
        /// <summary>
        /// Persistent bird identifier that owns this visual runtime record.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Last resolved high-level mood category.
        /// </summary>
        public BirdAnimationMood CurrentMood { get; set; } = BirdAnimationMood.Neutral;

        /// <summary>
        /// Current Markov state used to choose the next animation key.
        /// </summary>
        public BirdVisualState CurrentVisualState { get; set; } = BirdVisualState.IdleNeutral;

        /// <summary>
        /// Optional one-shot event id waiting to be consumed by the next advance.
        /// </summary>
        public string PendingOneShotEventId { get; set; }

        /// <summary>
        /// Number of consecutive advances that repeated <see cref="CurrentVisualState"/>.
        /// </summary>
        public int ConsecutiveRepeatCount { get; set; }

        /// <summary>
        /// Deterministic advancement counter incremented after each successful state advance.
        /// </summary>
        public int StepCounter { get; set; }
    }
}
