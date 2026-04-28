# Custom Reranker Guide

**Build your own reranking backend by implementing the IReranker interface.**

## Overview

The ElBruno.Reranking library is designed to be extensible. You can create custom rerankers for:

- **Proprietary models** — Use your organization's trained models
- **Hybrid backends** — Combine multiple rerankers
- **Domain-specific scoring** — Custom ranking logic
- **Legacy systems** — Wrap existing ranking APIs
- **Experimental models** — Test new approaches

## Interface Contract

All rerankers implement `IReranker`:

```csharp
public interface IReranker
{
    string Name { get; }
    
    Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

## Creating a Custom Reranker

### Step 1: Implement IReranker

```csharp
using ElBruno.Reranking;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class KeywordReranker : IReranker
{
    public string Name => "keyword-reranker";
    
    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));
        
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));
        
        var docList = documents.ToList();
        if (docList.Count == 0)
            throw new ArgumentException("Documents cannot be empty", nameof(documents));
        
        // Options
        options?.Validate();
        var topK = options?.TopK ?? int.MaxValue;
        var minScore = (float?)(options?.MinScore) ?? 0.0f;
        
        // Score documents
        var scores = ScoreDocuments(query, docList);
        
        // Filter and sort
        var ranked = scores
            .Where(s => s.Score >= minScore)
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .Select((s, rank) => new RankedDocument
            {
                Text = s.Document,
                Score = s.Score,
                Rank = rank + 1
            })
            .ToList();
        
        return new RerankResult
        {
            RankedDocuments = ranked,
            TotalDocuments = docList.Count,
            Metadata = new Dictionary<string, object>
            {
                { "backend", Name },
                { "elapsed_ms", 5 }
            }
        };
    }
    
    private List<(string Document, float Score)> ScoreDocuments(
        string query,
        List<string> documents)
    {
        var queryWords = query.ToLower().Split(' ');
        
        var scores = documents.Select(doc =>
        {
            var docWords = doc.ToLower().Split(' ');
            var matches = queryWords.Count(qw => docWords.Contains(qw));
            var score = (float)matches / queryWords.Length;
            return (doc, score);
        }).ToList();
        
        return scores;
    }
}
```

### Step 2: Use Your Custom Reranker

```csharp
using ElBruno.Reranking;

var reranker = new KeywordReranker();

var result = await reranker.RerankAsync(
    query: "machine learning",
    documents: new[]
    {
        "Machine learning is AI.",
        "Deep learning uses neural networks.",
        "The weather is sunny.",
    }
);

foreach (var doc in result.RankedDocuments)
{
    Console.WriteLine($"{doc.Rank}. {doc.Score:F3} — {doc.Text}");
}
```

## Advanced Example: Hybrid Reranker

Combine multiple backends for better results:

```csharp
using ElBruno.Reranking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class HybridReranker : IReranker
{
    private readonly IReranker _fast;      // BGE (ONNX)
    private readonly IReranker _precise;   // Claude
    
    public string Name => "hybrid-bge-claude";
    
    public HybridReranker(IReranker fast, IReranker precise)
    {
        _fast = fast ?? throw new ArgumentNullException(nameof(fast));
        _precise = precise ?? throw new ArgumentNullException(nameof(precise));
    }
    
    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));
        
        var docList = documents.ToList();
        if (docList.Count == 0)
            throw new ArgumentException("Documents cannot be empty", nameof(documents));
        
        // Stage 1: Fast ONNX reranking (all documents)
        var stageOneResult = await _fast.RerankAsync(
            query,
            docList,
            new RerankOptions { TopK = 10 },  // Get top 10 from BGE
            cancellationToken
        );
        
        // Stage 2: Precise Claude reranking (top 10)
        var topDocuments = stageOneResult.RankedDocuments
            .Select(d => d.Text)
            .ToList();
        
        var stageTwoResult = await _precise.RerankAsync(
            query,
            topDocuments,
            options,
            cancellationToken
        );
        
        return stageTwoResult;
    }
}
```

**Usage:**

```csharp
var onnx = new OnnxReranker("./models/bge-reranker-base.onnx");
var claude = new ClaudeReranker(apiKey);

var hybrid = new HybridReranker(onnx, claude);

var result = await hybrid.RerankAsync(query, documents);
```

## Example: Ensemble Reranker

Average scores from multiple backends:

```csharp
public class EnsembleReranker : IReranker
{
    private readonly IReranker[] _rerankers;
    
    public string Name => "ensemble";
    
    public EnsembleReranker(params IReranker[] rerankers)
    {
        if (rerankers?.Length == 0)
            throw new ArgumentException("At least one reranker required");
        
        _rerankers = rerankers;
    }
    
    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var docList = documents.ToList();
        
        // Rerank with all backends in parallel
        var results = await Task.WhenAll(
            _rerankers.Select(r => r.RerankAsync(query, docList, options, cancellationToken))
        );
        
