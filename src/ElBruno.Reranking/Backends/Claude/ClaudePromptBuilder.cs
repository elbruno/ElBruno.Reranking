namespace ElBruno.Reranking.Backends.Claude;

using System.Text;
using System.Text.Json;

/// <summary>
/// Builds prompts for Claude API to perform reranking.
/// </summary>
internal class ClaudePromptBuilder
{
    /// <summary>
    /// Builds a prompt for reranking items.
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="items">Items to rerank</param>
    /// <param name="includeExplanation">Whether to request explanations</param>
    /// <returns>Prompt text for Claude API</returns>
    public string BuildPrompt(string query, IEnumerable<RerankItem> items, bool includeExplanation = false)
    {
        var itemsList = items.ToList();

        var sb = new StringBuilder();
        sb.AppendLine("You are a semantic search reranking assistant.");
        sb.AppendLine("Your task is to rank the following documents by their relevance to the query.");
        sb.AppendLine();
        sb.AppendLine($"Query: {query}");
        sb.AppendLine();
        sb.AppendLine("Documents:");

        for (int i = 0; i < itemsList.Count; i++)
        {
            var item = itemsList[i];
            var id = item.Id ?? $"doc_{i}";
            sb.AppendLine($"{i + 1}. (ID: {id}) {item.Text}");
        }

        sb.AppendLine();

        if (includeExplanation)
        {
            sb.AppendLine("For each document, provide:");
            sb.AppendLine("1. Relevance score (0.0 to 1.0)");
            sb.AppendLine("2. Brief explanation for the score");
            sb.AppendLine();
            sb.AppendLine("Respond with a JSON array like:");
            sb.AppendLine("""
[
  {"index": 0, "score": 0.95, "explanation": "Directly matches the query"},
  {"index": 1, "score": 0.42, "explanation": "Partially relevant"},
  ...
]
""");
        }
        else
        {
            sb.AppendLine("Respond with a JSON array of scores (0.0 to 1.0) in order of document relevance:");
            sb.AppendLine("""
[
  {"index": 0, "score": 0.95},
  {"index": 1, "score": 0.42},
  ...
]
""");
        }

        sb.AppendLine();
        sb.AppendLine("Provide ONLY the JSON array, no other text.");

        return sb.ToString();
    }

    /// <summary>
    /// Parses Claude's response to extract scores.
    /// </summary>
    /// <param name="response">Claude's response text</param>
    /// <param name="itemCount">Expected number of items</param>
    /// <returns>Array of scores in document order</returns>
    public float[] ParseResponse(string response, int itemCount)
    {
        try
        {
            // Extract JSON from response (may contain extra text)
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                throw new InvalidOperationException("No JSON array found in response");

            var jsonText = response.Substring(jsonStart, jsonEnd - jsonStart);
            using var doc = JsonDocument.Parse(jsonText);

            var scores = new float[itemCount];
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("Response is not a JSON array");

            int scoreIndex = 0;
            foreach (var element in root.EnumerateArray())
            {
                if (scoreIndex >= itemCount)
                    break;

                float score = 0.5f; // default

                if (element.TryGetProperty("score", out var scoreElement))
                {
                    if (scoreElement.ValueKind == JsonValueKind.Number)
                    {
                        score = (float)scoreElement.GetDouble();
                    }
                }

                scores[scoreIndex] = Math.Clamp(score, 0f, 1f);
                scoreIndex++;
            }

            // Fill remaining with default scores if needed
            while (scoreIndex < itemCount)
            {
                scores[scoreIndex] = 0.5f;
                scoreIndex++;
            }

            return scores;
        }
        catch (Exception ex)
        {
            throw new RerankerException(
                $"Failed to parse Claude response: {ex.Message}",
                "claude-3-opus",
                "PARSE_ERROR",
                ex);
        }
    }
}
