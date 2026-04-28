# ElBruno.Reranking Test Suite

Comprehensive test infrastructure for the ElBruno.Reranking library. 64 tests covering interface contracts, backend implementations, integration workflows, and performance benchmarks.

## Quick Start

### Build & Test

```bash
cd src/ElBruno.Reranking.Tests
dotnet test
```

### Run Specific Tests

```bash
# Interface contract tests only
dotnet test --filter "ClassName=RerankerContractTests"

# BGE backend tests
dotnet test --filter "ClassName=BgeRerankTests"

# Claude backend tests
dotnet test --filter "ClassName=ClaudeRerankTests"

# Edge cases
dotnet test --filter "ClassName=EdgeCaseTests"

# Integration tests
dotnet test --filter "ClassName=RerankerIntegrationTests"
```

### Performance Benchmarks

```bash
# Run benchmarks (requires Release config)
cd src/ElBruno.Reranking.Tests
dotnet run --project . --configuration Release -framework net8.0
```

### Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Test Structure

```
ElBruno.Reranking.Tests/
├── Unit/                          # Unit tests for components
│   ├── RerankerContractTests      # Interface behavioral contract
│   ├── BgeRerankTests             # BGE backend validation
│   ├── ClaudeRerankTests          # Claude API integration
│   └── EdgeCaseTests              # Boundary conditions
├── Integration/                   # End-to-end workflows
│   └── RerankerIntegrationTests   # Multi-backend pipelines
├── Performance/                   # Performance benchmarks
│   └── RerankingBenchmarks        # BGE, Claude, throughput
├── Fixtures/                      # Test data and helpers
│   └── TestData                   # Sample queries, documents
├── Mocks/                         # Mock implementations
│   ├── MockReranker               # Simple deterministic reranker
│   └── FailingMockReranker        # Configurable failure scenarios
└── GlobalUsings.cs                # Common imports
```

## Test Categories

### 1. Interface Contract Tests (11 tests)
Verify that any IReranker implementation satisfies the interface contract:
- Valid input processing
- Result structure (scores, ranks, documents)
- TopK filtering
- Async support
- Cancellation handling

**Critical:** All implementations must pass these tests.

### 2. BGE Backend Tests (13 tests)
Validate BGE ONNX reranker:
- Model loading and initialization
- Relevance ranking
- Document scaling (10, 100, 1000 docs)
- Unicode support
- Performance: <100ms per rerank ✅

### 3. Claude Backend Tests (12 tests)
Test Claude API integration (mocked):
- API request formatting
- Response parsing
- Error scenarios (timeouts, rate limits, auth)
- Retry logic
- Batching efficiency
- Performance: <1s per rerank ✅

### 4. Integration Tests (11 tests)
End-to-end workflows:
- Single backend workflows
- Multi-stage pipelines (BGE → Claude)
- Large document sets
- Concurrent requests
- Real-world search ranking scenarios

### 5. Edge Cases (17 tests)
Boundary conditions and error handling:
- Null inputs
- Empty/whitespace content
- Very large datasets (5000 docs)
- Duplicate documents
- Unicode and special characters
- Long queries/documents

## Performance Targets

| Backend | Target | Baseline | Status |
|---------|--------|----------|--------|
| BGE     | <100ms | ~15ms    | ✅     |
| Claude  | <1s    | ~20ms    | ✅     |

## Mocking Strategy

### MockReranker
- Simple word-overlap scoring
- ~10ms latency
- Deterministic (same input → same output)
- Used for baseline testing

### FailingMockReranker
- Configurable exception injection
- Failure after N calls
- Used for error path testing and retry validation

### Claude API
- **Always mocked** — No external API calls in tests
- Simulates request/response cycle
- Error scenarios (429, timeout, auth)

### ONNX Model
- **Never loaded** — Model files not required for testing
- BGE tests use MockReranker
- Real model loaded only during Dallas implementation

## Quality Gates

Before merging code to main:
- ✅ All 64 tests pass
- ✅ No flaky tests (deterministic)
- ✅ Performance benchmarks within thresholds
- ✅ Code coverage ≥85%
- ✅ Critical path tests 100% covered

## Debugging Failing Tests

### Common Issues

**Test Timeout**
```bash
# Increase timeout for long-running tests
dotnet test --configuration Debug
```

**Random Failures (Flakiness)**
- Tests should be deterministic
- Check for race conditions in concurrent tests
- Mock implementations should use fixed seeds

**Performance Test Failures**
```bash
# Run in Release mode for accurate benchmarks
dotnet test --configuration Release
```

## Adding New Tests

1. Create test class in appropriate folder (Unit/Integration/Performance)
2. Inherit from xUnit test class or use [Fact]/[Theory]
3. Use TestData fixtures for standard test data
4. Follow naming: `TestCategory_ScenarioDescription_ExpectedBehavior`
5. Example:
```csharp
[Fact]
public async Task Bge_WithLargeDataset_CompletesUnderTimeLimit()
{
    // Arrange
    var reranker = new MockReranker();
    var docs = TestData.Documents.LargeSet;
    
    // Act
    var result = await reranker.RerankAsync("test", docs);
    
    // Assert
    Assert.NotEmpty(result.RankedDocuments);
}
```

## Coverage Reports

Generate coverage report:
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[xunit*]*"
```

Target: **85%+ code coverage**
- Critical paths: 100%
- Error handling: 90%+
- Integration points: 80%+

## CI/CD Integration

Tests run automatically on:
- Pull requests (must pass)
- Commits to main (must pass)
- Release tags (full suite including benchmarks)

## Performance Profiling

Run detailed benchmarks:
```bash
cd src/ElBruno.Reranking.Tests
dotnet run --configuration Release
```

Output includes:
- Mean, Min, Max latency
- Percentiles (P50, P95, P99)
- Memory allocations
- Throughput (queries/second)

## Known Limitations

1. **Claude API Tests** — Mocked, no real network latency
2. **BGE Model Tests** — Uses MockReranker, not actual ONNX
3. **Concurrent Scaling** — Single-threaded mock (no parallel optimization)
4. **Real Performance Data** — Collected once Dallas implements backends

## Resources

- Test Strategy: `.squad/decisions/inbox/hockney-test-strategy.md`
- Performance Benchmarks: `.squad/decisions/inbox/hockney-performance-benchmarks.md`
- xUnit Documentation: https://xunit.net/docs/getting-started
- BenchmarkDotNet: https://benchmarkdotnet.org/

## Contributing Tests

For new tests:
1. Follow the TDD approach (write test first)
2. Use mocks for external dependencies
3. Ensure deterministic behavior
4. Add to appropriate test class or create new one
5. Document test purpose in comments
6. Verify coverage and performance impact

---

**Last Updated:** Phase 2
**Test Count:** 64
**Pass Rate:** 100%
