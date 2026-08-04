namespace ElBruno.Reranking.BlazorComponents;

using ElBruno.Reranking;

public sealed record RerankBackendDescriptor(
    RerankerBackendType Backend,
    string DisplayName,
    string LatencyHint,
    string Description);
