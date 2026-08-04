namespace ElBruno.Reranking.Backends.Claude;

internal interface IClaudeApiClient : IDisposable
{
    Task<IReadOnlyList<ClaudeScoreResult>> RankAsync(
        string query,
        IEnumerable<RerankItem> items,
        bool includeExplanation,
        CancellationToken cancellationToken);
}
