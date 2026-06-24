using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains bird animation-state projection and command operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Gets the current shared animation state for one bird.
        /// </summary>
        /// <param name="birdId">The identifier of the bird to inspect.</param>
        /// <returns>The bird's animation state, or <see langword="null"/> when lookup fails.</returns>
        public BirdAnimationStateViewModel GetBirdAnimationState(string birdId)
        {
            // BirdVisualStateManager refreshes the runtime mood from live gameplay stats before
            // returning the persistent visual state.
            var result = _controller.BirdVisualStates.GetBirdAnimationState(birdId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return null;
            }

            return MapBirdAnimationState((BirdVisualRuntimeState)result.Payload);
        }

        /// <summary>
        /// Gets current shared animation states for every bird in roster order.
        /// </summary>
        /// <returns>A UI-ready list of animation states.</returns>
        public List<BirdAnimationStateViewModel> GetAllBirdAnimationStates()
        {
            var result = _controller.BirdVisualStates.GetAllBirdAnimationStates();
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return new List<BirdAnimationStateViewModel>();
            }

            // Preserve manager ordering so animation presenters remain aligned with the save roster.
            return ((List<BirdVisualRuntimeState>)result.Payload)
                .Select(MapBirdAnimationState)
                .ToList();
        }

        /// <summary>
        /// Advances one bird's animation state using deterministic mood-driven transitions.
        /// </summary>
        /// <param name="birdId">The identifier of the bird to advance.</param>
        /// <returns><see langword="true"/> when advancement succeeds; otherwise <see langword="false"/>.</returns>
        public bool AdvanceBirdAnimationState(string birdId)
        {
            // The manager and state machine own transition weights, repeat prevention, and the
            // deterministic advancement counter.
            var result = _controller.BirdVisualStates.AdvanceBirdAnimationState(birdId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Queues a temporary one-shot bird animation event for the next state advance.
        /// </summary>
        /// <param name="birdId">The identifier of the bird receiving the event.</param>
        /// <param name="eventId">The supported external animation-event identifier.</param>
        /// <returns><see langword="true"/> when the event is queued; otherwise <see langword="false"/>.</returns>
        public bool TriggerBirdAnimationEvent(string birdId, string eventId)
        {
            // Validation occurs in BirdVisualStateManager so every caller uses the same supported
            // event list and bird lookup rules.
            var result = _controller.BirdVisualStates.TriggerBirdAnimationEvent(birdId, eventId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Maps persistent visual runtime state into the animation contract consumed by UIs.
        /// </summary>
        /// <param name="runtime">The shared visual runtime record to project.</param>
        /// <returns>A UI-ready animation-state view model.</returns>
        private BirdAnimationStateViewModel MapBirdAnimationState(BirdVisualRuntimeState runtime)
        {
            // Species and costume live on the bird model, while transition state lives in the
            // separate runtime record. The facade combines both sources into one rendering contract.
            var bird = _controller.CurrentState.Birds
                .FirstOrDefault(candidate => candidate.Id == runtime.BirdId);

            return new BirdAnimationStateViewModel
            {
                BirdId = runtime.BirdId,
                SpeciesId = bird?.SpeciesId,
                CostumeId = bird?.CostumeId,
                CurrentMood = runtime.CurrentMood,
                CurrentVisualState = runtime.CurrentVisualState,
                CurrentVisualStateKey = BirdVisualStateMachine.ToExternalKey(runtime.CurrentVisualState),
                PendingOneShotEventId = runtime.PendingOneShotEventId
            };
        }
    }
}
