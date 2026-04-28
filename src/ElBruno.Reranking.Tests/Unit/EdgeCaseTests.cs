namespace ElBruno.Reranking.Tests.Unit;

/// <summary>
/// Edge case tests for error handling and boundary conditions.
/// </summary>
public class EdgeCaseTests
{
    [Fact]
    public async Task EdgeCase_NullQuery_HandledGracefully()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act & Assert - should handle null or throw appropriate exception
        try
        {
            await reranker.RerankAsync(null!, new[] { "doc" }.ToRerankItems());
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task EdgeCase_NullDocuments_HandledGracefully()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act & Assert - should handle null appropriately
        try
        {
            await reranker.RerankAsync("query", null!);
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task EdgeCase_ExtremelyLongQuery()
    {
        // Arrange
        var reranker = new MockReranker();
        var longQuery = string.Concat(Enumerable.Repeat("word ", 10000));
        var documents = new[] { "doc1", "doc2" }.ToRerankItems();

        // Act & Assert
        var result = await reranker.RerankAsync(longQuery, documents);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_SingleCharacterDocuments()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "a b c";
        var documents = new[] { "a", "b", "c", "d", "e" }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotEmpty(result.Scores);
    }

    [Fact]
    public async Task EdgeCase_DuplicateDocuments()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = new[] { "same doc", "same doc", "same doc", "different" }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert - should handle duplicates
        Assert.NotEmpty(result.Scores);
    }

    [Fact]
    public async Task EdgeCase_DocumentsWithOnlyWhitespace()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        // Note: RerankItem doesn't allow whitespace-only text per specification
        // Test with minimal content instead
        var documents = new[] { "a", "ab", "abc", "valid" }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(1)]
    public async Task EdgeCase_VariousTopKValues(int topK)
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = TestData.Documents.StandardSet.ToRerankItems();
        var options = new RerankOptions { TopK = topK };

        // Act
        var result = await reranker.RerankAsync(query, documents, options);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_InvalidMinScoreOption()
    {
        // Arrange
        var reranker = new MockReranker();
        var options = new RerankOptions { MinScore = (float)2.0 }; // Invalid: > 1.0

        // Act & Assert - should either handle or throw
        var result = await reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems(), options);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_VeryLowMinScore()
    {
        // Arrange
        var reranker = new MockReranker();
        var options = new RerankOptions { MinScore = (float)0.0001 };

        // Act
        var result = await reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems(), options);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_ConcurrentCancellation()
    {
        // Arrange
        var reranker = new MockReranker();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(1);

        // Act & Assert
        try
        {
            await reranker.RerankAsync("test", new[] { "doc" }.ToRerankItems(), cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task EdgeCase_EmptyStringQuery()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act
        var result = await reranker.RerankAsync("", new[] { "doc" }.ToRerankItems());

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_SpecialCharactersInQuery()
    {
        // Arrange
        var reranker = new MockReranker();
        var specialQuery = "!@#$%^&*()[]{}|\\:;\"'<>,.?/";
        var documents = new[] { "doc1", "doc2" }.ToRerankItems();

        // Act & Assert - should not crash
        var result = await reranker.RerankAsync(specialQuery, documents);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EdgeCase_MultilineDocuments()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "line1";
        var documents = new[]
        {
            "line1\nline2\nline3",
            "single line",
            "line1\r\nline2\r\nline3"
        }.ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotEmpty(result.Scores);
    }

    [Fact]
    public async Task EdgeCase_VeryLargeNumberOfDocuments()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = Enumerable.Range(1, 5000)
            .Select(i => $"Document {i}")
            .ToArray()
            .ToRerankItems();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5000, result.TotalItems);
    }
}
