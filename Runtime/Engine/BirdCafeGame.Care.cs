using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains evening bird-care projection and command operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Builds the care dashboard for all owned birds and current economy context.
        /// </summary>
        /// <returns>A UI-ready care dashboard.</returns>
        public CareDashboardViewModel GetCareDashboard()
        {
            var viewModel = new CareDashboardViewModel
            {
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                StoredBirdFoodUnits = _controller.CurrentState.PetStore.GetTotalFoodUnits()
            };

            // Map birds into separate view models. This prevents UI controls from directly
            // changing Bird instances stored in GameSave.
            foreach (var bird in _controller.CurrentState.Birds)
            {
                viewModel.Birds.Add(MapBirdToCareModel(bird));
            }

            return viewModel;
        }

        /// <summary>
        /// Returns care actions with affordability and readiness values precomputed for the UI.
        /// </summary>
        /// <param name="birdId">The bird for whom the actions are being displayed.</param>
        /// <returns>A list of currently presented care actions.</returns>
        /// <remarks>
        /// The current action set is common to every bird. The parameter remains part of the public
        /// contract so future bird-specific restrictions can be introduced without changing callers.
        /// </remarks>
        public List<CareActionViewModel> GetAvailableActions(string birdId)
        {
            var config = _controller.CurrentState.Config;
            decimal money = _controller.CurrentState.Economy.CurrentBalance;
            int foodInStorage = _controller.CurrentState.PetStore.GetTotalFoodUnits();

            var actions = new List<CareActionViewModel>
            {
                // Feeding consumes stored inventory rather than charging money at action time.
                new CareActionViewModel
                {
                    ActionId = CareActionIds.Feed, Label = "Feed (Use Stored Food)", Cost = 0, IsAffordable = foodInStorage > 0
                },
                new CareActionViewModel
                {
                    ActionId = CareActionIds.Play, Label = "Play (Mood)", Cost = config.BaselinePlayCost
                },
                new CareActionViewModel
                {
                    ActionId = CareActionIds.Vet, Label = "Vet Visit", Cost = config.BaselineVetCost
                }
            };

            // Non-food actions are paid directly from the economy, so affordability is based on
            // the current balance at the moment the dashboard is requested.
            foreach (var action in actions.Where(action => action.ActionId != CareActionIds.Feed))
            {
                action.IsAffordable = money >= action.Cost;
            }

            return actions;
        }

        /// <summary>
        /// Executes an evening care action through the authoritative care manager.
        /// </summary>
        /// <param name="birdId">The identifier of the bird receiving care.</param>
        /// <param name="actionId">The care-action identifier to perform.</param>
        /// <returns><see langword="true"/> when care succeeds; otherwise <see langword="false"/>.</returns>
        public bool PerformCare(string birdId, string actionId)
        {
            decimal oldAmount = _controller.CurrentState.Economy.CurrentBalance;

            // CareManager validates phase, bird availability, resource requirements, stat changes,
            // and any corresponding economy/ledger effects.
            var result = _controller.Care.PerformCareAction(birdId, actionId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            // Feeding also queues a one-shot visual response. The gameplay action remains successful
            // even though animation consumption occurs later in the presentation loop.
            if (actionId == CareActionIds.Feed)
            {
                _controller.BirdVisualStates.TriggerBirdAnimationEvent(
                    birdId,
                    BirdAnimationEventIds.TreatGiven);
            }

            decimal newAmount = _controller.CurrentState.Economy.CurrentBalance;
            if (newAmount != oldAmount)
            {
                OnMoneyChanged?.Invoke(oldAmount, newAmount);
            }

            return true;
        }

        /// <summary>
        /// Toggles whether a bird is assigned to rest tomorrow.
        /// </summary>
        /// <param name="birdId">The identifier of the bird whose rest assignment should change.</param>
        /// <returns><see langword="true"/> when the assignment changes; otherwise <see langword="false"/>.</returns>
        public bool ToggleRest(string birdId)
        {
            // The manager enforces evening-phase and bird-state restrictions before changing the
            // persistent next-day assignment.
            var result = _controller.Care.ToggleRest(birdId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Maps an authoritative bird model into the care screen's detached presentation model.
        /// </summary>
        /// <param name="bird">The bird domain model to project.</param>
        /// <returns>A UI-safe care view model.</returns>
        private static BirdCareViewModel MapBirdToCareModel(Bird bird)
        {
            // Numeric domain values are exposed as whole numbers because the current care UI renders
            // integer meters rather than simulation-level floating-point precision.
            return new BirdCareViewModel
            {
                Id = bird.Id,
                SpeciesId = bird.SpeciesId,
                Name = bird.Name,
                Hunger = (int)bird.Hunger,
                Mood = (int)bird.Mood,
                Energy = (int)bird.Energy,
                Health = (int)bird.Health,
                Trust = (int)bird.Trust,
                CostumeId = bird.CostumeId,
                PreferredFoodsText = bird.PreferredFoods.Count == 0
                    ? "None"
                    : string.Join(", ", bird.PreferredFoods),
                FriendshipCount = bird.FriendBirdIds.Count,
                IsSick = bird.IsSick,
                WillRestTomorrow = bird.AssignedDayOffNextDay
            };
        }
    }
}
