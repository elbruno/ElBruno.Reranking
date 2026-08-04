namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class RerankingStateServiceTests
{
    [Fact]
    public void TracksSelectedBackendAndResults()
    {
        var catalog = new RerankBackendCatalog();
        var state = new RerankingStateService(catalog);

        state.SetSelectedBackend(RerankerBackendType.Claude);
        state.SetQuery("What is semantic search?");
        state.SetCandidatesText("First\nSecond");
        state.SetResults(TestData.CreateResult(scores: [("First", 0.9f, 1, 1)]));

        state.SelectedBackend.Should().Be(RerankerBackendType.Claude);
        state.Query.Should().Be("What is semantic search?");
        state.CandidatesText.Should().Contain("First");
        state.Results.Should().ContainSingle();
        state.ExportResultsAsJson().Should().Contain("First");
    }
}
