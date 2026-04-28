namespace ElBruno.Reranking.Tests.Unit;

/// <summary>
/// Contract tests that any IReranker implementation must pass.
/// These define the behavioral specification for the interface.
/// </summary>
public class RerankerContractTests
{
    [Fact]
    public async Task RerankAsync_WithValidInputs_ReturnsRerankResult()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RankedDocuments);
    }

    [Fact]
    public async Task RerankAsync_WithEmptyDocuments_ReturnsEmptyResult()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = Array.Empty<string>();

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.RankedDocuments);
    }

    [Fact]
    public async Task RerankAsync_WithEmptyQuery_ShouldNotThrow()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "";
        var documents = new[] { "doc1", "doc2" };

        // Act & Assert - should not throw
        var result = await reranker.RerankAsync(query, documents);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RerankAsync_ResultScoresInValidRange()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        foreach (var doc in result.RankedDocuments)
        {
            Assert.InRange(doc.Score, 0.0, 1.0);
        }
    }

    [Fact]
    public async Task RerankAsync_RankedDocumentsHaveRanks()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        for (int i = 0; i < result.RankedDocuments.Count; i++)
        {
            Assert.Equal(i + 1, result.RankedDocuments[i].Rank);
        }
    }

    [Fact]
    public async Task RerankAsync_ScoresAreDescending()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        for (int i = 0; i < result.RankedDocuments.Count - 1; i++)
        {
            Assert.True(
                result.RankedDocuments[i].Score >= result.RankedDocuments[i + 1].Score,
                "Scores must be in descending order");
        }
    }

    [Fact]
    public async Task RerankAsync_TopKOptionLimitsResults()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3", "doc4", "doc5" };
        var options = new RerankOptions { TopK = 3 };

        // Act
        var result = await reranker.RerankAsync(query, documents, options);

        // Assert
        Assert.Equal(3, result.RankedDocuments.Count);
    }

    [Fact]
    public async Task RerankAsync_SupportsAsync_CompletesSynchronously()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2" };

        // Act
        var task = reranker.RerankAsync(query, documents);
        var completed = await Task.WhenAny(task, Task.Delay(5000));

        // Assert
        Assert.Equal(task, completed);
    }

    [Fact]
    public async Task RerankAsync_WithCancellationToken_Respects_Cancellation()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2" };
        var cts = new CancellationTokenSource();

        // Act
        var task = reranker.RerankAsync(query, documents, cancellationToken: cts.Token);
        cts.Cancel();

        // Assert - should either complete or throw OperationCanceledException
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public void Reranker_HasName_Property()
    {
        // Arrange
        var reranker = new MockReranker();

        // Act
        var name = reranker.Name;

        // Assert
        Assert.NotNull(name);
        Assert.NotEmpty(name);
    }

    [Fact]
    public async Task RerankAsync_PreservesOriginalDocumentText()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test";
        var documents = new[] { "document one", "document two", "document three" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        var resultTexts = result.RankedDocuments.Select(d => d.Text).ToList();
        foreach (var originalDoc in documents)
        {
            Assert.Contains(originalDoc, resultTexts);
        }
    }

    [Fact]
    public async Task RerankAsync_TotalDocumentsMatchesInput()
    {
        // Arrange
        var reranker = new MockReranker();
        var query = "test query";
        var documents = new[] { "doc1", "doc2", "doc3", "doc4", "doc5" };

        // Act
        var result = await reranker.RerankAsync(query, documents);

        // Assert
        Assert.Equal(documents.Length, result.TotalDocuments);
    }
}
