namespace ElBruno.Reranking.Tests.Mocks;

/// <summary>
/// Mock IReranker that simulates failure scenarios for testing error handling.
/// </summary>
public class FailingMockReranker : IReranker
{
    public string Name => "FailingMockReranker";
    
    private Exception? _nextException;
    private int _callCount;
    private int _failAfterCalls = -1;

    public void SetNextException(Exception exception) => _nextException = exception;
    public void SetFailAfterNthCall(int n) => _failAfterCalls = n;
    public int GetCallCount() => _callCount;

    public Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _callCount++;

        if (_nextException != null)
        {
            var ex = _nextException;
            _nextException = null;
            return Task.FromException<RerankResult>(ex);
        }

        if (_failAfterCalls > 0 && _callCount >= _failAfterCalls)
        {
            return Task.FromException<RerankResult>(
                new InvalidOperationException($"Simulated failure after {_callCount} calls"));
        }

        // Otherwise behave like a normal reranker
        var docList = documents.ToList();
        var ranked = docList
            .Select((doc, idx) => new RankedDocument
            {
                Text = doc,
                Score = 1.0 - (idx * 0.1),
                Index = idx,
                Rank = idx + 1
            })
            .ToList();

        return Task.FromResult(new RerankResult
        {
            RankedDocuments = ranked,
            TotalDocuments = docList.Count
        });
    }
}
