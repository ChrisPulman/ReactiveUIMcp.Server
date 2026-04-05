
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Knowledge.Services;

namespace ReactiveUIMcp.Tests;
/// <summary>
/// Tests for manifest loading and lookup behavior.
/// </summary>
public class CatalogLoadingTests
{
    /// <summary>
    /// Verifies that the embedded catalog loads all expected ecosystem areas.
    /// </summary>
    [Test]
    public async Task EmbeddedCatalog_Loads_All_Requested_Manifest_Areas()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        await Assert.That(catalog.GetAll().Count).IsGreaterThanOrEqualTo(19);
        await Assert.That(catalog.GetById("reactiveui-core")).IsNotNull();
        await Assert.That(catalog.GetById("reactiveui-maui")).IsNotNull();
        await Assert.That(catalog.GetById("dynamicdata")).IsNotNull();
    }

    /// <summary>
    /// Verifies that search can locate package guidance by keyword.
    /// </summary>
    [Test]
    public async Task Search_Finds_Binding_Source_Generator_Guidance()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var results = catalog.Search("binding source generators");
        await Assert.That(results.Any(result => result.Id == "reactiveui-binding-sourcegenerators")).IsTrue();
    }
}
