namespace ElBruno.Reranking.Tests.Mocks;

using ElBruno.Reranking.Utils;

/// <summary>
/// Mock IReranker that simulates failure scenarios for testing error handling.
/// </summary>
public class FailingMockReranker : IReranker
{
    public string Name => "failing-mock-reranker";
    public RerankerBackendType BackendType => RerankerBackendType.Custom;
    
    private Exception? _nextException;
    private int _callCount;
    private int _failAfterCalls = -1;

    public void SetNextException(Exception exception) => _nextException = exception;
    public void SetFailAfterNthCall(int n) => _failAfterCalls = n;
    public int GetCallCount() => _callCount;

    public async Task<RerankResult> RerankAsync(
        string query,
        IEnumerable<RerankItem> items,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _callCount++;

        if (_nextException != null)
        {
            var ex = _nextException;
            _nextException = null;
            throw ex;
        }

        if (_failAfterCalls > 0 && _callCount >= _failAfterCalls)
        {
            throw new InvalidOperationException($"Simulated failure after {_callCount} calls");
        }

        // Otherwise behave like a normal reranker
        using var timer = new TimingHelper();
        await Task.Delay(10, cancellationToken);

        var itemsList = items.ToList();

        if (!itemsList.Any())
        {
            return new RerankResult(
                new List<RerankScore>().AsReadOnly(),
                query,
                Name,
                0,
                timer.ElapsedMilliseconds);
        }

        var pairs = itemsList.Select((item, idx) => (item, 1.0f - (idx * 0.1f)));
        return ResultFormatter.Format(pairs, query, Name, options, timer.ElapsedMilliseconds);
    }
}

