namespace ElBruno.Reranking.BlazorComponents.Tests.Support;

internal sealed class FakeRerankService : IReranker
{
    private readonly Func<string, IReadOnlyList<RerankItem>, RerankResult> _handler;

    public int CallCount { get; private set; }
    public string? LastQuery { get; private set; }
    public IReadOnlyList<RerankItem>? LastCandidates { get; private set; }

    public string Name => "fake-rerank-service";

    public RerankerBackendType BackendType => RerankerBackendType.Custom;

    public FakeRerankService(Func<string, IReadOnlyList<RerankItem>, RerankResult> handler)
    {
        _handler = handler;
    }

    public Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> candidates,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastQuery = query;
        LastCandidates = candidates.ToList();
        return Task.FromResult(_handler(query, LastCandidates));
    }
}
