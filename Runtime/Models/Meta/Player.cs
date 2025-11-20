
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
        public bool TutorialsEnabled { get; set; } = true;
        public bool TooltipsEnabled { get; set; } = true;
        public float TextScale { get; set; } = 1.0f;
        public bool AnimationsEnabled { get; set; } = true;
        public string LocaleCode { get; set; } = "en-US";
    }

    /// <summary>
    /// Tracks narrative progression and tutorial flags.
    /// </summary>
    [Serializable]
    public class StoryState
    {
        public bool IntroEggShown { get; set; }
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