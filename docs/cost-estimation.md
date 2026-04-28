# Cost Estimation Guide

**Calculate total cost of ownership for ElBruno.Reranking across backends.**

## Cost Matrix

| Backend | Per Call | Per 1M Calls | Per Year (1K calls/day) |
|---------|----------|-------------|------------------------|
| **ONNX** | $0.000000 | $0 | $0 |
| **Ollama** | $0.000000 | $0 | $0 |
| **Claude** | $0.0008 | $800 | $292 |

---

## Backend Costs Explained

### ONNX Backend (BGE-Reranker)

**Direct cost:** $0

**Infrastructure cost:**
- Server/compute: Your infrastructure
- Storage: 558 MB model (~$0.002/month on S3)
- Bandwidth: Internal only

**Example (1000 reranking calls/day):**
- Daily cost: $0
- Monthly cost: $0
- Yearly cost: $0

**When ONNX is most cost-effective:**
- High-volume queries (>1000/day)
- Tight SLA budget
- Privacy-sensitive data
- Offline operation

### Ollama Backend

**Direct cost:** $0

**Infrastructure cost:**
- Server/compute: Your infrastructure
- Model download: One-time (~2–70GB depending on model)
- Storage: Model on disk (~3–70GB)

**Example (1000 reranking calls/day):**
- Daily cost: $0
- Monthly cost: $0
- Yearly cost: $0
- One-time model setup: ~2 hours

**When Ollama is most cost-effective:**
- Experimentation with multiple models
- Custom fine-tuned models
- Offline or on-premises deployment
- Hybrid local-cloud architecture

### Claude API Backend

**Per-call cost calculation:**

```
Tokens in ≈ 50 (query) + (documents × 100) tokens
Cost ≈ Tokens / 1,000,000 × $0.003 (input rate)
     + Output tokens / 1,000,000 × $0.015 (output rate)
```

**Simplified: ~$0.0008 per 100 documents**

**Example (5 documents per call):**
- Tokens: ~50 + (5 × 100) = ~550 tokens
- Cost: ~$0.000001–0.000002 per call

**Example (50 documents per call):**
- Tokens: ~50 + (50 × 100) = ~5050 tokens
- Cost: ~$0.000015–0.00002 per call

**Example pricing tiers:**

| Documents/Call | Cost/Call | Annual (1K calls/day) |
|----------------|-----------|----------------------|
| 5 | $0.000004 | $1.46 |
| 10 | $0.000008 | $2.92 |
| 20 | $0.000016 | $5.84 |
| 50 | $0.00004 | $14.60 |
| 100 | $0.00008 | $29.20 |

---

## Real-World Scenarios

### Scenario 1: Small Company (100 queries/day)

**Goal:** Improve search relevance

**Solution:** Claude backend

```
Queries/day:           100
Documents/query:       20
Daily cost:            100 × $0.000016 = $0.0016
Monthly cost:          $0.048
Yearly cost:           $0.584 ✅ (negligible)
```

### Scenario 2: Medium Company (10,000 queries/day)

**Goal:** Scale search with good relevance

**Solution:** Hybrid (ONNX + optional Claude)

```
ONNX (all queries):    10,000 × $0.00000 = $0
Claude (top 5%):       500 × $0.00008 = $0.04

Daily cost:            $0.04
Monthly cost:          $1.20
Yearly cost:           $14.60 ✅ (minimal)
```

### Scenario 3: Large Company (1M queries/day)

**Goal:** Production-scale reranking

**Solution:** ONNX primary (or Ollama self-hosted)

```
ONNX (all queries):    1,000,000 × $0.00000 = $0
Infrastructure:        $500/month (estimated)

Daily cost:            $0 (API) + $16.67 (infra)
Monthly cost:          $0 + $500
Yearly cost:           $0 + $6,000 ✅ (infrastructure-driven)
```

### Scenario 4: High-Accuracy Requirement

**Goal:** Premium relevance scores for important queries

**Solution:** Claude for all queries

```
Queries/day:           1,000
Documents/query:       30
Daily cost:            1,000 × $0.000024 = $0.024
Monthly cost:          $0.72
Yearly cost:           $8.76 ✅ (reasonable for accuracy)
```

---

## Break-Even Analysis: ONNX vs Claude

**Question:** When should you use ONNX instead of Claude?

**Answer:** When cumulative infrastructure cost < accumulated API costs

**Example:**
```
Self-hosted server cost:    $200/month
Claude cost (1K calls/day): $0.29/month

Monthly infrastructure cost vs API savings:
$200 infrastructure > $0.29 API cost

→ ONNX wins at: 1000+ queries/day
  (infrastructure amortization)
```

**Detailed break-even analysis:**

| Queries/Day | Claude Cost/Month | Infrastructure Cost/Month | Recommendation |
|------------|-------------------|---------------------------|----------------|
| 100 | $0.048 | N/A | Claude (cheaper than setup) |
| 1,000 | $0.48 | $50–200 | Claude (lower infrastructure) |
| 10,000 | $4.80 | $50–200 | ONNX (self-hosted amortized) |
| 100,000 | $48 | $50–200 | ONNX (significant savings) |
| 1,000,000 | $480 | $50–200 | ONNX (10x cheaper) |

