namespace ReactiveUIMcp.Tests;
/// <summary>
/// Tests for the MCP host bootstrap and DI registration.
/// </summary>
public class HostBootstrapTests
{
    /// <summary>
    /// Verifies that the server host can be built and resolves the knowledge services.
    /// </summary>
    [Test]
    public async Task CreateHost_Registers_Guidance_Services()
    {
        using var host = Program.CreateHost([]);

        var catalog = host.Services.GetRequiredService<IKnowledgeCatalog>();
        var guidance = host.Services.GetRequiredService<IReactiveUiGuidanceService>();

        await Assert.That(catalog.GetAll().Count).IsGreaterThanOrEqualTo(19);
        await Assert.That(guidance).IsNotNull();
    }

    /// <summary>
    /// Verifies that safe mode suppresses optional rich metadata fields from the MCP handshake.
    /// </summary>
    [Test]
    public async Task BuildServerInfo_DefaultMode_Suppresses_Rich_Metadata()
    {
        var previous = Environment.GetEnvironmentVariable("REACTIVEUI_MCP_SAFE_CLIENT_METADATA");
        Environment.SetEnvironmentVariable("REACTIVEUI_MCP_SAFE_CLIENT_METADATA", null);

        try
        {
            var serverInfo = Program.BuildServerInfo();

            await Assert.That(serverInfo.Name).IsEqualTo("reactiveui-mcp-server");
            await Assert.That(serverInfo.Version).IsNotNull();
            await Assert.That(serverInfo.Title).IsNull();
            await Assert.That(serverInfo.Description).IsNull();
            await Assert.That(serverInfo.WebsiteUrl).IsNull();
            await Assert.That(serverInfo.Icons).IsNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("REACTIVEUI_MCP_SAFE_CLIENT_METADATA", previous);
        }
    }

    /// <summary>
    /// Verifies the documented list of metadata fields suppressed in compatibility mode.
    /// </summary>
    [Test]
    public async Task GetSuppressedClientMetadataKeys_Returns_Expected_Fields()
    {
        var keys = Program.GetSuppressedClientMetadataKeys();

        await Assert.That(keys).Contains("Title");
        await Assert.That(keys).Contains("Description");
        await Assert.That(keys).Contains("WebsiteUrl");
        await Assert.That(keys).Contains("Icons");
    }
}
