namespace ElBruno.Reranking;

/// <summary>
/// Result of a reranking operation.
/// </summary>
public class RerankResult
{
    /// <summary>
    /// Ranked documents ordered by relevance (descending).
    /// </summary>
    public List<RankedDocument> RankedDocuments { get; set; } = new();

    /// <summary>
    /// Total number of documents that were reranked.
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Optional metadata about the reranking operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
