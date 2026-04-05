using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using ReactiveUIMcp.Core.Services;
using ReactiveUIMcp.Knowledge.Services;

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
}
