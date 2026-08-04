namespace ElBruno.Reranking.BlazorComponents.Services;

using System.Text.Json;
using ElBruno.Reranking;

public sealed class RerankingStateService
{
    private readonly RerankBackendCatalog _backendCatalog;
    private readonly List<RerankResult> _results = [];

    public event Action? OnStateChanged;

    public RerankingStateService(RerankBackendCatalog backendCatalog)
    {
        _backendCatalog = backendCatalog ?? throw new ArgumentNullException(nameof(backendCatalog));
    }

    public RerankerBackendType SelectedBackend { get; private set; } = RerankerBackendType.ONNX;

    public string Query { get; private set; } = string.Empty;

    public string CandidatesText { get; private set; } = string.Empty;

    public IReadOnlyList<RerankResult> Results => _results;

    public bool IsBusy { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset? LastUpdatedUtc { get; private set; }

    public IReadOnlyList<RerankBackendDescriptor> AvailableBackends => _backendCatalog.GetAll();

    public void SetSelectedBackend(RerankerBackendType backend)
    {
        if (SelectedBackend == backend)
            return;

        SelectedBackend = backend;
        NotifyStateChanged();
    }

    public void SetQuery(string? query)
    {
        Query = query ?? string.Empty;
        NotifyStateChanged();
    }

    public void SetCandidatesText(string? candidatesText)
    {
        CandidatesText = candidatesText ?? string.Empty;
        NotifyStateChanged();
    }

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
        NotifyStateChanged();
    }

    public void SetResults(RerankResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        _results.Clear();
        _results.Add(result);
        ErrorMessage = null;
        LastUpdatedUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    public void SetError(string? errorMessage)
    {
        ErrorMessage = errorMessage;
        NotifyStateChanged();
    }

    public void Reset()
    {
        _results.Clear();
        ErrorMessage = null;
        IsBusy = false;
        LastUpdatedUtc = null;
        NotifyStateChanged();
    }

    public string ExportResultsAsJson()
        => JsonSerializer.Serialize(Results, new JsonSerializerOptions { WriteIndented = true });

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
