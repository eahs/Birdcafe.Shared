
using System;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Data for the Weekly Report screen.
    /// </summary>
    [Serializable]
    public class WeeklyReportViewModel
    {
        /// <summary>
        /// The week number being reported.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Net profit for the week.
        /// </summary>
        public decimal TotalProfit { get; set; }

        /// <summary>
        /// Average health of the flock.
        /// </summary>
        public int AvgBirdHealth { get; set; }

        /// <summary>
        /// Text summary of performance.
        /// </summary>
        public string Narrative { get; set; }
    }

    /// <summary>
    /// Data for the Game Over screen.
    /// </summary>
    [Serializable]
    public class GameOverViewModel
    {
        /// <summary>
        /// Why the game ended (Bankruptcy/Popularity).
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// How many days the player lasted.
        /// </summary>
        public int DaysSurvived { get; set; }

        /// <summary>
        /// Final money score.
        /// </summary>
        public decimal FinalScore { get; set; }
    }
}