
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Represents a single state in the Oracle dialogue tree.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// The unique ID for this conversation state.
        /// </summary>
        public string StateId { get; set; }

        /// <summary>
        /// The text the Oracle speaks. Supports TextMeshPro formatting.
        /// </summary>
        public string OracleText { get; set; }

        /// <summary>
        /// The available responses the player can choose from.
        /// </summary>
        public List<ChatResponseOption> Options { get; set; } = new List<ChatResponseOption>();
    }

    /// <summary>
    /// Represents a choice the player can make in the chat.
    /// </summary>
    [Serializable]
    public class ChatResponseOption
    {
        /// <summary>
        /// The text displayed to the player for this option.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// The ID of the state to transition to if this option is selected.
        /// </summary>
        public string NextStateId { get; set; }

        /// <summary>
        /// If true, choosing this option ends the chat session.
        /// </summary>
        public bool IsExit { get; set; }
    }
}