namespace ElBruno.Reranking.Utils;

/// <summary>
/// Formats reranking results by sorting, filtering, and ranking.
/// </summary>
public static class ResultFormatter
{
    /// <summary>
    /// Formats raw item-score pairs into a sorted RerankResult.
    /// </summary>
    /// <param name="pairs">Item-score pairs to format</param>
    /// <param name="query">The original query</param>
    /// <param name="backendName">Name of the backend</param>
    /// <param name="options">Optional filtering options</param>
    /// <param name="elapsedMilliseconds">Time elapsed during operation</param>
    /// <param name="diagnostics">Optional diagnostic info</param>
    /// <returns>Sorted and filtered RerankResult</returns>
    public static RerankResult Format(
        IEnumerable<(RerankItem item, float score)> pairs,
        string query,
        string backendName,
        RerankOptions? options = null,
        long elapsedMilliseconds = 0,
        Dictionary<string, string>? diagnostics = null)
    {
        var totalItems = 0;
        var scores = new List<RerankScore>();

        // Convert pairs to scores and sort by score descending
        foreach (var (item, score) in pairs.OrderByDescending(p => p.score))
        {
            totalItems++;
            scores.Add(new RerankScore(item, ScoreNormalizer.Clamp(score), scores.Count + 1));
        }

        // Apply filters
        var filtered = scores.AsEnumerable();

        if (options?.MinScore.HasValue ?? false)
        {
            filtered = filtered.Where(s => s.Score >= options.MinScore.Value);
        }

        if (options?.TopK.HasValue ?? false)
        {
            filtered = filtered.Take(options.TopK.Value);
        }

        var filteredList = filtered.ToList();

        // Re-rank after filtering
        for (int i = 0; i < filteredList.Count; i++)
        {
            filteredList[i] = new RerankScore(
                filteredList[i].Item,
                filteredList[i].Score,
                i + 1,
                filteredList[i].Explanation);
        }

        return new RerankResult(
            filteredList.AsReadOnly(),
            query,
            backendName,
            totalItems,
            elapsedMilliseconds,
            diagnostics);
    }
}
