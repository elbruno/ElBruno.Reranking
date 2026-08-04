namespace ElBruno.Reranking.Backends.Claude;

using System.Text;
using System.Text.Json;

/// <summary>
/// HTTP client for Claude API integration.
/// </summary>
internal class ClaudeApiClient : IClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ClaudePromptBuilder _promptBuilder = new();

    private const string ApiEndpoint = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// Creates a new Claude API client.
    /// </summary>
    /// <param name="options">Claude-specific options</param>
    public ClaudeApiClient(ClaudeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("API key is required", nameof(options));

        _options = options;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(options.TimeoutMs) };
    }

    /// <summary>
    /// Calls Claude API to rerank items.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="items">Items to rerank</param>
    /// <param name="includeExplanation">Whether explanations are requested</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scores for each item in order</returns>
    public async Task<IReadOnlyList<ClaudeScoreResult>> RankAsync(
        string query,
        IEnumerable<RerankItem> items,
        bool includeExplanation,
        CancellationToken cancellationToken)
    {
        var itemsList = items.ToList();
        var prompt = _promptBuilder.BuildPrompt(query, itemsList, includeExplanation);

        var attempt = 0;
        int backoffMs = _options.InitialBackoffMs;

        while (attempt <= _options.MaxRetries)
        {
            try
            {
                var response = await CallApiAsync(prompt, cancellationToken);
                return _promptBuilder.ParseResponse(response, itemsList.Count, includeExplanation);
            }
            catch (HttpRequestException ex) when (ShouldRetry(ex) && attempt < _options.MaxRetries)
            {
                attempt++;
                if (attempt <= _options.MaxRetries)
                {
                    await Task.Delay(backoffMs, cancellationToken);
                    backoffMs *= 2; // Exponential backoff
                }
            }
        }

        throw new RerankerException(
            "Failed to rerank after max retries",
            "claude-3-opus",
            "API_TIMEOUT");
    }

    /// <summary>
    /// Makes the actual API call to Claude.
    /// </summary>
    private async Task<string> CallApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint) { Content = content };
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API error {response.StatusCode}: {errorContent}",
                null,
                response.StatusCode);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtractTextFromResponse(responseBody);
    }

    /// <summary>
    /// Extracts the text content from Claude's API response.
    /// </summary>
    private string ExtractTextFromResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in content.EnumerateArray())
            {
                if (element.TryGetProperty("type", out var type) &&
                    type.GetString() == "text" &&
                    element.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        throw new InvalidOperationException("No text content found in API response");
    }

    /// <summary>
    /// Determines if an error should trigger a retry.
    /// </summary>
    private bool ShouldRetry(HttpRequestException ex)
    {
        // Retry on server errors and specific client errors
        return ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||  // 429
               ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || // 503
               ex.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||  // 504
               ex.InnerException is TimeoutException;
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
