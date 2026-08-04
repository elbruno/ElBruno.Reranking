# BlazorRerankingDemo

Deterministic Blazor sample for the `ElBruno.Reranking.BlazorComponents` UI experience.

## What it shows

- `BackendSelector` — backend switching with stable latency hints
- `RerankResultList` — ordered scores, original rank, and deltas
- `ScoreHeatmap` — color-coded score bars
- `RerankPlayground` — editable query/candidates plus JSON export

## Run it

```bash
dotnet run --project src/samples/BlazorRerankingDemo/BlazorRerankingDemo.csproj
```

## Notes

- Uses Bootstrap 5.3.3 from a CDN
- Uses fixed demo content so screenshots and docs stay stable
- Renders the current `ElBruno.Reranking` result models through a deterministic sample service

## Pages

- `/backend-selector`
- `/rerank-result-list`
- `/score-heatmap`
- `/rerank-playground`

## Related docs

- [Package README](../../../src/ElBruno.Reranking.BlazorComponents/README.md)
- [Blazor components guide](../../../docs/blazor-components.md)
- [Root README](../../../README.md)
