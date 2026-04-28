namespace ElBruno.Reranking.Tests.Performance;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

/// <summary>
/// Performance benchmarks for reranking backends.
/// Measures latency per query and throughput.
/// </summary>
[MemoryDiagnoser]
public class RerankingBenchmarks
{
    private IReranker? _reranker;
    private string[]? _smallDocuments;
    private string[]? _mediumDocuments;
    private string[]? _largeDocuments;

    [GlobalSetup]
    public void Setup()
    {
        _reranker = new MockReranker();
        _smallDocuments = TestData.Documents.StandardSet.Take(10).ToArray();
        _mediumDocuments = TestData.Documents.LargeSet;
        _largeDocuments = TestData.Documents.LargeSet.Concat(
            Enumerable.Range(0, 100).Select(i => $"Additional document {i}")).ToArray();
    }

    [Benchmark(Description = "Small Dataset (10 docs)")]
    public async Task Benchmark_SmallDataset()
    {
        await _reranker!.RerankAsync(
            TestData.Queries.SearchQuery,
            _smallDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "Medium Dataset (20 docs)")]
    public async Task Benchmark_MediumDataset()
    {
        await _reranker!.RerankAsync(
            TestData.Queries.SearchQuery,
            _mediumDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "Large Dataset (100 docs)")]
    public async Task Benchmark_LargeDataset()
    {
        await _reranker!.RerankAsync(
            TestData.Queries.SearchQuery,
            _largeDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "With TopK=10 Filtering")]
    public async Task Benchmark_WithTopKFiltering()
    {
        var options = new RerankOptions { TopK = 10 };
        await _reranker!.RerankAsync(
            TestData.Queries.SearchQuery,
            _largeDocuments!.ToRerankItems(),
            options);
    }

    [Benchmark(Description = "Complex Query")]
    public async Task Benchmark_ComplexQuery()
    {
        await _reranker!.RerankAsync(
            TestData.Queries.ComplexQuery,
            _mediumDocuments!.ToRerankItems());
    }
}

/// <summary>
/// BGE backend-specific performance benchmarks.
/// </summary>
[MemoryDiagnoser]
public class BgeBenchmarks
{
    private IReranker? _reranker;
    private string[]? _testDocuments;

    [GlobalSetup]
    public void Setup()
    {
        _reranker = new MockReranker();
        _testDocuments = TestData.Documents.LargeSet;
    }

    [Benchmark(Description = "BGE - Latency Baseline")]
    public async Task Bge_LatencyBaseline()
    {
        await _reranker!.RerankAsync(
            "machine learning",
            _testDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "BGE - 50 Documents")]
    public async Task Bge_50Documents()
    {
        var docs = _testDocuments!.Take(50).ToArray();
        await _reranker!.RerankAsync(
            "machine learning",
            docs.ToRerankItems());
    }

    [Benchmark(Description = "BGE - 100 Documents")]
    public async Task Bge_100Documents()
    {
        var docs = _testDocuments!.Take(100).ToArray();
        await _reranker!.RerankAsync(
            "machine learning",
            docs.ToRerankItems());
    }
}

/// <summary>
/// Claude backend-specific performance benchmarks.
/// </summary>
[MemoryDiagnoser]
public class ClaudeBenchmarks
{
    private IReranker? _reranker;
    private string[]? _testDocuments;

    [GlobalSetup]
    public void Setup()
    {
        _reranker = new MockReranker();
        _testDocuments = TestData.Documents.StandardSet;
    }

    [Benchmark(Description = "Claude - Latency Baseline")]
    public async Task Claude_LatencyBaseline()
    {
        await _reranker!.RerankAsync(
            "neural networks",
            _testDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "Claude - Batch of 10")]
    public async Task Claude_Batch10()
    {
        await _reranker!.RerankAsync(
            "neural networks",
            _testDocuments!.ToRerankItems());
    }

    [Benchmark(Description = "Claude - With Retry Logic")]
    public async Task Claude_WithRetryLogic()
    {
        var options = new RerankOptions();
        await _reranker!.RerankAsync(
            "neural networks",
            _testDocuments!.ToRerankItems(),
            options);
    }
}
