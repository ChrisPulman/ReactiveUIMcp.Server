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
}
