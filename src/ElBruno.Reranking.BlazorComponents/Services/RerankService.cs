namespace ElBruno.Reranking.BlazorComponents.Services;

using System.Globalization;
using System.Text.RegularExpressions;
using ElBruno.Reranking;
using ElBruno.Reranking.Utils;

public sealed class RerankService : IReranker
{
    private readonly RerankingStateService _state;
    private readonly RerankBackendCatalog _backendCatalog;

    public RerankService(RerankingStateService state, RerankBackendCatalog backendCatalog)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _backendCatalog = backendCatalog ?? throw new ArgumentNullException(nameof(backendCatalog));
    }

    public string Name => _backendCatalog.Get(_state.SelectedBackend).DisplayName;

    public RerankerBackendType BackendType => _state.SelectedBackend;

    public Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(items);

        cancellationToken.ThrowIfCancellationRequested();
        options?.Validate();

        var itemList = items.ToList();
        var backend = _state.SelectedBackend;
        var profile = _backendCatalog.Get(backend);

        var pairs = itemList.Select((item, index) => (item, ScoreItem(backend, query, item, index)));
        var diagnostics = new Dictionary<string, string>
        {
            ["mode"] = "deterministic-demo",
            ["backend"] = profile.DisplayName,
            ["latencyHint"] = profile.LatencyHint,
        };

        var result = ResultFormatter.Format(
            pairs,
            query,
            profile.DisplayName,
            options,
            elapsedMilliseconds: GetLatencyMilliseconds(backend),
            diagnostics: diagnostics);

        _state.SetResults(result);
        return Task.FromResult(result);
    }

    private static float ScoreItem(RerankerBackendType backend, string query, RerankItem item, int index)
    {
        var queryTokens = Tokenize(query);
        var itemTokens = Tokenize(item.Text);
        var overlap = queryTokens.Intersect(itemTokens).Count();
        var tokenScore = queryTokens.Length == 0 ? 0f : overlap / (float)queryTokens.Length;
        var tagScore = TagBonus(backend, item);
        var lengthScore = Math.Clamp(1f - Math.Abs(item.Text.Length - 95) / 220f, 0f, 1f) * 0.08f;
        var positionScore = Math.Max(0f, 0.04f - (index * 0.005f));

        return BaseBias(backend) + tokenScore * 0.45f + tagScore + lengthScore + positionScore;
    }

    private static float TagBonus(RerankerBackendType backend, RerankItem item)
    {
        var tags = GetTags(item);
        if (tags.Count == 0)
        {
            return 0f;
        }

        var preferredTags = PreferredTags(backend);
        var matches = tags.Count(tag => preferredTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        return matches * 0.14f;
    }

    private static IReadOnlyList<string> GetTags(RerankItem item)
    {
        if (item.Metadata is null || !item.Metadata.TryGetValue("tags", out var rawTags) || rawTags is null)
        {
            return Array.Empty<string>();
        }

        return rawTags switch
        {
            string tag => new[] { tag },
            IEnumerable<string> tagList => tagList.ToArray(),
            IEnumerable<object> objectList => objectList.Select(value => value?.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToArray(),
            _ => Array.Empty<string>(),
        };
    }

    private static string[] Tokenize(string value)
        => Regex.Split(value.ToLowerInvariant(), @"[^\p{L}\p{Nd}]+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

    private static float BaseBias(RerankerBackendType backend)
        => backend switch
        {
            RerankerBackendType.ONNX => 0.34f,
            RerankerBackendType.Claude => 0.31f,
            RerankerBackendType.Ollama => 0.33f,
            _ => 0.29f,
        };

    private static long GetLatencyMilliseconds(RerankerBackendType backend)
        => backend switch
        {
            RerankerBackendType.ONNX => 15,
            RerankerBackendType.Claude => 800,
            RerankerBackendType.Ollama => 50,
            _ => 25,
        };

    private static string[] PreferredTags(RerankerBackendType backend)
        => backend switch
        {
            RerankerBackendType.ONNX => ["local", "fast", "offline"],
            RerankerBackendType.Claude => ["precision", "reasoning", "explain"],
            RerankerBackendType.Ollama => ["offline", "flexible", "custom"],
            _ => ["demo", "stable", "ui"],
        };
}
