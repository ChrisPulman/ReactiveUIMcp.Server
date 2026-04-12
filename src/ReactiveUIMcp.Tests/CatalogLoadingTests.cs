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

    /// <summary>
    /// Verifies that <see cref="IKnowledgeCatalog.GetById"/> returns null for an unknown identifier.
    /// </summary>
    [Test]
    public async Task GetById_Returns_Null_For_Unknown_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var result = catalog.GetById("this-manifest-does-not-exist");
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that look-up by id is case-insensitive so callers do not need to match casing exactly.
    /// </summary>
    [Test]
    public async Task GetById_Is_Case_Insensitive()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var lower = catalog.GetById("reactiveui-core");
        var upper = catalog.GetById("ReactiveUI-Core");

        await Assert.That(lower).IsNotNull();
        await Assert.That(upper).IsNotNull();
        await Assert.That(lower!.Id).IsEqualTo(upper!.Id);
    }

    /// <summary>
    /// Verifies that <see cref="IKnowledgeCatalog.Search"/> with no arguments returns all manifests.
    /// </summary>
    [Test]
    public async Task Search_With_No_Arguments_Returns_All_Manifests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var all = catalog.GetAll();
        var searchAll = catalog.Search();

        await Assert.That(searchAll.Count).IsEqualTo(all.Count);
    }

    /// <summary>
    /// Verifies that search with a null query and null category also returns all manifests.
    /// </summary>
    [Test]
    public async Task Search_With_Null_Query_And_Category_Returns_All_Manifests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var results = catalog.Search(null, null);
        await Assert.That(results.Count).IsEqualTo(catalog.GetAll().Count);
    }

    /// <summary>
    /// Verifies that a category-only filter restricts results to the requested category.
    /// </summary>
    [Test]
    public async Task Search_Category_Filter_Returns_Only_Matching_Category()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var results = catalog.Search(null, "platform");

        await Assert.That(results.Count).IsGreaterThan(0);
        await Assert.That(results.All(m => m.Category.Equals("platform", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that every manifest in the catalog has a non-empty id and display name.
    /// </summary>
    [Test]
    public async Task GetAll_Every_Manifest_Has_NonEmpty_Id_And_DisplayName()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var manifests = catalog.GetAll();
        await Assert.That(manifests.All(m => !string.IsNullOrWhiteSpace(m.Id))).IsTrue();
        await Assert.That(manifests.All(m => !string.IsNullOrWhiteSpace(m.DisplayName))).IsTrue();
    }

    /// <summary>
    /// Verifies that every manifest has at least one NuGet package listed.
    /// </summary>
    [Test]
    public async Task GetAll_Every_Manifest_Has_At_Least_One_NuGetPackage()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var manifests = catalog.GetAll();
        await Assert.That(manifests.All(m => m.NuGetPackages.Count > 0)).IsTrue();
    }

    /// <summary>
    /// Verifies that well-known platform manifests are present in the catalog.
    /// </summary>
    [Test]
    public async Task GetAll_Contains_All_Core_Platform_Manifests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var ids = catalog.GetAll().Select(m => m.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await Assert.That(ids).Contains("reactiveui-core");
        await Assert.That(ids).Contains("reactiveui-wpf");
        await Assert.That(ids).Contains("reactiveui-maui");
        await Assert.That(ids).Contains("reactiveui-avalonia");
        await Assert.That(ids).Contains("dynamicdata");
        await Assert.That(ids).Contains("akavache");
        await Assert.That(ids).Contains("reactiveui-testing");
    }
}
