namespace ElBruno.Reranking;

/// <summary>
/// Represents a single reranked item with relevance score.
/// Returned as part of RerankResult.
/// </summary>
public class RerankScore
{
    /// <summary>
    /// The original item that was reranked.
    /// </summary>
    public RerankItem Item { get; }

    /// <summary>
    /// Relevance score [0.0, 1.0].
    /// 1.0 = perfect relevance (query)
    /// 0.0 = no relevance
    /// Exact semantics depend on backend (logits, probabilities, etc.).
    /// </summary>
    public float Score { get; }

    /// <summary>
    /// New rank position (1-based, calculated from score).
    /// Rank 1 = highest score.
    /// </summary>
    public int Rank { get; }

    /// <summary>
    /// Optional reason/explanation for score (v1.0+ backends).
    /// Claude may populate this; ONNX may leave null.
    /// </summary>
    public string? Explanation { get; }

    /// <summary>
    /// Creates a new RerankScore with the specified item, score, and rank.
    /// </summary>
    /// <param name="item">The reranked item</param>
    /// <param name="score">Relevance score [0.0, 1.0]</param>
    /// <param name="rank">1-based rank position</param>
    /// <param name="explanation">Optional explanation for the score</param>
    /// <exception cref="ArgumentNullException">If item is null</exception>
    /// <exception cref="ArgumentException">If score is outside [0, 1] or rank is less than 1</exception>
    public RerankScore(RerankItem item, float score, int rank, string? explanation = null)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (score < 0f || score > 1f)
            throw new ArgumentException("Score must be in [0.0, 1.0]", nameof(score));
        if (rank < 1)
            throw new ArgumentException("Rank must be >= 1", nameof(rank));

        Item = item;
        Score = score;
        Rank = rank;
        Explanation = explanation;
    }
}
