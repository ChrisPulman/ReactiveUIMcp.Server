
using ReactiveUIMcp.Core.Models;

namespace ReactiveUIMcp.Core.Abstractions;
/// <summary>
/// Produces recommendations, comparisons, and review findings from the knowledge catalog.
/// </summary>
public interface IReactiveUiGuidanceService
{
    /// <summary>
    /// Builds a recommendation for a requested stack.
    /// </summary>
    /// <param name="request">The recommendation request.</param>
    /// <returns>A structured recommendation result.</returns>
    RecommendationResult Recommend(RecommendationRequest request);

    /// <summary>
    /// Reviews a proposed implementation plan against known ReactiveUI guidance.
    /// </summary>
    /// <param name="platform">The target platform.</param>
    /// <param name="libraries">Optional library list.</param>
    /// <param name="planText">The plan or code-generation guidance to review.</param>
    /// <returns>A structured review result.</returns>
    ReviewResult ReviewPlan(string? platform, string? libraries, string planText);

    /// <summary>
    /// Compares two ecosystem areas side-by-side.
    /// </summary>
    /// <param name="leftId">The left manifest identifier.</param>
    /// <param name="rightId">The right manifest identifier.</param>
    /// <returns>The comparison payload.</returns>
    ComparisonResult Compare(string leftId, string rightId);

    /// <summary>
    /// Creates a detailed implementation prompt for external AI code generators.
    /// </summary>
    /// <param name="request">The recommendation request.</param>
    /// <returns>A prompt containing package, pattern, and verification guidance.</returns>
    string CreateScaffoldPrompt(RecommendationRequest request);

    /// <summary>
    /// Creates a project-generation plan for a new ReactiveUI application or library.
    /// </summary>
    /// <param name="request">The recommendation request.</param>
    /// <returns>A structured project generation plan.</returns>
    ProjectBlueprintResult CreateProjectBlueprint(RecommendationRequest request);

    /// <summary>
    /// Creates a migration plan for legacy ReactiveUI, ReactiveUI.Fody, or outdated test projects.
    /// </summary>
    /// <param name="request">The migration request.</param>
    /// <returns>A structured migration plan.</returns>
    MigrationPlanResult CreateMigrationPlan(MigrationRequest request);

    /// <summary>
    /// Creates a wizard-oriented response for planning a new ReactiveUI solution.
    /// </summary>
    /// <param name="request">The current wizard request.</param>
    /// <returns>A response containing the next wizard step or the final blueprint.</returns>
    ReactiveUiSolutionWizardResponse CreateReactiveUiSolutionWizard(CreateReactiveUiSolutionWizardRequest request);
}
