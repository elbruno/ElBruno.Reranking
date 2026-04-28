# Performance Tuning Guide

**Optimize ElBruno.Reranking for your workload.**

## Overview

This guide covers profiling, optimization strategies, and performance best practices for each backend.

## Benchmarking Your Workload

### 1. Measure Baseline Performance

```csharp
using System.Diagnostics;
using ElBruno.Reranking;

var reranker = new OnnxReranker("./models/bge-reranker-base.onnx");
var sw = Stopwatch.StartNew();

for (int i = 0; i < 100; i++)
{
    var result = await reranker.RerankAsync(query, documents);
}

sw.Stop();

Console.WriteLine($"Average latency: {sw.ElapsedMilliseconds / 100}ms per query");
Console.WriteLine($"Throughput: {100000 / sw.ElapsedMilliseconds} queries/sec");
```

### 2. Profile Memory Usage

```csharp
var before = GC.GetTotalMemory(true);

var result = await reranker.RerankAsync(query, documents);

var after = GC.GetTotalMemory(true);

Console.WriteLine($"Memory allocated: {(after - before) / 1024} KB");
```

## ONNX Backend Optimization

### 1. Batch Size Optimization

**Rule of thumb:** 50–1000 documents per call

```csharp
// Measure latency for different batch sizes
var sizes = new[] { 10, 50, 100, 500, 1000 };

foreach (var size in sizes)
{
    var docs = documents.Take(size).ToList();
    var sw = Stopwatch.StartNew();
    
    var result = await reranker.RerankAsync(query, docs);
    
    sw.Stop();
    
    Console.WriteLine($"{size} docs: {sw.ElapsedMilliseconds}ms ({sw.ElapsedMilliseconds / (float)size:F2}ms per doc)");
}
```

**Results (typical):**
```
10 docs: 10ms (1.0ms per doc)
50 docs: 12ms (0.24ms per doc)
100 docs: 15ms (0.15ms per doc)
500 docs: 70ms (0.14ms per doc)
1000 docs: 150ms (0.15ms per doc)
```

**Recommendation:** 100–500 documents per call maximizes efficiency.

### 2. Parallel Processing

```csharp
// Sequential (slower)
var results = new List<RerankResult>();

foreach (var query in queries)
{
    var result = await reranker.RerankAsync(query, documents);
    results.Add(result);
}

// Parallel (faster on multi-core)
var results = await Task.WhenAll(
    queries.Select(q => reranker.RerankAsync(q, documents))
);
```

**Performance gain:** ~2-3x on 4 cores, ~4-7x on 8 cores

### 3. Caching Query Embeddings

```csharp
// Cache identical queries to avoid redundant inference
private readonly Dictionary<string, RerankResult> _cache = new();

public async Task<RerankResult> RerankWithCacheAsync(
    string query,
    IEnumerable<string> documents)
{
    var key = $"{query}:{string.Join(',', documents)}";
    
    if (_cache.TryGetValue(key, out var cached))
    {
        Console.WriteLine("Cache hit!");
        return cached;
    }
    
    var result = await reranker.RerankAsync(query, documents);
    _cache[key] = result;
    return result;
}
```

**Expected hit rate:** 30–80% depending on workload

### 4. Pre-filter Before Reranking

```csharp
// Bad: rerank all documents
var allDocs = await db.GetAllDocuments();
var result = await reranker.RerankAsync(query, allDocs);

// Good: filter first, rerank top candidates
var candidates = await db.SearchBM25(query, limit: 100);
var result = await reranker.RerankAsync(query, candidates);
```

**Latency reduction:** 50–90% depending on filtering effectiveness

## Claude Backend Optimization

### 1. Batch Size Optimization

**Rule of thumb:** 10–50 documents per call

```csharp
// Typical latency by batch size
// 5 docs: ~400ms
// 10 docs: ~600ms
// 50 docs: ~1.2s
// 100 docs: ~2s
```

**Recommendation:** 20–30 documents per call (sweet spot between latency and throughput)

### 2. Parallel API Calls

```csharp
// Sequential (slow)
foreach (var query in queries)
{
    var result = await reranker.RerankAsync(query, documents);
}

// Parallel with rate limit (faster)
await Parallel.ForEachAsync(
    queries,
    new ParallelOptions { MaxDegreeOfParallelism = 10 },
    async (query, ct) =>
    {
        var result = await reranker.RerankAsync(query, documents, cancellationToken: ct);
    }
);
```

**Expected throughput:** 5–50 requests/sec depending on parallelism

### 3. Request Caching

```csharp
public class CachedClaudeReranker
{
    private readonly IReranker _reranker;
    private readonly MemoryCache _cache;
    
    public async Task<RerankResult> RerankAsync(string query, IEnumerable<string> documents)
    {
        var key = $"{query}:{string.Join(',', documents)}";
        
        if (_cache.TryGetValue(key, out RerankResult? result))
            return result!;
        
        result = await _reranker.RerankAsync(query, documents);
        
        _cache.Set(key, result, TimeSpan.FromHours(1));
        
        return result;
    }
}
```

### 4. Timeout Configuration

```csharp
var options = new RerankOptions
{
    TimeoutMs = 30000,      // Short timeout for real-time scenarios
    EnableRetry = false,    // Disable retries if you have tight SLA
};

var result = await reranker.RerankAsync(query, documents, options);
```

## Monitoring & Profiling

### 1. Latency Percentiles

