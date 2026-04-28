# Keaton — Session History

## Project Overview
**ElBruno.Reranking** — A .NET library for semantic reranking of search results.

**Tech Stack:**
- C# / .NET 8+
- ONNX Runtime for BGE-Reranker-base (278M params, runs on CPU)
- Optional Claude API backend for high-precision reranking
- Optional Ollama backend for local LLMs (v1.1)
- xUnit for testing
- NuGet for packaging

**Goals:**
- v0.5.0: MVP with BGE and Claude backends, core tests, documentation
- v1.0: Production release, NuGet publish
- v1.1: Ollama backend support

**Key Requirements from PRD:**
- IReranker interface (extensible backend system)
- RerankResult and RerankOptions models
- Unit + integration tests
- Comprehensive documentation
- Performance benchmarks
- NuGet publishing workflow (copy from ElBruno.LocalLLMs)
- Promotion assets (blog, social, images)
- README with badges

## Learnings

### Architecture Phase (Phase 1 — Session 1)

#### Core Design Decisions
1. **IReranker Interface:** Single async method (`RerankAsync`), nullable options for per-call config.
   - Rationale: Simplicity + flexibility. Async/await required for .NET concurrency.
   - CancellationToken built-in for lifecycle support.

2. **Backend Isolation:** Each backend (ONNX, Claude, Ollama) in separate namespace.
   - Rationale: Clear separation, easier testing, future extensibility.
   - All backends inherit from IReranker, shared utils in `Utils/` namespace.

3. **Core Models:** RerankItem (input), RerankResult (output), RerankOptions (config), RerankScore (scored item).
   - All properties optional except Text (RerankItem).
   - Results immutable (`IReadOnlyList<RerankScore>`).
   - Options nullable for backend defaults.

4. **Error Handling:** Single `RerankerException` base class, backend-specific error codes.
   - Rationale: Unified error handling. ArgumentException for input validation.

#### Performance Assumptions
1. **ONNX (BGE):** <100ms target, max ~10k items per call.
   - Model: 278M params, CPU inference expected.
   - Lazy-load model once, reuse across calls (singleton session).
   - Batching amortizes tokenization cost.

2. **Claude:** <1s with retries, max ~500 items per call (token limits).
   - Network latency: 200–500ms typical.
   - Retry strategy: Exponential backoff (1s, 2s, 4s) for transient errors (429, 503, 504).
   - Cost: ~$0.0008 per call (50 items).

3. **Ollama:** 200ms–5s (model-dependent), no hard limit on items.
   - Model-specific latency.
   - Health check before first call.
   - Service must be running locally.

#### Integration Points for Dallas (Implementations)
1. **ONNX Backend:**
   - Implement `OnnxReranker.RerankAsync()` with lazy model loading.
   - Implement `BgeTokenizer` (use ONNX Runtime tokenizer).
   - Use `ScoreNormalizer.FromLogit()` for normalization.
   - Validate: max items <= 10k, query + items <= 512 tokens each.

2. **Claude Backend:**
   - Implement `ClaudeReranker.RerankAsync()` with API client.
   - Build prompt in `ClaudePromptBuilder` (JSON structured format preferred).
   - Implement retry logic in `ClaudeApiClient` (exponential backoff).
   - Parse response: extract scores [0.0, 1.0], optional explanations.
   - Validate: max items <= 500 (token safety margin).

3. **Ollama Backend:**
   - Implement `OllamaReranker.RerankAsync()` with service health check.
   - Build prompt in `OllamaPromptBuilder` (simple text format).
   - Call Ollama API: `POST /api/generate`.
   - Parse response: extract scores (normalize to [0.0, 1.0]).

#### Integration Points for Hockney (Testing)
1. **Unit Tests:**
   - Score normalization (sigmoid, probability, ordinal).
   - Result sorting (descending by score).
   - Input validation (nulls, empty, oversized).
   - Options validation (TopK >= 1, MinScore in [0, 1], etc.).

2. **Integration Tests:**
   - ONNX: Model loading, inference accuracy (compare against reference scores).
   - Claude: Mock API client, test retry logic, response parsing.
   - Ollama: Mock HTTP responses, health check behavior.

3. **Performance Tests:**
   - ONNX: Benchmark <100ms target on CI hardware.
   - All: Measure ElapsedMilliseconds, memory usage (via diagnostics).

#### Documentation Alignment (McManus Reference)
- **Quickstart.md:** Show all 3 backends (5-10 min read).
- **onnx-backend.md:** Model download, local setup, performance tuning.
- **claude-backend.md:** API key setup, pricing, best practices.
- **ollama-backend.md:** Service installation, model selection.
- **architecture.md:** System design, data flow, extensibility (from keaton-architecture-design.md).

#### Key Assumptions to Validate
1. **ONNX Model Availability:** BGE-Reranker-base model is freely available (HuggingFace ONNX format).
   - If not, may need to convert from PyTorch → ONNX ourselves (non-blocking for v0.5.0).

2. **Claude API Rate Limits:** Assume ~100 calls/min for typical plan. Users must handle rate limits.
   - Retry logic mitigates transient 429s.

3. **Ollama Service Model:** Assume Ollama can be installed locally and models pulled via CLI.
   - Ollama service should be resilient to concurrent requests (our code doesn't handle service restarts).

4. **Token Counting:** Use approximate tokenization (char count / 4 rule-of-thumb) until official tokenizer available.
   - Rationale: Simple, works for length estimation. Exact counts less critical for v0.5.0.

#### Future Extensibility (v1.1+)
- Adding new backends: Implement IReranker, add enum value, tests, docs.
- Users can extend: Ensemble rerankers, caching layer, monitoring.
- No breaking changes expected to core interface (stable by design).

#### What Dallas Must NOT Do (Architecture Constraints)
1. **No synchronous methods:** All I/O must be async.
2. **No external HTTP client library:** Use System.Net.Http only.
3. **No JSON library beyond System.Text.Json.**
4. **No dependency injection framework:** Manual instantiation for v0.5.0.
5. **No model files in repo:** Download or document setup separately.
6. **No blocking calls** in async methods (no `.Wait()`, `.Result`, `.GetAwaiter().GetResult()`).

#### Architecture Review Checklist (Before Approval)
- [ ] All backends implement IReranker correctly.
- [ ] Error handling: All failures throw RerankerException with ErrorCode.
- [ ] Async/await: No synchronous I/O, CancellationToken respected.
- [ ] Performance: ONNX <100ms, Claude <1s on CI.
- [ ] Input validation: Null checks, size limits, option constraints.
- [ ] Tests: Unit + integration coverage > 80%.
- [ ] Documentation: All guides updated, examples working.
