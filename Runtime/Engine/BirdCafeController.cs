using BirdCafe.Shared.Engine.Managers;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Engine
{
    /// <summary>
    /// The authoritative core engine. 
    /// Manages the GameSave, Phase State Machine, and sub-managers.
    /// </summary>
    public class BirdCafeController
    {
        /// <summary>
        /// Gets the current state of the game data, including players, cafe, and economy.
        /// </summary>
        public GameSave CurrentState { get; private set; }

        /// <summary>
        /// Gets the current phase of the game loop (e.g., Meta, DayLoop, EveningLoop).
        /// </summary>
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Meta;

        // Managers

        /// <summary>
        /// Manager responsible for meta-game operations like saving, loading, and starting new games.
        /// </summary>
        public MetaManager Meta { get; private set; }

        /// <summary>
        /// Manager responsible for running the daily simulation logic.
        /// </summary>
        public SimulationManager Simulation { get; private set; }

        /// <summary>
        /// Manager responsible for bird care interactions.
        /// </summary>
        public CareManager Care { get; private set; }

        /// <summary>
        /// Manager responsible for planning inventory and staffing for the next day.
        /// </summary>
        public PlanningManager Planning { get; private set; }

        /// <summary>
        /// Manager responsible for generating reports and checking game-over conditions.
        /// </summary>
        public ReportingManager Reporting { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BirdCafeController"/> class.
        /// Sets up all the sub-managers with a reference to this controller.
        /// </summary>
        public BirdCafeController()
        {
            // Initialize each manager, passing 'this' controller so they can access the shared state.
            Meta = new MetaManager(this);
            Simulation = new SimulationManager(this);
            Care = new CareManager(this);
            Planning = new PlanningManager(this);
            Reporting = new ReportingManager(this);

            Meta.StartNewGame("FBLA Judge", "FBLA Cafe");
        }

        /// <summary>
        /// Internal method to change the current game phase.
        /// Used by managers to transition the game state machine.
        /// </summary>
        /// <param name="newPhase">The new phase to transition to.</param>
        internal void SetPhase(GamePhase newPhase)
        {
            // Update the phase property to the new value.
            CurrentPhase = newPhase;
        }

        /// <summary>
        /// Internal method to inject a game save state.
        /// Usually called when loading a game or starting a new one.
        /// </summary>
        /// <param name="state">The game save data object.</param>
        internal void SetState(GameSave state)
        {
            // Replace the current state object with the one provided.
            CurrentState = state;
        }
    }
}