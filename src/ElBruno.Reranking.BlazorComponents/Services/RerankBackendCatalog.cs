namespace ElBruno.Reranking.BlazorComponents.Services;

using ElBruno.Reranking;

public sealed class RerankBackendCatalog
{
    private static readonly IReadOnlyList<RerankBackendDescriptor> Backends =
    [
        new(RerankerBackendType.ONNX, "BGE-ONNX", "~15 ms", "Local reranking with ONNX Runtime"),
        new(RerankerBackendType.Claude, "Claude API", "~800 ms", "Cloud reranking with high-quality reasoning"),
        new(RerankerBackendType.Ollama, "Ollama", "~50 ms", "Local LLM reranking with configurable models"),
    ];

    public IReadOnlyList<RerankBackendDescriptor> GetAll() => Backends;

    public RerankBackendDescriptor Get(RerankerBackendType backend) =>
        Backends.FirstOrDefault(x => x.Backend == backend)
        ?? new RerankBackendDescriptor(backend, backend.ToString(), "Variable", "Custom or unknown reranker backend");
}
