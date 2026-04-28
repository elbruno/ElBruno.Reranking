namespace ElBruno.Reranking;

/// <summary>
/// Base exception for all reranker errors.
/// Backends throw this (or subclasses) on failure.
/// </summary>
public class RerankerException : Exception
{
    /// <summary>
    /// Name of the backend that failed (e.g., "bge-reranker-base")
    /// </summary>
    public string BackendName { get; }

    /// <summary>
    /// Machine-readable error code (e.g., "MODEL_LOAD_FAILED")
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Creates a new RerankerException.
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="backendName">Name of the backend that failed</param>
    /// <param name="errorCode">Machine-readable error code</param>
    /// <param name="innerException">Inner exception, if any</param>
    public RerankerException(
        string message,
        string backendName,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        BackendName = backendName;
        ErrorCode = errorCode;
    }
}
