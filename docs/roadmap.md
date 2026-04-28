# Roadmap

**ElBruno.Reranking development roadmap and vision.**

## Current Version: v0.5.0 (MVP)

**Status:** Beta  
**Release Date:** Q1 2025

### Completed Features ✅

- [x] Core IReranker interface
- [x] ONNX backend (BGE-Reranker-base)
- [x] Claude API backend
- [x] Ollama backend (preview)
- [x] Comprehensive documentation
- [x] Performance benchmarks
- [x] Error handling and retries
- [x] Unit tests and CI/CD
- [x] Quick start guide

### Known Limitations

- GPU support not yet implemented (CPU only for ONNX)
- Caching layer is optional (not built-in)
- Limited to string documents (no rich metadata)
- No streaming/batch APIs
- Community backends not yet accepted

---

## v1.0 (Q3 2025)

**Focus:** Production-ready, stable API, expanded backends

### New Features

- [ ] **GPU acceleration** — 10–20x speedup for ONNX on CUDA/ROCm
- [ ] **Caching layer** — Semantic cache for embeddings (50–80% hit rate)
- [ ] **Jina Reranker backend** — Alternative cross-encoder model
- [ ] **API reference (DocFX)** — Automated API documentation
- [ ] **Batch processing utilities** — Helper methods for large-scale reranking
- [ ] **Structured logging** — Built-in diagnostics and monitoring
- [ ] **Metrics export** — Prometheus/Application Insights integration
- [ ] **Rate limiting** — Built-in throttling for API backends
- [ ] **Advanced retry policies** — Configurable exponential backoff

### Quality Gates

- [ ] 100% test coverage (unit + integration)
- [ ] Performance benchmarks for all backends
- [ ] Memory profiling and optimization
- [ ] Security audit (dependencies, secrets)
- [ ] Documentation 100% complete
- [ ] Production deployment guide
- [ ] SLA targets defined and met

### Tentative API Changes

None planned (v1.0 will be API-stable)

---

## v1.1 (Q4 2025)

**Focus:** Extensibility, ensemble methods, advanced features

### New Features

- [ ] **Ensemble reranker** — Combine multiple backends, averaging scores
- [ ] **Custom metric backends** — User-defined scoring functions
- [ ] **Hybrid rerankers** — Multi-stage pipelines (ONNX → Claude)
- [ ] **Community backends registry** — Curated third-party backends
- [ ] **Semantic cache** — Advanced caching with query similarity
- [ ] **Query classification** — Automatic backend selection
- [ ] **A/B testing utilities** — Compare backends on real workloads
- [ ] **Advanced async utilities** — Streaming results, long-running operations

### Potential New Backends

- [ ] **HyDE** — Hypothetical Document Embeddings (Jina)
- [ ] **Cohere Rerank API** — Another cloud option
- [ ] **Custom fine-tuned models** — Users' proprietary ONNX models
- [ ] **LLaMA-based rerankers** — Open-source LLM alternatives

---

## v2.0 (2026)

**Focus:** Scale, performance, advanced ML

### Vision

- [ ] **Distributed reranking** — Multi-node, load-balanced
- [ ] **Real-time analytics** — Latency tracking, cost monitoring
- [ ] **AutoML capabilities** — Automatic backend selection based on workload
- [ ] **Fine-tuning toolkit** — Train custom rerankers on your data
- [ ] **Knowledge distillation** — Compress Claude's knowledge to ONNX
- [ ] **Federated learning** — Privacy-preserving training
- [ ] **GraphQL API** — Alternative to REST/gRPC

---

## Proposed Feature: Semantic Caching

**Problem:** Repeated queries waste compute and money (especially Claude).

**Solution:** Cache embeddings and use semantic similarity to detect repeated queries.

**Expected impact:**
- 50–80% cache hit rate on typical workloads
- 50% API cost reduction
- 70% latency improvement on cache hits

**Timeline:** v1.0

**Example:**
```csharp
var cache = new SemanticCache(reranker);

// First call: Actual reranking
var result1 = await cache.RerankAsync("machine learning", documents);  // ~600ms

// Second call (similar query): Cache hit
var result2 = await cache.RerankAsync("ML algorithms", documents);  // ~50ms

// Cache hit rate
Console.WriteLine($"Hit rate: {cache.HitRate:P}");  // ~80%
```

