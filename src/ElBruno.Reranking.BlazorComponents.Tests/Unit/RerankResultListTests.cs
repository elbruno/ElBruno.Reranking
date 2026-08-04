namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class RerankResultListTests : BunitContext
{
    [Fact]
    public void RendersScoreBadgesAndRankDeltas()
    {
        var result = TestData.CreateResult(
            scores:
            [
                ("Semantic search supports meaning-based retrieval.", 0.91f, 1, 3),
                ("This document is about weather updates.", 0.12f, 2, 1)
            ]);

        var cut = Render<RerankResultList>(parameters => parameters
            .Add(p => p.Results, new[] { result }));

        cut.FindAll("[data-testid='rerank-result-item']").Should().HaveCount(2);
        cut.FindAll("[data-testid='score-badge']").Should().ContainSingle(x => x.TextContent == "0.91");
        cut.FindAll("[data-testid='score-badge']").Should().ContainSingle(x => x.TextContent == "0.12");
        cut.FindAll("[data-testid='rank-delta']").Should().ContainSingle(x => x.TextContent == "+2");
        cut.FindAll("[data-testid='rank-delta']").Should().ContainSingle(x => x.TextContent == "-1");
    }

    [Fact]
    public void HidesScoreBadgeWhenDisabled()
    {
        var result = TestData.CreateResult(scores: [("Only one candidate.", 0.8f, 1, 1)]);

        var cut = Render<RerankResultList>(parameters => parameters
            .Add(p => p.Results, new[] { result })
            .Add(p => p.ShowScoreBadge, false));

        cut.FindAll("[data-testid='score-badge']").Should().BeEmpty();
        cut.Markup.Should().Contain("Only one candidate.");
    }

    [Fact]
    public void ShowsEmptyStateWhenNoResults()
    {
        var cut = Render<RerankResultList>();

        cut.Find("[data-testid='rerank-result-empty']").TextContent.Should().Be("No rerank results yet.");
    }

    [Fact]
    public void UsesPlaceholderWhenOriginalRankIsMissing()
    {
        var result = TestData.CreateResult(scores: [("No metadata candidate.", 0.5f, 1, null)]);

        var cut = Render<RerankResultList>(parameters => parameters
            .Add(p => p.Results, new[] { result }));

        cut.Find("[data-testid='rank-delta']").TextContent.Should().Contain("n/a");
    }
}
