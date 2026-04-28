namespace ElBruno.Reranking.Tests;

/// <summary>
/// Test utilities for converting strings to RerankItem objects.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Converts an array of strings to an array of RerankItem objects.
    /// </summary>
    /// <param name="texts">Strings to convert</param>
    /// <returns>Array of RerankItem objects</returns>
    public static RerankItem[] ToRerankItems(this string[] texts)
    {
        return texts.Select((text, idx) => new RerankItem(text, $"item_{idx}")).ToArray();
    }

    /// <summary>
    /// Converts an enumerable of strings to an enumerable of RerankItem objects.
    /// </summary>
    /// <param name="texts">Strings to convert</param>
    /// <returns>Enumerable of RerankItem objects</returns>
    public static IEnumerable<RerankItem> ToRerankItems(this IEnumerable<string> texts)
    {
        return texts.Select((text, idx) => new RerankItem(text, $"item_{idx}"));
    }
}
