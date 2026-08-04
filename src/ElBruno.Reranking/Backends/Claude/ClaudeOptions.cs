namespace ElBruno.Reranking.Backends.Claude;

/// <summary>
/// Configuration options specific to Claude reranker.
/// </summary>
public class ClaudeOptions
{
    /// <summary>
    /// Claude API key (required).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Claude model ID to use (default: "claude-3-opus").
    /// </summary>
    public string Model { get; set; } = ClaudeModelNames.Default;

    /// <summary>
    /// Maximum tokens in response (default: 4096).
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Request timeout in milliseconds (default: 60000).
    /// </summary>
    public int TimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Maximum number of retry attempts (default: 3).
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial backoff delay in milliseconds (default: 1000).
    /// </summary>
    public int InitialBackoffMs { get; set; } = 1000;
}
