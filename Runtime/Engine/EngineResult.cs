
using System;

namespace BirdCafe.Shared.Engine
{
    /// <summary>
    /// Standardized return type for all Engine Commands.
    /// </summary>
    public class EngineResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorCode { get; set; }
        public string UserMessage { get; set; }
        public object Payload { get; set; }

        public static EngineResult Success(object payload = null)
        {
            return new EngineResult { IsSuccess = true, Payload = payload };
        }

        public static EngineResult Failure(string code, string message)
        {
            return new EngineResult { IsSuccess = false, ErrorCode = code, UserMessage = message };
        }
    }
}