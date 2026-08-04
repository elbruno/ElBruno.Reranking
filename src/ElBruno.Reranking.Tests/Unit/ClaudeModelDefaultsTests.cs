namespace ElBruno.Reranking.Tests.Unit;

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ElBruno.Reranking;
using ElBruno.Reranking.Backends.Claude;
using FluentAssertions;

public class ClaudeModelDefaultsTests
{
    [Fact]
    public void ClaudeOptions_DefaultsToClaude3Opus()
    {
        var options = new ClaudeOptions();

        options.Model.Should().Be(ClaudeModelNames.Default);
    }

    [Fact]
    public void ClaudeReranker_DefaultConstructor_UsesClaude3Opus()
    {
        var reranker = new ClaudeReranker("test-api-key");

        GetConfiguredModel(reranker).Should().Be(ClaudeModelNames.Default);
        reranker.Name.Should().Be(ClaudeModelNames.Default);
    }

    [Fact]
    public void RerankerFactory_CreateClaude_UsesClaude3Opus()
    {
        var reranker = Assert.IsType<ClaudeReranker>(RerankerFactory.CreateClaude("test-api-key"));

        GetConfiguredModel(reranker).Should().Be(ClaudeModelNames.Default);
        reranker.Name.Should().Be(ClaudeModelNames.Default);
    }

    [Fact]
    public void ClaudeReranker_CustomFullModelId_IsPreserved()
    {
        var model = "claude-3-haiku-20240307";
        var reranker = new ClaudeReranker("test-api-key", model);

        GetConfiguredModel(reranker).Should().Be(model);
        reranker.Name.Should().Be(model);
    }

    [Fact]
    public async Task ClaudeApiClient_SendsConfiguredModelIdUnchanged()
    {
        var model = "claude-3-haiku-20240307";
        var handler = new RecordingHandler();
        var apiClient = new ClaudeApiClient(
            new ClaudeOptions
            {
                ApiKey = "test-api-key",
                Model = model
            },
            handler);

        await apiClient.RankAsync(
            "query",
            new[] { new RerankItem("document") },
            includeExplanation: false,
            CancellationToken.None);

        using var requestJson = JsonDocument.Parse(handler.RequestBody ?? throw new InvalidOperationException("Request body not captured"));
        requestJson.RootElement.GetProperty("model").GetString().Should().Be(model);
    }

    private static string GetConfiguredModel(ClaudeReranker reranker)
    {
        var field = typeof(ClaudeReranker).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ClaudeReranker options field not found.");

        var options = (ClaudeOptions)field.GetValue(reranker)!;
        return options.Model;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {"content":[{"type":"text","text":"[{\"index\":0,\"score\":0.99}]"}]}
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
