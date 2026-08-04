using System.Globalization;
using ElBruno.Reranking;

namespace BlazorRerankingDemo.Components.Shared;

public sealed record DemoBackendProfile(
    RerankerBackendType BackendType,
    string DisplayName,
    string Description,
    string LatencyHint,
    string AccentClass,
    string BadgeClass);

public sealed record CodeSnippet(
    string Title,
    string Description,
    string Value);

public static class DemoCatalog
{
    private static readonly IReadOnlyList<RerankItem> DefaultItems = new[]
    {
        CreateItem(1, "BGE-ONNX keeps reranking fast and local for offline-first search.", "ONNX", "local", "fast", "offline"),
        CreateItem(2, "Claude API is ideal when rankings need nuanced reasoning and explanations.", "Claude", "precision", "reasoning", "explain"),
        CreateItem(3, "Ollama works well for configurable local LLM reranking without external calls.", "Ollama", "offline", "flexible", "custom"),
        CreateItem(4, "Rank deltas make the UI easier to scan when results move after reranking.", "UX", "ui", "explain", "delta"),
        CreateItem(5, "Deterministic demo data keeps screenshots stable across builds and releases.", "Docs", "demo", "stable", "docs"),
        CreateItem(6, "Bootstrap 5.3.x cards and tables keep the sample readable on every page.", "UI", "blazor", "ui", "bootstrap"),
    };

    public static string DefaultQuery { get; } = "Which backend should I use for a fast Blazor reranking demo?";

    public static IReadOnlyList<DemoBackendProfile> Backends { get; } = new[]
    {
        new DemoBackendProfile(
            RerankerBackendType.ONNX,
            "BGE-ONNX",
            "Fast local inference for offline demos.",
            "~18 ms",
            "border-primary",
            "text-bg-primary"),
        new DemoBackendProfile(
            RerankerBackendType.Claude,
            "Claude API",
            "Nuanced reasoning and friendly explanations.",
            "~480 ms",
            "border-warning",
            "text-bg-warning"),
        new DemoBackendProfile(
            RerankerBackendType.Ollama,
            "Ollama",
            "Configurable local LLMs with no external calls.",
            "~110 ms",
            "border-success",
            "text-bg-success"),
    };

    public static IReadOnlyList<RerankItem> DefaultCandidates => DefaultItems;

    public static string DefaultCandidateText => FormatCandidates(DefaultItems);

    public static string FormatCandidates(IEnumerable<RerankItem> items)
        => string.Join(
            Environment.NewLine,
            items.Select(item => item.Text));

    public static IReadOnlyList<RerankItem> ParseCandidates(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return DefaultItems;
        }

        var lines = text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length == 0)
        {
            return DefaultItems;
        }

        var items = new List<RerankItem>(lines.Length);
        for (var index = 0; index < lines.Length; index++)
        {
            items.Add(CreateItem(index + 1, StripListPrefix(lines[index]), "User input", "custom", "demo"));
        }

        return items;
    }

    private static string StripListPrefix(string value)
    {
        var trimmed = value.Trim();
        var dotIndex = trimmed.IndexOf(". ", StringComparison.Ordinal);
        if (dotIndex > 0 && int.TryParse(trimmed[..dotIndex], out _))
        {
            return trimmed[(dotIndex + 2)..].Trim();
        }

        return trimmed;
    }

    private static RerankItem CreateItem(int originalRank, string text, string source, params string[] tags)
        => new(
            text,
            id: originalRank.ToString(CultureInfo.InvariantCulture),
            metadata: new Dictionary<string, object>
            {
                ["originalRank"] = originalRank,
                ["source"] = source,
                ["tags"] = tags,
            });
}
