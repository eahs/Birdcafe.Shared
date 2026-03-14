
using System;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Enumerates all possible high-level screens in the game application.
    /// </summary>
    public enum GameScreen
    {
        /// <summary>
        /// The main start menu.
        /// </summary>
        MainMenu,

        /// <summary>
        /// The load game selection screen.
        /// </summary>
        LoadGame,

        /// <summary>
        /// The first-time user tutorial.
        /// </summary>
        Tutorial,

        /// <summary>
        /// The morning intro banner ("Day 1 - Monday").
        /// </summary>
        DayIntro,

        /// <summary>
        /// The active workday simulation view.
        /// </summary>
        DaySimulation,

        Hub,

        /// <summary>
        /// The end-of-day financial summary.
        /// </summary>
        EveningSummary,

        /// <summary>
        /// The evening care interface (feeding/petting).
        /// </summary>
        EveningCare,

        /// <summary>
        /// The shop and staffing planning interface.
        /// </summary>
        EveningPlanning,

        EveningPetStore,

        EveningPetStoreBirds,

        EveningPetStoreSupplies,

        /// <summary>
        /// The weekly performance report.
        /// </summary>
        WeeklySummary,

        /// <summary>
        /// The game over screen.
        /// </summary>
        GameOver,

        /// <summary>
        /// The Oracle Chat interface.
        /// </summary>
        Chat
    }

    /// <summary>
    /// Represents a save file slot for the load menu.
    /// </summary>
    [Serializable]
    public class SaveSlotViewModel
    {
        /// <summary>
        /// ID of the save.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of player in save.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Current day reached.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Current week reached.
        /// </summary>
        public int WeekNumber { get; set; }
    }
}