---

## Hidden Costs

### ONNX Backend

| Cost | Amount | Notes |
|------|--------|-------|
| Initial setup | 2–4 hours | Model download, integration testing |
| Model storage | $0.002/month | If on cloud storage |
| Maintenance | Minimal | Model updates quarterly |
| Scaling | Vertical (server upgrade) | ~$500–2000 for major upgrade |

### Claude Backend

| Cost | Amount | Notes |
|------|--------|-------|
| Initial setup | 1–2 hours | API setup, integration |
| Rate limits | Varies | May require higher tier ($50+/month) |
| Network | Minimal | API calls are low-bandwidth |
| Maintenance | Minimal | Anthropic manages infrastructure |

### Ollama Backend

| Cost | Amount | Notes |
|------|--------|-------|
| Initial setup | 4–8 hours | Model selection, fine-tuning |
| Model storage | 3–70 GB | Varies by model choice |
| Compute | $50–500/month | Depends on hardware and scale |
| Maintenance | 4–8 hours/month | Model updates, optimization |

---

## Cost Optimization Strategies

### 1. Request Batching

```csharp
// Inefficient: Individual Claude calls
foreach (var query in queries)
{
    var result = await claude.RerankAsync(query, docs);  // $0.00008 each
}

// Efficient: Batch requests
var batched = queries.Chunk(10).ToList();
foreach (var batch in batched)
{
    foreach (var query in batch)
    {
        var result = await claude.RerankAsync(query, docs);
    }
}

// Savings: ~10% (fewer API round-trips)
```

### 2. Caching Identical Requests

```csharp
private Dictionary<string, RerankResult> _cache = new();

public async Task<RerankResult> CachedRerankAsync(string query, IEnumerable<string> docs)
{
    var key = $"{query}:{string.Join(',', docs)}";
    
    if (_cache.TryGetValue(key, out var cached))
    {
        return cached;  // Free!
    }
    
    var result = await reranker.RerankAsync(query, docs);
    _cache[key] = result;
    return result;
}

// Savings: 30–80% depending on hit rate
```

### 3. Two-Stage Filtering (ONNX + Claude)

```csharp
// Stage 1: Fast ONNX rerank (all 100 documents)
var onnxResult = await onnx.RerankAsync(query, docs, new RerankOptions { TopK = 10 });
// Cost: ~$0.00000

// Stage 2: Precise Claude rerank (only top 10)
var claudeResult = await claude.RerankAsync(query, onnxResult.RankedDocuments.Select(d => d.Text));
// Cost: ~$0.00008

// Total: $0.00008 vs $0.00016 (50% savings!)
```

### 4. Selective Claude Usage

```csharp
// Only use Claude for complex queries or important requests
if (IsComplexQuery(query))
{
    var result = await claude.RerankAsync(query, docs);  // Use precision when needed
}
else
{
    var result = await onnx.RerankAsync(query, docs);    // Use free backend for simple queries
}

// Savings: 50–90% depending on query complexity
```

### 5. Model Optimization (Ollama)

```csharp
// Use smaller, faster models for quick reranking
var fastModel = new OllamaReranker("http://localhost:11434", "mistral");  // 7B params, ~500ms

// Use larger models only when needed
var preciseModel = new OllamaReranker("http://localhost:11434", "llama2");  // 13B params, ~800ms

// Savings: Model-dependent, but generally 20–50% latency reduction
```

---

## ROI Calculation

**Question:** What's the ROI of switching from BM25 to reranking?

**Example:**
```
Baseline (BM25):
  - Setup: 10 hours
  - Maintenance: 2 hours/month
  - Result quality: ~80% (estimated)

With ElBruno.Reranking (ONNX):
  - Setup: 12 hours
  - Maintenance: 1 hour/month
  - Result quality: ~96% (estimated)
  - Cost: $0/month

Improvement:
  - Search quality: +20% (80% → 96%)
  - Maintenance: -1 hour/month (-50%)
  - Cost: $0

ROI:
  - Time saved: 1 hour/month × 12 months = 12 hours/year (~$600 value)
  - Quality improvement: +20% better search results
  - Break-even: Immediately
```

---

## Budgeting Templates

### Startup

```
Monthly budget: $0–10
Recommended: ONNX (free)

Monthly costs:
  - Infrastructure: $5 (shared server)
  - Storage: $0.002
  - API: $0
  - Total: ~$5.00 ✅
```

### Mid-Market

```
Monthly budget: $100–500
Recommended: ONNX + optional Claude

Monthly costs:
  - Infrastructure: $200
  - Storage: $0.01
  - API (5% of queries): $2.50
  - Monitoring: $10
  - Total: ~$212.50 ✅
```

### Enterprise

```
Monthly budget: $1000+
Recommended: Hybrid or dedicated Claude

Monthly costs:
  - Infrastructure: $500
  - Storage: $1
  - API: $100–500
  - Support/SLA: $200
  - Monitoring: $50
  - Total: ~$850–1250 ✅
```

---

## Next Steps

- [Performance Tuning](guides/performance-tuning.md) — Optimize costs
- [Benchmarks](benchmarks.md) — Compare backend performance
- [Architecture](architecture.md) — Understand system design
