using ReactiveUIMcp.Server.Tools;
using System.Text.Json;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the <see cref="GuidanceTools"/> MCP tool class.
/// </summary>
public class GuidanceToolsTests
{
    private static (IKnowledgeCatalog catalog, IReactiveUiGuidanceService guidance) BuildServices()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);
        return (catalog, guidance);
    }

    // ── Recommend ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the Recommend tool returns valid JSON that includes selected manifest ids
    /// and suggested packages for a WPF request.
    /// </summary>
    [Test]
    public async Task Recommend_Returns_Json_With_Manifests_And_Packages_For_Wpf()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.Recommend(guidance, platform: "WPF", appKind: "desktop app");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("selectedManifestIds", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("suggestedPackages", out _)).IsTrue();

        var manifestIds = root.GetProperty("selectedManifestIds")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();
        await Assert.That(manifestIds).Contains("reactiveui-wpf");
    }

    /// <summary>
    /// Verifies that the Recommend tool surfaces Avalonia-specific packages when requested.
    /// </summary>
    [Test]
    public async Task Recommend_Returns_Avalonia_Manifest_For_Avalonia_Platform()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.Recommend(guidance, platform: "Avalonia", appKind: "desktop app");
        using var doc = JsonDocument.Parse(json);

        var ids = doc.RootElement
            .GetProperty("selectedManifestIds")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(ids).Contains("reactiveui-avalonia");
    }

    /// <summary>
    /// Verifies that a test project recommendation includes ReactiveUI.Testing.
    /// </summary>
    [Test]
    public async Task Recommend_Includes_Testing_Manifest_For_Test_Project()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.Recommend(guidance, appKind: "test project");
        using var doc = JsonDocument.Parse(json);

        var ids = doc.RootElement
            .GetProperty("selectedManifestIds")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(ids).Contains("reactiveui-testing");
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.Recommend"/> throws when guidance service is null.
    /// </summary>
    [Test]
    public async Task Recommend_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.Recommend(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }

    // ── ReviewPlan ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that RXUI001 is raised when the plan references the obsolete ReactiveList type.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI001_For_ReactiveList_Usage()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "ReactiveList is my collection type.", platform: "WPF");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI001");
    }

    /// <summary>
    /// Verifies that RXUI002 is raised when subscriptions are created without lifecycle scoping.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI002_For_Unscoped_Subscriptions()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Call this.WhenAnyValue(x => x.Prop).Subscribe(x => {});");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI002");
    }

    /// <summary>
    /// Verifies that RXUI003 is raised for XAML platforms that omit WhenActivated.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI003_For_Xaml_Platform_Without_WhenActivated()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Create bindings in the constructor.", platform: "WPF");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI003");
    }

    /// <summary>
    /// Verifies that RXUI004 is raised when the plan references Fody-era patterns.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI004_For_Fody_Reference()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Use Fody for property weaving.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI004");
    }

    /// <summary>
    /// Verifies that RXUI005 is raised when global service location is used in feature code.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI005_For_Locator_Current_Usage()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Resolve services via Locator.Current throughout feature services.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI005");
    }

    /// <summary>
    /// Verifies that RXUI006 is raised when ObservableCollection is used with sort/filter.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI006_For_ObservableCollection_With_Sort_And_Filter()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Use ObservableCollection to display sorted and filtered live updates.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI006");
    }

    /// <summary>
    /// Verifies that RXUI007 is raised when setter side effects are used for derived state.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI007_For_Property_Setter_Side_Effects()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Use property setter side effect to compute derived state.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI007");
    }

    /// <summary>
    /// Verifies that RXUI008 is raised when ReactiveUI.Extensions.Async patterns are referenced.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI008_For_Async_Observable_Reference()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Introduce IObservableAsync from ReactiveUI.Extensions.Async.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI008");
    }

    /// <summary>
    /// Verifies that RXUI000 is returned when no anti-patterns are detected.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Returns_RXUI000_For_Clean_Plan()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Use ReactiveUI.SourceGenerators. Scope all bindings with WhenActivated and DisposeWith. Use DynamicData SourceCache for collections.");
        using var doc = JsonDocument.Parse(json);

        var rules = doc.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(static el => el.GetProperty("rule").GetString())
            .ToList();

        await Assert.That(rules).Contains("RXUI000");
    }

    /// <summary>
    /// Verifies the review result contains a summary and a recommendedManifestIds array.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Result_Contains_Summary_And_RecommendedManifestIds()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.ReviewPlan(guidance, "Standard ReactiveUI plan using WhenActivated.");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("summary", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("recommendedManifestIds", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.ReviewPlan"/> throws when the guidance service is null.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.ReviewPlan(null!, "some plan"))
            .Throws<ArgumentNullException>();
    }

    // ── Compare ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that Compare returns a JSON summary for two known manifest areas.
    /// </summary>
    [Test]
    public async Task Compare_Returns_Comparison_Json_For_Known_Manifests()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.Compare(guidance, "dynamicdata", "akavache");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("summary", out _)).IsTrue();
        await Assert.That(root.GetProperty("leftId").GetString()).IsEqualTo("dynamicdata");
        await Assert.That(root.GetProperty("rightId").GetString()).IsEqualTo("akavache");
        await Assert.That(root.GetProperty("tradeoffs").GetArrayLength()).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that Compare includes package lists for both sides.
    /// </summary>
    [Test]
    public async Task Compare_Includes_Package_Lists_For_Both_Sides()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.Compare(guidance, "reactiveui-maui", "reactiveui-wpf");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("leftPackages", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("rightPackages", out _)).IsTrue();
        await Assert.That(root.GetProperty("leftPackages").GetArrayLength()).IsGreaterThan(0);
        await Assert.That(root.GetProperty("rightPackages").GetArrayLength()).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that Compare throws <see cref="InvalidOperationException"/> for an unknown left id.
    /// </summary>
    [Test]
    public async Task Compare_Throws_For_Unknown_Left_Id()
    {
        var (_, guidance) = BuildServices();

        await Assert.That(() => GuidanceTools.Compare(guidance, "unknown-left", "dynamicdata"))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that Compare throws <see cref="InvalidOperationException"/> for an unknown right id.
    /// </summary>
    [Test]
    public async Task Compare_Throws_For_Unknown_Right_Id()
    {
        var (_, guidance) = BuildServices();

        await Assert.That(() => GuidanceTools.Compare(guidance, "dynamicdata", "unknown-right"))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.Compare"/> throws when guidance service is null.
    /// </summary>
    [Test]
    public async Task Compare_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.Compare(null!, "dynamicdata", "akavache"))
            .Throws<ArgumentNullException>();
    }

    // ── CreateScaffoldPrompt ─────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the scaffold prompt output contains package, pattern, and verification sections.
    /// </summary>
    [Test]
    public async Task CreateScaffoldPrompt_Contains_Packages_Patterns_And_Checklist()
    {
        var (_, guidance) = BuildServices();

        var prompt = GuidanceTools.CreateScaffoldPrompt(guidance, platform: "MAUI", appKind: "mobile app");

        await Assert.That(prompt).Contains("Required package considerations:");
        await Assert.That(prompt).Contains("Preferred patterns:");
        await Assert.That(prompt).Contains("Verification checklist:");
        await Assert.That(prompt).Contains("ReactiveUI.SourceGenerators");
    }

    /// <summary>
    /// Verifies that the scaffold prompt names the requested platform.
    /// </summary>
    [Test]
    public async Task CreateScaffoldPrompt_Includes_Requested_Platform()
    {
        var (_, guidance) = BuildServices();

        var prompt = GuidanceTools.CreateScaffoldPrompt(guidance, platform: "Avalonia");

        await Assert.That(prompt).Contains("Avalonia");
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.CreateScaffoldPrompt"/> throws when guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateScaffoldPrompt_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.CreateScaffoldPrompt(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }

    // ── CreateProjectBlueprint ───────────────────────────────────────────────

    /// <summary>
    /// Verifies that the project blueprint JSON contains all structural guidance fields.
    /// </summary>
    [Test]
    public async Task CreateProjectBlueprint_Returns_Json_With_Expected_Sections()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.CreateProjectBlueprint(guidance, platform: "WPF", appKind: "desktop app");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("suggestedPackages", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("projectStructureHints", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("codeGenerationRules", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("testProjectRecommendations", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that the blueprint includes ReactiveUI.Testing guidance for any app kind.
    /// </summary>
    [Test]
    public async Task CreateProjectBlueprint_Recommends_ReactiveUiTesting()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.CreateProjectBlueprint(guidance, platform: "MAUI", appKind: "mobile app");
        using var doc = JsonDocument.Parse(json);

        var testRecs = doc.RootElement.GetProperty("testProjectRecommendations")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(testRecs.Any(r => r!.Contains("ReactiveUI.Testing", StringComparison.Ordinal))).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.CreateProjectBlueprint"/> throws when guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateProjectBlueprint_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.CreateProjectBlueprint(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }

    // ── CreateMigrationPlan ──────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the migration plan JSON contains all required migration sections.
    /// </summary>
    [Test]
    public async Task CreateMigrationPlan_Returns_Json_With_All_Sections()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.CreateMigrationPlan(guidance,
            platform: "WPF",
            projectType: "app",
            currentPackages: "ReactiveUI.Fody",
            upgradeGoals: "source generators");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.TryGetProperty("packageActions", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("codeActions", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("testActions", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("validationSteps", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("risks", out _)).IsTrue();
    }

    /// <summary>
    /// Verifies that a Fody migration plan recommends ReactiveUI.SourceGenerators.
    /// </summary>
    [Test]
    public async Task CreateMigrationPlan_Fody_Upgrade_Recommends_SourceGenerators()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.CreateMigrationPlan(guidance,
            currentPackages: "ReactiveUI.Fody",
            upgradeGoals: "source generators");
        using var doc = JsonDocument.Parse(json);

        var packageActions = doc.RootElement.GetProperty("packageActions")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(packageActions.Any(a => a!.Contains("ReactiveUI.SourceGenerators", StringComparison.Ordinal))).IsTrue();
    }

    /// <summary>
    /// Verifies that a test-project migration goal includes test-specific actions.
    /// </summary>
    [Test]
    public async Task CreateMigrationPlan_Test_Project_Goal_Includes_Test_Actions()
    {
        var (_, guidance) = BuildServices();

        var json = GuidanceTools.CreateMigrationPlan(guidance,
            projectType: "test project",
            currentPackages: "ReactiveUI.Testing",
            upgradeGoals: "test migration");
        using var doc = JsonDocument.Parse(json);

        var testActions = doc.RootElement.GetProperty("testActions")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(testActions.Count).IsGreaterThan(0);
        await Assert.That(testActions.Any(a => a!.Contains("scheduler", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="GuidanceTools.CreateMigrationPlan"/> throws when guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateMigrationPlan_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => GuidanceTools.CreateMigrationPlan(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }
}
