using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains main-menu, save-session, and top-level lifecycle operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Retrieves the save slots available to load-game screens.
        /// </summary>
        /// <returns>A list of UI-ready save-slot descriptions.</returns>
        public List<SaveSlotViewModel> GetSaveSlots()
        {
            // Save discovery belongs to MetaManager. The facade simply exposes its UI-safe result.
            return _controller.Meta.GetAvailableSaves();
        }

        /// <summary>
        /// Creates a new game and enters the tutorial when creation succeeds.
        /// </summary>
        /// <param name="playerName">The display name to assign to the player profile.</param>
        /// <param name="cafeName">The display name to assign to the cafe.</param>
        public void StartNewGame(string playerName, string cafeName)
        {
            // A new session cannot safely retain cached playback or a minigame launched from the
            // previous session, so transient facade state is cleared first.
            ClearTransientFacadeState();

            var result = _controller.Meta.StartNewGame(playerName, cafeName);
            if (!result.IsSuccess)
            {
                // Expected validation failures are rendered by the UI rather than thrown.
                FireToast(result.UserMessage);
                return;
            }

            // New players enter the tutorial before beginning the first simulated day.
            TransitionTo(GameScreen.Tutorial);
        }

        /// <summary>
        /// Continues from a selected save slot and enters the day-introduction flow.
        /// </summary>
        /// <param name="saveId">The identifier of the save slot selected by the player.</param>
        /// <remarks>
        /// The current implementation preserves the existing behavior and performs only facade
        /// navigation; save loading is expected to be completed by the surrounding integration.
        /// </remarks>
        public void LoadGame(string saveId)
        {
            // Do not allow presentation state from the previous session to leak into the loaded
            // session, regardless of where deserialization is performed.
            ClearTransientFacadeState();
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Requests contextual help content from the active UI.
        /// </summary>
        /// <param name="context">The help-topic key understood by the presentation layer.</param>
        public void FireHelpPopup(string context = "General")
        {
            // The facade does not render help. It publishes the topic so each front end can choose
            // the correct visual treatment.
            OnHelpPopup?.Invoke(context);
        }

        /// <summary>
        /// Returns navigation to the main-menu screen and clears transient workflows.
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Returning to the menu ends any presentation-only workflow still in progress.
            ClearTransientFacadeState();
            TransitionTo(GameScreen.MainMenu);
        }
    }
}
