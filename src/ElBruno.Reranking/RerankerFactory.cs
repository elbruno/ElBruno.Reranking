namespace ElBruno.Reranking;

using ElBruno.Reranking.Backends.Claude;
using ElBruno.Reranking.Backends.ONNX;

/// <summary>
/// Factory for creating reranker instances.
/// Provides convenient methods for instantiating different backend types.
/// </summary>
public static class RerankerFactory
{
    /// <summary>
    /// Creates an ONNX-based reranker (BGE model).
    /// </summary>
    /// <param name="modelPath">Path to the ONNX model file</param>
    /// <param name="maxBatchSize">Maximum batch size for inference (optional)</param>
    /// <returns>OnnxReranker instance</returns>
    public static IReranker CreateOnnx(string modelPath, int maxBatchSize = 32)
    {
        return new OnnxReranker(modelPath, maxBatchSize);
    }

    /// <summary>
    /// Creates a Claude API-based reranker.
    /// </summary>
    /// <param name="apiKey">Anthropic API key</param>
    /// <param name="model">Claude model ID (optional, default: "3-opus")</param>
    /// <returns>ClaudeReranker instance</returns>
    public static IReranker CreateClaude(string apiKey, string model = "3-opus")
    {
        return new ClaudeReranker(apiKey, model);
    }

    /// <summary>
    /// Creates a Claude API-based reranker with custom options.
    /// </summary>
    /// <param name="options">Claude-specific options</param>
    /// <returns>ClaudeReranker instance</returns>
    public static IReranker CreateClaude(ClaudeOptions options)
    {
        return new ClaudeReranker(options);
    }
}
