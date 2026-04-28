namespace ElBruno.Reranking.Tests.Mocks;

/// <summary>
/// Mock IReranker for testing. Returns deterministic results based on query and document text.
/// </summary>
public class MockReranker : IReranker
{
    public string Name => "MockReranker";

    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate I/O

        var docList = documents.ToList();
        if (!docList.Any())
            return new RerankResult { RankedDocuments = [] };

        // Simple mock: score based on query-document overlap
        var ranked = docList
            .Select((doc, idx) =>
            {
                var score = CalculateMockScore(query, doc);
                return new RankedDocument
                {
                    Text = doc,
                    Score = score,
                    Index = idx
                };
            })
            .OrderByDescending(x => x.Score)
            .Select((doc, rank) =>
            {
                doc.Rank = rank + 1;
                return doc;
            })
            .ToList();

        var topK = options?.TopK ?? ranked.Count;
        return new RerankResult
        {
            RankedDocuments = ranked.Take(topK).ToList(),
            TotalDocuments = docList.Count
        };
    }

    private static double CalculateMockScore(string query, string document)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(document))
            return 0.0;
            
        var queryWords = query.ToLower().Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var docWords = document.ToLower().Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        var matches = docWords.Count(w => queryWords.Contains(w));
        return Math.Min(1.0, matches / (double)Math.Max(queryWords.Count, 1));
    }
}
