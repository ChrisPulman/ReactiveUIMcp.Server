namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for legacy migration planning guidance.
/// </summary>
public class MigrationPlanTests
{
    /// <summary>
    /// Verifies that System.Reactive migrations choose lean Primitives unless compatibility is required.
    /// </summary>
    [Test]
    public async Task MigrationPlan_SystemReactive_Defaults_To_Lean_Primitives()
    {
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.CreateMigrationPlan(new MigrationRequest(
            "WPF",
            "library",
            ["System.Reactive", "System.Reactive.Linq"],
            ["remove System.Reactive", "migrate to Primitives"],
            []));

        await Assert.That(result.PackageActions.Any(static action => action.Contains("Replace System.Reactive with ReactiveUI.Primitives", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Wpf", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Reactive", StringComparison.Ordinal))).IsFalse();
        await Assert.That(result.CodeActions.Any(static action => action.Contains("RxVoid", StringComparison.Ordinal) && action.Contains("ISequencer", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.CodeActions.Any(static action => action.Contains("Signal<T>", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.CodeActions.Any(static action => action.Contains("MultipleDisposable", StringComparison.Ordinal))).IsTrue();
    }

    /// <summary>
    /// Verifies that explicit System.Reactive compatibility chooses only the .Reactive family.
    /// </summary>
    [Test]
    public async Task MigrationPlan_SystemReactive_Compatibility_Uses_Reactive_Variants()
    {
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.CreateMigrationPlan(new MigrationRequest(
            "WPF",
            "public library",
            ["System.Reactive"],
            ["modernize implementation"],
            ["preserve System.Reactive public API and IScheduler"]));

        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Reactive", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Wpf.Reactive", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.CodeActions.Any(static action => action.Contains("compatibility project or boundary", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.Risks.Any(static risk => risk.Contains("intentionally depends on System.Reactive", StringComparison.Ordinal))).IsTrue();
    }

    /// <summary>
    /// Verifies that every supported UI platform receives its matching lean Primitives sequencer package.
    /// </summary>
    [Test]
    public async Task MigrationPlan_Lean_Migrations_Select_Matching_Ui_Primitives_Packages()
    {
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(new EmbeddedKnowledgeCatalog());
        (string Platform, string Package)[] cases =
        [
            ("WinForms", "ReactiveUI.Primitives.WinForms"),
            ("WinUI", "ReactiveUI.Primitives.WinUI"),
            ("Blazor", "ReactiveUI.Primitives.Blazor"),
            ("Avalonia", "ReactiveUI.Primitives.Avalonia"),
            ("MAUI", "ReactiveUI.Primitives.Maui")
        ];

        foreach (var testCase in cases)
        {
            var result = guidance.CreateMigrationPlan(new MigrationRequest(
                testCase.Platform,
                "application",
                ["System.Reactive"],
                ["remove System.Reactive"],
                []));

            await Assert.That(result.PackageActions.Any(action => action.Contains(testCase.Package, StringComparison.Ordinal))).IsTrue();
        }
    }

    /// <summary>
    /// Verifies that optional async migration guidance selects the matching lean async package.
    /// </summary>
    [Test]
    public async Task MigrationPlan_Async_Lean_Migration_Uses_Primitives_Async()
    {
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.CreateMigrationPlan(new MigrationRequest(
            null,
            "library",
            ["System.Reactive"],
            ["remove System.Reactive", "async observable"],
            []));

        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Async", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.PackageActions.Any(static action => action.Contains("ReactiveUI.Primitives.Async.Reactive", StringComparison.Ordinal))).IsFalse();
    }

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
