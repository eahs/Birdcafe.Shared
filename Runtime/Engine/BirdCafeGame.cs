using BirdCafe.Shared.Engine;
using BirdCafe.Shared.ViewModels;
using System;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Provides the UI-facing facade for the Bird Cafe shared game engine.
    /// </summary>
    /// <remarks>
    /// This core partial contains the singleton, shared dependencies, navigation state, events,
    /// and helpers used by the feature-specific <c>BirdCafeGame.*.cs</c> partial files.
    /// UI projects should communicate with the engine through this facade rather than calling
    /// managers or mutating the save model directly.
    /// </remarks>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Gets the single facade instance used by the application.
        /// </summary>
        public static BirdCafeGame Instance { get; } = new BirdCafeGame();

        /// <summary>
        /// Owns the authoritative game state and manager graph.
        /// </summary>
        /// <remarks>
        /// Feature partials delegate domain work to this controller. They should not duplicate
        /// manager validation or mutate persistent state when a manager operation already exists.
        /// </remarks>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Stores the screen currently selected by the facade-driven navigation flow.
        /// </summary>
        private GameScreen _currentScreen = GameScreen.MainMenu;

        /// <summary>
        /// Exposes the underlying controller for legacy integrations.
        /// </summary>
        /// <remarks>
        /// New UI code should prefer facade methods so validation, event publication, and
        /// navigation behavior remain centralized in <see cref="BirdCafeGame"/>.
        /// </remarks>
        public BirdCafeController Controller => _controller;

        /// <summary>
        /// Gets the currently active screen in the facade-driven UI flow.
        /// </summary>
        public GameScreen CurrentScreen => _currentScreen;

        /// <summary>
        /// Raised after the facade transitions to a different screen.
        /// </summary>
        public event Action<GameScreen> OnScreenChanged;

        /// <summary>
        /// Raised when an action needs short, user-facing feedback.
        /// </summary>
        public event Action<string> OnToastMessage;

        /// <summary>
        /// Raised when the observable money balance changes.
        /// </summary>
        /// <remarks>
        /// The first argument is the previous balance and the second argument is the new balance.
        /// </remarks>
        public event Action<decimal, decimal> OnMoneyChanged;

        /// <summary>
        /// Raised when the UI should display contextual help content.
        /// </summary>
        public event Action<string> OnHelpPopup;

        /// <summary>
        /// Raised when the Oracle chat interface should open or refresh.
        /// </summary>
        /// <remarks>
        /// The event carries no payload. Consumers obtain the current node by calling
        /// <see cref="GetCurrentChatNode"/> after receiving the signal.
        /// </remarks>
        public event Action OnChatPopup;

        /// <summary>
        /// Initializes the singleton facade and its authoritative controller.
        /// </summary>
        private BirdCafeGame()
        {
            // The controller creates and owns every manager. Keeping that graph in one place
            // prevents UI integrations from constructing partially configured engine objects.
            _controller = new BirdCafeController();
        }

        /// <summary>
        /// Changes the current UI screen and notifies all subscribed presentation layers.
        /// </summary>
        /// <param name="screen">The screen that should become active.</param>
        private void TransitionTo(GameScreen screen)
        {
            // Update state before publishing the event so event handlers can immediately query
            // CurrentScreen and observe the same value supplied in the callback.
            _currentScreen = screen;
            OnScreenChanged?.Invoke(screen);
        }

        /// <summary>
        /// Clears facade-only state that must not survive a new load, new game, or menu return.
        /// </summary>
        private void ClearTransientFacadeState()
        {
            // Simulation results are presentation caches, not save-state. A different game must
            // never reuse playback data from the previously active session.
            _cachedSimResult = null;

            // Minigame sessions are also transient UI workflows and must be discarded when the
            // surrounding game session changes.
            ClearActiveMinigameSession();
        }

        /// <summary>
        /// Publishes a safe toast message to the UI.
        /// </summary>
        /// <param name="message">The user-facing message supplied by the engine.</param>
        private void FireToast(string message)
        {
            // Engine failures should normally provide a message. The fallback avoids passing a
            // null string into front ends that assume every toast has renderable content.
            OnToastMessage?.Invoke(message ?? "Unknown error");
        }

        /// <summary>
        /// Adds money directly to the active save balance for development and debugging.
        /// </summary>
        /// <param name="v">The signed amount to add to the current balance.</param>
        /// <remarks>
        /// This bypasses the normal economy and ledger workflow and therefore should not be used
        /// for production gameplay transactions.
        /// </remarks>
        public void AddMoney(int v)
        {
            var oldAmount = _controller.CurrentState.Economy.CurrentBalance;

            // Keep the existing diagnostic output because console-based development tools rely
            // on seeing when this debug-only operation is used.
            Console.WriteLine($"Adding ${v} to balance.");
            _controller.CurrentState.Economy.CurrentBalance += v;

            var newAmount = _controller.CurrentState.Economy.CurrentBalance;

            // Only notify observers when the value actually changed. This avoids unnecessary UI
            // refresh work when a caller supplies zero.
            if (newAmount != oldAmount)
            {
                OnMoneyChanged?.Invoke(oldAmount, newAmount);
            }
        }
    }
}
