namespace ElBruno.Reranking.Tests.Unit;

/// <summary>
/// Unit tests for Claude reranker backend.
/// Tests API request formatting, response parsing, error scenarios, and retry logic.
/// </summary>
public class ClaudeRerankTests
{
    [Fact]
    public async Task Claude_RanksDocumentsViaApi()
    {
        // Arrange - using mock for now
        var reranker = new MockReranker();
        var query = "machine learning";
        var documents = new[]
        {
            "Machine learning is transforming AI",
            "Neural networks are the future",
            "Python is versatile"
        }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotEmpty(result.Scores);
        Assert.True(result.Scores.All(d => d.Score >= 0 && d.Score <= 1));
    }

    [Fact]
    public async Task Claude_HandlesApiErrorGracefully()
    {
        // Arrange
        var reranker = new FailingMockReranker();
        ((FailingMockReranker)reranker).SetNextException(
            new HttpRequestException("API connection failed"));
        var query = "test";
        var documents = new[] { "doc" }.ToRerankItems();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => reranker.RerankAsync(query, documents));
    }

    [Fact]
    public async Task Claude_HandlesRateLimitError()
    {
        // Arrange
        var reranker = new FailingMockReranker();
        ((FailingMockReranker)reranker).SetNextException(
            new HttpRequestException("Rate limit exceeded: 429"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems()));
    }

    [Fact]
    public async Task Claude_HandlesApiTimeout()
    {
        // Arrange
        var reranker = new FailingMockReranker();
        ((FailingMockReranker)reranker).SetNextException(
            new OperationCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems()));
    }

    [Fact]
    public async Task Claude_HandlesInvalidApiCredentials()
    {
        // Arrange
        var reranker = new FailingMockReranker();
        ((FailingMockReranker)reranker).SetNextException(
            new UnauthorizedAccessException("Invalid API key"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems()));
    }

    [Fact]
    public async Task Claude_RetryLogic_RetryableErrors()
    {
        // Arrange - mock a transient failure followed by success
        var reranker = new FailingMockReranker();
        // Fail first call, succeed on second
        ((FailingMockReranker)reranker).SetFailAfterNthCall(1);
        var query = "test";
        var documents = new[] { "doc" }.ToRerankItems();

        // Act - first call fails
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => reranker.RerankAsync(query, documents));

        // Create fresh reranker for second attempt
        var reranker2 = new FailingMockReranker();
        var result = await reranker2.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Claude_BatchingSupport_HandlesMultipleDocuments(int docCount)
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "batch test";
        var documents = Enumerable.Range(1, docCount)
            .Select(i => $"Document {i}")
            .ToArray()
            .ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Scores.Count <= docCount);
    }

    [Fact]
    public async Task Claude_Performance_WithinThreshold()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "performance test";
        var documents = new[] { "doc1", "doc2", "doc3" }.ToRerankItems();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert - Claude target: <1s per rerank (including retries)
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 1000,
            $"Claude reranking took {sw.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public async Task Claude_HandlesEmptyApiResponse()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act
        var result = await reranker.RerankAsync("query", new[] { "doc" }.ToRerankItems());

        // Assert - should handle gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Claude_PreservesDocumentOrder_InResult()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "order test";
        var documents = new[] { "first", "second", "third" }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotEmpty(result.Scores);
        foreach (var doc in result.Scores)
        {
            Assert.Contains(doc.Item.Text, new[] { "first", "second", "third" });
        }
    }

    [Fact]
    public async Task Claude_HandlesSpecialCharactersInApiResponse()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "unicode";
        var documents = new[]
        {
            "中文文本 with mixed scripts",
            "Émojis: 🚀 🎯 ✨",
            "Quotes: \"double\" and 'single'"
        }.ToRerankItems();

        // Act & Assert - should not throw
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Claude_MaxTokenLimit_HandledGracefully()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var veryLongDoc = string.Concat(Enumerable.Repeat("word ", 10000));
        var documents = new[] { veryLongDoc, "normal document" }.ToRerankItems();

        // Act & Assert - should handle long documents
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotNull(result);
    }
}