```csharp
var latencies = new List<long>();

for (int i = 0; i < 1000; i++)
{
    var sw = Stopwatch.StartNew();
    var result = await reranker.RerankAsync(query, documents);
    sw.Stop();
    
    latencies.Add(sw.ElapsedMilliseconds);
}

var sorted = latencies.OrderBy(x => x).ToList();

Console.WriteLine($"P50: {sorted[(int)(sorted.Count * 0.50)]}ms");
Console.WriteLine($"P95: {sorted[(int)(sorted.Count * 0.95)]}ms");
Console.WriteLine($"P99: {sorted[(int)(sorted.Count * 0.99)]}ms");
```

### 2. Memory Profiling

```csharp
using var profiler = new MemoryProfile();

var result = await reranker.RerankAsync(query, documents);

Console.WriteLine($"Peak memory: {profiler.PeakMemory} MB");
Console.WriteLine($"GC collections: {profiler.GcCollections}");
```

### 3. Throughput Monitoring

```csharp
public class ThroughputMonitor
{
    private long _totalRequests;
    private long _totalTime;
    
    public async Task<RerankResult> RerankAsync(
        IReranker reranker,
        string query,
        IEnumerable<string> documents)
    {
        var sw = Stopwatch.StartNew();
        var result = await reranker.RerankAsync(query, documents);
        sw.Stop();
        
        _totalRequests++;
        _totalTime += sw.ElapsedMilliseconds;
        
        if (_totalRequests % 100 == 0)
        {
            var avgLatency = _totalTime / _totalRequests;
            var throughput = 1000 / avgLatency;
            Console.WriteLine($"Throughput: {throughput} requests/sec");
        }
        
        return result;
    }
}
```

## Optimization Strategies by Scenario

### Scenario 1: Search Result Reranking (100 results)

```csharp
// Approach: Two-stage pipeline
// 1. ONNX: Rerank all 100 documents (~15ms)
// 2. Claude: Rerank top 10 only (~600ms if enabled)
// Total: ~15ms or ~615ms depending on complexity

var onnxResult = await onnx.RerankAsync(query, allResults, new RerankOptions { TopK = 10 });

if (needsHighPrecision)
{
    var claudeResult = await claude.RerankAsync(query, onnxResult.RankedDocuments.Select(d => d.Text));
    return claudeResult;
}
else
{
    return onnxResult;
}
```

### Scenario 2: RAG Pipeline (Initial retrieval → Reranking → LLM context)

```csharp
// Approach: Batch + Filter + Rerank
// 1. Vector DB: Retrieve 100 candidates
// 2. ONNX: Rerank to top 10 (~10ms)
// 3. Claude: Optional precision reranking (~400ms)
// 4. LLM context: Use top 5

var candidates = await vectorDb.SearchAsync(query, k: 100);
var onnxReranked = await onnx.RerankAsync(query, candidates);
var finalRanked = onnxReranked.RankedDocuments.Take(5);

var context = string.Join("\n\n", finalRanked.Select(d => d.Text));
```

### Scenario 3: Batch Processing (1000+ queries)

```csharp
// Approach: Parallel processing with caching
// 1. Cache results for repeated queries
// 2. Use max parallelism without rate limiting
// 3. Batch similar queries together

var cache = new Dictionary<string, RerankResult>();

async Task<RerankResult> RerankWithCacheAsync(string q, IEnumerable<string> docs)
{
    var key = $"{q}:{string.Join(',', docs)}";
    if (cache.TryGetValue(key, out var result))
        return result;
    
    result = await reranker.RerankAsync(q, docs);
    cache[key] = result;
    return result;
}

await Parallel.ForEachAsync(
    queries,
    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
    async (query, ct) => await RerankWithCacheAsync(query, documents)
);
```

## Performance Targets

### ONNX Backend

- **P50 latency:** <15ms
- **P99 latency:** <25ms
- **Throughput:** 60+ queries/sec
- **Memory:** <1 KB per call

### Claude Backend

- **P50 latency:** <600ms
- **P99 latency:** <2s
- **Throughput:** 5–10 queries/sec (sequential), 50+ with parallelism
- **Memory:** <10 KB per call

## Troubleshooting Performance Issues

### Issue: High Latency

```csharp
// Check 1: Are you reranking too many documents?
if (documents.Count > 1000)
{
    // Solution: Batch or pre-filter
    var topCandidates = PrefilterDocuments(query, documents);
    var result = await reranker.RerankAsync(query, topCandidates);
}

// Check 2: Are you creating new instances repeatedly?
// Bad:
var reranker = new OnnxReranker(modelPath);  // Creates every request
// Good:
static reranker = new OnnxReranker(modelPath);  // Once at startup

// Check 3: Are you using synchronous operations?
// Bad: .Result blocks threads
// Good: async/await throughout
```

### Issue: High Memory Usage

```csharp
// Check 1: Are you keeping references to old results?
var results = new List<RerankResult>();
for (int i = 0; i < 10000; i++)
{
    results.Add(await reranker.RerankAsync(query, documents));
}
// Solution: Process and discard immediately

// Check 2: Are you caching too much?
// Solution: Set cache expiry
_cache.Set(key, value, TimeSpan.FromHours(1));
```

### Issue: Throttling / Rate Limits (Claude)

```csharp
// Solution: Reduce parallelism
var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };

// Solution: Add delays
var tasks = queries.Select(async q =>
{
    var result = await reranker.RerankAsync(q, documents);
    await Task.Delay(100);
    return result;
});
```

## Production Deployment Checklist

- ✅ Baseline benchmarks established
- ✅ Optimal batch sizes identified
- ✅ Caching strategy implemented
- ✅ Parallel processing configured
- ✅ Monitoring alerts set
- ✅ Error handling in place
- ✅ SLA targets defined
- ✅ Scaling plan for growth

## Next Steps

- [Benchmarks Guide](../benchmarks.md) — Full performance data
- [Cost Estimation](../cost-estimation.md) — Optimize costs
