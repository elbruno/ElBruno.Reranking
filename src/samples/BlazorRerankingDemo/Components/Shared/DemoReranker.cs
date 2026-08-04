using System.Globalization;
using System.Text.RegularExpressions;
using ElBruno.Reranking;
using ElBruno.Reranking.Utils;

namespace BlazorRerankingDemo.Components.Shared;

public sealed class DemoReranker : IReranker
{
    private static readonly IReadOnlyDictionary<RerankerBackendType, BackendProfile> Profiles = new Dictionary<RerankerBackendType, BackendProfile>
    {
        [RerankerBackendType.ONNX] = new("demo-onnx-reranker", 18, 0.34f, new[] { "local", "fast", "offline" }),
        [RerankerBackendType.Claude] = new("demo-claude-reranker", 480, 0.31f, new[] { "precision", "reasoning", "explain" }),
        [RerankerBackendType.Ollama] = new("demo-ollama-reranker", 110, 0.33f, new[] { "offline", "flexible", "custom" }),
        [RerankerBackendType.Custom] = new("demo-custom-reranker", 75, 0.29f, new[] { "demo", "stable", "ui" }),
    };

    public DemoReranker(RerankerBackendType backendType)
    {
        BackendType = backendType;
    }

    public string Name => Profiles[BackendType].Name;

    public RerankerBackendType BackendType { get; }

    public Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var itemList = items.ToList();
        var profile = Profiles[BackendType];

        var pairs = itemList.Select((item, index) => (item, ScoreItem(profile, query, item, index)));
        var diagnostics = new Dictionary<string, string>
        {
            ["mode"] = "deterministic-demo",
            ["backend"] = Name,
            ["latencyMs"] = profile.LatencyMs.ToString(CultureInfo.InvariantCulture),
        };

        var result = ResultFormatter.Format(
            pairs,
            query,
            Name,
            options,
            profile.LatencyMs,
            diagnostics);

        return Task.FromResult(result);
    }

    private static float ScoreItem(BackendProfile profile, string query, RerankItem item, int index)
    {
        var queryTokens = Tokenize(query);
        var itemTokens = Tokenize(item.Text);
        var overlap = queryTokens.Intersect(itemTokens).Count();
        var tokenScore = queryTokens.Length == 0 ? 0f : overlap / (float)queryTokens.Length;
        var tagScore = TagBonus(profile, item);
        var lengthScore = Math.Clamp(1f - Math.Abs(item.Text.Length - 95) / 220f, 0f, 1f) * 0.08f;
        var positionScore = Math.Max(0f, 0.04f - (index * 0.005f));

        return profile.BaseBias + tokenScore * 0.45f + tagScore + lengthScore + positionScore;
    }

    private static float TagBonus(BackendProfile profile, RerankItem item)
    {
        var tags = GetTags(item);
        if (tags.Count == 0)
        {
            return 0f;
        }

        var matches = tags.Count(tag => profile.PreferredTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
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

    private sealed record BackendProfile(
        string Name,
        long LatencyMs,
        float BaseBias,
        string[] PreferredTags);
}
