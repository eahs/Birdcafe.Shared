
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Cafe;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Meta;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared
{
    /// <summary>
    /// The root data object for a saved game.
    /// Contains all persistent data needed to restore a session.
    /// </summary>
    [Serializable]
    public class GameSave
    {
        /// <summary>
        /// Unique ID for the save file.
        /// </summary>
        public string SaveId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Version number of the save format for backward compatibility.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// Timestamp of when the file was last written.
        /// </summary>
        public DateTime LastSaved { get; set; } = DateTime.Now;

        // Time

        /// <summary>
        /// The current integer day number (1, 2, 3...).
        /// </summary>
        public int CurrentDayNumber { get; set; } = 1;

        /// <summary>
        /// The current day of the week (Monday, Tuesday...).
        /// </summary>
        public DayOfWeek CurrentDayName { get; set; } = DayOfWeek.Monday;

        /// <summary>
        /// The current week number.
        /// </summary>
        public int CurrentWeekNumber { get; set; } = 1;

        // Meta

        /// <summary>
        /// Player profile information.
        /// </summary>
        public PlayerProfile Profile { get; set; } = new PlayerProfile();

        /// <summary>
        /// User preferences/settings.
        /// </summary>
        public PlayerPreferences Preferences { get; set; } = new PlayerPreferences();

        /// <summary>
        /// Game configuration values (constants and balancing).
        /// </summary>
        public GameConfiguration Config { get; set; } = new GameConfiguration();

        /// <summary>
        /// Tracks narrative progression flags.
        /// </summary>
        public StoryState Story { get; set; } = new StoryState();

        // World

        /// <summary>
        /// State of the physical cafe (Inventory, Popularity).
        /// </summary>
        public CafeState Cafe { get; set; } = new CafeState();

        /// <summary>
        /// Financial state (Balance, Ledger).
        /// </summary>
        public EconomyState Economy { get; set; } = new EconomyState();

        /// <summary>
        /// List of all birds owned by the player.
        /// </summary>
        public List<Bird> Birds { get; set; } = new List<Bird>();

        // History

        /// <summary>
        /// Complete history of detailed simulation results for previous days.
        /// </summary>
        public List<DaySimulationResult> PastDayResults { get; set; } = new List<DaySimulationResult>();

        /// <summary>
        /// History of weekly summary reports.
        /// </summary>
        public List<WeeklySummary> PastWeeklySummaries { get; set; } = new List<WeeklySummary>();

        // Current State logic

        /// <summary>
        /// The active state for the current day, including the daily plan.
        /// </summary>
        public DayState CurrentDayState { get; set; } = new DayState();
    }
}