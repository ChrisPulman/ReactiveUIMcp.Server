using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Server.Services;
using ReactiveUIMcp.Server.Tools;
using System.Text.Json;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the <see cref="SolutionWizardTools"/> MCP tool class,
/// exercising every wizard step and the file-generation path.
/// </summary>
public class SolutionWizardToolsTests
{
    private static (IReactiveUiGuidanceService guidance, IReactiveUiSolutionScaffolder scaffolder) BuildServices()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);
        IReactiveUiSolutionScaffolder scaffolder = new ReactiveUiSolutionScaffolder();
        return (guidance, scaffolder);
    }

    // ── Step 1 / start ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 1 returns solutionName and uiEndpoints questions.
    /// </summary>
    [Test]
    public async Task Step1_Returns_SolutionName_And_UiEndpoints_Questions()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "1");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("1");
        await Assert.That(doc.RootElement.GetProperty("isComplete").GetBoolean()).IsFalse();
        await Assert.That(questions).Contains("solutionName");
        await Assert.That(questions).Contains("uiEndpoints");
    }

    /// <summary>
    /// Verifies that the alias "start" resolves to wizard step 1.
    /// </summary>
    [Test]
    public async Task Step_Alias_Start_Resolves_To_Step1()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "start");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("1");
    }

    // ── Step 2 / di ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 2 returns the DI provider question with the expected options.
    /// </summary>
    [Test]
    public async Task Step2_Returns_DiProvider_Question_With_Options()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder,
            step: "2",
            solutionName: "MyApp",
            uiEndpoints: "WPF");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions").EnumerateArray().ToList();
        var diQuestion = questions.Single(q => q.GetProperty("id").GetString() == "diProvider");

        var options = diQuestion.GetProperty("options")
            .EnumerateArray()
            .Select(static el => el.GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("2");
        await Assert.That(options).Contains("Splat.Microsoft.Extensions.DependencyInjection");
    }

    /// <summary>
    /// Verifies that the alias "di" resolves to wizard step 2.
    /// </summary>
    [Test]
    public async Task Step_Alias_Di_Resolves_To_Step2()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "di");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("2");
    }

    // ── Step 3 / features ────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 3 asks for additional ReactiveUI features.
    /// </summary>
    [Test]
    public async Task Step3_Returns_AdditionalFeatures_Question()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "3");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("3");
        await Assert.That(questions).Contains("additionalFeatures");
    }

    /// <summary>
    /// Verifies that the alias "features" resolves to wizard step 3.
    /// </summary>
    [Test]
    public async Task Step_Alias_Features_Resolves_To_Step3()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "features");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("3");
    }

    // ── Step 4 / storage ─────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 4 asks for the settings store option.
    /// </summary>
    [Test]
    public async Task Step4_Returns_SettingsStore_Question()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "4");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("4");
        await Assert.That(questions).Contains("settingsStore");
    }

    /// <summary>
    /// Verifies that the alias "storage" resolves to wizard step 4.
    /// </summary>
    [Test]
    public async Task Step_Alias_Storage_Resolves_To_Step4()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "storage");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("4");
    }

    // ── Step 5 / application ─────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 5 asks for application features and primary colors.
    /// </summary>
    [Test]
    public async Task Step5_Returns_ApplicationFeatures_And_Colors_Questions()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "5");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("5");
        await Assert.That(questions).Contains("applicationFeatures");
        await Assert.That(questions).Contains("primaryColors");
        await Assert.That(questions).Contains("validationMode");
    }

    /// <summary>
    /// Verifies that the alias "application" resolves to wizard step 5.
    /// </summary>
    [Test]
    public async Task Step_Alias_Application_Resolves_To_Step5()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "application");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("5");
    }

    // ── Step 6 / views ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 6 asks for views-by-endpoint configuration.
    /// </summary>
    [Test]
    public async Task Step6_Returns_ViewsByEndpoint_Question()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "6");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("6");
        await Assert.That(questions).Contains("viewsByEndpoint");
    }

    /// <summary>
    /// Verifies that the alias "views" resolves to wizard step 6.
    /// </summary>
    [Test]
    public async Task Step_Alias_Views_Resolves_To_Step6()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "views");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("6");
    }

    // ── Step 7 / blueprint ───────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 7 returns a blueprint-review confirmation question.
    /// </summary>
    [Test]
    public async Task Step7_Returns_BlueprintReview_Question()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "7");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("7");
        await Assert.That(questions).Contains("blueprintReview");
    }

    /// <summary>
    /// Verifies that the alias "blueprint" resolves to wizard step 7.
    /// </summary>
    [Test]
    public async Task Step_Alias_Blueprint_Resolves_To_Step7()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "blueprint");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("7");
    }

    // ── Step 8 / migration ───────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 8 returns a migration-notes question.
    /// </summary>
    [Test]
    public async Task Step8_Returns_MigrationNotes_Question()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "8");
        using var doc = JsonDocument.Parse(json);

        var questions = doc.RootElement.GetProperty("questions")
            .EnumerateArray()
            .Select(static el => el.GetProperty("id").GetString())
            .ToList();

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("8");
        await Assert.That(questions).Contains("migrationNotes");
    }

    /// <summary>
    /// Verifies that the alias "migration" resolves to wizard step 8.
    /// </summary>
    [Test]
    public async Task Step_Alias_Migration_Resolves_To_Step8()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "migration");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("8");
    }

    // ── Step 9 / complete ────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that step 9 returns a completed response with blueprint, projects, and view scaffolds.
    /// </summary>
    [Test]
    public async Task Step9_Returns_Complete_Blueprint_With_Projects_And_Views()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder,
            step: "9",
            solutionName: "Acme.Solution",
            uiEndpoints: "WPF,MAUI",
            diProvider: "Splat.Microsoft.Extensions.DependencyInjection",
            additionalFeatures: "ReactiveUI.SourceGenerators,ReactiveUI.Testing",
            settingsStore: "Akavache SQLite",
            applicationFeatures: "Authentication,Settings Page",
            validationMode: "ReactiveUI.Validation",
            viewsByEndpoint: "WPF:Login,Dashboard|MAUI:Login,Home");
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        await Assert.That(root.GetProperty("isComplete").GetBoolean()).IsTrue();
        await Assert.That(root.TryGetProperty("blueprint", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("projects", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("viewScaffolds", out _)).IsTrue();
        await Assert.That(root.GetProperty("projects").GetArrayLength()).IsGreaterThan(0);
        await Assert.That(root.GetProperty("viewScaffolds").GetArrayLength()).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that the alias "complete" resolves to wizard step 9.
    /// </summary>
    [Test]
    public async Task Step_Alias_Complete_Resolves_To_Step9()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: "complete");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("isComplete").GetBoolean()).IsTrue();
    }

    /// <summary>
    /// Verifies that a null/missing step defaults to step 1.
    /// </summary>
    [Test]
    public async Task Step_Null_Defaults_To_Step1()
    {
        var (guidance, scaffolder) = BuildServices();

        var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder, step: null);
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("currentStep").GetString()).IsEqualTo("1");
    }

    // ── generateFiles = true ─────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <c>generateFiles = true</c> on step 9 produces a solution skeleton on disk and
    /// embeds the output path in the response summary.
    /// </summary>
    [Test]
    public async Task Step9_With_GenerateFiles_Creates_Solution_On_Disk()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "RxMcpWizardToolTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var (guidance, scaffolder) = BuildServices();

            var json = SolutionWizardTools.CreateReactiveUiSolution(guidance, scaffolder,
                step: "9",
                solutionName: "Wizard.Generated",
                uiEndpoints: "WPF",
                diProvider: "Splat",
                additionalFeatures: "ReactiveUI.SourceGenerators,ReactiveUI.Testing",
                settingsStore: "JSON File",
                applicationFeatures: "Authentication",
                viewsByEndpoint: "WPF:Login,Dashboard",
                outputRoot: tempRoot,
                generateFiles: true);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            await Assert.That(root.GetProperty("isComplete").GetBoolean()).IsTrue();

            var summary = root.GetProperty("summary").GetString();
            await Assert.That(summary).Contains("Generated solution files under");
            await Assert.That(Directory.Exists(tempRoot)).IsTrue();
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    // ── Null guards ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="SolutionWizardTools.CreateReactiveUiSolution"/> throws when
    /// the guidance service is null.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiSolution_Throws_When_GuidanceService_Is_Null()
    {
        IReactiveUiSolutionScaffolder scaffolder = new ReactiveUiSolutionScaffolder();

        await Assert.That(() => SolutionWizardTools.CreateReactiveUiSolution(null!, scaffolder, step: "1"))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="SolutionWizardTools.CreateReactiveUiSolution"/> throws when
    /// the scaffolder is null.
    /// </summary>
    [Test]
    public async Task CreateReactiveUiSolution_Throws_When_Scaffolder_Is_Null()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        await Assert.That(() => SolutionWizardTools.CreateReactiveUiSolution(guidance, null!, step: "1"))
            .Throws<ArgumentNullException>();
    }
}
