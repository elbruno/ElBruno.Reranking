namespace ElBruno.Reranking;

/// <summary>
/// Per-call configuration for reranking operations.
/// All properties are optional; backends use sensible defaults if null.
/// </summary>
public class RerankOptions
{
    /// <summary>
    /// Top-k items to return (default: all items returned, sorted).
    /// If set, only top-k highest-scoring items are included in result.
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// Minimum score threshold [0.0, 1.0].
    /// Items below this score are excluded from results (but counted in TotalItems).
    /// Useful for filtering low-confidence results.
    /// </summary>
    public float? MinScore { get; set; }

    /// <summary>
    /// Maximum number of items to process (default: backend-specific).
    /// For ONNX: ~10,000 (memory limit).
    /// For Claude: ~500 (token limit).
    /// If exceeded, throws ArgumentException.
    /// </summary>
    public int? MaxItems { get; set; }

    /// <summary>
    /// Timeout in milliseconds for backend operation.
    /// Applies to model inference or API calls.
    /// Default: backend-specific (ONNX: 30s, Claude: 60s).
    /// </summary>
    public int? TimeoutMs { get; set; }

    /// <summary>
    /// Include explanation for each score (if backend supports it).
    /// Only Claude and some LLM backends populate explanations.
    /// Default: false (faster, no explanation).
    /// </summary>
    public bool IncludeExplanation { get; set; } = false;

    /// <summary>
    /// Backend-specific options as key-value pairs.
    /// E.g., { "batch_size": "32", "use_gpu": "true" }.
    /// Ignored by backends that don't recognize the key.
    /// </summary>
    public Dictionary<string, string>? CustomOptions { get; set; }

    /// <summary>
    /// Validate options against constraints.
    /// Called by backends before processing.
    /// Throws ArgumentException if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">If options are invalid</exception>
    public void Validate()
    {
        if (TopK.HasValue && TopK.Value < 1)
            throw new ArgumentException("TopK must be >= 1", nameof(TopK));
        if (MinScore.HasValue && (MinScore.Value < 0f || MinScore.Value > 1f))
            throw new ArgumentException("MinScore must be in [0.0, 1.0]", nameof(MinScore));
        if (MaxItems.HasValue && MaxItems.Value < 1)
            throw new ArgumentException("MaxItems must be >= 1", nameof(MaxItems));
        if (TimeoutMs.HasValue && TimeoutMs.Value < 100)
            throw new ArgumentException("TimeoutMs must be >= 100", nameof(TimeoutMs));
    }
}
