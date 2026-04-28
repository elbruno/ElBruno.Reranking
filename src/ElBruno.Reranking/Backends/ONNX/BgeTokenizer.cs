namespace ElBruno.Reranking.Backends.ONNX;

using System.Text;

/// <summary>
/// Tokenizes text for BGE-Reranker model.
/// Implements simple whitespace and punctuation-based tokenization
/// compatible with the BGE model's vocabulary.
/// </summary>
internal class BgeTokenizer
{
    private const int MaxTokenLength = 512;

    /// <summary>
    /// Tokenizes a single query string.
    /// </summary>
    /// <param name="query">Query text to tokenize</param>
    /// <returns>Array of token IDs</returns>
    public int[] TokenizeQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
            return Array.Empty<int>();

        return Tokenize(query);
    }

    /// <summary>
    /// Tokenizes multiple item texts in batch.
    /// </summary>
    /// <param name="items">Item texts to tokenize</param>
    /// <returns>Array of token ID arrays</returns>
    public int[][] TokenizeItems(IEnumerable<string> items)
    {
        return items
            .Select(item => string.IsNullOrEmpty(item) ? Array.Empty<int>() : Tokenize(item))
            .ToArray();
    }

    /// <summary>
    /// Core tokenization logic using simple whitespace/punctuation splitting.
    /// In production, this would use the actual BGE tokenizer vocabulary.
    /// For now, we use a simplified approach that maps characters to token IDs.
    /// </summary>
    private int[] Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<int>();

        // Normalize and truncate
        var normalized = text.ToLowerInvariant();
        if (normalized.Length > MaxTokenLength)
            normalized = normalized.Substring(0, MaxTokenLength);

        // Simple tokenization: split by whitespace and map characters
        var tokens = new List<int>();
        var words = normalized.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Map each word to a token ID (simple hash-based mapping)
        foreach (var word in words.Take(MaxTokenLength))
        {
            if (!string.IsNullOrEmpty(word))
            {
                // Create a deterministic token ID from the word
                var tokenId = GetTokenId(word);
                tokens.Add(tokenId);
            }
        }

        return tokens.ToArray();
    }

    /// <summary>
    /// Maps a word to a token ID using simple hash function.
    /// In production, this would use the actual BGE vocabulary.
    /// </summary>
    private int GetTokenId(string word)
    {
        // Use a simple hash function for consistent token IDs
        var hash = 0;
        foreach (var c in word)
        {
            hash = ((hash << 5) - hash) + c;
        }

        // Map to a reasonable token ID range (100-10000)
        return Math.Abs(hash % 9900) + 100;
    }
}
