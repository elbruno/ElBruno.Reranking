using System.Globalization;
using System.Text.RegularExpressions;
using ElBruno.Reranking;
using ElBruno.Reranking.BlazorComponents;
using ElBruno.Reranking.BlazorComponents.Services;
using ElBruno.Reranking.Utils;

namespace BlazorRerankingDemo.Components.Shared;

public sealed class DeterministicRerankService : IReranker
{
    private static readonly IReadOnlyDictionary<RerankerBackendType, BackendProfile> Profiles = new Dictionary<RerankerBackendType, BackendProfile>
    {
        [RerankerBackendType.ONNX] = new("BGE-ONNX", 15, 0.34f, new[] { "local", "fast", "offline" }),
        [RerankerBackendType.Claude] = new("Claude API", 800, 0.31f, new[] { "precision", "reasoning", "explain" }),
        [RerankerBackendType.Ollama] = new("Ollama", 50, 0.33f, new[] { "offline", "flexible", "custom" }),
    };

    public DeterministicRerankService(RerankerBackendType backend)
    {
        BackendType = backend;
    }

    public string Name => Profiles[BackendType].DisplayName;

    public RerankerBackendType BackendType { get; }

    public Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> candidates,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(candidates);
        cancellationToken.ThrowIfCancellationRequested();

        var profile = Profiles[BackendType];
        var pairs = candidates.Select((candidate, index) => (candidate, ScoreItem(profile, query, candidate, index)));

        var diagnostics = new Dictionary<string, string>
        {
            ["mode"] = "deterministic-demo",
            ["backend"] = profile.DisplayName,
            ["latencyMs"] = profile.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture),
        };

        var result = ResultFormatter.Format(
            pairs,
            query,
            profile.DisplayName,
            options,
            elapsedMilliseconds: profile.ElapsedMilliseconds,
            diagnostics: diagnostics);

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
        string DisplayName,
        long ElapsedMilliseconds,
        float BaseBias,
        string[] PreferredTags);
}
