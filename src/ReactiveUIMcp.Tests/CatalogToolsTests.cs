using ReactiveUIMcp.Server.Tools;
using System.Text.Json;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the <see cref="CatalogTools"/> MCP tool class.
/// </summary>
public class CatalogToolsTests
{
    // ── ListCatalog ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CatalogTools.ListCatalog"/> returns valid JSON containing
    /// the expected count and a non-empty items array.
    /// </summary>
    [Test]
    public async Task ListCatalog_Returns_Valid_Json_With_Count_And_Items()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.ListCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("count").GetInt32()).IsGreaterThanOrEqualTo(19);
        await Assert.That(root.GetProperty("items").GetArrayLength()).IsGreaterThanOrEqualTo(19);
    }

    /// <summary>
    /// Verifies that each item in the list payload exposes id, displayName, category, and summary fields.
    /// </summary>
    [Test]
    public async Task ListCatalog_Items_Expose_Required_Fields()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.ListCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        var firstItem = doc.RootElement.GetProperty("items").EnumerateArray().First();
        await Assert.That(firstItem.TryGetProperty("id", out _)).IsTrue();
        await Assert.That(firstItem.TryGetProperty("displayName", out _)).IsTrue();
        await Assert.That(firstItem.TryGetProperty("category", out _)).IsTrue();
        await Assert.That(firstItem.TryGetProperty("summary", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogTools.ListCatalog"/> throws <see cref="ArgumentNullException"/>
    /// when the catalog argument is null.
    /// </summary>
    [Test]
    public async Task ListCatalog_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogTools.ListCatalog(null!)).Throws<ArgumentNullException>();
    }

    // ── SearchCatalog ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a keyword search returns manifests matching the query term.
    /// </summary>
    [Test]
    public async Task SearchCatalog_With_Query_Returns_Relevant_Results()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.SearchCatalog(catalog, query: "maui");
        using var doc = JsonDocument.Parse(json);

        var results = doc.RootElement.GetProperty("results").EnumerateArray().ToList();
        await Assert.That(results.Count).IsGreaterThan(0);
        await Assert.That(results.Any(item => item.GetProperty("id").GetString()!.Contains("maui", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that a category filter restricts results to manifests in that category.
    /// </summary>
    [Test]
    public async Task SearchCatalog_With_Category_Returns_Only_Matching_Category()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.SearchCatalog(catalog, category: "platform");
        using var doc = JsonDocument.Parse(json);

        var results = doc.RootElement.GetProperty("results").EnumerateArray().ToList();
        await Assert.That(results.Count).IsGreaterThan(0);
        await Assert.That(results.All(item =>
            item.GetProperty("category").GetString()!.Equals("platform", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that the search result JSON echoes back the query and category fields.
    /// </summary>
    [Test]
    public async Task SearchCatalog_Result_Echoes_Query_And_Category()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.SearchCatalog(catalog, query: "akavache", category: "persistence");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("query").GetString()).IsEqualTo("akavache");
        await Assert.That(root.GetProperty("category").GetString()).IsEqualTo("persistence");
    }

    /// <summary>
    /// Verifies that each search result exposes the nuGetPackages array.
    /// </summary>
    [Test]
    public async Task SearchCatalog_Results_Include_NuGetPackages_Field()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.SearchCatalog(catalog, query: "dynamicdata");
        using var doc = JsonDocument.Parse(json);

        var results = doc.RootElement.GetProperty("results").EnumerateArray().ToList();
        await Assert.That(results.Count).IsGreaterThan(0);
        await Assert.That(results.All(item => item.TryGetProperty("nuGetPackages", out _))).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogTools.SearchCatalog"/> throws <see cref="ArgumentNullException"/>
    /// when the catalog argument is null.
    /// </summary>
    [Test]
    public async Task SearchCatalog_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogTools.SearchCatalog(null!)).Throws<ArgumentNullException>();
    }

    // ── GetManifest ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CatalogTools.GetManifest"/> returns the full manifest JSON
    /// for a known id.
    /// </summary>
    [Test]
    public async Task GetManifest_Returns_Full_Manifest_For_Known_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.GetManifest(catalog, "reactiveui-core");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("id").GetString()).IsEqualTo("reactiveui-core");
        await Assert.That(root.TryGetProperty("displayName", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("nuGetPackages", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("recommendedPatterns", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogTools.GetManifest"/> returns a manifest for the
    /// dynamicdata area including its primary package name.
    /// </summary>
    [Test]
    public async Task GetManifest_DynamicData_Contains_Primary_Package()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.GetManifest(catalog, "dynamicdata");
        using var doc = JsonDocument.Parse(json);

        var packages = doc.RootElement.GetProperty("nuGetPackages")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(packages.Any(p => p!.Contains("DynamicData", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogTools.GetManifest"/> throws <see cref="InvalidOperationException"/>
    /// for an unknown manifest id.
    /// </summary>
    [Test]
    public async Task GetManifest_Throws_InvalidOperationException_For_Unknown_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        await Assert.That(() => CatalogTools.GetManifest(catalog, "unknown-does-not-exist"))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="CatalogTools.GetManifest"/> throws <see cref="ArgumentNullException"/>
    /// when the catalog argument is null.
    /// </summary>
    [Test]
    public async Task GetManifest_Throws_When_Catalog_Is_Null()
    {
        await Assert.That(() => CatalogTools.GetManifest(null!, "reactiveui-core"))
            .Throws<ArgumentNullException>();
    }
}