        // Average scores
        var averaged = new Dictionary<string, float>();
        
        foreach (var doc in docList)
        {
            var scores = results
                .SelectMany(r => r.RankedDocuments)
                .Where(d => d.Text == doc)
                .Select(d => d.Score)
                .ToList();
            
            if (scores.Count > 0)
            {
                averaged[doc] = scores.Average();
            }
        }
        
        // Sort by averaged score
        var ranked = averaged
            .OrderByDescending(x => x.Value)
            .Select((x, rank) => new RankedDocument
            {
                Text = x.Key,
                Score = x.Value,
                Rank = rank + 1
            })
            .ToList();
        
        return new RerankResult
        {
            RankedDocuments = ranked,
            TotalDocuments = docList.Count,
            Metadata = new Dictionary<string, object>
            {
                { "backend", Name },
                { "ensemble_size", _rerankers.Length }
            }
        };
    }
}
```

## Best Practices

### 1. Input Validation

Always validate inputs at the start:

```csharp
if (string.IsNullOrWhiteSpace(query))
    throw new ArgumentException("Query cannot be empty", nameof(query));

if (documents == null)
    throw new ArgumentNullException(nameof(documents));

var docList = documents.ToList();
if (docList.Count == 0)
    throw new ArgumentException("Documents cannot be empty", nameof(documents));

options?.Validate();
```

### 2. Handle Cancellation

Respect cancellation tokens for long-running operations:

```csharp
public async Task<RerankResult> RerankAsync(
    string query,
    IEnumerable<string> documents,
    RerankOptions? options = null,
    CancellationToken cancellationToken = default)
{
    // Check for cancellation
    cancellationToken.ThrowIfCancellationRequested();
    
    // Long-running work
    await Task.Delay(1000, cancellationToken);
    
    cancellationToken.ThrowIfCancellationRequested();
    
    return result;
}
```

### 3. Measure Performance

Include timing in metadata:

```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();

// Do work...

sw.Stop();

return new RerankResult
{
    RankedDocuments = ranked,
    TotalDocuments = docList.Count,
    Metadata = new Dictionary<string, object>
    {
        { "elapsed_ms", sw.ElapsedMilliseconds }
    }
};
```

### 4. Score Normalization

Always return scores in [0.0, 1.0]:

```csharp
// Good: normalized to [0, 1]
var score = (float)matches / maxMatches;

// Bad: unbounded score
var score = matches * 1000;
```

### 5. Error Handling

Propagate meaningful errors:

```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    // Re-throw with context
    throw new InvalidOperationException(
        $"Reranking failed for query '{query}'",
        ex
    );
}
```

## Testing Your Custom Reranker

### Unit Tests

```csharp
using Xunit;

public class CustomRerankerTests
{
    [Fact]
    public async Task RerankAsync_WithValidInput_ReturnsRankedResults()
    {
        // Arrange
        var reranker = new KeywordReranker();
        var documents = new[] { "machine learning", "deep learning", "weather" };
        
        // Act
        var result = await reranker.RerankAsync("machine", documents);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalDocuments);
        Assert.Equal(2, result.RankedDocuments.Count);
        Assert.Equal("machine learning", result.RankedDocuments[0].Text);
    }
    
    [Fact]
    public async Task RerankAsync_WithEmptyQuery_ThrowsException()
    {
        // Arrange
        var reranker = new KeywordReranker();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => reranker.RerankAsync("", new[] { "doc" })
        );
    }
}
```

## Publishing Your Reranker

To share your custom reranker:

1. **Create NuGet package** — Package as separate NuGet if generic
2. **Document usage** — Include examples and API docs
3. **Add tests** — Comprehensive unit tests
4. **Performance benchmarks** — Show latency and throughput

## Common Patterns

### Caching Results

```csharp
public class CachedReranker : IReranker
{
    private readonly IReranker _inner;
    private readonly Dictionary<string, RerankResult> _cache = new();
    
    public string Name => _inner.Name;
    
    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var key = $"{query}:{string.Join(",", documents)}";
        
        if (_cache.TryGetValue(key, out var cached))
            return cached;
        
        var result = await _inner.RerankAsync(query, documents, options, cancellationToken);
        _cache[key] = result;
        return result;
    }
}
```

## Production Checklist

- ✅ Input validation for all parameters
- ✅ Cancellation token support
- ✅ Error handling with meaningful messages
- ✅ Scores normalized to [0.0, 1.0]
- ✅ Performance metrics in metadata
- ✅ Comprehensive unit tests
- ✅ Documentation and examples
- ✅ Benchmarks showing latency/throughput

## Next Steps

- [Architecture Deep-Dive](../architecture.md) — Understand system design
- [Performance Tuning Guide](performance-tuning.md) — Optimize your implementation
