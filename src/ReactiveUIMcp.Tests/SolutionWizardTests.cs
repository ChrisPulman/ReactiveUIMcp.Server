namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for the Create ReactiveUI Solution wizard flow.
/// </summary>
public class SolutionWizardTests
{
    /// <summary>
    /// Verifies that the wizard starts by asking for the solution name and UI endpoints.
    /// </summary>
    [Test]
    public async Task Wizard_Start_Returns_Project_Type_And_Endpoint_Questions()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "1",
            null,
            [],
            null,
            [],
            null,
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("1");
        await Assert.That(result.Questions.Any(question => question.Id == "solutionName")).IsTrue();
        await Assert.That(result.Questions.Any(question => question.Id == "uiEndpoints")).IsTrue();
    }

    /// <summary>
    /// Verifies that wizard step 2 asks for the Splat DI provider choice.
    /// </summary>
    [Test]
    public async Task Wizard_Step2_Asks_For_Splat_Di_Provider()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "2",
            "Contoso.AppSuite",
            ["WPF", "MAUI"],
            null,
            [],
            null,
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("2");
        await Assert.That(result.Questions.Any(question => question.Id == "diProvider")).IsTrue();
        await Assert.That(result.Questions.Single(question => question.Id == "diProvider").Options).Contains("Splat.Microsoft.Extensions.DependencyInjection");
    }

    /// <summary>
    /// Verifies that wizard step 3 asks for additional ReactiveUI features.
    /// </summary>
    [Test]
    public async Task Wizard_Step3_Asks_For_Additional_Features()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "3",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            null,
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("3");
        await Assert.That(result.Questions.Any(question => question.Id == "additionalFeatures")).IsTrue();
        await Assert.That(result.Questions.Single(question => question.Id == "additionalFeatures").Options).Contains("ReactiveUI.SourceGenerators");
    }

    /// <summary>
    /// Verifies that wizard step 4 asks for the settings store option.
    /// </summary>
    [Test]
    public async Task Wizard_Step4_Asks_For_Settings_Store()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "4",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            null,
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("4");
        await Assert.That(result.Questions.Any(question => question.Id == "settingsStore")).IsTrue();
        await Assert.That(result.Questions.Single(question => question.Id == "settingsStore").Options).Contains("Akavache SQLite");
    }

    /// <summary>
    /// Verifies that wizard step 5 asks for application features, primary colors, and validation mode.
    /// </summary>
    [Test]
    public async Task Wizard_Step5_Asks_For_Application_Features_Colors_And_Validation()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "5",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            "Akavache SQLite",
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("5");
        await Assert.That(result.Questions.Any(question => question.Id == "applicationFeatures")).IsTrue();
        await Assert.That(result.Questions.Any(question => question.Id == "primaryColors")).IsTrue();
        await Assert.That(result.Questions.Any(question => question.Id == "validationMode")).IsTrue();
        await Assert.That(result.Questions.Single(question => question.Id == "validationMode").Options).Contains("ReactiveUI.Validation");
    }

    /// <summary>
    /// Verifies that wizard step 6 asks for the views-by-endpoint mapping.
    /// </summary>
    [Test]
    public async Task Wizard_Step6_Asks_For_ViewsByEndpoint()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "6",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            "None",
            ["Authentication"],
            null,
            "ReactiveUI.Validation",
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("6");
        await Assert.That(result.Questions.Any(question => question.Id == "viewsByEndpoint")).IsTrue();
    }

    /// <summary>
    /// Verifies that wizard step 7 returns a blueprint-review confirmation question.
    /// </summary>
    [Test]
    public async Task Wizard_Step7_Returns_Blueprint_Review_Question()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "7",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            "None",
            [],
            null,
            null,
            "WPF:Login,Dashboard"));

        await Assert.That(result.CurrentStep).IsEqualTo("7");
        await Assert.That(result.Questions.Any(question => question.Id == "blueprintReview")).IsTrue();
    }

    /// <summary>
    /// Verifies that wizard step 8 asks for any legacy migration notes.
    /// </summary>
    [Test]
    public async Task Wizard_Step8_Asks_For_Migration_Notes()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "8",
            "Contoso.AppSuite",
            ["WPF"],
            "Splat",
            [],
            "None",
            [],
            null,
            null,
            "WPF:Login,Dashboard"));

        await Assert.That(result.CurrentStep).IsEqualTo("8");
        await Assert.That(result.Questions.Any(question => question.Id == "migrationNotes")).IsTrue();
    }

    /// <summary>
    /// Verifies that the completed wizard returns a multi-endpoint blueprint with view scaffolds and test guidance.
    /// </summary>
    [Test]
    public async Task Wizard_Complete_Returns_Blueprint_Projects_And_ViewScaffolds()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "9",
            "Contoso.AppSuite",
            ["WPF", "MAUI"],
            "Splat.Microsoft.Extensions.DependencyInjection",
            ["ReactiveUI.SourceGenerators", "ReactiveUI.Extensions.Async", "ReactiveUI.Testing"],
            "Akavache SQLite",
            ["Authentication", "Settings Page", "Theming", "Reactive Validation"],
            "#1D4ED8,#0F172A,#F59E0B",
            "ReactiveUI.Validation",
            "WPF:Login,Dashboard,Settings|MAUI:Login,Home,Settings"));

        await Assert.That(result.IsComplete).IsTrue();
        await Assert.That(result.Blueprint).IsNotNull();
        await Assert.That(result.Projects).IsNotNull();
        await Assert.That(result.ViewScaffolds).IsNotNull();
        await Assert.That(result.Projects!.Any(project => project.Name.Contains("WPF", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.Projects.Any(project => project.Name.Contains("MAUI", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.Projects.Any(project => project.Name.EndsWith(".Tests", StringComparison.Ordinal))).IsTrue();
        await Assert.That(result.Projects.Any(project => project.Notes.Any(note => note.Contains("Splat.Microsoft.Extensions.DependencyInjection", StringComparison.Ordinal)))).IsTrue();
        await Assert.That(result.ViewScaffolds!.Any(view => view.ViewName == "Login")).IsTrue();
        await Assert.That(result.Blueprint!.SuggestedPackages).Contains("ReactiveUI.Testing");
    }

    /// <summary>
    /// Verifies that the completed wizard response records all wizard selections in the Selections dictionary.
    /// </summary>
    [Test]
    public async Task Wizard_Complete_Captures_All_Selections_In_Response()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            "9",
            "Acme.Suite",
            ["Blazor"],
            "Splat",
            [],
            "JSON File",
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentSelections["solutionName"]).IsEqualTo("Acme.Suite");
        await Assert.That(result.CurrentSelections["diProvider"]).IsEqualTo("Splat");
        await Assert.That(result.CurrentSelections["settingsStore"]).IsEqualTo("JSON File");
    }

    /// <summary>
    /// Verifies that a null/missing step value defaults to step 1.
    /// </summary>
    [Test]
    public async Task Wizard_Null_Step_Defaults_To_Step1()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveUiGuidanceService guidance = new ReactiveUiGuidanceService(catalog);

        var result = guidance.CreateReactiveUiSolutionWizard(new CreateReactiveUiSolutionWizardRequest(
            null,
            null,
            [],
            null,
            [],
            null,
            [],
            null,
            null,
            null));

        await Assert.That(result.CurrentStep).IsEqualTo("1");
        await Assert.That(result.IsComplete).IsFalse();
    }
}
