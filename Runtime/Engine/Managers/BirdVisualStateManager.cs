using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Owns per-bird persistent visual runtime state and deterministic visual-state advancement.
    /// </summary>
    public class BirdVisualStateManager
    {
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new visual-state manager bound to the shared controller.
        /// </summary>
        public BirdVisualStateManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Backfills missing runtime visual-state records for all birds in the current save.
        /// Safe to call repeatedly.
        /// </summary>
        public void EnsureRuntimeStateForAllBirds()
        {
            var state = _controller.CurrentState;
            if (state.BirdVisualStates == null)
                state.BirdVisualStates = new List<BirdVisualRuntimeState>();

            foreach (var bird in state.Birds)
            {
                EnsureRuntimeState(bird);
            }
        }

        /// <summary>
        /// Returns current UI-safe visual state for one bird after refreshing live mood from bird stats.
        /// </summary>
        public EngineResult GetBirdAnimationState(string birdId)
        {
            var bird = FindBird(birdId);
            if (bird == null)
                return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            var runtime = EnsureRuntimeState(bird);
            BirdVisualStateMachine.RefreshMood(bird, runtime);

            return EngineResult.Success(runtime);
        }

        /// <summary>
        /// Returns UI-safe visual state for all birds in roster order.
        /// </summary>
        public EngineResult GetAllBirdAnimationStates()
        {
            EnsureRuntimeStateForAllBirds();

            var orderedStates = new List<BirdVisualRuntimeState>();
            foreach (var bird in _controller.CurrentState.Birds)
            {
                var runtime = EnsureRuntimeState(bird);
                BirdVisualStateMachine.RefreshMood(bird, runtime);
                orderedStates.Add(runtime);
            }

            return EngineResult.Success(orderedStates);
        }

        /// <summary>
        /// Advances one bird's current visual state using deterministic mood-driven transitions.
        /// </summary>
        public EngineResult AdvanceBirdAnimationState(string birdId)
        {
            var bird = FindBird(birdId);
            if (bird == null)
                return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            var runtime = EnsureRuntimeState(bird);
            BirdVisualStateMachine.Advance(bird, runtime, _controller.CurrentState);
            return EngineResult.Success(runtime);
        }

        /// <summary>
        /// Queues a temporary one-shot animation event to be consumed on next visual-state advance.
        /// </summary>
        public EngineResult TriggerBirdAnimationEvent(string birdId, string eventId)
        {
            var bird = FindBird(birdId);
            if (bird == null)
                return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            if (!BirdVisualStateMachine.IsSupportedEventId(eventId))
                return EngineResult.Failure("InvalidAnimationEvent", "Unknown bird animation event.");

            var runtime = EnsureRuntimeState(bird);
            runtime.PendingOneShotEventId = eventId;

            return EngineResult.Success(runtime);
        }

        /// <summary>
        /// Ensures one bird has a runtime state entry. Intended for paths that add new birds.
        /// </summary>
        public BirdVisualRuntimeState EnsureRuntimeState(Bird bird)
        {
            if (bird == null)
                throw new ArgumentNullException(nameof(bird));

            var save = _controller.CurrentState;
            if (save.BirdVisualStates == null)
                save.BirdVisualStates = new List<BirdVisualRuntimeState>();

            var existing = save.BirdVisualStates.FirstOrDefault(s => s.BirdId == bird.Id);
            if (existing != null)
                return existing;

            var runtime = new BirdVisualRuntimeState
            {
                BirdId = bird.Id,
                CurrentMood = BirdMoodResolver.Resolve(bird),
                CurrentVisualState = BirdVisualState.IdleNeutral,
                PendingOneShotEventId = null,
                ConsecutiveRepeatCount = 0,
                StepCounter = 0
            };

            save.BirdVisualStates.Add(runtime);
            return runtime;
        }

        private Bird FindBird(string birdId)
        {
            if (string.IsNullOrWhiteSpace(birdId))
                return null;

            return _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
        }
    }
}
