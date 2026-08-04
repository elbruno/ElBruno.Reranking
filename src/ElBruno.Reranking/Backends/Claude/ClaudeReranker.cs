namespace ElBruno.Reranking.Backends.Claude;

using ElBruno.Reranking.Utils;

/// <summary>
/// Claude API backend for semantic reranking.
/// Provides high-accuracy reranking using Claude's language understanding.
/// </summary>
public class ClaudeReranker : IReranker
{
    private readonly ClaudeOptions _options;
    private IClaudeApiClient? _apiClient;

    /// <summary>
    /// Gets the name of this reranker.
    /// </summary>
    public string Name => $"claude-{_options.Model}";

    /// <summary>
    /// Gets the backend type.
    /// </summary>
    public RerankerBackendType BackendType => RerankerBackendType.Claude;

    /// <summary>
    /// Creates a new ClaudeReranker instance.
    /// </summary>
    /// <param name="apiKey">Anthropic API key</param>
    /// <param name="model">Claude model to use (default: "3-opus")</param>
    public ClaudeReranker(string apiKey, string model = "3-opus")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        _options = new ClaudeOptions
        {
            ApiKey = apiKey,
            Model = model
        };
    }

    /// <summary>
    /// Creates a new ClaudeReranker instance with custom options.
    /// </summary>
    /// <param name="options">Claude-specific options</param>
    public ClaudeReranker(ClaudeOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(options));

        _options = options;
    }

    internal ClaudeReranker(ClaudeOptions options, IClaudeApiClient apiClient)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(options));
        if (apiClient == null)
            throw new ArgumentNullException(nameof(apiClient));

        _options = options;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Asynchronously reranks items based on their relevance to the query.
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="items">Items to rerank</param>
    /// <param name="options">Optional reranking options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reranked results sorted by relevance score</returns>
    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var timer = new TimingHelper();

        // Validate inputs
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        options?.Validate();

        // Convert to list and validate
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
        {
            return new RerankResult(
                new List<RerankScore>().AsReadOnly(),
                query,
                Name,
                0,
                timer.ElapsedMilliseconds);
        }

        // Check max items constraint (token budget)
        const int maxItems = 500; // Conservative estimate based on token limits
        if (itemsList.Count > maxItems)
            throw new ArgumentException($"Maximum {maxItems} items allowed, but {itemsList.Count} provided", nameof(items));

        // Check max items option
        if (options?.MaxItems.HasValue ?? false)
        {
            if (itemsList.Count > options.MaxItems.Value)
                throw new ArgumentException(
                    $"MaxItems option set to {options.MaxItems.Value}, but {itemsList.Count} items provided",
                    nameof(options));
        }

        // Initialize API client
        _apiClient ??= new ClaudeApiClient(_options);

        // Call Claude API
        var scores = await _apiClient.RankAsync(query, itemsList, options?.IncludeExplanation ?? false, cancellationToken);

        // Format results
        var pairs = itemsList.Zip(scores, (item, score) => (item, score.Score, score.Explanation));
        return ResultFormatter.Format(pairs, query, Name, options, timer.ElapsedMilliseconds);
    }

    /// <summary>
    /// Disposes the reranker and releases resources.
    /// </summary>
    public void Dispose()
    {
        _apiClient?.Dispose();
    }
}
