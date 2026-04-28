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
        var documents = TestData.Documents.StandardSet;

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.RankedDocuments);
        Assert.Equal(documents.Length, result.TotalDocuments);
    }

    [Fact]
    public async Task EndToEnd_MultipleBackends_Sequential()
    {
        // Arrange
        var reranker1 = new MockReranker();
        var reranker2 = new MockReranker();
        var query = "neural networks";
        var documents = TestData.Documents.StandardSet;

        // Act - First reranker
        var result1 = await reranker1.RerankAsync(query, documents);

        // Use top results from first reranker as input to second
        var topDocs = result1.RankedDocuments.Take(5).Select(d => d.Text).ToArray();
        var result2 = await reranker2.RerankAsync(query, topDocs);

        // Assert
        Assert.NotEmpty(result1.RankedDocuments);
        Assert.NotEmpty(result2.RankedDocuments);
        Assert.True(result2.RankedDocuments.Count <= 5);
    }

    [Fact]
    public async Task EndToEnd_LargeDocumentSet()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.ComplexQuery;
        var documents = TestData.Documents.LargeSet;

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.Equal(documents.Length, result.TotalDocuments);
        Assert.NotEmpty(result.RankedDocuments);
        foreach (var doc in result.RankedDocuments)
        {
            Assert.InRange(doc.Score, 0.0, 1.0);
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
            allDocs,
            new RerankOptions { TopK = 5 });

        // Stage 2: Claude high-precision reranking
        var topDocs = stage1Result.RankedDocuments.Select(d => d.Text).ToArray();
        var stage2Result = await claudeReranker.RerankAsync(query, topDocs);

        // Assert
        Assert.NotEmpty(stage1Result.RankedDocuments);
        Assert.NotEmpty(stage2Result.RankedDocuments);
        Assert.True(stage2Result.RankedDocuments.Count <= stage1Result.RankedDocuments.Count);
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
        var documents = TestData.Documents.StandardSet;

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
            Assert.NotEmpty(result.RankedDocuments);
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
            (query: "", docs: TestData.Documents.StandardSet),
            (query: TestData.Queries.SearchQuery, docs: Array.Empty<string>()),
            (query: TestData.Queries.SingleWordQuery, docs: TestData.Documents.EdgeCaseSet),
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
        var documents = TestData.Documents.StandardSet;

        // Act - concurrent calls
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => reranker.RerankAsync(query, documents))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, results.Length);
        foreach (var result in results)
        {
            Assert.NotEmpty(result.RankedDocuments);
        }
    }

    [Fact]
    public async Task EndToEnd_WithTopKFiltering()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.LargeSet;
        var options = new RerankOptions { TopK = 10 };

        // Act
        var result = await reranker.RerankAsync(query, documents, options);

        // Assert
        Assert.Equal(10, result.RankedDocuments.Count);
        Assert.Equal(documents.Length, result.TotalDocuments);
    }

    [Fact]
    public async Task EndToEnd_ScoreDistribution()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = TestData.Queries.SearchQuery;
        var documents = TestData.Documents.LargeSet;

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        var scores = result.RankedDocuments.Select(d => d.Score).ToList();
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
        var documents = new[] { "document 1", "document 2", "document 3" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert - each ranked document has required metadata
        foreach (var doc in result.RankedDocuments)
        {
            Assert.NotNull(doc.Text);
            Assert.True(doc.Rank > 0);
            Assert.InRange(doc.Score, 0.0, 1.0);
        }
    }
}
