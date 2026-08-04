using ElBruno.Reranking.BlazorComponents.Tests.Support;

namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class RerankPlaygroundTests : BunitContext
{
    [Fact]
    public void RendersDefaultsAndRunsRerank()
    {
        Services.AddRerankingBlazorComponents();

        var service = new FakeRerankService((query, candidates) => TestData.CreateResult(
            query: query,
            scores:
            [
                (candidates[0].Text, 0.91f, 1, 2),
                (candidates[1].Text, 0.42f, 2, 1)
            ]));

        var cut = Render<RerankPlayground>(parameters => parameters
            .Add(p => p.Reranker, service)
            .Add(p => p.DefaultQuery, "What is semantic search?")
            .Add(p => p.ShowExportButton, true));

        cut.Find("[data-testid='rerank-query']").Change("What is semantic search?");
        cut.Find("[data-testid='rerank-candidates']").Change("Semantic retrieval\nWeather report");
        cut.Find("[data-testid='rerank-submit']").Click();

        service.CallCount.Should().Be(1);
        service.LastQuery.Should().Be("What is semantic search?");
        service.LastCandidates.Should().HaveCount(2);

        cut.FindAll("[data-testid='rerank-result-item']").Should().HaveCount(2);
        cut.Find("[data-testid='rerank-export']").Click();
        cut.Find("[data-testid='export-json']").TextContent.Should().Contain("Semantic retrieval");
    }

    [Fact]
    public void UsesTheDefaultRerankerWhenNoneIsProvided()
    {
        Services.AddRerankingBlazorComponents();

        var cut = Render<RerankPlayground>(parameters => parameters
            .Add(p => p.DefaultQuery, "What is semantic search?")
            .Add(p => p.DefaultCandidates, "Semantic retrieval\nWeather report"));

        cut.Find("[data-testid='rerank-submit']").Click();

        cut.FindAll("[data-testid='rerank-result-item']").Should().NotBeEmpty();
        cut.Find("[data-testid='rerank-export']").Click();
        cut.Find("[data-testid='export-json']").TextContent.Should().Contain("Semantic retrieval");
    }

    [Fact]
    public void IgnoresBlankCandidateLines()
    {
        Services.AddRerankingBlazorComponents();

        var service = new FakeRerankService((query, candidates) => TestData.CreateResult(
            query: query,
            scores: candidates.Select((candidate, index) => (candidate.Text, 1f - index * 0.1f, index + 1, (int?) (index + 1))).ToArray()));

        var cut = Render<RerankPlayground>(parameters => parameters
            .Add(p => p.Reranker, service));

        cut.Find("[data-testid='rerank-query']").Change("Query");
        cut.Find("[data-testid='rerank-candidates']").Change("First line\n\n  \nSecond line");
        cut.Find("[data-testid='rerank-submit']").Click();

        service.LastCandidates.Should().HaveCount(2);
    }

    [Fact]
    public void HidesExportButtonWhenDisabled()
    {
        Services.AddRerankingBlazorComponents();

        var service = new FakeRerankService((query, candidates) => TestData.CreateResult(
            query: query,
            scores: [("Candidate", 0.8f, 1, 1)]));

        var cut = Render<RerankPlayground>(parameters => parameters
            .Add(p => p.Reranker, service)
            .Add(p => p.ShowExportButton, false));

        cut.FindAll("[data-testid='rerank-export']").Should().BeEmpty();
    }

    [Fact]
    public void ShowsValidationForMissingCandidates()
    {
        Services.AddRerankingBlazorComponents();

        var service = new FakeRerankService((query, candidates) => TestData.CreateResult(
            query: query,
            scores: [("Candidate", 0.8f, 1, 1)]));

        var cut = Render<RerankPlayground>(parameters => parameters
            .Add(p => p.Reranker, service));

        cut.Find("[data-testid='rerank-query']").Change("Query");
        cut.Find("[data-testid='rerank-submit']").Click();

        cut.Find("[data-testid='validation-message']").TextContent.Should().Be("Enter at least one candidate.");
        service.CallCount.Should().Be(0);
    }
}
