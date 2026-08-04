namespace ElBruno.Reranking.BlazorComponents.Tests;

public static class TestData
{
    public static RerankResult CreateResult(
        string query = "What is semantic search?",
        string backend = "BGE-ONNX",
        params (string Text, float Score, int NewRank, int? OriginalRank)[] scores)
    {
        var scoreItems = scores
            .Select(item =>
            {
                var metadata = item.OriginalRank is null
                    ? null
                    : new Dictionary<string, object> { ["OriginalRank"] = item.OriginalRank.Value };

                return new RerankScore(new RerankItem(item.Text, metadata: metadata), item.Score, item.NewRank);
            })
            .ToList();

        return new RerankResult(scoreItems, query, backend, scoreItems.Count, 12);
    }

    public static IReadOnlyList<RerankItem> CreateCandidates(params string[] values)
        => values.Select((value, index) => new RerankItem(value, $"candidate-{index}")).ToList();
}
