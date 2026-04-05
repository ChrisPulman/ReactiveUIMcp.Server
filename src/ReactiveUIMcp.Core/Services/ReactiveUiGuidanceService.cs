using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using System.Text;

namespace ReactiveUIMcp.Core.Services;

/// <summary>
/// Applies lightweight recommendation and review rules over the harvested knowledge catalog.
/// </summary>
public sealed class ReactiveUiGuidanceService(IKnowledgeCatalog catalog) : IReactiveUiGuidanceService
{
    private static readonly string[] s_xamlPlatforms = ["wpf", "maui", "winui", "avalonia", "uno", "androidx"];
    private static readonly string[] s_supportedEndpoints = ["WPF", "WinForms", "Blazor", "MAUI", "WinUI", "Avalonia", "Uno", "AndroidX"];
    private static readonly string[] s_diProviderOptions =
    [
        "Splat",
        "Splat.DependencyInjection.SourceGenerator",
        "Splat.Microsoft.Extensions.DependencyInjection",
        "Splat.Autofac",
        "Splat.DryIoc",
        "Splat.Ninject",
        "Splat.SimpleInjector"
    ];
    private static readonly string[] s_additionalFeatureOptions =
    [
        "ReactiveUI.SourceGenerators",
        "ReactiveUI.Binding.SourceGenerators",
        "ReactiveUI.Extensions.Async",
        "Refit",
        "Akavache",
        "ReactiveUI.Validation",
        "ReactiveUI.Testing",
        "Fusillade",
        "punchclock"
    ];
    private static readonly string[] s_settingsStoreOptions =
    [
        "Akavache SQLite",
        "SQLite Repository",
        "LiteDB Repository",
        "JSON File",
        "In Memory",
        "None"
    ];
    private static readonly string[] s_applicationFeatureOptions =
    [
        "Authentication",
        "Settings Page",
        "Theming",
        "Navigation",
        "Reactive Validation",
        "Offline Sync",
        "Logging",
        "Telemetry",
        "Notifications",
        "Role Management"
    ];
    private static readonly string[] s_validationModes =
    [
        "ReactiveUI.Validation",
        "DataAnnotations",
        "Custom Rules",
        "None"
    ];

    /// <inheritdoc />
    public RecommendationResult Recommend(RecommendationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var manifestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "reactiveui-core",
            "reactiveui-sourcegenerators"
        };

        var signalText = string.Join(' ', [request.Platform ?? string.Empty, request.AppKind ?? string.Empty, .. request.Features, .. request.Constraints, .. request.ExistingLibraries]).ToLowerInvariant();

        AddPlatformManifest(request.Platform, manifestIds);
        AddBySignals(signalText, manifestIds);

        if (IsTestProject(request.AppKind, request.Features, request.ExistingLibraries))
        {
            manifestIds.Add("reactiveui-testing");
        }

        var manifests = manifestIds
            .Select(catalog.GetById)
            .Where(static manifest => manifest is not null)
            .Cast<EcosystemManifest>()
            .ToArray();

        var summary = $"Recommended {manifests.Length} ecosystem areas for {request.Platform ?? "general"} ReactiveUI work.";

