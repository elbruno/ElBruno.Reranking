# Quickstart Guide

Get reranking working in 5 minutes with step-by-step examples for each backend.

## Prerequisites

- .NET 6.0 or later
- Visual Studio, VS Code, or Rider

## Step 1: Install Package

```bash
dotnet add package ElBruno.Reranking
```

## Step 2: Choose Your Backend

### Option A: ONNX Backend (Local, Fast)

#### Download the BGE Model

First, download the BGE-Reranker model file:

```bash
# Linux/Mac
wget https://huggingface.co/BAAI/bge-reranker-base/resolve/main/onnx/model.onnx -O ./models/bge-reranker-base.onnx

# Windows (PowerShell)
Invoke-WebRequest -Uri "https://huggingface.co/BAAI/bge-reranker-base/resolve/main/onnx/model.onnx" -OutFile "./models/bge-reranker-base.onnx"
```

Or use HuggingFace CLI:
```bash
huggingface-cli download BAAI/bge-reranker-base --include "onnx/model.onnx" --local-dir ./models
```

#### Create Your Application

```csharp
using ElBruno.Reranking;

var documents = new[]
{
    "Machine learning enables computers to learn from data.",
    "Deep learning uses artificial neural networks.",
    "The weather today is rainy.",
    "Natural language processing processes text.",
};

var reranker = new OnnxReranker("./models/bge-reranker-base.onnx");

var result = await reranker.RerankAsync(
    query: "What is machine learning?",
    documents: documents
);

Console.WriteLine($"Backend: {reranker.Name}");
Console.WriteLine($"Total documents: {result.TotalDocuments}");
Console.WriteLine($"Top result: {result.RankedDocuments[0].Text}");
Console.WriteLine($"Score: {result.RankedDocuments[0].Score:F3}");
```

**Run it:**
```bash
dotnet run
```

**Expected output:**
```
Backend: bge-reranker-base
Total documents: 4
Top result: Machine learning enables computers to learn from data.
Score: 0.918
```

### Option B: Claude Backend (Cloud, High-Precision)

#### Set up API Key

Get your API key from [Anthropic Console](https://console.anthropic.com):

```bash
export ANTHROPIC_API_KEY=sk-ant-...  # Linux/Mac
$env:ANTHROPIC_API_KEY="sk-ant-..."  # Windows PowerShell
```

#### Create Your Application

```csharp
using ElBruno.Reranking;

var documents = new[]
{
    "Paris is the capital of France.",
    "The Eiffel Tower is located in Paris.",
    "Rome is the capital of Italy.",
};

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY not set");

var reranker = new ClaudeReranker(apiKey);

var result = await reranker.RerankAsync(
    query: "What is the capital of France?",
    documents: documents,
    options: new RerankOptions { TopK = 2 }
);

Console.WriteLine($"Backend: {reranker.Name}");
foreach (var doc in result.RankedDocuments)
{
    Console.WriteLine($"Score: {doc.Score:F3}, Text: {doc.Text}");
}
```

**Run it:**
```bash
dotnet run
```

**Expected output:**
```
Backend: claude-3-opus
Score: 0.95, Text: Paris is the capital of France.
Score: 0.87, Text: The Eiffel Tower is located in Paris.
```

## Step 3: Understand the Results

Each reranked document includes:

| Field | Type | Description |
|-------|------|-------------|
| `Text` | string | Original document text |
| `Score` | double | Relevance score [0.0–1.0] |
| `Rank` | int | Position in ranked list (1-based) |

```csharp
foreach (var doc in result.RankedDocuments)
{
    Console.WriteLine($"Rank: {doc.Rank}");          // 1, 2, 3, ...
    Console.WriteLine($"Score: {doc.Score}");        // 0.0–1.0
    Console.WriteLine($"Text: {doc.Text}");          // Content
}
```

## Step 4: Add Configuration

Use `RerankOptions` to customize behavior:

```csharp
var options = new RerankOptions
{
    TopK = 5,                   // Return only top 5
    MinScore = 0.7,             // Filter scores < 0.7
    TimeoutMs = 5000,           // 5 second timeout
    EnableRetry = true,         // Retry transient errors
    MaxRetries = 2              // Max 2 retry attempts
};

var result = await reranker.RerankAsync(query, documents, options);
```

## Step 5: Error Handling

```csharp
try
{
    var result = await reranker.RerankAsync(query, documents);
}
catch (ArgumentException ex)
{
    // Input validation error
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (Exception ex)
{
    // Backend error
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Troubleshooting

### ONNX: "Model file not found"
- Ensure the model path is correct
- Check that the file exists: `ls ./models/bge-reranker-base.onnx`

### Claude: "API key invalid"
- Check environment variable: `echo $ANTHROPIC_API_KEY`
- Verify key format (starts with `sk-ant-`)
- Ensure key has correct permissions in Anthropic console

### "Query is empty"
- Ensure query is not null or whitespace
- Check documents array is not empty

### Timeout errors
- Increase `TimeoutMs` in `RerankOptions`
- Check network connectivity (Claude backend)
- Check ONNX model file size is reasonable

## Next Steps

- Read [ONNX Backend Guide](onnx-backend.md) for production deployment
- Read [Claude Backend Guide](claude-backend.md) for API best practices
- Read [Performance Tuning](performance-tuning.md) for optimization
- Read [Custom Reranker Guide](custom-reranker.md) to build your own backend
