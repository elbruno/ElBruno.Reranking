namespace ElBruno.Reranking;

/// <summary>
/// Complete result of a reranking operation.
/// Contains all scored items, metadata, and diagnostics.
/// </summary>
public class RerankResult
{
    /// <summary>
    /// All reranked items, sorted by score descending (highest first).
    /// </summary>
    public IReadOnlyList<RerankScore> Scores { get; }

    /// <summary>
    /// The query that was used for reranking.
    /// Useful for logging/diagnostics.
    /// </summary>
    public string Query { get; }

    /// <summary>
    /// Which backend produced this result (e.g., "bge-reranker-base").
    /// </summary>
    public string BackendName { get; }

    /// <summary>
    /// Total input items that were reranked.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Time taken for reranking (milliseconds).
    /// Includes inference, but not network latency for async operations.
    /// </summary>
    public long ElapsedMilliseconds { get; }

    /// <summary>
    /// Any diagnostic information or warnings from the backend.
    /// E.g., "Model loaded from cache", "Batch timeout warning", etc.
    /// </summary>
    public Dictionary<string, string>? Diagnostics { get; }

    /// <summary>
    /// Creates a new RerankResult with the specified scores and metadata.
    /// </summary>
    /// <param name="scores">Sorted list of reranked items with scores</param>
    /// <param name="query">The query used for reranking</param>
    /// <param name="backendName">Name of the backend that produced this result</param>
    /// <param name="totalItems">Total number of items that were reranked</param>
    /// <param name="elapsedMilliseconds">Time taken for the reranking operation</param>
    /// <param name="diagnostics">Optional diagnostic information</param>
    /// <exception cref="ArgumentNullException">If scores, query, or backendName is null</exception>
    public RerankResult(
        IReadOnlyList<RerankScore> scores,
        string query,
        string backendName,
        int totalItems,
        long elapsedMilliseconds,
        Dictionary<string, string>? diagnostics = null)
    {
        if (scores == null)
            throw new ArgumentNullException(nameof(scores));
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        if (backendName == null)
            throw new ArgumentNullException(nameof(backendName));

        Scores = scores;
        Query = query;
        BackendName = backendName;
        TotalItems = totalItems;
        ElapsedMilliseconds = elapsedMilliseconds;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Convenience method: Get top-k scores.
    /// </summary>
    /// <param name="k">Number of top items to return</param>
    /// <returns>Top k ranked items</returns>
    public IEnumerable<RerankScore> GetTopK(int k) => Scores.Take(k);

    /// <summary>
    /// Convenience method: Filter by minimum score threshold.
    /// </summary>
    /// <param name="minScore">Minimum score threshold [0.0, 1.0]</param>
    /// <returns>Items with score >= minScore</returns>
    public IEnumerable<RerankScore> FilterByScore(float minScore)
        => Scores.Where(s => s.Score >= minScore);
}
