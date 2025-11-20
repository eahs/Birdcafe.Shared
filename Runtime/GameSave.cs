
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Cafe;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Meta;
using BirdCafe.Shared.Models.Simulation;

namespace BirdCafe.Shared
{
    [Serializable]
    public class GameSave
    {
        public string SaveId { get; set; } = Guid.NewGuid().ToString();
        public int SchemaVersion { get; set; } = 1;
        public DateTime LastSaved { get; set; } = DateTime.Now;

        // Time
        public int CurrentDayNumber { get; set; } = 1;
        public DayOfWeek CurrentDayName { get; set; } = DayOfWeek.Monday;
        public int CurrentWeekNumber { get; set; } = 1;
        
        // Meta
        public PlayerProfile Profile { get; set; } = new PlayerProfile();
        public PlayerPreferences Preferences { get; set; } = new PlayerPreferences();
        public GameConfiguration Config { get; set; } = new GameConfiguration();
        public StoryState Story { get; set; } = new StoryState();
        
        // World
        public CafeState Cafe { get; set; } = new CafeState();
        public EconomyState Economy { get; set; } = new EconomyState();
        public List<Bird> Birds { get; set; } = new List<Bird>();
        
        // History
        /// <summary>
        /// Complete history of detailed simulation results for previous days.
        /// </summary>
        public List<DaySimulationResult> PastDayResults { get; set; } = new List<DaySimulationResult>();
        
        public List<WeeklySummary> PastWeeklySummaries { get; set; } = new List<WeeklySummary>();

        // Current State logic
        public DayState CurrentDayState { get; set; } = new DayState();
    }
}