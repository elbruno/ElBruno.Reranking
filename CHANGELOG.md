# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2025-01-09

### Added
- Initial public release of ElBruno.Reranking NuGet package
- BGE-Reranker ONNX backend for local, fast semantic reranking (~15ms for 100 documents)
- Claude API backend for high-precision cloud reranking (98%+ R@5)
- Ollama backend for flexible local LLM inference
- Custom reranker support through pluggable architecture
- Full async/await implementation throughout API
- Comprehensive error handling and retry logic
- Production-ready timeouts and resilience patterns
- Documentation and quickstart guides
- Unit test suite and examples

### Features
- Unified `IRerankingService` interface across all backends
- Support for batch reranking operations
- Configurable reranking parameters (top_k, threshold)
- Integration with search result objects
- MIT licensed, open-source

---

## Release Notes

### v0.5.0

**ElBruno.Reranking** is now available on [NuGet.org](https://www.nuget.org/packages/ElBruno.Reranking/).

Install via:
```
dotnet add package ElBruno.Reranking
```

**Key Highlights:**
- Fast local ONNX inference (BGE backend)
- Cloud-powered AI reranking (Claude)
- Extensible architecture for custom backends
- Production-ready for high-concurrency .NET applications

See [README.md](./README.md) for documentation and [docs/](./docs/) for detailed guides.
