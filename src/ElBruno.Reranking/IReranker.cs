namespace ElBruno.Reranking;

/// <summary>
/// Interface for reranking backends.
/// Implementations should provide semantic reranking of documents.
/// </summary>
public interface IReranker
{
    /// <summary>
    /// Backend name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Reranks documents by relevance to the query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="documents">Documents to rerank.</param>
    /// <param name="options">Reranking options (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ranked documents ordered by relevance (descending).</returns>
    Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default);
}
