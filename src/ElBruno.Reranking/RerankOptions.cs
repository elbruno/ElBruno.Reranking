namespace ElBruno.Reranking;

/// <summary>
/// Options for reranking operation.
/// </summary>
public class RerankOptions
{
    /// <summary>
    /// Maximum number of top results to return.
    /// </summary>
    public int TopK { get; set; } = int.MaxValue;

    /// <summary>
    /// Minimum relevance score threshold (0-1).
    /// </summary>
    public double MinScore { get; set; } = 0.0;

    /// <summary>
    /// Request timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Enable retries for transient failures.
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
