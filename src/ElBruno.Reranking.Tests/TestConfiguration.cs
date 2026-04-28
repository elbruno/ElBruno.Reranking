namespace ElBruno.Reranking.Tests;

/// <summary>
/// Empty fixture for sequential test collection.
/// </summary>
public class SequentialCollectionFixture { }

/// <summary>
/// Configures xUnit test behavior and parallel execution settings.
/// </summary>
[CollectionDefinition("Sequential")]
public class SequentialCollection : ICollectionFixture<SequentialCollectionFixture>
{
    // This class has no code, it's used to define a sequential collection for tests
}

/// <summary>
/// Test configuration constants.
/// </summary>
public static class TestConfiguration
{
    // Performance thresholds
    public const int BgePerformanceThresholdMs = 100;
    public const int ClaudePerformanceThresholdMs = 1000;
    
    // Test timeouts
    public const int DefaultTestTimeoutMs = 5000;
    public const int LongRunningTestTimeoutMs = 30000;
    
    // Coverage targets
    public const double CodeCoverageTarget = 0.85; // 85%
    public const double CriticalPathCoverageTarget = 1.0; // 100%
}
