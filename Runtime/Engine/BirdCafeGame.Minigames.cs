using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.ViewModels;
using System;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains minigame session lifecycle, validation, and deferred-reward operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Stores the active minigame session while the UI is displaying a minigame.
        /// </summary>
        private MinigameSessionViewModel _activeMinigameSession;

        /// <summary>
        /// Stores the screen to restore after the active minigame completes or is cancelled.
        /// </summary>
        private GameScreen _activeMinigameReturnScreen;

        /// <summary>
        /// Stores the minigame used by the Play care workflow when no per-action choice is needed.
        /// </summary>
        private MinigameId _defaultCareMinigame = MinigameId.Flappy;

        /// <summary>
        /// Determines whether a minigame session is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when a session exists; otherwise <see langword="false"/>.</returns>
        public bool HasActiveMinigame()
        {
            return _activeMinigameSession != null;
        }

        /// <summary>
        /// Returns a detached copy of the currently active minigame session.
        /// </summary>
        /// <returns>The active session, or <see langword="null"/> when no minigame is active.</returns>
        public MinigameSessionViewModel GetCurrentMinigameSession()
        {
            if (_activeMinigameSession == null)
            {
                return null;
            }

            // Returning a copy prevents UI code from modifying the facade's active workflow state.
            return CloneSession(_activeMinigameSession);
        }

        /// <summary>
        /// Attempts to start a standalone minigame session for a specific bird.
        /// </summary>
        /// <param name="minigameId">The minigame to launch.</param>
        /// <param name="birdId">The bird associated with the minigame session.</param>
        /// <returns><see langword="true"/> when the session starts; otherwise <see langword="false"/>.</returns>
        public bool TryStartMinigame(MinigameId minigameId, string birdId)
        {
            // Standalone sessions do not carry a pending care reward.
            return TryStartMinigameInternal(
                minigameId,
                birdId,
                wasStartedFromCare: false,
                pendingRewardActionId: null);
        }

        /// <summary>
        /// Attempts to start a care-driven minigame and defer care reward application until success.
        /// </summary>
        /// <param name="birdId">The bird associated with the care action.</param>
        /// <param name="actionId">The care action requesting minigame gating.</param>
        /// <returns><see langword="true"/> when the session starts; otherwise <see langword="false"/>.</returns>
        public bool TryStartCareMinigame(string birdId, string actionId)
        {
            // Only Play currently uses skill-based gating. Feed and Vet execute directly through
            // PerformCare because their effects are not tied to a minigame result.
            if (!string.Equals(actionId, CareActionIds.Play, StringComparison.Ordinal))
            {
                FireToast("Only Play currently supports minigame flow.");
                return false;
            }

            MinigameId minigameId = ResolveDefaultCareMinigame(actionId);
            return TryStartMinigameInternal(
                minigameId,
                birdId,
                wasStartedFromCare: true,
                pendingRewardActionId: actionId);
        }

        /// <summary>
        /// Selects which supported minigame should launch for future Play care actions.
        /// </summary>
        /// <param name="minigameId">The minigame to use for subsequent care launches.</param>
        /// <returns><see langword="true"/> when the selection is supported; otherwise <see langword="false"/>.</returns>
        public bool SetDefaultCareMinigame(MinigameId minigameId)
        {
            if (!IsSupportedCareMinigame(minigameId))
            {
                FireToast("The selected minigame is not supported for care flow.");
                return false;
            }

            _defaultCareMinigame = minigameId;
            return true;
        }

        /// <summary>
        /// Completes the active minigame and applies any pending reward after a successful result.
        /// </summary>
        /// <param name="completion">The completion payload reported by the minigame UI.</param>
        /// <returns><see langword="true"/> when the completion is accepted; otherwise <see langword="false"/>.</returns>
        public bool CompleteCurrentMinigame(MinigameCompletionViewModel completion)
        {
            if (_activeMinigameSession == null)
            {
                FireToast("No active minigame session.");
                return false;
            }

            if (completion == null)
            {
                FireToast("Invalid minigame completion payload.");
                return false;
            }

            if (completion.Status == MinigameCompletionStatus.Success)
            {
                // The reward is deliberately applied only after success. Failed or cancelled runs
                // return to the prior screen without executing the deferred care action.
                bool rewardApplied = ApplyPendingMinigameReward(_activeMinigameSession);
                if (!rewardApplied)
                {
                    // Preserve the original flow: the minigame still closes even if the manager
                    // rejects the reward because state changed while the minigame was active.
                    FireToast("Minigame reward could not be granted.");
                }
            }

            ReturnFromMinigame();
            return true;
        }

        /// <summary>
        /// Cancels the active minigame without applying its deferred reward.
        /// </summary>
        /// <returns><see langword="true"/> when a session is cancelled; otherwise <see langword="false"/>.</returns>
        public bool CancelCurrentMinigame()
        {
            if (_activeMinigameSession == null)
            {
                FireToast("No active minigame session.");
                return false;
            }

            return ReturnFromMinigame();
        }

        /// <summary>
        /// Validates common preconditions for launching a minigame.
        /// </summary>
        /// <param name="minigameId">The requested minigame.</param>
        /// <param name="birdId">The requested bird identifier.</param>
        /// <returns><see langword="true"/> when all launch preconditions are satisfied.</returns>
        private bool IsMinigameStartAllowed(MinigameId minigameId, string birdId)
        {
            // Only one active session is tracked. Starting another would overwrite the return screen
            // and pending reward associated with the first session.
            if (_activeMinigameSession != null)
            {
                FireToast("A minigame is already active.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(birdId))
            {
                FireToast("Bird is required to start a minigame.");
                return false;
            }

            if (!IsMinigameAllowedForCurrentPhase(minigameId))
            {
                FireToast("This minigame is not available in the current phase.");
                return false;
            }

            if (!TryResolveBird(birdId, out _))
            {
                FireToast("Bird ID not found.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates and activates a minigame session after validating launch conditions.
        /// </summary>
        /// <param name="minigameId">The minigame to launch.</param>
        /// <param name="birdId">The bird associated with the session.</param>
        /// <param name="wasStartedFromCare">Whether the session was launched by care flow.</param>
        /// <param name="pendingRewardActionId">The deferred care action, if any.</param>
        /// <returns><see langword="true"/> when activation succeeds; otherwise <see langword="false"/>.</returns>
        private bool TryStartMinigameInternal(
            MinigameId minigameId,
            string birdId,
            bool wasStartedFromCare,
            string pendingRewardActionId)
        {
            if (!IsMinigameStartAllowed(minigameId, birdId))
            {
                return false;
            }

            // Capture the current screen before navigating away so completion can restore the exact
            // workflow that launched the minigame.
            _activeMinigameReturnScreen = _currentScreen;
            _activeMinigameSession = BuildMinigameSession(
                minigameId,
                birdId,
                wasStartedFromCare,
                pendingRewardActionId);

            TransitionTo(GameScreen.Minigame);
            return true;
        }

        /// <summary>
        /// Resolves a bird from the current save-state roster.
        /// </summary>
        /// <param name="birdId">The identifier to locate.</param>
        /// <param name="bird">Receives the matching bird when found.</param>
        /// <returns><see langword="true"/> when a matching bird exists.</returns>
        private bool TryResolveBird(string birdId, out Bird bird)
        {
            bird = _controller.CurrentState.Birds.FirstOrDefault(candidate => candidate.Id == birdId);
            return bird != null;
        }

        /// <summary>
        /// Builds the mutable session record held by the facade during minigame play.
        /// </summary>
        /// <param name="minigameId">The selected minigame.</param>
        /// <param name="birdId">The associated bird.</param>
        /// <param name="wasStartedFromCare">Whether care flow launched the session.</param>
        /// <param name="pendingRewardActionId">The reward action to execute after success.</param>
        /// <returns>A newly initialized session view model.</returns>
        private static MinigameSessionViewModel BuildMinigameSession(
            MinigameId minigameId,
            string birdId,
            bool wasStartedFromCare,
            string pendingRewardActionId)
        {
            return new MinigameSessionViewModel
            {
                Minigame = minigameId,
                BirdId = birdId,
                WasStartedFromCare = wasStartedFromCare,
                PendingRewardActionId = pendingRewardActionId,
                Title = BuildMinigameTitle(minigameId),
                Instructions = BuildMinigameInstructions(minigameId),
                AllowCancel = true
            };
        }

        /// <summary>
        /// Resolves the configured minigame for a care action.
        /// </summary>
        /// <param name="actionId">The care action requesting a minigame.</param>
        /// <returns>The configured care minigame.</returns>
        private MinigameId ResolveDefaultCareMinigame(string actionId)
        {
            // This branch intentionally preserves the action-based extension point even though Play
            // is currently the only supported action and uses the shared default.
            if (string.Equals(actionId, CareActionIds.Play, StringComparison.Ordinal))
            {
                return _defaultCareMinigame;
            }

            return _defaultCareMinigame;
        }

        /// <summary>
        /// Applies the reward deferred by a successful minigame session.
        /// </summary>
        /// <param name="session">The completed minigame session.</param>
        /// <returns><see langword="true"/> when no reward is needed or the reward succeeds.</returns>
        private bool ApplyPendingMinigameReward(MinigameSessionViewModel session)
        {
            if (session == null || string.IsNullOrEmpty(session.PendingRewardActionId))
            {
                return true;
            }

            // Route the reward through the public care facade path. This retains manager validation,
            // economy notifications, visual hooks, and error handling in one authoritative workflow.
            return PerformCare(session.BirdId, session.PendingRewardActionId);
        }

        /// <summary>
        /// Clears the active minigame and restores the screen that launched it.
        /// </summary>
        /// <returns>Always <see langword="true"/> after restoration completes.</returns>
        private bool ReturnFromMinigame()
        {
            // Capture the return screen before clearing session state, because the clear operation
            // resets the stored target to MainMenu for safety.
            GameScreen returnScreen = _activeMinigameReturnScreen;
            ClearActiveMinigameSession();
            TransitionTo(returnScreen);
            return true;
        }

        /// <summary>
        /// Removes all facade-only state associated with an active minigame.
        /// </summary>
        private void ClearActiveMinigameSession()
        {
            _activeMinigameSession = null;
            _activeMinigameReturnScreen = GameScreen.MainMenu;
        }

        /// <summary>
        /// Creates a detached copy of a minigame session for UI consumption.
        /// </summary>
        /// <param name="session">The session to clone.</param>
        /// <returns>A copy of the session, or <see langword="null"/> when the input is null.</returns>
        private static MinigameSessionViewModel CloneSession(MinigameSessionViewModel session)
        {
            if (session == null)
            {
                return null;
            }

            return new MinigameSessionViewModel
            {
                Minigame = session.Minigame,
                BirdId = session.BirdId,
                WasStartedFromCare = session.WasStartedFromCare,
                PendingRewardActionId = session.PendingRewardActionId,
                Title = session.Title,
                Instructions = session.Instructions,
                AllowCancel = session.AllowCancel
            };
        }

        /// <summary>
        /// Builds the player-facing title for a minigame.
        /// </summary>
        /// <param name="minigameId">The minigame whose title is needed.</param>
        /// <returns>A localized-ready title string.</returns>
        private static string BuildMinigameTitle(MinigameId minigameId)
        {
            switch (minigameId)
            {
                case MinigameId.TimingBarGame:
                    return "Timing Bar";
                case MinigameId.Flappy:
                default:
                    return "Flappy Bird Training";
            }
        }

        /// <summary>
        /// Builds the player-facing instructions for a minigame.
        /// </summary>
        /// <param name="minigameId">The minigame whose instructions are needed.</param>
        /// <returns>A concise instruction string.</returns>
        private static string BuildMinigameInstructions(MinigameId minigameId)
        {
            switch (minigameId)
            {
                case MinigameId.TimingBarGame:
                    return "Stop the marker in the target zone.";
                case MinigameId.Flappy:
                default:
                    return "Keep flying and avoid obstacles to win.";
            }
        }

        /// <summary>
        /// Determines whether a minigame is permitted in the controller's current phase.
        /// </summary>
        /// <param name="minigameId">The minigame to evaluate.</param>
        /// <returns><see langword="true"/> when the current phase permits the minigame.</returns>
        private bool IsMinigameAllowedForCurrentPhase(MinigameId minigameId)
        {
            switch (minigameId)
            {
                case MinigameId.Flappy:
                case MinigameId.TimingBarGame:
                    // Current minigames are evening activities and must not interrupt deterministic
                    // daytime simulation or reporting flow.
                    return _controller.CurrentPhase == GamePhase.EveningLoop;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether a minigame can be selected as the Play care minigame.
        /// </summary>
        /// <param name="minigameId">The minigame to evaluate.</param>
        /// <returns><see langword="true"/> when care flow supports the minigame.</returns>
        private static bool IsSupportedCareMinigame(MinigameId minigameId)
        {
            switch (minigameId)
            {
                case MinigameId.Flappy:
                case MinigameId.TimingBarGame:
                    return true;
                default:
                    return false;
            }
        }
    }
}
