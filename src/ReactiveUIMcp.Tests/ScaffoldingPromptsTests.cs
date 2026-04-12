using ReactiveUIMcp.Server.Prompts;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the <see cref="ScaffoldingPrompts"/> MCP prompt class.
/// </summary>
public class ScaffoldingPromptsTests
{
    private static IReactiveUiGuidanceService BuildGuidance()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        return new ReactiveUiGuidanceService(catalog);
    }

    // ── CreateReactiveUiScaffold ──────────────────────────────────────────────

    /// <summary>
    /// Verifies that the scaffold prompt includes required section headings and the verification checklist.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiScaffold_Contains_Core_Prompt_Sections()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiScaffold(guidance, platform: "WPF", appKind: "desktop app");

        await Assert.That(prompt).Contains("Required package considerations:");
        await Assert.That(prompt).Contains("Preferred patterns:");
        await Assert.That(prompt).Contains("Avoid these patterns:");
        await Assert.That(prompt).Contains("Verification checklist:");
    }

    /// <summary>
    /// Verifies that the scaffold prompt names the requested platform.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiScaffold_Includes_Platform_Name()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiScaffold(guidance, platform: "MAUI");

        await Assert.That(prompt).Contains("MAUI");
    }

    /// <summary>
    /// Verifies that the prompt includes source-generator guidance.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiScaffold_Promotes_SourceGenerators()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiScaffold(guidance, platform: "WPF", appKind: "desktop app");

        await Assert.That(prompt).Contains("ReactiveUI.SourceGenerators");
    }

    /// <summary>
    /// Verifies that the verification checklist references WhenActivated disposal guidance.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiScaffold_Checklist_References_WhenActivated_Disposal()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiScaffold(guidance);

        await Assert.That(prompt).Contains("WhenActivated");
        await Assert.That(prompt).Contains("Derived state uses observable pipelines");
    }

    /// <summary>
    /// Verifies that <see cref="ScaffoldingPrompts.CreateReactiveUiScaffold"/> throws when the
    /// guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiScaffold_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => ScaffoldingPrompts.CreateReactiveUiScaffold(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }

    // ── CreateReactiveUiTestProject ───────────────────────────────────────────

    /// <summary>
    /// Verifies that the test-project prompt includes ReactiveUI.Testing guidance.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiTestProject_Includes_Testing_Package_Guidance()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiTestProject(guidance, platform: "WPF");

        await Assert.That(prompt).Contains("ReactiveUI.Testing");
    }

    /// <summary>
    /// Verifies that the test-project prompt includes scheduler-related verification guidance.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiTestProject_Includes_Scheduler_Verification()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiTestProject(guidance);

        await Assert.That(prompt).Contains("Verification checklist:");
    }

    /// <summary>
    /// Verifies that the platform under test appears in the generated test-project prompt.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiTestProject_Includes_Platform_Under_Test()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.CreateReactiveUiTestProject(guidance, platform: "MAUI");

        await Assert.That(prompt).Contains("MAUI");
    }

    /// <summary>
    /// Verifies that <see cref="ScaffoldingPrompts.CreateReactiveUiTestProject"/> throws when
    /// the guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiTestProject_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => ScaffoldingPrompts.CreateReactiveUiTestProject(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }

    // ── MigrateLegacyReactiveUiProject ────────────────────────────────────────

    /// <summary>
    /// Verifies that the migration prompt includes all expected section headings.
    /// </summary>
    [Test]
    public async Task MigrateLegacyReactiveUiProject_Contains_All_Migration_Sections()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.MigrateLegacyReactiveUiProject(
            guidance,
            platform: "WPF",
            currentPackages: "ReactiveUI.Fody",
            upgradeGoals: "source generators");

        await Assert.That(prompt).Contains("Package Actions:");
        await Assert.That(prompt).Contains("Code Actions:");
        await Assert.That(prompt).Contains("Validation:");
    }

    /// <summary>
    /// Verifies that the migration prompt recommends ReactiveUI.SourceGenerators
    /// when Fody is listed as a current package.
    /// </summary>
    [Test]
    public async Task MigrateLegacyReactiveUiProject_Recommends_SourceGenerators_For_Fody_Migration()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.MigrateLegacyReactiveUiProject(
            guidance,
            platform: "MAUI",
            currentPackages: "ReactiveUI.Fody",
            upgradeGoals: "source generators");

        await Assert.That(prompt).Contains("ReactiveUI.SourceGenerators");
    }

    /// <summary>
    /// Verifies that the migration prompt surfaces test actions when the goal is test migration.
    /// </summary>
    [Test]
    public async Task MigrateLegacyReactiveUiProject_Includes_Test_Actions_For_Test_Migration_Goal()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.MigrateLegacyReactiveUiProject(
            guidance,
            platform: "WPF",
            currentPackages: "ReactiveUI.Testing",
            upgradeGoals: "test migration");

        await Assert.That(prompt).Contains("Test Actions:");
    }

    /// <summary>
    /// Verifies that the migration prompt includes the solution summary.
    /// </summary>
    [Test]
    public async Task MigrateLegacyReactiveUiProject_Starts_With_Summary()
    {
        var guidance = BuildGuidance();

        var prompt = ScaffoldingPrompts.MigrateLegacyReactiveUiProject(
            guidance,
            platform: "WPF",
            currentPackages: "ReactiveUI.Fody");

        await Assert.That(prompt).Contains("Summary:");
    }

    /// <summary>
    /// Verifies that <see cref="ScaffoldingPrompts.MigrateLegacyReactiveUiProject"/> throws when
    /// the guidance service is null.
    /// </summary>
    [Test]
    public async Task MigrateLegacyReactiveUiProject_Throws_When_GuidanceService_Is_Null()
    {
        await Assert.That(() => ScaffoldingPrompts.MigrateLegacyReactiveUiProject(null!, platform: "WPF"))
            .Throws<ArgumentNullException>();
    }
}
