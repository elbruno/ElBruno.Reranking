namespace ElBruno.Reranking.Backends.Claude;

internal static class ClaudeModelNames
{
    public const string Default = "claude-3-opus";

    public static string Normalize(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return Default;
        }

        return model.StartsWith("claude-", StringComparison.OrdinalIgnoreCase)
            ? model
            : $"claude-{model}";
    }
}
