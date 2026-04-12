using ReactiveUIMcp.Server.Resources;
using System.Text.Json;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the <see cref="CatalogResources"/> MCP resource class.
/// </summary>
public class CatalogResourcesTests
{
    // ── GetCatalog ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetCatalog"/> returns valid JSON containing
    /// a count, categories, and items array.
    /// </summary>
    [Test]
    public async Task GetCatalog_Returns_Json_With_Count_Categories_And_Items()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("count").GetInt32()).IsGreaterThanOrEqualTo(19);
        await Assert.That(root.GetProperty("categories").GetArrayLength()).IsGreaterThan(0);
        await Assert.That(root.GetProperty("items").GetArrayLength()).IsGreaterThanOrEqualTo(19);
    }

    /// <summary>
    /// Verifies that the categories array contains expected catalog sections.
    /// </summary>
    [Test]
    public async Task GetCatalog_Categories_Include_Platform_And_Core()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        var categories = doc.RootElement.GetProperty("categories")
            .EnumerateArray()
            .Select(static el => el.GetString()!)
            .ToList();

        await Assert.That(categories.Any(c => c.Equals("platform", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(categories.Any(c => c.Equals("core", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that each catalog item exposes id, displayName, and category.
    /// </summary>
    [Test]
    public async Task GetCatalog_Items_Expose_Id_DisplayName_And_Category()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        var firstItem = doc.RootElement.GetProperty("items").EnumerateArray().First();
        await Assert.That(firstItem.TryGetProperty("id", out _)).IsTrue();
        await Assert.That(firstItem.TryGetProperty("displayName", out _)).IsTrue();
        await Assert.That(firstItem.TryGetProperty("category", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetCatalog"/> throws when catalog is null.
    /// </summary>
    [Test]
    public async Task GetCatalog_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogResources.GetCatalog(null!))
            .Throws<ArgumentNullException>();
    }

    // ── GetManifest ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetManifest"/> returns full manifest JSON for a known id.
    /// </summary>
    [Test]
    public async Task GetManifest_Returns_Full_Manifest_For_Known_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetManifest(catalog, "reactiveui-testing");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("id").GetString()).IsEqualTo("reactiveui-testing");
        await Assert.That(root.TryGetProperty("nuGetPackages", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("recommendedPatterns", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetManifest"/> throws <see cref="InvalidOperationException"/>
    /// for an unknown manifest id.
    /// </summary>
    [Test]
    public async Task GetManifest_Throws_For_Unknown_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        await Assert.That(() => CatalogResources.GetManifest(catalog, "not-a-real-manifest"))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetManifest"/> throws when catalog is null.
    /// </summary>
    [Test]
    public async Task GetManifest_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogResources.GetManifest(null!, "reactiveui-core"))
            .Throws<ArgumentNullException>();
    }

    // ── GetProjectGenerationGuidance ──────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetProjectGenerationGuidance"/> includes all
    /// project-generation-focused manifests and has a Purpose field.
    /// </summary>
    [Test]
    public async Task GetProjectGenerationGuidance_Contains_Purpose_And_Manifests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetProjectGenerationGuidance(catalog);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("purpose", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("manifests", out _)).IsTrue();
        await Assert.That(root.GetProperty("manifests").GetArrayLength()).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that the project-generation guidance includes the core and testing manifests.
    /// </summary>
    [Test]
    public async Task GetProjectGenerationGuidance_Includes_Core_SourceGenerators_And_Testing_Manifests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetProjectGenerationGuidance(catalog);
        using var doc = JsonDocument.Parse(json);

        var ids = doc.RootElement.GetProperty("manifests")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(ids).Contains("reactiveui-core");
        await Assert.That(ids).Contains("reactiveui-sourcegenerators");
        await Assert.That(ids).Contains("reactiveui-testing");
        await Assert.That(ids).Contains("dynamicdata");
    }

    /// <summary>
    /// Verifies that <see cref="CatalogResources.GetProjectGenerationGuidance"/> throws when catalog is null.
    /// </summary>
    [Test]
    public async Task GetProjectGenerationGuidance_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogResources.GetProjectGenerationGuidance(null!))
            .Throws<ArgumentNullException>();
    }
}
