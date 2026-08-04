# ElBruno.Reranking

![GitHub Actions](https://img.shields.io/github/actions/workflow/status/ElBruno/ElBruno.Reranking/dotnet.yml?branch=main)
![NuGet](https://img.shields.io/nuget/v/ElBruno.Reranking)
![Downloads](https://img.shields.io/nuget/dt/ElBruno.Reranking)
![License](https://img.shields.io/github/license/ElBruno/ElBruno.Reranking)

**Semantic reranking for .NET: Local-first ONNX, cloud-ready APIs, and extensible backends.**

ElBruno.Reranking improves search result relevance through intelligent semantic reordering. It provides a unified interface for multiple reranking backends:

- **BGE-Reranker (ONNX):** Fast local reranking (~15ms, CPU)
- **Claude API:** High-precision cloud reranking (98%+ R@5, <1s)
- **Ollama:** Flexible local LLMs (free, offline)
- **Custom:** Bring your own reranker

## Features

✨ **Simple API** — Single `RerankAsync(query, items, options)` method for all backends
⚡ **Fast ONNX inference** — BGE reranker: <100ms for 100 docs  
🧠 **Cloud-ready Claude backend** — Leverage LLMs for high-precision reranking  
🎯 **Pluggable architecture** — Extend with custom backends  
🔄 **Async/await throughout** — Built for high-concurrency .NET applications  
🛠️ **Production-ready** — Error handling, retry logic, timeouts  

## Packages & features

- **[ElBruno.Reranking](https://www.nuget.org/packages/ElBruno.Reranking/)** — core semantic reranking package
- **[ElBruno.Reranking.BlazorComponents](src/ElBruno.Reranking.BlazorComponents/README.md)** — component package README
- **[Blazor components guide](docs/blazor-components.md)** — component map and UI notes
- **[BlazorRerankingDemo](src/samples/BlazorRerankingDemo/README.md)** — deterministic Blazor sample app
- **[Quickstart guide](docs/guides/quickstart.md)** — setup and backend selection
- **[Performance tuning](docs/guides/performance-tuning.md)** — practical optimization tips

## What's New

- Added a Blazor component sample app for the reranking UI experience
- Documented the planned component surface in `docs/blazor-components.md`
- Added `CodeSample` and `CodeSnippets` helpers for repeatable doc blocks
- Standardized the sample on Bootstrap 5.3.3 and deterministic demo data
- Added a release instruction reminding NuGet publishes to review this section

## Installation

```bash
dotnet add package ElBruno.Reranking
dotnet add package ElBruno.Reranking.BlazorComponents
```

Or via NuGet Package Manager:
```
Install-Package ElBruno.Reranking
```

## Quick Start (3 minutes)

### ONNX Backend (Local Reranking)

```csharp
using ElBruno.Reranking;

// Documents to rerank
var items = new[]
{
    new RerankItem("Machine learning is a subset of artificial intelligence."),
    new RerankItem("Deep learning uses neural networks with many layers."),
    new RerankItem("The weather is sunny today."),
    new RerankItem("Natural language processing enables text understanding."),
};

// Create reranker (requires BGE model file)
var reranker = new OnnxReranker("./models/bge-reranker-base.onnx");

// Rerank
var result = await reranker.RerankAsync(
    query: "What is machine learning?",
    items: items,
    options: new RerankOptions { TopK = 5 }
);

// Results
foreach (var score in result.Scores)
{
    Console.WriteLine($"Score: {score.Score:F3}, Text: {score.Item.Text}");
}
```

**Output:**
```
Score: 0.918, Text: Machine learning is a subset of artificial intelligence.
Score: 0.876, Text: Deep learning uses neural networks with many layers.
Score: 0.654, Text: Natural language processing enables text understanding.
Score: 0.142, Text: The weather is sunny today.
```

### Claude Backend (Cloud Reranking)

```csharp
using ElBruno.Reranking;

var items = new[]
{
    new RerankItem("The capital of France is Paris."),
    new RerankItem("Paris is a city known for the Eiffel Tower."),
    new RerankItem("The capital of Germany is Berlin."),
};

// Create Claude reranker
var reranker = new ClaudeReranker(apiKey: Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));

// Rerank with explanation
var result = await reranker.RerankAsync(
    query: "What is the capital of France?",
    items: items,
    options: new RerankOptions
    {
        TopK = 3,
        MinScore = 0.2f,
        IncludeExplanation = true
    }
);

foreach (var score in result.Scores)
{
    Console.WriteLine($"Score: {score.Score:F3}, Text: {score.Item.Text}");
}
```

### Blazor Components

```csharp
using ElBruno.Reranking.BlazorComponents.Extensions;

builder.Services.AddRerankingBlazorComponents();
```

Use `BackendSelector`, `RerankResultList`, `ScoreHeatmap`, and `RerankPlayground` to build a reranking UI quickly.

## Documentation

- **[Quickstart Guide](docs/guides/quickstart.md)** — Step-by-step setup for all backends
- **[BGE-Reranker (ONNX) Guide](docs/guides/onnx-backend.md)** — Local-first reranking
- **[Claude Backend Guide](docs/guides/claude-backend.md)** — API-based high-precision reranking
- **[Custom Reranker Guide](docs/guides/custom-reranker.md)** — Extend with your own backend
- **[Performance Tuning Guide](docs/guides/performance-tuning.md)** — Optimize for your workload
- **[Architecture Deep-Dive](docs/architecture.md)** — System design and abstractions
- **[Blazor Components Guide](docs/blazor-components.md)** — UI component map and sample references
- **[Blazor Components Package](src/ElBruno.Reranking.BlazorComponents/README.md)** — package README and public surface
- **[Performance Benchmarks](docs/benchmarks.md)** — Real-world latency and throughput
- **[Cost Estimation](docs/cost-estimation.md)** — BGE vs Claude cost analysis
- **[Roadmap](docs/roadmap.md)** — Future backends and features

## Performance Benchmarks

| Backend | Latency (100 docs) | Throughput | Cost | Privacy |
|---------|-------------------|-----------|------|---------|
| **BGE (ONNX)** | ~15ms | 67 QPS | Free | Local only |
| **Claude API** | <1s (incl. network) | 5-10 QPS | ~$0.0008/call | Cloud |
| **Ollama** | 200ms–5s | ~100 QPS | Free | Local only |

*Full benchmarks in [docs/benchmarks.md](docs/benchmarks.md)*

## Core Concepts

### RerankItem (Input)

```csharp
public class RerankItem
{
    public string? Id { get; set; }                          // Caller-provided ID
    public string Text { get; set; }                         // Content to rerank
    public Dictionary<string, object>? Metadata { get; set; } // Custom metadata
}
```

### RerankScore (Output Item)

```csharp
public class RerankScore
{
    public RerankItem Item { get; }                 // Original item
    public float Score { get; }                     // Relevance score [0.0, 1.0]
    public int Rank { get; }                        // 1-based rank (1 = highest)
    public string? Explanation { get; }             // Optional reasoning (Claude backend)
}
```

### RerankResult (Output)

```csharp
public class RerankResult
{
    public IReadOnlyList<RerankScore> Scores { get; }             // Sorted by score (highest first)
    public int TotalItems { get; }                                // Total reranked
    public string Query { get; }                                  // Query used for reranking
    public string BackendName { get; }                            // Backend that produced result
    public long ElapsedMilliseconds { get; }                      // Time taken
    public Dictionary<string, string>? Diagnostics { get; }       // Diagnostics info
}
```

### RerankOptions (Configuration)

```csharp
public class RerankOptions
{
    public int? TopK { get; set; }                          // Return top-k only
    public float? MinScore { get; set; }                    // Filter by threshold
    public int? MaxItems { get; set; }                      // Maximum items to process
    public int? TimeoutMs { get; set; }                     // Request timeout
    public bool IncludeExplanation { get; set; } = false;  // Include per-item explanations
    public Dictionary<string, string>? CustomOptions { get; set; } // Backend-specific options
}
```

## When to Use Each Backend

### Choose BGE (ONNX) if you need:
- ✅ Fast local reranking (<100ms)
- ✅ Offline operation (no API key)
- ✅ Lower cost (free inference)
- ✅ Privacy (data stays local)

### Choose Claude API if you need:
- ✅ High precision (98%+ R@5)
- ✅ Complex semantic reasoning
- ✅ Explanations for rankings
- ✅ Handling complex queries

### Choose Custom if you need:
- ✅ Proprietary models
- ✅ Ensemble reranking
- ✅ Domain-specific scoring

## Common Use Cases

**Search Result Reranking** — Improve BM25 or Elasticsearch rankings
```csharp
var search = await elasticsearch.SearchAsync(query);
var items = search.Documents.Select(document => new RerankItem(document?.ToString() ?? string.Empty)).ToArray();
var reranked = await reranker.RerankAsync(query, items);
```

**RAG Pipeline Enhancement** — Improve retrieval quality for LLM context
```csharp
var retrieved = vectorDb.Search(query, k: 50);  // Get many candidates
var items = retrieved.Select(document => new RerankItem(document?.ToString() ?? string.Empty)).ToArray();
var refined = await reranker.RerankAsync(query, items, new RerankOptions { TopK = 5 });
var context = refined.Scores.Select(s => s.Item.Text);
```

**Content Ranking** — Reorder recommendations by query relevance
```csharp
var candidates = await db.GetCandidates();
var items = candidates.Select(candidate => new RerankItem(candidate?.ToString() ?? string.Empty)).ToArray();
var ranked = await reranker.RerankAsync(userQuery, items);
```

## Error Handling

```csharp
var items = new[]
{
    new RerankItem("Machine learning is a subset of artificial intelligence."),
};

try
{
    var result = await reranker.RerankAsync(query, items);
}
catch (ArgumentException ex)
{
    // Input validation error (empty query, too many documents)
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (RerankerException ex)
{
    // Backend-specific error
    Console.WriteLine($"Reranker failed: {ex.ErrorCode} - {ex.Message}");
}
```

## Contributing

We welcome contributions! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License — see [LICENSE](LICENSE) for details.

## Author

**ElBruno** — AI/ML engineer passionate about semantic search and .NET

- 🌐 [elbruno.com](https://elbruno.com)
- 🐦 [@elbruno](https://twitter.com/elbruno)
- 💼 [LinkedIn](https://linkedin.com/in/elbruno)

## Acknowledgments

- **BGE Model** — [BAAI](https://huggingface.co/BAAI/bge-reranker-base)
- **Claude API** — [Anthropic](https://anthropic.com)
- **Ollama** — [ollama.ai](https://ollama.ai)
