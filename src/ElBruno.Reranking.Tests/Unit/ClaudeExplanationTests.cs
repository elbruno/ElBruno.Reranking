namespace ElBruno.Reranking.Tests.Unit;

using ElBruno.Reranking;
using ElBruno.Reranking.Backends.Claude;

public class ClaudeExplanationTests
{
    [Fact]
    public void ClaudePromptBuilder_RespectsIncludeExplanationFlag()
    {
        var builder = new ClaudePromptBuilder();
        var items = new[]
        {
            new RerankItem("First document", "doc-a"),
            new RerankItem("Second document")
        };

        var promptWithoutExplanation = builder.BuildPrompt("query", items, false);
        Assert.Contains("zero-based original index", promptWithoutExplanation);
        Assert.DoesNotContain("\"explanation\"", promptWithoutExplanation);

        var promptWithExplanation = builder.BuildPrompt("query", items, true);
        Assert.Contains("brief explanation", promptWithExplanation, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"explanation\"", promptWithExplanation);
    }

    [Fact]
    public void ClaudePromptBuilder_ParsesResponseAndPopulatesExplanations_WhenRequested()
    {
        var builder = new ClaudePromptBuilder();
        var response = """
Intro text that should be ignored.
```json
[
  {"index": 2, "score": "0.10", "explanation": "third"},
  {"index": 0, "score": 0.90, "explanation": "first"},
  {"index": 1, "score": "0.50", "explanation": "second"}
]
```
Trailing text that should be ignored.
""";

        var results = builder.ParseResponse(response, 3, includeExplanation: true);

        Assert.Equal(0.90f, results[0].Score, 3);
        Assert.Equal("first", results[0].Explanation);
        Assert.Equal(0.50f, results[1].Score, 3);
        Assert.Equal("second", results[1].Explanation);
        Assert.Equal(0.10f, results[2].Score, 3);
        Assert.Equal("third", results[2].Explanation);
    }

    [Fact]
    public void ClaudePromptBuilder_IgnoresExplanations_WhenNotRequested()
    {
        var builder = new ClaudePromptBuilder();
        var response = """
Intro text that should be ignored.
```json
[
  {"index": 2, "score": "0.10", "explanation": "third"},
  {"index": 0, "score": 0.90, "explanation": "first"},
  {"index": 1, "score": "0.50", "explanation": "second"}
]
```
Trailing text that should be ignored.
""";

        var results = builder.ParseResponse(response, 3, includeExplanation: false);

        Assert.Equal(0.90f, results[0].Score, 3);
        Assert.Null(results[0].Explanation);
        Assert.Equal(0.50f, results[1].Score, 3);
        Assert.Null(results[1].Explanation);
        Assert.Equal(0.10f, results[2].Score, 3);
        Assert.Null(results[2].Explanation);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClaudeReranker_PreservesRequestedExplanations(bool includeExplanation)
    {
        string? expectedExplanation = includeExplanation ? "high relevance" : null;

        var apiClient = new FakeClaudeApiClient(
            includeExplanationResults: new[]
            {
                new ClaudeScoreResult(0.20f, "low relevance"),
                new ClaudeScoreResult(0.90f, "high relevance")
            },
            plainResults: new[]
            {
                new ClaudeScoreResult(0.20f),
                new ClaudeScoreResult(0.90f)
            });

        var reranker = new ClaudeReranker(
            new ClaudeOptions { ApiKey = "test-api-key" },
            apiClient);

        var result = await reranker.RerankAsync(
            "query",
            new[]
            {
                new RerankItem("Low relevance"),
                new RerankItem("High relevance")
            },
            new RerankOptions
            {
                IncludeExplanation = includeExplanation,
                TopK = 1
            });

        Assert.Equal(includeExplanation, apiClient.LastIncludeExplanation);
        Assert.Single(result.Scores);
        Assert.Equal("High relevance", result.Scores[0].Item.Text);
        Assert.Equal(expectedExplanation, result.Scores[0].Explanation);
    }

    private sealed class FakeClaudeApiClient : IClaudeApiClient
    {
        private readonly IReadOnlyList<ClaudeScoreResult> _includeExplanationResults;
        private readonly IReadOnlyList<ClaudeScoreResult> _plainResults;

        public FakeClaudeApiClient(
            IReadOnlyList<ClaudeScoreResult> includeExplanationResults,
            IReadOnlyList<ClaudeScoreResult> plainResults)
        {
            _includeExplanationResults = includeExplanationResults;
            _plainResults = plainResults;
        }

        public bool? LastIncludeExplanation { get; private set; }

        public Task<IReadOnlyList<ClaudeScoreResult>> RankAsync(
            string query,
            IEnumerable<RerankItem> items,
            bool includeExplanation,
            CancellationToken cancellationToken)
        {
            LastIncludeExplanation = includeExplanation;
            return Task.FromResult(includeExplanation ? _includeExplanationResults : _plainResults);
        }

        public void Dispose()
        {
        }
    }
}
