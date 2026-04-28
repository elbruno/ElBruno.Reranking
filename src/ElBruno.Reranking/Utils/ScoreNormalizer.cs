namespace ElBruno.Reranking.Utils;

/// <summary>
/// Normalizes scores from different backends to [0.0, 1.0] range.
/// </summary>
public static class ScoreNormalizer
{
    /// <summary>
    /// Converts a logit to a probability using sigmoid function.
    /// Used for ONNX/BGE backend.
    /// </summary>
    /// <param name="logit">Raw logit value</param>
    /// <returns>Probability in [0.0, 1.0]</returns>
    public static float FromLogit(float logit)
    {
        // Sigmoid: 1 / (1 + e^-x)
        return 1f / (1f + MathF.Exp(-logit));
    }

    /// <summary>
    /// Passes through a probability value (already in [0.0, 1.0]).
    /// Used for Claude and other probability-based backends.
    /// </summary>
    /// <param name="probability">Probability value</param>
    /// <returns>Same value, clamped to [0.0, 1.0]</returns>
    public static float FromProbability(float probability)
    {
        return Math.Clamp(probability, 0f, 1f);
    }

    /// <summary>
    /// Converts an ordinal rank to a score based on total items.
    /// Used for LLM-based ranking (e.g., Ollama).
    /// </summary>
    /// <param name="rank">Rank position (1-based)</param>
    /// <param name="totalItems">Total number of items</param>
    /// <returns>Score in [0.0, 1.0] where higher rank = lower score</returns>
    public static float FromOrdinalRank(int rank, int totalItems)
    {
        if (totalItems <= 0)
            return 0f;
        
        return (totalItems - rank + 1f) / totalItems;
    }

    /// <summary>
    /// Clamps a score to [0.0, 1.0] range.
    /// </summary>
    /// <param name="score">Raw score value</param>
    /// <returns>Score clamped to [0.0, 1.0]</returns>
    public static float Clamp(float score)
    {
        return Math.Clamp(score, 0f, 1f);
    }
}
