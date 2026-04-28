namespace ElBruno.Reranking;

/// <summary>
/// Enumerates supported backend types for reranking.
/// </summary>
public enum RerankerBackendType
{
    /// <summary>ONNX Runtime (local, CPU/GPU inference)</summary>
    ONNX,

    /// <summary>Claude API (cloud-based, high precision)</summary>
    Claude,

    /// <summary>Ollama (local LLM, customizable)</summary>
    Ollama,

    /// <summary>Custom user-provided backend</summary>
    Custom
}

/// <summary>
/// Core abstraction for semantic reranking backends.
/// Implementations may use ONNX, API-based (Claude), or local LLMs (Ollama).
/// </summary>
public interface IReranker
{
    /// <summary>
    /// Asynchronously reranks a collection of items based on a query.
    /// </summary>
    /// <param name="query">The query/search context (e.g., user question)</param>
    /// <param name="items">Items to rerank (documents, search results, candidates)</param>
    /// <param name="options">Optional configuration (model, top-k, batch size, etc.)</param>
    /// <param name="cancellationToken">Cancellation support for long-running operations</param>
    /// <returns>Reranked items with relevance scores, sorted by score descending</returns>
    /// <exception cref="ArgumentNullException">If query or items is null</exception>
    /// <exception cref="ArgumentException">If items is empty or exceeds backend limits</exception>
    /// <exception cref="RerankerException">Backend-specific errors (model load, API, etc.)</exception>
    Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name/identifier of this reranker (e.g., "bge-reranker-base", "claude-3-opus")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the backend type (ONNX, API, LocalLLM, etc.)
    /// </summary>
    RerankerBackendType BackendType { get; }
}
