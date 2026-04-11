namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for legacy migration planning guidance.
/// </summary>
public class MigrationPlanTests
{
    /// <summary>
    /// Verifies that a legacy app migration plan includes Fody replacement and testing updates.
    /// </summary>
    [Test]
    public async Task MigrationPlan_Includes_Fody_Replacement_And_Testing_Updates()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateMigrationPlan(new MigrationRequest(
            "WPF",
            "test project",
            ["ReactiveUI.Fody", "ReactiveUI.Testing"],
            ["source generators", "test migration"],
            []));

        await Assert.That(result.PackageActions.Any(action => action.Contains("ReactiveUI.SourceGenerators", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.PackageActions.Any(action => action.Contains("ReactiveUI.Testing", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.CodeActions.Any(action => action.Contains("ReactiveList", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.TestActions.Any(action => action.Contains("scheduler", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}
