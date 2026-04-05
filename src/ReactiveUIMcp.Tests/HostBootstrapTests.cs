
using Microsoft.Extensions.DependencyInjection;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Server;

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
}
