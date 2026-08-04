namespace ElBruno.Reranking.Backends.Claude;

using System.Globalization;
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

        sb.AppendLine(includeExplanation
            ? "Return a JSON array where each element includes the zero-based original index, a relevance score (0.0 to 1.0), and a brief explanation."
            : "Return a JSON array where each element includes the zero-based original index and a relevance score (0.0 to 1.0).");

        sb.AppendLine("Use this shape:");
        sb.AppendLine(includeExplanation
            ? """
[
  {"index": 0, "score": 0.95, "explanation": "Directly matches the query"},
  {"index": 1, "score": 0.42, "explanation": "Partially relevant"},
  ...
]
"""
            : """
[
  {"index": 0, "score": 0.95},
  {"index": 1, "score": 0.42},
  ...
]
""");

        sb.AppendLine();
        sb.AppendLine("Provide ONLY the JSON array, no other text.");

        return sb.ToString();
    }

    /// <summary>
    /// Parses Claude's response to extract scores.
    /// </summary>
    /// <param name="response">Claude's response text</param>
    /// <param name="itemCount">Expected number of items</param>
    /// <param name="includeExplanation">Whether to read per-item explanations from the response</param>
    /// <param name="backendName">Backend identifier to include in parse errors</param>
    /// <returns>Scores in document order, with explanations when requested</returns>
    public IReadOnlyList<ClaudeScoreResult> ParseResponse(
        string response,
        int itemCount,
        bool includeExplanation = false,
        string backendName = ClaudeModelNames.Default)
    {
        try
        {
            var jsonText = ExtractJsonArray(response);
            using var doc = JsonDocument.Parse(jsonText);

            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("Response is not a JSON array");

            var scores = new ClaudeScoreResult[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                scores[i] = new ClaudeScoreResult(0.5f);
            }

            var fallbackIndex = 0;
            foreach (var element in root.EnumerateArray())
            {
                var targetIndex = TryGetIndex(element, fallbackIndex) ?? fallbackIndex;
                if (targetIndex < 0 || targetIndex >= itemCount)
                {
                    fallbackIndex++;
                    continue;
                }

                var score = ReadScore(element);
                string? explanation = null;

                if (includeExplanation)
                {
                    explanation = ReadExplanation(element);
                }

                scores[targetIndex] = new ClaudeScoreResult(Math.Clamp(score, 0f, 1f), explanation);
                fallbackIndex++;
            }

            return scores;
        }
        catch (Exception ex)
        {
            throw new RerankerException(
                $"Failed to parse Claude response: {ex.Message}",
                ClaudeModelNames.Normalize(backendName),
                "PARSE_ERROR",
                ex);
        }
    }

    private static string ExtractJsonArray(string response)
    {
        for (var start = 0; start < response.Length; start++)
        {
            if (response[start] != '[')
                continue;

            var end = FindMatchingBracket(response, start);
            if (end < 0)
                continue;

            var candidate = response.Substring(start, end - start + 1);
            try
            {
                using var doc = JsonDocument.Parse(candidate);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return candidate;
                }
            }
            catch (JsonException)
            {
                continue;
            }
        }

        throw new InvalidOperationException("No JSON array found in response");
    }

    private static int FindMatchingBracket(string text, int startIndex)
    {
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = startIndex; i < text.Length; i++)
        {
            var current = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (current == '\\')
                {
                    escape = true;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '[')
            {
                depth++;
                continue;
            }

            if (current == ']')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static int? TryGetIndex(JsonElement element, int fallbackIndex)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("index", out var indexElement))
        {
            if (indexElement.ValueKind == JsonValueKind.Number)
            {
                if (indexElement.TryGetInt32(out var index))
                {
                    return index;
                }
            }
            else if (indexElement.ValueKind == JsonValueKind.String &&
                     int.TryParse(indexElement.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIndex))
            {
                return parsedIndex;
            }
        }

        return fallbackIndex;
    }

    private static float ReadScore(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetSingle(out var score)
                ? score
                : 0.5f;
        }

        if (element.ValueKind == JsonValueKind.String &&
            float.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedScore))
        {
            return parsedScore;
        }

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("score", out var scoreElement))
        {
            if (scoreElement.ValueKind == JsonValueKind.Number && scoreElement.TryGetSingle(out var objectScore))
            {
                return objectScore;
            }

            if (scoreElement.ValueKind == JsonValueKind.String &&
                float.TryParse(scoreElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedObjectScore))
            {
                return parsedObjectScore;
            }
        }

        return 0.5f;
    }

    private static string? ReadExplanation(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty("explanation", out var explanationElement) ||
            explanationElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return explanationElement.GetString();
    }
}
