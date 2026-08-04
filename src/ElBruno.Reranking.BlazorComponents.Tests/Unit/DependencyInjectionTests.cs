namespace ElBruno.Reranking.BlazorComponents.Tests.Unit;

public class DependencyInjectionTests
{
    [Fact]
    public void RegistersStateAndCatalogServices()
    {
        var services = new ServiceCollection();
        services.AddRerankingBlazorComponents();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<RerankingStateService>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<RerankBackendCatalog>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IReranker>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<RerankService>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IReranker>().Should().BeSameAs(scope.ServiceProvider.GetRequiredService<RerankService>());
    }

    [Fact]
    public void CanBeCalledMultipleTimesWithoutDuplicateCatalogRegistration()
    {
        var services = new ServiceCollection();
        services.AddRerankingBlazorComponents();
        services.AddRerankingBlazorComponents();

        services.Count(x => x.ServiceType == typeof(RerankBackendCatalog)).Should().Be(1);
    }

    [Fact]
    public void CatalogExposesAllBackends()
    {
        var catalog = new RerankBackendCatalog();

        catalog.GetAll().Should().ContainEquivalentOf(new RerankBackendDescriptor(RerankerBackendType.ONNX, "BGE-ONNX", "~15 ms", "Local reranking with ONNX Runtime"));
        catalog.GetAll().Should().ContainEquivalentOf(new RerankBackendDescriptor(RerankerBackendType.Claude, "Claude API", "~800 ms", "Cloud reranking with high-quality reasoning"));
        catalog.GetAll().Should().ContainEquivalentOf(new RerankBackendDescriptor(RerankerBackendType.Ollama, "Ollama", "~50 ms", "Local LLM reranking with configurable models"));
    }
}
