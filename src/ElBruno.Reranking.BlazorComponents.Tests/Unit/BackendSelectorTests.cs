using ElBruno.Reranking.BlazorComponents.Tests.Support;

namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class BackendSelectorTests : BunitContext
{
    [Fact]
    public void RendersBackendsWithLatencyHints()
    {
        Services.AddRerankingBlazorComponents();

        var cut = Render<BackendSelector>();

        cut.FindAll("[data-testid^='backend-option-']").Should().HaveCount(3);
        cut.Markup.Should().Contain("BGE-ONNX");
        cut.Markup.Should().Contain("~15 ms");
        cut.Markup.Should().Contain("Claude API");
        cut.Markup.Should().Contain("Ollama");
    }

    [Fact]
    public void ClickingBackendRaisesCallback()
    {
        Services.AddRerankingBlazorComponents();

        var selected = RerankerBackendType.ONNX;
        var cut = Render<BackendSelector>(parameters => parameters
            .Add(p => p.OnBackendChanged, backend => selected = backend));

        cut.Find("[data-testid='backend-option-Claude']").Click();

        selected.Should().Be(RerankerBackendType.Claude);
    }

    [Fact]
    public void HidesLatencyHintsWhenDisabled()
    {
        Services.AddRerankingBlazorComponents();

        var cut = Render<BackendSelector>(parameters => parameters
            .Add(p => p.ShowLatencyHint, false));

        cut.Markup.Should().NotContain("~15 ms");
        cut.Markup.Should().NotContain("~800 ms");
        cut.Markup.Should().NotContain("~50 ms");
    }
}
