namespace ElBruno.Reranking;

/// <summary>
/// Represents a ranked document in reranking results.
/// </summary>
public class RankedDocument
{
    /// <summary>
    /// The original document text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score between 0 and 1.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// 1-based rank position.
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Original index in the input document list.
    /// </summary>
    public int Index { get; set; }
}
