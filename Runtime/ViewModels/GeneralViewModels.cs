
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    public enum GameScreen
    {
        MainMenu,
        LoadGame,
        DayIntro,          // "Day 1 - Monday" Banner
        DaySimulation,     // The active workday view
        EveningSummary,    // "You made $50 today"
        EveningCare,       // Feed/Pet birds
        EveningPlanning,   // Shop & Staffing
        WeeklySummary,     // End of week report
        GameOver
    }

    [Serializable]
    public class SaveSlotViewModel
    {
        public string Id { get; set; }
        public string PlayerName { get; set; }
        public int DayNumber { get; set; }
        public int WeekNumber { get; set; }
    }
}