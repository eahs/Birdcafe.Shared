
using System;

namespace BirdCafe.Shared.ViewModels
{
    [Serializable]
    public class WeeklyReportViewModel
    {
        public int WeekNumber { get; set; }
        public decimal TotalProfit { get; set; }
        public int AvgBirdHealth { get; set; }
        public string Narrative { get; set; }
    }

    [Serializable]
    public class GameOverViewModel
    {
        public string Reason { get; set; }
        public int DaysSurvived { get; set; }
        public decimal FinalScore { get; set; }
    }
}