using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using ReactiveUIMcp.Core.Services;
using ReactiveUIMcp.Knowledge.Services;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for recommendation and comparison behavior.
/// </summary>
public class RecommendationTests
{
    /// <summary>
    /// Verifies that a MAUI offline/dynamic-data request recommends the expected companion libraries.
    /// </summary>
    [Test]
    public async Task Recommend_Includes_Maui_Akavache_And_DynamicData_When_Signals_Match()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.Recommend(new RecommendationRequest(
            "MAUI",
            "mobile app",
            ["offline cache", "dynamic collections", "REST API"],
            ["trimming"],
            []));

        await Assert.That(result.SelectedManifestIds).Contains("reactiveui-maui");
        await Assert.That(result.SelectedManifestIds).Contains("akavache");
        await Assert.That(result.SelectedManifestIds).Contains("dynamicdata");
        await Assert.That(result.SelectedManifestIds).Contains("refit");
        await Assert.That(result.SuggestedPackages).Contains("ReactiveUI.Maui");
    }

    /// <summary>
    /// Verifies that test-project generation recommends ReactiveUI.Testing.
    /// </summary>
    [Test]
    public async Task Recommend_Includes_ReactiveUiTesting_For_Test_Projects()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.Recommend(new RecommendationRequest(
            "WPF",
            "test project",
            ["scheduler tests", "command tests"],
            [],
            []));

        await Assert.That(result.SelectedManifestIds).Contains("reactiveui-testing");
        await Assert.That(result.SuggestedPackages).Contains("ReactiveUI.Testing");
    }

    /// <summary>
    /// Verifies that compare can produce a meaningful side-by-side result.
    /// </summary>
    [Test]
    public async Task Compare_Returns_A_Summary_For_Two_Manifest_Areas()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.Compare("reactiveui-maui", "reactiveui-androidx");

        await Assert.That(result.Summary).Contains("Compared");
        await Assert.That(result.Tradeoffs.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that a project blueprint promotes source generators, async extensions, and testing guidance.
    /// </summary>
    [Test]
    public async Task ProjectBlueprint_Promotes_SourceGenerators_Async_Extensions_And_Testing()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateProjectBlueprint(new RecommendationRequest(
            "MAUI",
            "mobile app",
            ["project generation", "async streams", "offline cache"],
            ["trimming"],
            []));

        await Assert.That(result.CodeGenerationRules.Any(rule => rule.Contains("ReactiveUI.SourceGenerators", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.CodeGenerationRules.Any(rule => rule.Contains("ReactiveUI.Extensions.Async", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.TestProjectRecommendations.Any(rule => rule.Contains("ReactiveUI.Testing", StringComparison.Ordinal))).IsTrue();
    }
}
