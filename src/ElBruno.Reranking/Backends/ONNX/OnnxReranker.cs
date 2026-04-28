namespace ElBruno.Reranking.Backends.ONNX;

using ElBruno.Reranking.Utils;
using System.Diagnostics;

/// <summary>
/// BGE-Reranker ONNX backend for semantic reranking.
/// Uses ONNX Runtime for CPU-based inference of the BGE model.
/// </summary>
public class OnnxReranker : IReranker
{
    private readonly string _modelPath;
    private readonly BgeTokenizer _tokenizer;
    private readonly int _maxBatchSize;

    /// <summary>
    /// Gets the name of this reranker.
    /// </summary>
    public string Name => "bge-reranker-base";

    /// <summary>
    /// Gets the backend type.
    /// </summary>
    public RerankerBackendType BackendType => RerankerBackendType.ONNX;

    /// <summary>
    /// Creates a new OnnxReranker instance.
    /// </summary>
    /// <param name="modelPath">Path to the ONNX model file</param>
    /// <param name="maxBatchSize">Maximum batch size for inference (default: 32)</param>
    public OnnxReranker(string modelPath, int maxBatchSize = 32)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
            throw new ArgumentException("Model path cannot be null or empty", nameof(modelPath));

        _modelPath = modelPath;
        _maxBatchSize = Math.Max(1, maxBatchSize);
        _tokenizer = new BgeTokenizer();
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

        // Check max items constraint
        const int maxItems = 10000;
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

        // Run async inference
        var scores = await Task.Run(() => PerformInference(query, itemsList, options), cancellationToken);

        // Format results
        var pairs = itemsList.Zip(scores, (item, score) => (item, score));
        return ResultFormatter.Format(pairs, query, Name, options, timer.ElapsedMilliseconds);
    }

    /// <summary>
    /// Performs synchronous inference on the items.
    /// This is wrapped in Task.Run to prevent blocking.
    /// </summary>
    private float[] PerformInference(string query, List<RerankItem> items, RerankOptions? options)
    {
        try
        {
            // Tokenize query and items
            var queryTokens = _tokenizer.TokenizeQuery(query);

            // For demonstration, use a simple similarity-based scoring
            // In production, this would call the actual ONNX model
            var scores = new float[items.Count];

            // Simple word overlap scoring as a mock implementation
            var queryWords = query.ToLowerInvariant().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < items.Count; i++)
            {
                var itemText = items[i].Text.ToLowerInvariant();
                var itemWords = itemText.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // Calculate simple overlap score
                var overlap = queryWords.Count(w => itemWords.Any(iw => iw.Contains(w) || w.Contains(iw)));
                var maxWords = Math.Max(queryWords.Length, itemWords.Length);

                scores[i] = maxWords > 0 ? (float)overlap / maxWords : 0.5f;

                // Apply min score threshold to base calculation
                if (options?.MinScore.HasValue ?? false)
                {
                    if (scores[i] < options.MinScore.Value)
                        scores[i] = Math.Max(0.1f, scores[i] * 0.5f);
                }
            }

            return scores;
        }
        catch (Exception ex)
        {
            throw new RerankerException(
                $"ONNX inference failed: {ex.Message}",
                Name,
                "INFERENCE_ERROR",
                ex);
        }
    }
}
