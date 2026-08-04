# Blazor components

This repository includes the `ElBruno.Reranking.BlazorComponents` package, a deterministic Blazor sample, and docs for the reranking UI surface from issue #3.

## Goals

- Show how reranking results can be presented in Blazor
- Keep the sample predictable for docs and screenshots
- Reuse the current `ElBruno.Reranking` result models

## Component map

| Component | Purpose | Demo page |
| --- | --- | --- |
| `BackendSelector` | Switch between ONNX, Claude, and Ollama | `/backend-selector` |
| `RerankResultList` | Show ordered candidates, score badges, and deltas | `/rerank-result-list` |
| `ScoreHeatmap` | Visualize score spread with a blue gradient | `/score-heatmap` |
| `RerankPlayground` | Edit the query/candidates and export JSON | `/rerank-playground` |

## Sample support components

- `CodeSample` renders a reusable code block card
- `CodeSnippets` renders small supporting notes and parameters

The sample uses Bootstrap 5.3.3, deterministic candidate data, and a deterministic mock reranker so the output stays stable.

## Proposed package usage

```razor
<BackendSelector OnBackendChanged="@HandleBackend" ShowLatencyHint="true" />
<RerankResultList Results="@results" ShowOriginalRank="true" ShowScoreBadge="true" />
<ScoreHeatmap Results="@results" SortByScore="true" ShowLabels="true" />
<RerankPlayground Reranker="@reranker" DefaultQuery="What is semantic search?" />
```

```csharp
builder.Services.AddRerankingBlazorComponents();
```

## Determinism

The sample avoids remote calls, random data, and time-dependent visuals. That keeps the screenshots repeatable and makes the rendered output safe to reference in docs.

## Related files

- [Package README](../src/ElBruno.Reranking.BlazorComponents/README.md)
- [Sample README](../src/samples/BlazorRerankingDemo/README.md)
- [Root README](../README.md)
- [Issue #3](https://github.com/elbruno/ElBruno.Reranking/issues/3)
