
using System;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Data for the Day Intro banner.
    /// </summary>
    [Serializable]
    public class DayIntroViewModel
    {
        /// <summary>
        /// The day number.
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// The name of the day.
        /// </summary>
        public string DayName { get; set; }

        /// <summary>
        /// The name of the cafe.
        /// </summary>
        public string CafeName { get; set; }

        /// <summary>
        /// Current popularity.
        /// </summary>
        public int Popularity { get; set; }

        /// <summary>
        /// Welcome message text.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// A UI-friendly version of a simulation event.
    /// </summary>
    [Serializable]
    public class UiTimelineEvent
    {
        /// <summary>
        /// Raw time in seconds.
        /// </summary>
        public float TimeSeconds { get; set; }

        /// <summary>
        /// Formatted string (e.g. "08:30 AM").
        /// </summary>
        public string FormattedTime { get; set; }

        /// <summary>
        /// Type of event string.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Description of what happened.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of bird involved.
        /// </summary>
        public string BirdName { get; set; }

        /// <summary>
        /// Icon ID for product.
        /// </summary>
        public string IconId { get; set; }

        /// <summary>
        /// Money change.
        /// </summary>
        public decimal MoneyDelta { get; set; }

        /// <summary>
        /// Pop change.
        /// </summary>
        public float PopularityDelta { get; set; }
    }
}