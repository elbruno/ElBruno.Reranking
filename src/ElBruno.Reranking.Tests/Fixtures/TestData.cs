namespace ElBruno.Reranking.Tests.Fixtures;

/// <summary>
/// Standard test data and documents for unit and integration tests.
/// </summary>
public static class TestData
{
    // Sample queries for testing
    public static class Queries
    {
        public const string SearchQuery = "machine learning algorithms";
        public const string SimpleQuery = "python programming";
        public const string ComplexQuery = "deep neural networks for natural language processing";
        public const string EmptyQuery = "";
        public const string SingleWordQuery = "AI";
    }

    // Sample documents for ranking
    public static class Documents
    {
        public static readonly string[] StandardSet = new[]
        {
            "Machine learning is a subset of artificial intelligence that focuses on learning from data.",
            "Python is a popular programming language for data science and machine learning.",
            "Neural networks are inspired by biological neurons and used in deep learning.",
            "Supervised learning requires labeled training data.",
            "Unsupervised learning discovers patterns in unlabeled data.",
            "Reinforcement learning trains agents through reward signals.",
            "Natural language processing enables computers to understand human language.",
            "Computer vision algorithms process and analyze images.",
            "Transfer learning reuses pre-trained models for new tasks.",
            "Ensemble methods combine multiple models for better predictions."
        };

        public static readonly string[] LargeSet = StandardSet.Concat(new[]
        {
            "Gradient descent optimizes model parameters iteratively.",
            "Backpropagation efficiently computes gradients in neural networks.",
            "Regularization prevents model overfitting.",
            "Cross-validation assesses model generalization.",
            "Hyperparameter tuning improves model performance.",
            "Feature engineering transforms raw data into useful features.",
            "Normalization scales features to similar ranges.",
            "Batch processing handles multiple samples simultaneously.",
            "Stochastic methods use randomness for optimization.",
            "Attention mechanisms focus on relevant parts of input."
        }).ToArray();

        public static readonly string[] EdgeCaseSet = new[]
        {
            "",
            " ",
            "a",
            "Very long document " + string.Join(" ", Enumerable.Repeat("word", 1000)),
            "Special!@#$%^&*()Characters",
            "\n\t\r",
            "你好世界 中文文本",
            "Émojis and äccënts 🚀"
        };
    }

    // Expected ranking scenarios
    public static class ExpectedResults
    {
        /// <summary>
        /// For query "machine learning algorithms", documents should rank by relevance
        /// </summary>
        public static int[] MachineLearningSortedIndices => new[] { 0, 1, 3, 4, 8 };

        /// <summary>
        /// Expected scores are between 0 and 1
        /// </summary>
        public const double MinScore = 0.0;
        public const double MaxScore = 1.0;
    }
}

/// <summary>
/// Fixture class for parametrized test scenarios.
/// </summary>
public class RerankerTestFixture : IAsyncLifetime
{
    public IReranker? Reranker { get; set; }
    public List<string> TestDocuments { get; set; } = new();
    public string TestQuery { get; set; } = TestData.Queries.SearchQuery;

    public async Task InitializeAsync()
    {
        TestDocuments.AddRange(TestData.Documents.StandardSet);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}

/// <summary>
/// Collection of test fixtures for multiple scenarios.
/// </summary>
[CollectionDefinition("Reranker Collection")]
public class RerankerCollection : ICollectionFixture<RerankerTestFixture>
{
    // This class has no code, it's just used to define a collection
}
