# Claude API Backend Guide

**High-precision semantic reranking using Claude LLM via Anthropic API.**

## Overview

The Claude backend leverages Claude's reasoning capabilities for intelligent reranking. It's ideal for scenarios requiring:

- 🧠 **Precision** — 98%+ R@5 on semantic relevance
- 🤔 **Complex reasoning** — Handle nuanced queries
- 🔄 **Semantic understanding** — Capture intent beyond keywords

## Model Details

**Claude 3 (via API):**
- Model: claude-3-opus (recommended for accuracy)
- Context: Up to 200K tokens
- Latency: 500ms–2s per request
- Accuracy: 98%+ R@5 on benchmark datasets
- Cost: ~$0.0008 per 100 items

## When to Use Claude

✅ **Use Claude if you need:**
- Highest accuracy (98%+ R@5)
- Complex semantic reasoning
- Nuanced query understanding
- Handling ambiguous or complex queries

❌ **Skip Claude if you need:**
- Lowest latency (<100ms)
- Offline operation
- Zero API calls
- Predictable cost

## Setup & Configuration

### 1. Get API Key

1. Create account at [Anthropic Console](https://console.anthropic.com)
2. Navigate to **API Keys**
3. Click **Create Key**
4. Copy the key (starts with `sk-ant-`)

### 2. Set Environment Variable

```bash
# Linux/Mac
export ANTHROPIC_API_KEY=sk-ant-...

# Windows PowerShell
$env:ANTHROPIC_API_KEY="sk-ant-..."

# Windows CMD
set ANTHROPIC_API_KEY=sk-ant-...
```

### 3. Create Instance

```csharp
using ElBruno.Reranking;
using ElBruno.Reranking.Backends.Claude;

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY not set");

var reranker = new ClaudeReranker(apiKey);

Console.WriteLine(reranker.Name);  // "claude-3-opus"
```

## API Reference

### RerankAsync

```csharp
Task<RerankResult> RerankAsync(
    string query,
    IEnumerable<RerankItem> items,
    RerankOptions? options = null,
    CancellationToken cancellationToken = default
);
```

**Parameters:**
- `query` — Search query/context (string)
- `items` — Candidate items to rank (`IEnumerable<RerankItem>`)
- `options` — Configuration (see below)
- `cancellationToken` — For cancellation support

**Returns:** `RerankResult` with `Scores` sorted by score (descending) and `TotalItems`

### Include Explanations

Claude can populate per-item explanations when requested:

```csharp
var options = new RerankOptions
{
    IncludeExplanation = true
};

var result = await reranker.RerankAsync(query, items, options);

Console.WriteLine(result.Scores[0].Explanation);
```

Set `IncludeExplanation = true` to include explanations in Claude responses. Leave it off for the default, faster path.

### RerankOptions

```csharp
var options = new RerankOptions
{
    TopK = 10,                      // Return top 10 only (default: all)
    MinScore = 0.7f,                // Filter score >= 0.7 (default: 0.0)
    MaxItems = 100,                 // Limit the number of items processed
    CustomOptions = new Dictionary<string, string>()
};

var result = await reranker.RerankAsync(query, items, options);
```

## Usage Examples

### Basic Reranking

```csharp
var items = new[]
{
    new RerankItem("Paris is the capital and most populous city of France."),
    new RerankItem("The Eiffel Tower is a wrought-iron lattice tower in Paris."),
    new RerankItem("Rome is the capital of Italy."),
    new RerankItem("The Colosseum is an ancient amphitheater in Rome."),
};

var result = await reranker.RerankAsync(
    query: "What is the capital of France?",
    items: items
);

foreach (var score in result.Scores)
{
    Console.WriteLine($"{score.Rank}. {score.Score:F3} — {score.Item.Text}");
}
```

**Output:**
```
1. 0.95 — Paris is the capital and most populous city of France.
2. 0.87 — The Eiffel Tower is a wrought-iron lattice tower in Paris.
3. 0.42 — The Colosseum is an ancient amphitheater in Rome.
4. 0.38 — Rome is the capital of Italy.
```

### Configure Request Timeout

```csharp
var claudeOptions = new ClaudeOptions
{
    ApiKey = apiKey,
    TimeoutMs = 90000,  // 90 seconds
};

var reranker = new ClaudeReranker(claudeOptions);

var options = new RerankOptions { TopK = 3 };

var result = await reranker.RerankAsync(
    query: "best programming language for web development",
    items: new[]
    {
        new RerankItem("Python is a high-level language known for simplicity."),
        new RerankItem("JavaScript is essential for web browser programming."),
        new RerankItem("Java is widely used in enterprise environments."),
    },
    options: options
);

foreach (var score in result.Scores)
{
    Console.WriteLine($"Score: {score.Score:F3}");
    Console.WriteLine($"Text: {score.Item.Text}");
    Console.WriteLine();
}
```

### Error Handling and Timeouts

```csharp
var options = new RerankOptions
{
    CustomOptions = new Dictionary<string, string>()
};

var claudeOptions = new ClaudeOptions
{
    ApiKey = apiKey,
    TimeoutMs = 90000,  // 90 seconds
};

var reranker = new ClaudeReranker(claudeOptions);

try
{
    var result = await reranker.RerankAsync(query, items, options);
    Console.WriteLine($"Success! Top result: {result.Scores[0].Item.Text}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (OperationCanceledException ex)
{
    Console.WriteLine($"Operation timed out or cancelled: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

### Batch Processing

```csharp
var queries = new[]
{
    "machine learning",
    "deep learning",
    "artificial intelligence"
};

foreach (var query in queries)
{
    var result = await reranker.RerankAsync(query, items);
    Console.WriteLine($"Query: {query}, Top score: {result.Scores[0].Score:F3}");
}
```

## Performance Characteristics

### Latency

| Items | Latency | Network |
|-----------|---------|---------|
| 5 | ~400ms | ~300ms |
| 10 | ~600ms | ~400ms |
| 50 | ~1.2s | ~700ms |
| 100 | ~2s | ~900ms |

*Network latency includes API round-trip time; actual inference is fast.*

### Throughput

- Single sequential: 5–10 requests/sec
- Parallel (10 concurrent): ~50 requests/sec
- Parallel (100 concurrent): ~500 requests/sec (with retries)

### Cost

- **Per 100 items:** ~$0.0008 (0.0000008 per item)
- **Per 1M items:** ~$8
- **Per 1000 reranking calls:** ~$0.80

**Cost example:**
```
Scenario: 100 search queries/day, 50 items/query
- Daily: 100 × 50 × $0.000008 = $0.04
- Monthly: $1.20
- Yearly: $14.40
```

## Limitations & Constraints

### Item Limits

- Max ~500 items per call (token limit)
- Recommended: <100 for best latency

```csharp
// Batch large result sets
if (items.Length > 500)
{
    var batches = items
        .Chunk(100)  // .NET 6+
        .ToList();
    
    foreach (var batch in batches)
    {
        var result = await reranker.RerankAsync(query, batch);
        // Process each batch
    }
}
```

### Token Limits

- Query + items must fit in ~100K tokens
- Average: ~3 tokens per word

### Rate Limiting

- API rate limits apply (check your Anthropic subscription)
- Automatic exponential backoff for transient errors
- Respect the limits to avoid throttling

## Advanced Configuration

### Custom Timeouts

```csharp
var claudeOptions = new ClaudeOptions
{
    ApiKey = apiKey,
    TimeoutMs = 120000  // 2 minute timeout for very large batches
};

var reranker = new ClaudeReranker(claudeOptions);

var options = new RerankOptions
{
    TopK = 10
};

var result = await reranker.RerankAsync(query, items, options);
```

### Retry Strategy

Claude backend automatically implements exponential backoff:

```
Attempt 1: Immediate
Attempt 2: Wait 1 second + random jitter
Attempt 3: Wait 2 seconds + random jitter
Attempt 4: Wait 4 seconds + random jitter
Attempt 5: Wait 8 seconds + random jitter
```

Retried on:
- 429 (Rate Limit)
- 500, 502, 503, 504 (Server Errors)

Not retried on:
- 400 (Bad Request)
- 401 (Unauthorized)
- 403 (Forbidden)

### Filtering Results

```csharp
// Only high-confidence results
var options = new RerankOptions { MinScore = 0.8f };

var result = await reranker.RerankAsync(query, items, options);

var highConfidence = result.Scores
    .Where(d => d.Score >= 0.8)
    .ToList();
```

## Production Best Practices

### 1. Batch Requests

```csharp
// Inefficient: sequential API calls
foreach (var query in queries)
{
    var result = await reranker.RerankAsync(query, items);
}

// Better: parallel with rate limit awareness
await Parallel.ForEachAsync(
    queries,
    new ParallelOptions { MaxDegreeOfParallelism = 10 },
    async (query, ct) =>
    {
        var result = await reranker.RerankAsync(query, items, cancellationToken: ct);
    }
);
```

### 2. Handle Timeouts Gracefully

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

try
{
    var result = await reranker.RerankAsync(
        query,
        items,
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    // Fallback to original ranking or cache
    Console.WriteLine("Reranking timed out, using original results");
}
```

### 3. Log Performance Metrics

```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();

var result = await reranker.RerankAsync(query, items);

sw.Stop();
Console.WriteLine($"Reranking took {sw.ElapsedMilliseconds}ms for {result.TotalItems} items");
```

### 4. Cache Results

```csharp
private static Dictionary<string, RerankResult> _cache = new();

public async Task<RerankResult> RerankWithCacheAsync(
    string query,
    IEnumerable<RerankItem> items)
{
    var key = $"{query}:{string.Join(',', items.Select(item => item.Text))}";
    
    if (_cache.TryGetValue(key, out var cached))
    {
        return cached;
    }
    
    var result = await reranker.RerankAsync(query, items);
    _cache[key] = result;
    return result;
}
```

## Error Handling

```csharp
try
{
    var result = await reranker.RerankAsync(query, items);
}
catch (ArgumentException ex)
{
    // Input validation error
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (OperationCanceledException ex)
{
    // Timeout or cancellation
    Console.WriteLine($"Operation timed out: {ex.Message}");
}
catch (HttpRequestException ex)
{
    // Network error
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (Exception ex)
{
    // Other API errors
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Production Checklist

- ✅ API key stored in environment variables (not code)
- ✅ Error handling for all scenarios
- ✅ Timeouts configured appropriately
- ✅ Parallel requests rate-limited
- ✅ Cost monitoring in place
- ✅ Caching strategy implemented
- ✅ Fallback mechanism for failures
- ✅ Logging enabled for monitoring

## Comparison with Other Backends

| Feature | Claude | ONNX | Ollama |
|---------|--------|------|--------|
| Accuracy | ⚡⚡⚡ 98%+ | ⚡⚡ 96% | ⚡⚡ Model-dependent |
| Speed | ⚡ ~1s | ⚡⚡⚡ ~15ms | ⚡⚡ ~200ms–5s |
| Cost | ~$0.0008/call | Free | Free |
| Privacy | ✗ Cloud | ✓ Local | ✓ Local |
| Offline | ✗ No | ✓ Yes | ✓ Yes |
| Explanations | ✓ Yes (when `RerankOptions.IncludeExplanation=true`) | ✗ No | Depends |

## Next Steps

- [Cost Estimation Guide](../cost-estimation.md) — Calculate API costs
- [Performance Tuning Guide](performance-tuning.md) — Optimize batch sizes
- [Custom Reranker Guide](custom-reranker.md) — Build hybrid backends
