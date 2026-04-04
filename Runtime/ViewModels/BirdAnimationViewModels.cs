using BirdCafe.Shared.Enums;
using System;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// UI-safe current animation state payload for one bird.
    /// </summary>
    [Serializable]
    public class BirdAnimationStateViewModel
    {
        /// <summary>
        /// Bird id.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Bird species id used for appearance set resolution.
        /// </summary>
        public string SpeciesId { get; set; }

        /// <summary>
        /// Equipped costume id, or null when no costume is equipped.
        /// </summary>
        public string CostumeId { get; set; }

        /// <summary>
        /// Current high-level visual mood.
        /// </summary>
        public BirdAnimationMood CurrentMood { get; set; }

        /// <summary>
        /// Current internal visual state enum.
        /// </summary>
        public BirdVisualState CurrentVisualState { get; set; }

        /// <summary>
        /// Stable external key for client-side animation lookup.
        /// </summary>
        public string CurrentVisualStateKey { get; set; }

        /// <summary>
        /// Pending one-shot event id, if any.
        /// </summary>
        public string PendingOneShotEventId { get; set; }
    }
}
