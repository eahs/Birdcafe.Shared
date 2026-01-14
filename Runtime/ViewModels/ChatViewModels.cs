
using System;

namespace BirdCafe.Shared.ViewModels
{
    /// <summary>
    /// Represents a single message in the chat interface.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// The name of the person sending the message.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// The message text.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// When it was sent.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// True if sent by the human player.
        /// </summary>
        public bool IsUser { get; set; }
    }
}