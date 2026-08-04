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

var items = new[]
{
    new RerankItem("Machine learning enables computers to learn from data."),
    new RerankItem("Deep learning uses artificial neural networks."),
    new RerankItem("The weather today is rainy."),
    new RerankItem("Natural language processing processes text."),
};

var reranker = new OnnxReranker("./models/bge-reranker-base.onnx");

var result = await reranker.RerankAsync(
    query: "What is machine learning?",
    items: items
);

Console.WriteLine($"Backend: {reranker.Name}");
Console.WriteLine($"Total items: {result.TotalItems}");
Console.WriteLine($"Top result: {result.Scores[0].Item.Text}");
Console.WriteLine($"Score: {result.Scores[0].Score:F3}");
```

**Run it:**
```bash
dotnet run
```

**Expected output:**
```
Backend: bge-reranker-base
Total items: 4
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

var items = new[]
{
    new RerankItem("Paris is the capital of France."),
    new RerankItem("The Eiffel Tower is located in Paris."),
    new RerankItem("Rome is the capital of Italy."),
};

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY not set");

var reranker = new ClaudeReranker(apiKey);

var result = await reranker.RerankAsync(
    query: "What is the capital of France?",
    items: items,
    options: new RerankOptions { TopK = 2, IncludeExplanation = true }
);

Console.WriteLine($"Backend: {reranker.Name}");
foreach (var score in result.Scores)
{
    Console.WriteLine($"Score: {score.Score:F3}, Text: {score.Item.Text}");
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

Each reranked item includes:

| Field | Type | Description |
|-------|------|-------------|
| `Item.Text` | string | Original item text |
| `Score` | double | Relevance score [0.0–1.0] |
| `Rank` | int | Position in ranked list (1-based) |
| `Explanation` | string? | Optional explanation when enabled |

```csharp
foreach (var score in result.Scores)
{
    Console.WriteLine($"Rank: {score.Rank}");              // 1, 2, 3, ...
    Console.WriteLine($"Score: {score.Score}");            // 0.0–1.0
    Console.WriteLine($"Text: {score.Item.Text}");         // Content
    Console.WriteLine($"Explanation: {score.Explanation}"); // Optional
}
```

## Step 4: Add Configuration

Use `RerankOptions` to customize behavior:

```csharp
var options = new RerankOptions
{
    TopK = 5,                   // Return only top 5
    MinScore = 0.7f,            // Filter scores < 0.7
    MaxItems = 100,             // Process up to 100 items
    TimeoutMs = 5000,           // 5 second timeout
    IncludeExplanation = true,  // Include explanations when available
    CustomOptions = new Dictionary<string, string>
    {
        ["batch_size"] = "32"
    }
};

var result = await reranker.RerankAsync(query, items, options);
```

## Step 5: Error Handling

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
- Check items array is not empty

### Timeout errors
- Increase `TimeoutMs` in `RerankOptions`
- Check network connectivity (Claude backend)
- Check ONNX model file size is reasonable

## Next Steps

- Read [ONNX Backend Guide](onnx-backend.md) for production deployment
- Read [Claude Backend Guide](claude-backend.md) for API best practices
- Read [Performance Tuning](performance-tuning.md) for optimization
- Read [Custom Reranker Guide](custom-reranker.md) to build your own backend