        return new RecommendationResult(
            summary,
            manifests.Select(static manifest => manifest.Id).ToArray(),
            Merge(manifests, static manifest => manifest.NuGetPackages),
            Merge(manifests, static manifest => manifest.RecommendedPatterns),
            Merge(manifests, static manifest => manifest.AvoidPatterns),
            Merge(manifests, static manifest => manifest.SetupSteps),
            Merge(manifests, static manifest => manifest.CommonPitfalls),
            Merge(manifests, static manifest => manifest.RelatedLibraries));
    }

    /// <inheritdoc />
    public ReviewResult ReviewPlan(string? platform, string? libraries, string planText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planText);

        var findings = new List<ReviewFinding>();
        var combined = string.Join(' ', platform ?? string.Empty, libraries ?? string.Empty, planText).ToLowerInvariant();
        var recommendedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "reactiveui-core",
            "reactiveui-sourcegenerators"
        };

        AddPlatformManifest(platform, recommendedIds);
        AddBySignals(combined, recommendedIds);

        if (combined.Contains("reactivelist", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "error",
                "RXUI001",
                "The plan references ReactiveList, which is obsolete in modern ReactiveUI guidance.",
                "Use DynamicData with SourceCache<TObject, TKey> or SourceList<T> instead."));

            recommendedIds.Add("dynamicdata");
        }

        if (combined.Contains(".subscribe(", StringComparison.Ordinal) &&
            !combined.Contains("whenactivated", StringComparison.Ordinal) &&
            !combined.Contains("disposewith", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "warning",
                "RXUI002",
                "The plan appears to create subscriptions without view activation/lifetime scoping.",
                "Move UI bindings and subscriptions into WhenActivated and dispose them with DisposeWith."));
        }

        if (!string.IsNullOrWhiteSpace(platform) &&
            s_xamlPlatforms.Any(keyword => platform.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
            !combined.Contains("whenactivated", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "warning",
                "RXUI003",
                $"{platform} code usually needs explicit WhenActivated usage for bindings and subscriptions.",
                "Add WhenActivated and DisposeWith guidance to the generated code plan."));
        }

        if (combined.Contains("fody", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "warning",
                "RXUI004",
                "The plan references Fody-era patterns.",
                "Prefer ReactiveUI.SourceGenerators for new work, and for upgrades create an explicit migration plan from ReactiveUI.Fody to ReactiveUI.SourceGenerators."));
        }

        if (combined.Contains("applocator.current", StringComparison.Ordinal) || combined.Contains("locator.current", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "warning",
                "RXUI005",
                "The plan relies on global service location in feature code.",
                "Keep Splat usage concentrated at the composition root and prefer constructor injection elsewhere."));

            recommendedIds.Add("splat");
        }

        if (combined.Contains("observablecollection", StringComparison.Ordinal) &&
            (combined.Contains("sort", StringComparison.Ordinal) || combined.Contains("filter", StringComparison.Ordinal) || combined.Contains("live updates", StringComparison.Ordinal)))
        {
            findings.Add(new ReviewFinding(
                "info",
                "RXUI006",
                "The plan may benefit from DynamicData for live filtered/sorted collections.",
                "Use SourceCache/SourceList privately and expose a ReadOnlyObservableCollection via Bind(out ...)."));

            recommendedIds.Add("dynamicdata");
        }

        if (combined.Contains("property setter", StringComparison.Ordinal) || combined.Contains("setter side effect", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "warning",
                "RXUI007",
                "The plan suggests imperative setter-side effects for derived state.",
                "Prefer WhenAnyValue pipelines and ObservableAsPropertyHelper or [ObservableAsProperty]."));
        }

        if (combined.Contains("iobservableasync", StringComparison.Ordinal) || combined.Contains("reactiveui.extensions.async", StringComparison.Ordinal))
        {
            findings.Add(new ReviewFinding(
                "info",
                "RXUI008",
                "The plan references ReactiveUI.Extensions.Async patterns.",
                "When async observable semantics are the right fit, use IObservableAsync-based code intentionally rather than improvised Task wrappers."));

            recommendedIds.Add("extensions");
        }

        if (combined.Contains("test", StringComparison.Ordinal) || combined.Contains("reactiveui.testing", StringComparison.Ordinal))
        {
            recommendedIds.Add("reactiveui-testing");
        }

        if (findings.Count == 0)
        {
            findings.Add(new ReviewFinding(
                "info",
                "RXUI000",
                "No obvious high-risk ReactiveUI anti-patterns were detected in the submitted plan.",
                "Still validate scheduler boundaries, command exception handling, disposal behavior, and package alignment during implementation."));
        }

        return new ReviewResult(
            $"Found {findings.Count} ReactiveUI guidance item(s).",
            findings,
            recommendedIds.ToArray());
    }

    /// <inheritdoc />
    public ComparisonResult Compare(string leftId, string rightId)
    {
        var left = catalog.GetById(leftId) ?? throw new InvalidOperationException($"Unknown manifest id '{leftId}'.");
        var right = catalog.GetById(rightId) ?? throw new InvalidOperationException($"Unknown manifest id '{rightId}'.");

        var tradeoffs = new List<string>
        {
            $"{left.DisplayName} is optimized for: {left.Summary}",
            $"{right.DisplayName} is optimized for: {right.Summary}",
            $"Prefer {left.DisplayName} when you need: {string.Join(", ", left.RecommendedPatterns.Take(2))}.",
            $"Prefer {right.DisplayName} when you need: {string.Join(", ", right.RecommendedPatterns.Take(2))}."
        };

        return new ComparisonResult(
            $"Compared {left.DisplayName} with {right.DisplayName}.",
            left.Id,
            right.Id,
            left.NuGetPackages,
            right.NuGetPackages,
            left.RecommendedPatterns,
            right.RecommendedPatterns,
            tradeoffs);
    }

    /// <inheritdoc />
    public string CreateScaffoldPrompt(RecommendationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recommendation = Recommend(request);
        var builder = new StringBuilder();
        builder.AppendLine("Generate production-quality ReactiveUI code that follows the latest ecosystem guidance.");
        builder.AppendLine();
        builder.AppendLine($"Platform: {request.Platform ?? "unspecified"}");
        builder.AppendLine($"Application kind: {request.AppKind ?? "unspecified"}");
        builder.AppendLine($"Features: {Format(request.Features)}");
        builder.AppendLine($"Constraints: {Format(request.Constraints)}");
        builder.AppendLine();
        builder.AppendLine("Required package considerations:");
        foreach (var package in recommendation.SuggestedPackages)
        {
            builder.AppendLine($"- {package}");
        }

        builder.AppendLine();
        builder.AppendLine("Preferred patterns:");
        foreach (var pattern in recommendation.RecommendedPatterns.Take(12))
        {
            builder.AppendLine($"- {pattern}");
        }

        builder.AppendLine();
        builder.AppendLine("Avoid these patterns:");
        foreach (var pattern in recommendation.AvoidPatterns.Take(12))
        {
            builder.AppendLine($"- {pattern}");
        }

        builder.AppendLine();
        builder.AppendLine("Verification checklist:");
        builder.AppendLine("- Commands use ReactiveCommand and surface ThrownExceptions.");
        builder.AppendLine("- UI bindings/subscriptions are scoped with WhenActivated where appropriate.");
        builder.AppendLine("- Derived state uses observable pipelines or OAPH/source-generator equivalents.");
        builder.AppendLine("- Scheduler boundaries are explicit for UI updates.");
        builder.AppendLine("- ReactiveUI.SourceGenerators is the default choice unless an existing codebase requires an explicit migration path.");
        builder.AppendLine("- ReactiveUI.Extensions.Async is used intentionally when async observable semantics are required.");
        builder.AppendLine("- Test projects use ReactiveUI.Testing where reactive behavior and schedulers must be validated.");
        return builder.ToString().TrimEnd();
    }

    /// <inheritdoc />
    public ProjectBlueprintResult CreateProjectBlueprint(RecommendationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recommendation = Recommend(request);
        var structureHints = new List<string>
        {
            "Create a platform host project with a clear composition root.",
            "Keep ViewModels in a separate folder or project and prefer ReactiveUI.SourceGenerators for properties and commands.",
            "Place services/repositories behind interfaces, especially for Akavache, Refit, and sync/network behavior.",
            "Create a dedicated test project and add ReactiveUI.Testing for scheduler-aware tests."
        };

        var codeRules = new List<string>
        {
            "Prefer ReactiveUI.SourceGenerators over ReactiveUI.Fody for new work.",
            "Use WhenActivated and DisposeWith in views where lifecycle scoping matters.",
            "Use DynamicData for live sorted/filtered collections instead of ReactiveList.",
            "Use ReactiveUI.Extensions.Async IObservableAsync-based code when async streaming semantics are a good fit.",
            "Prefer System.Text.Json and typed API abstractions when Refit or Akavache serialization is involved."
        };

        var testRecommendations = new List<string>
        {
            "Add ReactiveUI.Testing to the test project.",
            "Use scheduler-controlled tests instead of wall-clock sleeps.",
            "Test ReactiveCommand canExecute, IsExecuting, ThrownExceptions, and observable state behavior."
        };

        if (IsTestProject(request.AppKind, request.Features, request.ExistingLibraries))
        {
            testRecommendations.Add("If the requested project is itself a test project, start with ReactiveUI.Testing as a first-class package.");
        }

        return new ProjectBlueprintResult(
            $"Created a project blueprint for {request.Platform ?? "general"} ReactiveUI development.",
            recommendation.SuggestedPackages,
            recommendation.SetupSteps,
            structureHints,
            codeRules,
            testRecommendations);
    }

    /// <inheritdoc />
    public MigrationPlanResult CreateMigrationPlan(MigrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var packageActions = new List<string>();
        var codeActions = new List<string>();
        var testActions = new List<string>();
        var validationSteps = new List<string>();
        var risks = new List<string>();

        var current = string.Join(' ', request.CurrentPackages).ToLowerInvariant();
        var goals = string.Join(' ', request.UpgradeGoals).ToLowerInvariant();
        var projectType = request.ProjectType?.ToLowerInvariant() ?? string.Empty;

        packageActions.Add("Align ReactiveUI core and platform package versions first.");

        if (current.Contains("reactiveui.fody", StringComparison.Ordinal) || goals.Contains("fody", StringComparison.Ordinal))
        {
            packageActions.Add("Remove ReactiveUI.Fody and any Fody weaving configuration once equivalent source-generator-based members are in place.");
            packageActions.Add("Add ReactiveUI.SourceGenerators as the primary replacement package.");
            codeActions.Add("Convert woven properties/commands to [Reactive], [ObservableAsProperty], and [ReactiveCommand] on partial types.");
            risks.Add("Mixed Fody and source-generator patterns can create confusing duplicate behavior during migration if not staged carefully.");
        }

        codeActions.Add("Replace obsolete ReactiveList usage with DynamicData SourceCache or SourceList where appropriate.");
        codeActions.Add("Move constructor-created UI subscriptions into WhenActivated and DisposeWith.");
        codeActions.Add("Review scheduler boundaries and ensure UI updates occur on the proper main-thread scheduler.");

        if (current.Contains("extensions", StringComparison.Ordinal) || goals.Contains("async", StringComparison.Ordinal) || goals.Contains("stream", StringComparison.Ordinal))
        {
            codeActions.Add("Use ReactiveUI.Extensions.Async namespaces when migrating or introducing IObservableAsync-based code paths.");
        }

        if (projectType.Contains("test", StringComparison.Ordinal) || current.Contains("reactiveui.testing", StringComparison.Ordinal) || goals.Contains("test", StringComparison.Ordinal))
        {
            packageActions.Add("Add or upgrade ReactiveUI.Testing in the test project.");
            testActions.Add("Replace wall-clock sleeps with scheduler-aware ReactiveUI.Testing or Rx scheduler-based tests.");
            testActions.Add("Update tests for ReactiveCommand canExecute, IsExecuting, ThrownExceptions, and observable state behavior.");
            risks.Add("Legacy tests may silently depend on implicit global scheduler behavior and need explicit scheduler control after migration.");
        }

        validationSteps.Add("Build after each migration slice rather than attempting a single large rewrite.");
        validationSteps.Add("Run the full test suite after package alignment and again after behavioral refactors.");
        validationSteps.Add("Validate generated code for partial types, command wiring, bindings, scheduler usage, disposal behavior, and validation logic.");

        return new MigrationPlanResult(
            $"Created a migration plan for {request.Platform ?? "general"} ReactiveUI {request.ProjectType ?? "project"} modernization.",
            packageActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            codeActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            testActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            validationSteps,
            risks.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    /// <inheritdoc />
    public ReactiveUiSolutionWizardResponse CreateReactiveUiSolutionWizard(CreateReactiveUiSolutionWizardRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var step = NormalizeWizardStep(request.Step);
        var selections = BuildSelections(request);

        return step switch
        {
            "1" => new ReactiveUiSolutionWizardResponse(
                "1",
                "2",
                false,
                "Step 1: name the solution and choose one or more ReactiveUI UI endpoints.",
                [
                    new WizardQuestion(
                        "solutionName",
                        "What is the solution name?",
                        "text",
                        false,
                        [],
                        "Contoso.AppSuite",
                        null),
                    new WizardQuestion(
                        "uiEndpoints",
                        "Select one or more UI endpoints to include in the solution.",
                        "multiselect",
                        true,
                        s_supportedEndpoints,
                        null,
                        "You can combine endpoints such as WPF + MAUI + Blazor in the same solution plan.")
                ],
                selections),
            "2" => new ReactiveUiSolutionWizardResponse(
                "2",
                "3",
                false,
                "Step 2: choose the Splat-based DI provider package to use for composition and service registration.",
                [
                    new WizardQuestion(
                        "diProvider",
                        "Select the DI provider integration to use with Splat.",
                        "single-select",
                        false,
                        s_diProviderOptions,
                        null,
                        "Splat is a direct dependency. Choose the concrete Splat integration package that best matches the solution architecture.")
                ],
                selections),
            "3" => new ReactiveUiSolutionWizardResponse(
                "3",
                "4",
                false,
                "Step 3: choose additional ReactiveUI and companion features beyond the built-in Splat and DynamicData dependencies.",
                [
                    new WizardQuestion(
                        "additionalFeatures",
                        "Select any additional ReactiveUI or companion features to include.",
                        "multiselect",
                        true,
                        s_additionalFeatureOptions,
                        null,
                        "Splat and DynamicData are treated as direct dependencies and are not offered again here. ReactiveUI.SourceGenerators remains the primary recommendation for new code. ReactiveUI.Extensions.Async is available for IObservableAsync-based flows.")
                ],
                selections),
            "4" => new ReactiveUiSolutionWizardResponse(
                "4",
                "5",
                false,
                "Step 4: choose how application settings and local state should be stored.",
                [
                    new WizardQuestion(
                        "settingsStore",
                        "Select the settings or local database store.",
                        "single-select",
                        false,
                        s_settingsStoreOptions,
                        null,
                        "Akavache SQLite is the preferred option when you want a ReactiveUI-friendly database-backed settings store.")
                ],
                selections),
            "5" => new ReactiveUiSolutionWizardResponse(
                "5",
                "6",
                false,
                "Step 5: choose common application features, theming, and validation behavior.",
                [
                    new WizardQuestion(
                        "applicationFeatures",
                        "Select any common application features to scaffold.",
                        "multiselect",
                        true,
                        s_applicationFeatureOptions,
                        null,
                        "Authentication and Settings Page are common starting features. Theming supports prime color selection."),
                    new WizardQuestion(
                        "primaryColors",
                        "List the prime colors or theme colors to seed the solution themes with.",
                        "text",
                        false,
                        [],
                        "#1D4ED8, #0F172A, #F59E0B",
                        null),
                    new WizardQuestion(
                        "validationMode",
                        "Select the validation approach.",
                        "single-select",
                        false,
                        s_validationModes,
                        null,
                        "Use ReactiveUI.Validation when view validation is needed in a ReactiveUI-first way.")
                ],
                selections),
            "6" => new ReactiveUiSolutionWizardResponse(
                "6",
                "7",
                false,
                "Step 6: provide the views required for each UI endpoint so matching views and view models can be scaffolded.",
                [
                    new WizardQuestion(
                        "viewsByEndpoint",
                        "List the views for each endpoint using endpoint:view1,view2|endpoint:view1,view2 syntax.",
                        "text",
                        false,
                        [],
                        "WPF:Login,Dashboard,Settings|MAUI:Login,Home,Settings",
                        "Each endpoint can have its own tailored view set while sharing domain or service layers.")
                ],
                selections),
            "7" => new ReactiveUiSolutionWizardResponse(
                "7",
                "8",
                false,
                "Step 7: review the high-level project blueprint that will be generated from the chosen endpoints and features.",
                [
                    new WizardQuestion(
                        "blueprintReview",
                        "Confirm that the generated project blueprint should include shared core, endpoint projects, and a shared ReactiveUI.Testing test project.",
                        "confirmation",
                        false,
                        ["Yes", "No"],
                        null,
                        "This step exists so AI clients can present the structure before moving to legacy upgrade considerations or completion.")
                ],
                selections),
            "8" => new ReactiveUiSolutionWizardResponse(
                "8",
                "9",
                false,
                "Step 8: optionally identify any legacy migration requirements, such as upgrading from ReactiveUI.Fody or older ReactiveUI.Testing patterns.",
                [
                    new WizardQuestion(
                        "migrationNotes",
                        "List any legacy packages or upgrade goals that should be considered in the generated guidance.",
                        "text",
                        false,
                        [],
                        "ReactiveUI.Fody, legacy scheduler tests",
                        "Use this step when the new solution is also intended to help migrate existing applications or tests.")
                ],
                selections),
            _ => BuildCompletedWizardResponse(request, selections),
        };
    }

    private static string NormalizeWizardStep(string? step)
    {
        if (string.IsNullOrWhiteSpace(step))
        {
            return "1";
        }

        var value = step.Trim().ToLowerInvariant();
        return value switch
        {
            "start" => "1",
            "1" => "1",
            "di" => "2",
            "2" => "2",
            "features" => "3",
            "3" => "3",
            "storage" => "4",
            "4" => "4",
            "application" => "5",
            "5" => "5",
            "views" => "6",
            "6" => "6",
            "blueprint" => "7",
            "7" => "7",
            "migration" => "8",
            "8" => "8",
            "complete" => "9",
            "9" => "9",
            _ => "9",
        };
    }

    private ReactiveUiSolutionWizardResponse BuildCompletedWizardResponse(
        CreateReactiveUiSolutionWizardRequest request,
        IReadOnlyDictionary<string, string> selections)
    {
        var featureList = new List<string>();
        featureList.AddRange(request.AdditionalFeatures);
        featureList.AddRange(request.ApplicationFeatures);

        if (!string.IsNullOrWhiteSpace(request.ValidationMode))
        {
            featureList.Add(request.ValidationMode!);
        }

        if (string.Equals(request.SettingsStore, "Akavache SQLite", StringComparison.OrdinalIgnoreCase))
        {
            featureList.Add("Akavache");
        }

        if (!string.IsNullOrWhiteSpace(request.DiProvider) &&
            !string.Equals(request.DiProvider, "Splat", StringComparison.OrdinalIgnoreCase))
        {
            featureList.Add(request.DiProvider);
        }

        var features = featureList
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingLibraries = new List<string>(request.AdditionalFeatures)
        {
            "Splat",
            "DynamicData"
        };

        if (!string.IsNullOrWhiteSpace(request.DiProvider))
        {
            existingLibraries.Add(request.DiProvider!);
        }

        var appKind = request.UiEndpoints.Count > 1 ? "multi-endpoint application" : "application";
        var recommendation = Recommend(new RecommendationRequest(
            request.UiEndpoints.Count > 0 ? string.Join(", ", request.UiEndpoints) : null,
            appKind,
            features,
            [],
            existingLibraries));

        var blueprint = CreateProjectBlueprint(new RecommendationRequest(
            request.UiEndpoints.Count > 0 ? string.Join(", ", request.UiEndpoints) : null,
            appKind,
            features,
            [],
            existingLibraries));

        var projects = BuildProjectPlans(request, recommendation);
        var views = BuildViewScaffolds(request);

        var summary = $"Prepared a ReactiveUI solution wizard result for {request.SolutionName ?? "UnnamedSolution"} with {request.UiEndpoints.Count} UI endpoint(s).";

        return new ReactiveUiSolutionWizardResponse(
            "9",
            "9",
            true,
            summary,
            [],
            selections,
            blueprint,
            projects,
            views);
    }

    private static IReadOnlyDictionary<string, string> BuildSelections(CreateReactiveUiSolutionWizardRequest request)
    {
        var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["step"] = NormalizeWizardStep(request.Step),
            ["solutionName"] = request.SolutionName ?? string.Empty,
            ["uiEndpoints"] = string.Join(", ", request.UiEndpoints),
            ["diProvider"] = request.DiProvider ?? string.Empty,
            ["additionalFeatures"] = string.Join(", ", request.AdditionalFeatures),
            ["settingsStore"] = request.SettingsStore ?? string.Empty,
            ["applicationFeatures"] = string.Join(", ", request.ApplicationFeatures),
            ["primaryColors"] = request.PrimaryColors ?? string.Empty,
            ["validationMode"] = request.ValidationMode ?? string.Empty,
            ["viewsByEndpoint"] = request.ViewsByEndpoint ?? string.Empty,
        };

        return selections;
    }

    private static IReadOnlyList<ReactiveUiSolutionProjectPlan> BuildProjectPlans(
        CreateReactiveUiSolutionWizardRequest request,
        RecommendationResult recommendation)
    {
        var coreNotes = new List<string>
        {
            "Prefer ReactiveUI.SourceGenerators for shared ViewModels.",
            "Treat Splat as the foundational dependency for composition."
        };

        if (!string.IsNullOrWhiteSpace(request.DiProvider))
        {
            coreNotes.Add($"Use {request.DiProvider} as the selected DI integration package.");
        }

        var projects = new List<ReactiveUiSolutionProjectPlan>
        {
            new(
                $"{request.SolutionName ?? "Solution"}.Core",
                "Shared domain models, services, and ViewModels used across UI endpoints.",
                "classlib",
                request.UiEndpoints,
                recommendation.SuggestedPackages,
                coreNotes)
        };

        foreach (var endpoint in request.UiEndpoints)
        {
            projects.Add(new ReactiveUiSolutionProjectPlan(
                $"{request.SolutionName ?? "Solution"}.{endpoint}",
                $"{endpoint} UI endpoint project.",
                endpoint,
                [endpoint],
                recommendation.SuggestedPackages.Where(package => package.Contains(endpoint, StringComparison.OrdinalIgnoreCase) || package.StartsWith("ReactiveUI", StringComparison.OrdinalIgnoreCase)).ToArray(),
                ["Use WhenActivated and endpoint-specific reactive view base types."]));
        }

        projects.Add(new ReactiveUiSolutionProjectPlan(
            $"{request.SolutionName ?? "Solution"}.Tests",
            "Shared test project for reactive behavior and scheduler-aware tests.",
            "test project",
            request.UiEndpoints,
            ["ReactiveUI.Testing"],
            ["Use scheduler-aware tests instead of wall-clock sleeps."]));

        return projects;
    }

    private static IReadOnlyList<ReactiveUiViewScaffold> BuildViewScaffolds(CreateReactiveUiSolutionWizardRequest request)
    {
        var results = new List<ReactiveUiViewScaffold>();
        if (string.IsNullOrWhiteSpace(request.ViewsByEndpoint))
        {
            return results;
        }

        foreach (var mapping in request.ViewsByEndpoint.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = mapping.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var endpoint = parts[0];
            var views = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var view in views)
            {
                results.Add(new ReactiveUiViewScaffold(
                    endpoint,
                    view,
                    $"{view}ViewModel",
                    [
                        $"Views/{view}View.*",
                        $"ViewModels/{view}ViewModel.cs"
                    ],
                    [
                        "Prefer ReactiveUI.SourceGenerators in the ViewModel.",
                        "Add WhenActivated bindings in the view.",
                        string.Equals(request.ValidationMode, "ReactiveUI.Validation", StringComparison.OrdinalIgnoreCase)
                            ? "Include ReactiveUI.Validation rules when the view captures user input."
                            : "Add validation only when required by the selected approach."
                    ]));
            }
        }

        return results;
    }

    private static bool IsTestProject(string? appKind, IReadOnlyList<string> features, IReadOnlyList<string> existingLibraries)
    {
        var combined = string.Join(' ', appKind ?? string.Empty, string.Join(' ', features), string.Join(' ', existingLibraries));
        return combined.Contains("test", StringComparison.OrdinalIgnoreCase) || combined.Contains("reactiveui.testing", StringComparison.OrdinalIgnoreCase);
    }

    private static string Format(IReadOnlyList<string> values) => values.Count == 0 ? "none" : string.Join(", ", values);

    private static IReadOnlyList<string> Merge(IEnumerable<EcosystemManifest> manifests, Func<EcosystemManifest, IReadOnlyList<string>> selector) =>
        manifests
            .SelectMany(selector)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static void AddPlatformManifest(string? platform, ISet<string> manifestIds)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            return;
        }

        var key = platform.Trim().ToLowerInvariant();
        if (key.Contains("wpf", StringComparison.Ordinal)) manifestIds.Add("reactiveui-wpf");
        if (key.Contains("winforms", StringComparison.Ordinal)) manifestIds.Add("reactiveui-winforms");
        if (key.Contains("blazor", StringComparison.Ordinal)) manifestIds.Add("reactiveui-blazor");
        if (key.Contains("maui", StringComparison.Ordinal)) manifestIds.Add("reactiveui-maui");
        if (key.Contains("winui", StringComparison.Ordinal)) manifestIds.Add("reactiveui-winui");
        if (key.Contains("android", StringComparison.Ordinal)) manifestIds.Add("reactiveui-androidx");
        if (key.Contains("avalonia", StringComparison.Ordinal)) manifestIds.Add("reactiveui-avalonia");
        if (key.Contains("uno", StringComparison.Ordinal)) manifestIds.Add("reactiveui-uno");
    }

    private static void AddBySignals(string signalText, ISet<string> manifestIds)
    {
        if (ContainsAny(signalText, "validation", "form validation", "errors")) manifestIds.Add("reactiveui-validation");
        if (ContainsAny(signalText, "http", "api", "rest", "refit")) manifestIds.Add("refit");
        if (ContainsAny(signalText, "cache", "offline", "persist", "sqlite", "sync", "settings store", "settings")) manifestIds.Add("akavache");
        if (ContainsAny(signalText, "aot", "trim", "trimming", "binding", "bind")) manifestIds.Add("reactiveui-binding-sourcegenerators");
        if (ContainsAny(signalText, "dynamicdata", "collections", "sourcecache", "sourcelist", "live updates")) manifestIds.Add("dynamicdata");
        if (ContainsAny(signalText, "retry", "backoff", "heartbeat", "conflate", "extensions", "iobservableasync", "reactiveui.extensions.async")) manifestIds.Add("extensions");
        if (ContainsAny(signalText, "splat", "di", "ioc", "composition root", "autofac", "dryioc", "ninject", "simpleinjector", "microsoft.extensions.dependencyinjection")) manifestIds.Add("splat");
        if (ContainsAny(signalText, "priority", "prioritization", "dedup", "network queue", "speculative"))
        {
            manifestIds.Add("fusillade");
            manifestIds.Add("punchclock");
        }

        if (ContainsAny(signalText, "test", "testing", "reactiveui.testing", "scheduler test"))
        {
            manifestIds.Add("reactiveui-testing");
        }

        if (ContainsAny(signalText, "fody", "legacy", "upgrade", "migration"))
        {
            manifestIds.Add("reactiveui-sourcegenerators");
        }
    }

    private static bool ContainsAny(string source, params string[] values) =>
        values.Any(value => source.Contains(value, StringComparison.OrdinalIgnoreCase));
}
