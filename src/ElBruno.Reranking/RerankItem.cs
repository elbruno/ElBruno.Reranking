namespace ElBruno.Reranking;

/// <summary>
/// Represents a single item (document, passage, search result) to be reranked.
/// </summary>
public class RerankItem
{
    /// <summary>
    /// Unique identifier for the item (optional but recommended).
    /// Can be document ID, URL, index, or any caller-defined identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The text content to rerank (document snippet, full passage, etc.).
    /// This is what the reranker scores against the query.
    /// Typically 100–500 chars for optimal performance.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Optional metadata (caller's original rank, source, tags, etc.).
    /// Preserved in RerankScore output for reference.
    /// Serialized as JSON in output if populated.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Creates a new RerankItem with the specified text.
    /// </summary>
    /// <param name="text">The text content to rerank (required)</param>
    /// <param name="id">Optional identifier for the item</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <exception cref="ArgumentException">If text is null or whitespace</exception>
    public RerankItem(string text, string? id = null, Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or whitespace", nameof(text));

        Text = text;
        Id = id;
        Metadata = metadata;
    }
}
