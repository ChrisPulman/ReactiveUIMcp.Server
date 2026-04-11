namespace ReactiveUIMcp.Tests;
/// <summary>
/// Smoke tests for the initial solution bootstrap.
/// </summary>
public class BootstrapTests
{
    /// <summary>
    /// Verifies that the catalog service can be created in the test project.
    /// </summary>
    [Test]
    public async Task EmbeddedCatalog_Service_Can_Be_Created()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        await Assert.That(catalog).IsNotNull();
    }
}
