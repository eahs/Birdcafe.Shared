using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.ViewModels;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains Oracle chat state and conversation-navigation operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Gets the identifier of the currently selected Oracle conversation node.
        /// </summary>
        public string CurrentChatStateKey { get; private set; } = ChatData.ROOT_ID;

        /// <summary>
        /// Opens the Oracle chat interface and resets the conversation to its root node.
        /// </summary>
        public void FireChatPopup()
        {
            // Every explicit chat opening begins from the known root topic. This keeps separate
            // popup sessions predictable and prevents a stale branch from reappearing.
            CurrentChatStateKey = ChatData.ROOT_ID;

            // The UI receives a lightweight signal, then asks the facade for the current node.
            OnChatPopup?.Invoke();
        }

        /// <summary>
        /// Retrieves the complete chat node represented by <see cref="CurrentChatStateKey"/>.
        /// </summary>
        /// <returns>The current Oracle chat message and its available options.</returns>
        public ChatMessage GetCurrentChatNode()
        {
            // ChatData is the canonical conversation graph. The facade stores only the current key.
            return ChatData.GetNode(CurrentChatStateKey);
        }

        /// <summary>
        /// Applies a player's selected option to advance the Oracle conversation.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the selected option.</param>
        public void SelectChatOption(int optionIndex)
        {
            var node = GetCurrentChatNode();

            // Ignore out-of-range UI input rather than allowing a collection exception to escape
            // from a normal interaction path.
            if (optionIndex < 0 || optionIndex >= node.Options.Count)
            {
                return;
            }

            var selection = node.Options[optionIndex];

            // Only the state key changes here. The UI can immediately call GetCurrentChatNode()
            // again to render the newly selected branch.
            CurrentChatStateKey = selection.NextStateId;
        }
    }
}
