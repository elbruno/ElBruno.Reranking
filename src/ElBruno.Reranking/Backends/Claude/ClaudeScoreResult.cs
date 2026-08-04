namespace ElBruno.Reranking.Backends.Claude;

internal readonly record struct ClaudeScoreResult(float Score, string? Explanation = null);
