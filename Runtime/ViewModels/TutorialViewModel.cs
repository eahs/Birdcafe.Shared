
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Data for the tutorial screen.
    /// </summary>
    [Serializable]
    public class TutorialViewModel
    {
        /// <summary>
        /// Title of the tutorial.
        /// </summary>
        public string Title { get; set; } = "Your First Day at the Bird Cafe";

        /// <summary>
        /// List of steps to show.
        /// </summary>
        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();
    }

    /// <summary>
    /// A single step in the tutorial.
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        /// <summary>
        /// Step title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Step description.
        /// </summary>
        public string Description { get; set; }
    }
}