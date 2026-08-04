using ElBruno.Reranking.BlazorComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ElBruno.Reranking.BlazorComponents.Extensions;

public static class RerankingBlazorComponentsServiceExtensions
{
    public static IServiceCollection AddRerankingBlazorComponents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<RerankBackendCatalog>();
        services.TryAddScoped<RerankingStateService>();
        services.TryAddScoped<RerankService>();
        services.TryAddScoped<IReranker>(sp => sp.GetRequiredService<RerankService>());

        return services;
    }
}
