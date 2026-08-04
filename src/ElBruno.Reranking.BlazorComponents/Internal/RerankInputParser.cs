namespace ElBruno.Reranking.BlazorComponents.Internal;

using ElBruno.Reranking;

internal static class RerankInputParser
{
    public static IReadOnlyList<RerankItem> ParseCandidates(string? candidatesText)
    {
        if (string.IsNullOrWhiteSpace(candidatesText))
        {
            return [];
        }

        var items = new List<RerankItem>();
        var lines = candidatesText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            var text = StripBullet(line);
            var originalRank = items.Count + 1;
            var metadata = new Dictionary<string, object>
            {
                ["originalRank"] = originalRank
            };

            items.Add(new RerankItem(text, $"candidate-{originalRank}", metadata));
        }

        return items;
    }

    private static string StripBullet(string line)
    {
        var trimmed = line.Trim();

        if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
            trimmed.StartsWith("* ", StringComparison.Ordinal) ||
            trimmed.StartsWith("• ", StringComparison.Ordinal))
        {
            return trimmed[2..].Trim();
        }

        var dotIndex = trimmed.IndexOf(". ", StringComparison.Ordinal);
        if (dotIndex > 0 && int.TryParse(trimmed[..dotIndex], out _))
        {
            return trimmed[(dotIndex + 2)..].Trim();
        }

        return trimmed;
    }
}
