namespace BirdCafe.Shared.Engine
{
    /// <summary>
    /// Standardized return type for all Engine Commands.
    /// Acts as a wrapper to indicate success or failure and carry data.
    /// </summary>
    public class EngineResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets a computer-readable error code if the operation failed.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets a human-readable message intended for the user interface.
        /// </summary>
        public string UserMessage { get; set; }

        /// <summary>
        /// Gets or sets any data object returned by the operation (optional).
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// Creates a successful result object.
        /// </summary>
        /// <param name="payload">Optional data to return.</param>
        /// <returns>A successful EngineResult.</returns>
        public static EngineResult Success(object payload = null)
        {
            return new EngineResult { IsSuccess = true, Payload = payload };
        }

        /// <summary>
        /// Creates a failure result object.
        /// </summary>
        /// <param name="code">The technical error code.</param>
        /// <param name="message">The message to display to the user.</param>
        /// <returns>A failed EngineResult.</returns>
        public static EngineResult Failure(string code, string message)
        {
            return new EngineResult { IsSuccess = false, ErrorCode = code, UserMessage = message };
        }
    }
}