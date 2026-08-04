namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class ScoreHeatmapTests : BunitContext
{
    [Fact]
    public void RendersRowsInOriginalOrderWhenNotSorted()
    {
        var result = new RerankResult(
            [
                new RerankScore(new RerankItem("Low"), 0.2f, 2),
                new RerankScore(new RerankItem("High"), 0.9f, 1),
                new RerankScore(new RerankItem("Medium"), 0.5f, 3)
            ],
            "query",
            "backend",
            3,
            1);

        var cut = Render<ScoreHeatmap>(parameters => parameters
            .Add(p => p.Results, new[] { result }));

        cut.FindAll("[data-testid='heatmap-row']").Should().HaveCount(3);
        cut.FindAll("[data-testid='heatmap-row']")[0].TextContent.Should().Contain("Low");
        cut.FindAll("[data-testid='heatmap-row']")[1].TextContent.Should().Contain("High");
    }

    [Fact]
    public void SortByScoreReordersDescending()
    {
        var result = new RerankResult(
            [
                new RerankScore(new RerankItem("Low"), 0.2f, 2),
                new RerankScore(new RerankItem("High"), 0.9f, 1),
                new RerankScore(new RerankItem("Medium"), 0.5f, 3)
            ],
            "query",
            "backend",
            3,
            1);

        var cut = Render<ScoreHeatmap>(parameters => parameters
            .Add(p => p.Results, new[] { result })
            .Add(p => p.SortByScore, true));

        cut.FindAll("[data-testid='heatmap-row']")[0].TextContent.Should().Contain("High");
    }

    [Fact]
    public void CanHideLabels()
    {
        var result = TestData.CreateResult(scores: [("Candidate", 0.8f, 1, 1)]);

        var cut = Render<ScoreHeatmap>(parameters => parameters
            .Add(p => p.Results, new[] { result })
            .Add(p => p.ShowLabels, false));

        cut.Markup.Should().NotContain("Candidate");
        cut.Markup.Should().Contain("score-heatmap__bar");
    }

    [Fact]
    public void ShowsEmptyStateForMissingResults()
    {
        var cut = Render<ScoreHeatmap>();

        cut.Find("[data-testid='heatmap-empty']").TextContent.Should().Be("No scores to display.");
    }
}
