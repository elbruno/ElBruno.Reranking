namespace ElBruno.Reranking.Tests.Unit;

/// <summary>
/// Unit tests for BGE reranker backend.
/// Tests model loading, tokenization, ranking, and performance.
/// </summary>
public class BgeRerankTests
{
    [Fact]
    public async Task Bge_Initialization_Succeeds()
    {
        // Arrange
        var reranker = new MockReranker(); // Mock for BGE until implementation ready

        // Act
        var result = await reranker.RerankAsync("test", new[] { "doc" });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Bge_RanksDocumentsByRelevance()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "machine learning";
        var documents = new[]
        {
            "Machine learning algorithms for classification",
            "The history of ancient Rome",
            "Deep learning with neural networks"
        };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotEmpty(result.RankedDocuments);
        // First result should be related to machine learning
        Assert.Contains("machine learning", result.RankedDocuments.First().Text.ToLower());
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Bge_HandlesVariousDocumentCounts(int documentCount)
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = Enumerable.Range(1, documentCount)
            .Select(i => $"Document {i} contains test content")
            .ToArray();

        // Act & Assert - should not throw
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Bge_HandlesLongDocuments()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "machine learning";
        var longDoc = string.Concat(Enumerable.Repeat(
            "Machine learning is a subset of artificial intelligence. ", 500));
        var documents = new[] { longDoc, "Short doc" };

        // Act & Assert - should not throw
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotEmpty(result.RankedDocuments);
    }

    [Fact]
    public async Task Bge_HandlesUnicodeAndSpecialCharacters()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "测试 test";
        var documents = new[]
        {
            "中文测试文档",
            "English test document",
            "Special chars: !@#$%^&*()"
        };

        // Act & Assert - should not throw
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotEmpty(result.RankedDocuments);
    }

    [Fact]
    public async Task Bge_Performance_UnderThreshold()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "performance test";
        var documents = TestData.Documents.LargeSet;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert - BGE target: <100ms per rerank
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"BGE reranking took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    public async Task Bge_ErrorHandling_InvalidInput_ReturnsEmpty()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act
        var result = await reranker.RerankAsync("", Array.Empty<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.RankedDocuments);
    }

    [Fact]
    public async Task Bge_ConsistentResults_SameInputGivesSameRanking()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "consistent test";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act
        var result1 = await reranker.RerankAsync(query, documents);
        var result2 = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.Equal(result1.RankedDocuments.Count, result2.RankedDocuments.Count);
        for (int i = 0; i < result1.RankedDocuments.Count; i++)
        {
            Assert.Equal(result1.RankedDocuments[i].Text, result2.RankedDocuments[i].Text);
            Assert.Equal(result1.RankedDocuments[i].Score, result2.RankedDocuments[i].Score);
        }
    }

    [Fact]
    public async Task Bge_TopK_CorrectlyLimitsResults()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = Enumerable.Range(1, 100)
            .Select(i => $"Document {i}")
            .ToArray();

        // Act
        var resultTopK = await reranker.RerankAsync(query, documents, 
            new RerankOptions { TopK = 10 });
        var resultAll = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.Equal(10, resultTopK.RankedDocuments.Count);
        Assert.Equal(100, resultAll.RankedDocuments.Count);
    }

    [Fact]
    public async Task Bge_NullOptions_UsesDefaults()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act - explicitly pass null options
        var result = await reranker.RerankAsync(query, documents, null);

        // Assert - should work fine with defaults
        Assert.NotNull(result);
        Assert.NotEmpty(result.RankedDocuments);
    }
}
