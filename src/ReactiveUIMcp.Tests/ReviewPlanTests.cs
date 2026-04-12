namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for plan-review heuristics.
/// </summary>
public class ReviewPlanTests
{
    /// <summary>
    /// Verifies that obvious ReactiveUI anti-patterns are detected.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_Constructor_Subscriptions_And_ReactiveList()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        const string plan = """
            Use ReactiveList for the main list.
            In the view constructor, call this.WhenAnyValue(x => x.ViewModel).Subscribe(_ => { });
            Use Splat's AppLocator.Current everywhere in feature services.
            """;

        var result = guidance.ReviewPlan("WPF", "Splat", plan);

        await Assert.That(result.Findings.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI001")).IsTrue();
        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI002")).IsTrue();
        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI005")).IsTrue();
    }

    /// <summary>
    /// Verifies that Fody migration and async extensions guidance are surfaced.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_Fody_And_AsyncObservable_Migration_Guidance()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        const string plan = """
            Migrate this legacy ReactiveUI.Fody codebase.
            Keep the old Fody weaver.
            Introduce IObservableAsync flows from ReactiveUI.Extensions.Async for async streaming work.
            Also modernize the tests.
            """;

        var result = guidance.ReviewPlan("MAUI", "ReactiveUI.Fody, ReactiveUI.Testing", plan);

        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI004")).IsTrue();
        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI008")).IsTrue();
        await Assert.That(result.RecommendedManifestIds).Contains("reactiveui-testing");
    }

    /// <summary>
    /// Verifies that RXUI003 is raised for XAML platforms that omit WhenActivated usage.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI003_For_Xaml_Platform_Missing_WhenActivated()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.ReviewPlan("WPF", null, "Create bindings in the constructor.");

        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI003")).IsTrue();
    }

    /// <summary>
    /// Verifies that RXUI006 is raised when ObservableCollection is used with sort and filter.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI006_For_ObservableCollection_Sort_And_Filter()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.ReviewPlan(null, null, "Use ObservableCollection to display sorted and filtered live updates.");

        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI006")).IsTrue();
        await Assert.That(result.RecommendedManifestIds).Contains("dynamicdata");
    }

    /// <summary>
    /// Verifies that RXUI007 is raised when setter side effects are used for derived state.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_RXUI007_For_Property_Setter_Side_Effect()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.ReviewPlan(null, null, "Compute derived state using property setter side effect.");

        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI007")).IsTrue();
    }

    /// <summary>
    /// Verifies that RXUI000 is returned when no anti-patterns are present in the plan.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Returns_RXUI000_For_Clean_Modern_Plan()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        const string plan = """
            Use ReactiveUI.SourceGenerators for all reactive properties and commands.
            Scope all bindings and subscriptions inside WhenActivated and dispose them with DisposeWith.
            Use DynamicData SourceCache for all live collections.
            Derive state through WhenAnyValue pipelines and ObservableAsPropertyHelper.
            """;

        var result = guidance.ReviewPlan(null, null, plan);

        await Assert.That(result.Findings.Any(finding => finding.Rule == "RXUI000")).IsTrue();
    }

    /// <summary>
    /// Verifies that each finding exposes a non-empty severity, rule, description, and fix.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Each_Finding_Has_Required_Fields()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.ReviewPlan("WPF", null, "Use ReactiveList. Subscribe without DisposeWith.");

        await Assert.That(result.Findings.All(f =>
            !string.IsNullOrWhiteSpace(f.Severity) &&
            !string.IsNullOrWhiteSpace(f.Rule) &&
            !string.IsNullOrWhiteSpace(f.Message) &&
            !string.IsNullOrWhiteSpace(f.Recommendation))).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="IReactiveUiGuidanceService.ReviewPlan"/> throws
    /// <see cref="ArgumentException"/> when planText is empty.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Throws_For_Empty_Plan_Text()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        await Assert.That(() => guidance.ReviewPlan(null, null, "   ")).Throws<ArgumentException>();
    }
}
