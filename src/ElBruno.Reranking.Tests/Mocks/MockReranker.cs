namespace ElBruno.Reranking.Tests.Mocks;

using ElBruno.Reranking.Utils;

/// <summary>
/// Mock IReranker for testing. Returns deterministic results based on query and document text.
/// </summary>
public class MockReranker : IReranker
{
    public string Name => "mock-reranker";

    public RerankerBackendType BackendType => RerankerBackendType.Custom;

    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var timer = new TimingHelper();
        await Task.Delay(10, cancellationToken); // Simulate I/O

        if (query == null)
            throw new ArgumentNullException(nameof(query));
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var itemsList = items.ToList();

        if (!itemsList.Any())
        {
            return new RerankResult(
                new List<RerankScore>().AsReadOnly(),
                query,
                Name,
                0,
                timer.ElapsedMilliseconds);
        }

        // Simple mock: score based on query-document overlap
        var pairs = itemsList.Select(item =>
        {
            var score = CalculateMockScore(query, item.Text);
            return (item, score);
        });

        return ResultFormatter.Format(pairs, query, Name, options, timer.ElapsedMilliseconds);
    }

    private static float CalculateMockScore(string query, string document)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(document))
            return 0.0f;

        var queryWords = query.ToLower().Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var docWords = document.ToLower().Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var matches = docWords.Count(w => queryWords.Contains(w));
        return Math.Min(1.0f, matches / (float)Math.Max(queryWords.Count, 1));
    }
}

