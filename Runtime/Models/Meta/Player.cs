
using System;

namespace BirdCafe.Shared.Models.Meta
{
    /// <summary>
    /// Stores identity and meta-data about the human player.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        /// <summary>
        /// Unique ID for the profile.
        /// </summary>
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Player's chosen display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// When the profile was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The initial funds provided when starting a new game.
        /// </summary>
        public decimal StartingFunds { get; set; } = 100.00m;
    }

    /// <summary>
    /// User settings and preferences.
    /// </summary>
    [Serializable]
    public class PlayerPreferences
    {
        /// <summary>
        /// Whether tutorials should be shown.
        /// </summary>
        public bool TutorialsEnabled { get; set; } = true;

        /// <summary>
        /// Whether tooltip hints should be enabled.
        /// </summary>
        public bool TooltipsEnabled { get; set; } = true;

        /// <summary>
        /// Scale factor for UI text.
        /// </summary>
        public float TextScale { get; set; } = 1.0f;

        /// <summary>
        /// Whether animations should play.
        /// </summary>
        public bool AnimationsEnabled { get; set; } = true;

        /// <summary>
        /// Locale code for language (e.g. "en-US").
        /// </summary>
        public string LocaleCode { get; set; } = "en-US";
    }

    /// <summary>
    /// Tracks narrative progression and tutorial flags.
    /// </summary>
    [Serializable]
    public class StoryState
    {
        /// <summary>
        /// True if the intro egg cutscene has been shown.
        /// </summary>
        public bool IntroEggShown { get; set; }

        /// <summary>
        /// True if Day 1 tutorial is done.
        /// </summary>
        public bool Day1TutorialCompleted { get; set; }

        /// <summary>
        /// List of story event IDs that have already occurred.
        /// </summary>
        public System.Collections.Generic.List<string> TriggeredEventIds { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>
        /// List of achievements or narrative milestones reached.
        /// </summary>
        public System.Collections.Generic.List<string> NarrativeMilestones { get; set; } = new System.Collections.Generic.List<string>();
    }
}