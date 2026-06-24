using BirdCafe.Shared.ViewModels;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains evening-hub navigation and dashboard projection for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Navigates to the evening hub screen.
        /// </summary>
        public void GoToHub()
        {
            TransitionTo(GameScreen.Hub);
        }

        /// <summary>
        /// Navigates to the evening summary screen.
        /// </summary>
        public void GoToSummary()
        {
            TransitionTo(GameScreen.EveningSummary);
        }

        /// <summary>
        /// Navigates to the evening bird-care screen.
        /// </summary>
        public void GoToCare()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        /// <summary>
        /// Navigates to the evening planning screen.
        /// </summary>
        public void GoToPlanning()
        {
            TransitionTo(GameScreen.EveningPlanning);
        }

        /// <summary>
        /// Navigates to the pet-store dashboard screen.
        /// </summary>
        public void GoToPetStore()
        {
            TransitionTo(GameScreen.EveningPetStore);
        }

        /// <summary>
        /// Navigates to the pet-store bird-offers screen.
        /// </summary>
        public void GoToPetStoreBirds()
        {
            TransitionTo(GameScreen.EveningPetStoreBirds);
        }

        /// <summary>
        /// Navigates to the pet-store supply-offers screen.
        /// </summary>
        public void GoToPetStoreSupplies()
        {
            TransitionTo(GameScreen.EveningPetStoreSupplies);
        }

        /// <summary>
        /// Builds summary data for the evening hub screen.
        /// </summary>
        /// <returns>A UI-ready snapshot of day number, money, and popularity.</returns>
        public EveningHubViewModel GetEveningHub()
        {
            var state = _controller.CurrentState;

            // Keep the hub contract intentionally small. Feature screens request their own detailed
            // view models after the player chooses a destination.
            return new EveningHubViewModel
            {
                DayNumber = state.CurrentDayNumber,
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity
            };
        }
    }
}
