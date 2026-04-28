namespace ElBruno.Reranking.Tests.Integration;

/// <summary>
/// Integration tests for end-to-end reranking workflows.
/// Tests multiple backends in sequence and realistic scenarios.
/// </summary>
public class RerankerIntegrationTests
{
    [Fact]
    public async Task EndToEnd_SingleBackend_Workflow()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.StandardSet.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Scores);
        Assert.Equal(TestData.Documents.StandardSet.Length, result.TotalItems);
    }

    [Fact]
    public async Task EndToEnd_MultipleBackends_Sequential()
    {
        // Arrange
        var reranker1 = new MockReranker();
        var reranker2 = new MockReranker();
        var query = "neural networks";
        var documents = TestData.Documents.StandardSet.ToRerankItems();

        // Act - First reranker
        var result1 = await reranker1.RerankAsync(query, documents);

        // Use top results from first reranker as input to second
        var topDocs = result1.Scores.Take(5).Select(d => d.Item.Text).ToArray().ToRerankItems();
        var result2 = await reranker2.RerankAsync(query, topDocs);

        // Assert
        Assert.NotEmpty(result1.Scores);
        Assert.NotEmpty(result2.Scores);
        Assert.True(result2.Scores.Count <= 5);
    }

    [Fact]
    public async Task EndToEnd_LargeDocumentSet()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.ComplexQuery;
        var documents = TestData.Documents.LargeSet.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.Equal(TestData.Documents.LargeSet.Length, result.TotalItems);
        Assert.NotEmpty(result.Scores);
        foreach (var doc in result.Scores)
        {
            Assert.InRange(doc.Score, 0.0f, 1.0f);
            Assert.True(doc.Rank > 0);
        }
    }

    [Fact]
    public async Task EndToEnd_RealWorldScenario()
    {
        // Arrange - simulate search + rerank workflow
        var query = "machine learning";
        var allDocs = TestData.Documents.StandardSet;
        
        // First pass: basic filtering
        var bgeReranker = new MockReranker();
        
        // Second pass: precision reranking
        var claudeReranker = new MockReranker();

        // Act - Stage 1: BGE fast reranking
        var stage1Result = await bgeReranker.RerankAsync(
            query, 
            allDocs.ToRerankItems(),
            new RerankOptions { TopK = 5 });

        // Stage 2: Claude high-precision reranking
        var topDocs = stage1Result.Scores.Select(d => d.Item.Text).ToArray().ToRerankItems();
        var stage2Result = await claudeReranker.RerankAsync(query, topDocs);

        // Assert
        Assert.NotEmpty(stage1Result.Scores);
        Assert.NotEmpty(stage2Result.Scores);
        Assert.True(stage2Result.Scores.Count <= stage1Result.Scores.Count);
    }

    [Fact]
    public async Task EndToEnd_MultipleQueries_SameBackend()
    {
        // Arrange
        var reranker = new MockReranker();
        var queries = new[]
        {
            TestData.Queries.SearchQuery,
            TestData.Queries.SimpleQuery,
            TestData.Queries.ComplexQuery
        };
        var documents = TestData.Documents.StandardSet.ToRerankItems();

        // Act
        var results = new List<RerankResult>();
        foreach (var query in queries)
        {
            var result = await reranker.RerankAsync(query, documents);
            results.Add(result);
        }

        // Assert
        Assert.Equal(queries.Length, results.Count);
        foreach (var result in results)
        {
            Assert.NotEmpty(result.Scores);
        }
    }

    [Fact]
    public async Task EndToEnd_HandleEdgeCasesInPipeline()
    {
        // Arrange
        var reranker = new MockReranker();
        
        // Test various edge case combinations
        var testCases = new[]
        {
            (query: "", docs: TestData.Documents.StandardSet.ToRerankItems()),
            (query: TestData.Queries.SearchQuery, docs: Array.Empty<string>().ToRerankItems()),
            (query: TestData.Queries.SingleWordQuery, docs: TestData.Documents.EdgeCaseSet.ToRerankItems()),
        };

        // Act & Assert - all should complete without throwing
        foreach (var (query, docs) in testCases)
        {
            var result = await reranker.RerankAsync(query, docs);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task EndToEnd_ConcurrentRequests()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.StandardSet.ToRerankItems();

        // Act - concurrent calls
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => reranker.RerankAsync(query, documents))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, results.Length);
        foreach (var result in results)
        {
            Assert.NotEmpty(result.Scores);
        }
    }

    [Fact]
    public async Task EndToEnd_WithTopKFiltering()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.LargeSet.ToRerankItems();
        var options = new RerankOptions { TopK = 10 };

        // Act
        var result = await reranker.RerankAsync(query, documents, options);

        // Assert
        Assert.Equal(10, result.Scores.Count);
        Assert.Equal(TestData.Documents.LargeSet.Length, result.TotalItems);
    }

    [Fact]
    public async Task EndToEnd_ScoreDistribution()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.LargeSet.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        var scores = result.Scores.Select(d => (double)d.Score).ToList();
        var avgScore = scores.Average();
        var minScore = scores.Min();
        var maxScore = scores.Max();

        Assert.InRange(avgScore, 0.0, 1.0);
        Assert.InRange(minScore, 0.0, 1.0);
        Assert.InRange(maxScore, 0.0, 1.0);
    }

    [Fact]
    public async Task EndToEnd_DocumentMetadata_Preserved()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = new[] { "document 1", "document 2", "document 3" }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert - each ranked document has required metadata
        foreach (var doc in result.Scores)
        {
            Assert.NotNull(doc.Item.Text);
            Assert.True(doc.Rank > 0);
            Assert.InRange(doc.Score, 0.0f, 1.0f);
        }
    }
}
