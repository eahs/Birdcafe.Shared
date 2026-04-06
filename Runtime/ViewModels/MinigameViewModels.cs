using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Represents the currently active minigame session for UI consumers.
    /// </summary>
    public class MinigameSessionViewModel
    {
        /// <summary>
        /// Gets or sets the minigame that is currently active.
        /// </summary>
        public MinigameId Minigame { get; set; }

        /// <summary>
        /// Gets or sets the bird id associated with the minigame launch.
        /// </summary>
        public string BirdId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this minigame was launched from care flow.
        /// </summary>
        public bool WasStartedFromCare { get; set; }

        /// <summary>
        /// Gets or sets the pending reward action id to apply on successful completion.
        /// </summary>
        public string PendingRewardActionId { get; set; }

        /// <summary>
        /// Gets or sets the UI title for this minigame session.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets optional UI instructions for this minigame session.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current minigame can be cancelled.
        /// </summary>
        public bool AllowCancel { get; set; }
    }

    /// <summary>
    /// Represents completion data submitted by a UI when ending a minigame.
    /// </summary>
    public class MinigameCompletionViewModel
    {
        /// <summary>
        /// Gets or sets the high-level completion status.
        /// </summary>
        public MinigameCompletionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the score reported by the minigame.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the player-facing completion message.
        /// </summary>
        public string ResultMessage { get; set; }
    }
}
