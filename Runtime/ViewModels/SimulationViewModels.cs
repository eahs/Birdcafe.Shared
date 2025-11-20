
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    [Serializable]
    public class DayIntroViewModel
    {
        public int DayNumber { get; set; }
        public string DayName { get; set; }
        public int Popularity { get; set; }
        public string Message { get; set; }
    }

    [Serializable]
    public class UiTimelineEvent
    {
        public float TimeSeconds { get; set; }
        public string EventType { get; set; } // "Arrived", "Served", "LeftAngry"
        public string Description { get; set; }
        public string BirdName { get; set; }
        public string IconId { get; set; } // e.g. "Coffee"
    }
}