---

## Proposed Feature: Ensemble Reranker

**Problem:** Single reranker may have blind spots. Different backends excel at different queries.

**Solution:** Combine multiple backends, average scores, or use voting.

**Timeline:** v1.1

**Example:**
```csharp
var ensemble = new EnsembleReranker(
    new OnnxReranker(modelPath),
    new ClaudeReranker(apiKey),
    new OllamaReranker("http://localhost:11434", "mistral")
);

var result = await ensemble.RerankAsync(query, documents);
// Scores = average of all three backends
```

---

## Proposed Feature: Query Classification

**Problem:** Different queries benefit from different backends.
- Simple factual queries → ONNX is fine
- Complex reasoning queries → Claude is better

**Solution:** Automatically classify queries and route to appropriate backend.

**Timeline:** v1.1+

**Example:**
```csharp
var router = new SmartReranker(
    simple: new OnnxReranker(modelPath),
    complex: new ClaudeReranker(apiKey)
);

var result = await router.RerankAsync(query, documents);
// Routes "What is AI?" to ONNX
// Routes "How does AI compare to human cognition?" to Claude
```

---

## Community Roadmap

### We're Looking For

- **Backend implementations** — Cohere, HyDE, custom models
- **Performance optimizations** — GPU, parallelism, caching
- **Documentation improvements** — Examples, troubleshooting guides
- **Use case studies** — Real-world applications and results
- **Performance benchmarks** — Edge cases, hardware variations

### How to Contribute

1. **Open an issue** — Suggest features or report bugs
2. **Submit a PR** — Implement new backends or features
3. **Share feedback** — Tell us how you're using ElBruno.Reranking
4. **Contribute docs** — Write guides, examples, or blog posts

See [CONTRIBUTING.md](../CONTRIBUTING.md) for details.

---

## Release Schedule

| Version | Target | Focus |
|---------|--------|-------|
| v0.5.0 | Q1 2025 | MVP, three backends |
| v1.0 | Q3 2025 | Production-ready, GPU, caching |
| v1.1 | Q4 2025 | Ensemble, extensibility |
| v2.0 | 2026 | Scale, distributed, AutoML |

---

## Versioning Policy

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR (x.0.0)** — Breaking API changes or new backends
- **MINOR (1.x.0)** — New features, backward compatible
- **PATCH (1.0.x)** — Bug fixes, performance improvements

Stable versions (1.0+) maintain backward compatibility within major versions.

---

## Backward Compatibility Guarantee

v1.0 and beyond guarantee:
- ✅ `IReranker` interface unchanged
- ✅ `RerankAsync()` method signature unchanged
- ✅ Core models (RerankItem, RerankResult, RerankOptions) backward compatible
- ✅ Existing code continues to work

---

## Long-Term Vision

**ElBruno.Reranking aims to become:**

- The de facto reranking library for .NET
- Multi-backend support for all major reranking models
- Easy-to-use for beginners, powerful for experts
- Production-ready and battle-tested
- Open-source and community-driven
- Well-documented with real-world examples

---

## Feedback & Feature Requests

Have ideas? We'd love to hear them!

- 🐙 [GitHub Issues](https://github.com/ElBruno/ElBruno.Reranking/issues)
- 💬 [Discussions](https://github.com/ElBruno/ElBruno.Reranking/discussions)
- 🐦 [@elbruno on Twitter](https://twitter.com/elbruno)
- 📧 Email: [Add contact if available]

---

## FAQ

**Q: When will v1.0 be released?**  
A: Target Q3 2025, pending feature completion and stability testing.

**Q: Will you maintain backward compatibility?**  
A: Yes, v1.0+ maintains API stability within major versions.

**Q: Can I use v0.5.0 in production?**  
A: It's beta, but production-ready with caveats. See the risk disclosure.

**Q: How often are releases?**  
A: Quarterly releases planned (v1.0, v1.1, etc.). Patches as needed.

**Q: Can you add [backend] support?**  
A: Yes! Open an issue to discuss or submit a PR.

**Q: Will ONNX models update?**  
A: Probably not for v1.0. BGE is stable and proven.
