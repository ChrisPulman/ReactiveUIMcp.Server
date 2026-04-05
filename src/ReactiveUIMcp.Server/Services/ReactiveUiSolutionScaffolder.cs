namespace ReactiveUIMcp.Server.Services;

using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using System.Text;

/// <summary>
/// Generates a multi-project ReactiveUI solution skeleton from wizard selections.
/// </summary>
public sealed class ReactiveUiSolutionScaffolder : IReactiveUiSolutionScaffolder
{
    /// <inheritdoc />
    public GeneratedReactiveUiSolutionResult Generate(CreateReactiveUiSolutionWizardRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SolutionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputRoot);

        var createdFiles = new List<string>();
        var notes = new List<string>();
        var parsedViews = ParseViews(request);

        var solutionRoot = Path.Combine(request.OutputRoot!, request.SolutionName!);
        var srcRoot = Path.Combine(solutionRoot, "src");

        Directory.CreateDirectory(solutionRoot);
        Directory.CreateDirectory(srcRoot);

        WriteFile(Path.Combine(solutionRoot, "README.md"), CreateRootReadme(request), createdFiles);
        WriteFile(Path.Combine(solutionRoot, "global.json"), CreateGlobalJson(), createdFiles);
        WriteFile(Path.Combine(solutionRoot, "Directory.Build.props"), CreateDirectoryBuildProps(), createdFiles);
        WriteFile(Path.Combine(solutionRoot, "Directory.Packages.props"), CreateDirectoryPackagesProps(request), createdFiles);
        WriteFile(Path.Combine(solutionRoot, "testconfig.json"), CreateTestConfigJson(), createdFiles);

        var coreProjectName = $"{request.SolutionName}.Core";
        var coreProjectDir = Path.Combine(srcRoot, coreProjectName);
        Directory.CreateDirectory(coreProjectDir);
        Directory.CreateDirectory(Path.Combine(coreProjectDir, "Services"));
        Directory.CreateDirectory(Path.Combine(coreProjectDir, "Themes"));
        Directory.CreateDirectory(Path.Combine(coreProjectDir, "Settings"));
        Directory.CreateDirectory(Path.Combine(coreProjectDir, "ViewModels"));

        WriteFile(
            Path.Combine(coreProjectDir, $"{coreProjectName}.csproj"),
            CreateClassLibraryCsproj(coreProjectName, BuildCorePackages(request)),
            createdFiles);
        WriteFile(Path.Combine(coreProjectDir, "Services", "AppBootstrap.cs"), CreateAppBootstrapSource(request), createdFiles);
        WriteFile(Path.Combine(coreProjectDir, "Themes", "AppTheme.cs"), CreateThemeSource(request), createdFiles);
        WriteFile(Path.Combine(coreProjectDir, "Settings", "SettingsStoreOptions.cs"), CreateSettingsStoreSource(request), createdFiles);

        foreach (var view in parsedViews)
        {
            WriteFile(
                Path.Combine(coreProjectDir, "ViewModels", $"{view.ViewModelName}.cs"),
                CreateViewModelSource(view, request),
                createdFiles);
        }

        var solutionProjects = new List<string>
        {
            $"  <Project Path=\"{coreProjectName}/{coreProjectName}.csproj\" />"
        };

        foreach (var endpoint in request.UiEndpoints)
        {
            var endpointProjectName = $"{request.SolutionName}.{endpoint}";
            var endpointProjectDir = Path.Combine(srcRoot, endpointProjectName);
            var endpointViews = parsedViews.Where(view => string.Equals(view.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase)).ToArray();

            Directory.CreateDirectory(endpointProjectDir);
            Directory.CreateDirectory(Path.Combine(endpointProjectDir, "Views"));

            WriteFile(
                Path.Combine(endpointProjectDir, $"{endpointProjectName}.csproj"),
                CreateEndpointCsproj(endpointProjectName, endpoint, coreProjectName, request),
                createdFiles);
            WriteFile(Path.Combine(endpointProjectDir, "AppInfo.txt"), CreateEndpointNotes(endpoint, request, endpointViews), createdFiles);

            foreach (var hostFile in CreateEndpointHostFiles(endpointProjectDir, endpointProjectName, endpoint, request, endpointViews))
            {
                WriteFile(hostFile.Path, hostFile.Content, createdFiles);
            }

            foreach (var view in endpointViews)
            {
                foreach (var generated in CreateEndpointViewFiles(endpointProjectDir, endpointProjectName, endpoint, view, request))
                {
                    WriteFile(generated.Path, generated.Content, createdFiles);
                }
            }

            solutionProjects.Add($"  <Project Path=\"{endpointProjectName}/{endpointProjectName}.csproj\" />");
        }

        var testProjectName = $"{request.SolutionName}.Tests";
        var testProjectDir = Path.Combine(srcRoot, testProjectName);
        Directory.CreateDirectory(testProjectDir);

        WriteFile(
            Path.Combine(testProjectDir, $"{testProjectName}.csproj"),
            CreateTestCsproj(testProjectName, coreProjectName),
            createdFiles);
        WriteFile(Path.Combine(testProjectDir, "SampleReactiveTests.cs"), CreateSampleTestSource(request), createdFiles);
        solutionProjects.Add($"  <Project Path=\"{testProjectName}/{testProjectName}.csproj\" />");

        WriteFile(
            Path.Combine(srcRoot, $"{request.SolutionName}.slnx"),
            "<Solution>\n" + string.Join("\n", solutionProjects) + "\n</Solution>\n",
            createdFiles);

        notes.Add("Generated solution skeleton uses ReactiveUI.SourceGenerators as the default approach.");
        notes.Add("Generated tests include ReactiveUI.Testing and TUnit for scheduler-aware reactive tests.");
        notes.Add("Splat and DynamicData are treated as direct dependencies in the generated solution guidance.");
        notes.Add("WPF, MAUI, and Blazor endpoints now receive real startup files and basic reactive view scaffolding instead of plain placeholders.");

        if (!string.IsNullOrWhiteSpace(request.DiProvider))
        {
            notes.Add($"Selected DI integration: {request.DiProvider}.");
        }

        return new GeneratedReactiveUiSolutionResult(solutionRoot, createdFiles, notes);
    }

    private static IReadOnlyList<ReactiveUiViewScaffold> ParseViews(CreateReactiveUiSolutionWizardRequest request)
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
            foreach (var view in parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                results.Add(new ReactiveUiViewScaffold(endpoint, view, $"{view}ViewModel", [], []));
            }
        }

        return results;
    }

    private static IReadOnlyList<string> BuildCorePackages(CreateReactiveUiSolutionWizardRequest request)
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ReactiveUI",
            "ReactiveUI.SourceGenerators",
            "DynamicData",
            "Splat"
        };

        if (!string.IsNullOrWhiteSpace(request.DiProvider) && !string.Equals(request.DiProvider, "Splat", StringComparison.OrdinalIgnoreCase))
        {
            packages.Add(request.DiProvider!);
        }

        foreach (var feature in request.AdditionalFeatures)
        {
            switch (feature)
            {
                case "ReactiveUI.Binding.SourceGenerators":
                    packages.Add("ReactiveUI.Binding");
                    packages.Add("ReactiveUI.Binding.Reactive");
                    break;
                case "ReactiveUI.Extensions.Async":
                    packages.Add("ReactiveUI.Extensions");
                    break;
                case "Akavache":
                    packages.Add("Akavache.Sqlite3");
                    packages.Add("Akavache.SystemTextJson");
                    break;
                default:
                    packages.Add(feature);
                    break;
            }
        }

        if (string.Equals(request.SettingsStore, "Akavache SQLite", StringComparison.OrdinalIgnoreCase))
        {
            packages.Add("Akavache.Sqlite3");
            packages.Add("Akavache.SystemTextJson");
        }

        if (string.Equals(request.ValidationMode, "ReactiveUI.Validation", StringComparison.OrdinalIgnoreCase))
        {
            packages.Add("ReactiveUI.Validation");
        }

        return packages.OrderBy(static package => package, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string CreateGlobalJson() =>
        "{\n" +
        "  \"sdk\": {\n" +
        "    \"version\": \"10.0.201\",\n" +
        "    \"rollForward\": \"latestFeature\"\n" +
        "  },\n" +
        "  \"test\": {\n" +
        "    \"runner\": \"Microsoft.Testing.Platform\"\n" +
        "  }\n" +
        "}\n";

    private static string CreateTestConfigJson() =>
        "{\n" +
        "  \"$schema\": \"https://aka.ms/dotnet-mtp-json-schema\",\n" +
        "  \"diagnostic\": {\n" +
        "    \"enabled\": false\n" +
        "  }\n" +
        "}\n";

    private static string CreateDirectoryBuildProps() =>
        "<Project>\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net10.0</TargetFramework>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <LangVersion>preview</LangVersion>\n" +
        "  </PropertyGroup>\n" +
        "</Project>\n";

    private static string CreateDirectoryPackagesProps(CreateReactiveUiSolutionWizardRequest request)
    {
        var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ReactiveUI"] = "23.2.1",
            ["ReactiveUI.SourceGenerators"] = "2.6.1",
            ["DynamicData"] = "9.4.31",
            ["Splat"] = "19.3.1",
            ["ReactiveUI.Binding"] = "3.2.1",
            ["ReactiveUI.Binding.Reactive"] = "3.2.1",
            ["ReactiveUI.Extensions"] = "2.3.1",
            ["Refit"] = "10.1.6",
            ["Akavache.Sqlite3"] = "11.5.1",
            ["Akavache.SystemTextJson"] = "11.5.1",
            ["ReactiveUI.Validation"] = "7.0.5",
            ["ReactiveUI.Testing"] = "23.2.1",
            ["Fusillade"] = "4.0.3",
            ["punchclock"] = "6.0.1",
            ["TUnit"] = "1.28.5",
            ["TUnit.Assertions"] = "1.28.5"
        };

        foreach (var endpoint in request.UiEndpoints)
        {
            switch (endpoint)
            {
                case "WPF": versions["ReactiveUI.WPF"] = "23.2.1"; break;
                case "WinForms": versions["ReactiveUI.WinForms"] = "23.2.1"; break;
                case "Blazor": versions["ReactiveUI.Blazor"] = "23.2.1"; break;
                case "MAUI": versions["ReactiveUI.Maui"] = "23.2.1"; break;
                case "WinUI": versions["ReactiveUI.WinUI"] = "23.2.1"; break;
                case "Avalonia": versions["ReactiveUI.Avalonia"] = "23.2.1"; break;
                case "Uno": versions["ReactiveUI.Uno"] = "23.2.1"; break;
                case "AndroidX": versions["ReactiveUI.AndroidX"] = "23.2.1"; break;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.DiProvider) &&
            !string.Equals(request.DiProvider, "Splat", StringComparison.OrdinalIgnoreCase) &&
            !versions.ContainsKey(request.DiProvider!))
        {
            versions[request.DiProvider!] = "19.3.1";
        }

        var builder = new StringBuilder();
        builder.AppendLine("<Project>");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine("    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>");
        builder.AppendLine("  </PropertyGroup>");
        builder.AppendLine("  <ItemGroup>");
        foreach (var pair in versions.OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"    <PackageVersion Include=\"{pair.Key}\" Version=\"{pair.Value}\" />");
        }
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("</Project>");
        return builder.ToString();
    }

    private static string CreateClassLibraryCsproj(string projectName, IReadOnlyList<string> packages)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
        builder.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");
        builder.AppendLine("  </PropertyGroup>");
        builder.AppendLine("  <ItemGroup>");
        foreach (var package in packages)
        {
            if (IsSourceGeneratorPackage(package))
            {
                builder.AppendLine($"    <PackageReference Include=\"{package}\" PrivateAssets=\"all\" />");
            }
            else
            {
                builder.AppendLine($"    <PackageReference Include=\"{package}\" />");
            }
        }
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("</Project>");
        return builder.ToString();
    }

    private static string CreateEndpointCsproj(string projectName, string endpoint, string coreProjectName, CreateReactiveUiSolutionWizardRequest request)
    {
        var sdk = string.Equals(endpoint, "Blazor", StringComparison.OrdinalIgnoreCase)
            ? "Microsoft.NET.Sdk.Web"
            : "Microsoft.NET.Sdk";

        var builder = new StringBuilder();
        builder.AppendLine($"<Project Sdk=\"{sdk}\">");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
        builder.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");

        var packageRefs = new List<string>();
        switch (endpoint)
        {
            case "WPF":
                builder.AppendLine("    <TargetFramework>net10.0-windows</TargetFramework>");
                builder.AppendLine("    <UseWPF>true</UseWPF>");
                builder.AppendLine("    <OutputType>WinExe</OutputType>");
                packageRefs.Add("ReactiveUI.WPF");
                break;
            case "WinForms":
                builder.AppendLine("    <TargetFramework>net10.0-windows</TargetFramework>");
                builder.AppendLine("    <UseWindowsForms>true</UseWindowsForms>");
                builder.AppendLine("    <OutputType>WinExe</OutputType>");
                packageRefs.Add("ReactiveUI.WinForms");
                break;
            case "Blazor":
                builder.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
                packageRefs.Add("ReactiveUI.Blazor");
                break;
            case "MAUI":
                builder.AppendLine("    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>");
                builder.AppendLine("    <UseMaui>true</UseMaui>");
                builder.AppendLine("    <SingleProject>true</SingleProject>");
                builder.AppendLine("    <OutputType>Exe</OutputType>");
                packageRefs.Add("ReactiveUI.Maui");
                break;
            case "WinUI":
                builder.AppendLine("    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>");
                builder.AppendLine("    <OutputType>WinExe</OutputType>");
                packageRefs.Add("ReactiveUI.WinUI");
                break;
            case "Avalonia":
                builder.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
                packageRefs.Add("ReactiveUI.Avalonia");
                break;
            case "Uno":
                builder.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
                packageRefs.Add("ReactiveUI.Uno");
                break;
            case "AndroidX":
                builder.AppendLine("    <TargetFramework>net10.0-android</TargetFramework>");
                packageRefs.Add("ReactiveUI.AndroidX");
                break;
            default:
                builder.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
                break;
        }

        builder.AppendLine("  </PropertyGroup>");
        builder.AppendLine("  <ItemGroup>");
        foreach (var package in packageRefs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"    <PackageReference Include=\"{package}\" />");
        }
        if (request.AdditionalFeatures.Contains("ReactiveUI.Binding.SourceGenerators", StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine("    <PackageReference Include=\"ReactiveUI.Binding\" PrivateAssets=\"all\" />");
            builder.AppendLine("    <PackageReference Include=\"ReactiveUI.Binding.Reactive\" PrivateAssets=\"all\" />");
        }
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("  <ItemGroup>");
        builder.AppendLine($"    <ProjectReference Include=\"..\\{coreProjectName}\\{coreProjectName}.csproj\" />");
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("</Project>");
        return builder.ToString();
    }

    private static string CreateTestCsproj(string projectName, string coreProjectName)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
        builder.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");
        builder.AppendLine("    <IsTestProject>true</IsTestProject>");
        builder.AppendLine("    <OutputType>Exe</OutputType>");
        builder.AppendLine("  </PropertyGroup>");
        builder.AppendLine("  <ItemGroup>");
        builder.AppendLine("    <PackageReference Include=\"ReactiveUI.Testing\" />");
        builder.AppendLine("    <PackageReference Include=\"TUnit\" />");
        builder.AppendLine("    <PackageReference Include=\"TUnit.Assertions\" />");
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("  <ItemGroup>");
        builder.AppendLine($"    <ProjectReference Include=\"..\\{coreProjectName}\\{coreProjectName}.csproj\" />");
        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("</Project>");
        return builder.ToString();
    }

    private static string CreateRootReadme(CreateReactiveUiSolutionWizardRequest request)
    {
        var endpoints = request.UiEndpoints.Count == 0 ? "None specified" : string.Join(", ", request.UiEndpoints);
        var features = request.AdditionalFeatures.Count == 0 ? "None specified" : string.Join(", ", request.AdditionalFeatures);
        var appFeatures = request.ApplicationFeatures.Count == 0 ? "None specified" : string.Join(", ", request.ApplicationFeatures);

        var builder = new StringBuilder();
        builder.AppendLine($"# {request.SolutionName}");
        builder.AppendLine();
        builder.AppendLine("Generated by /CreateReactiveUISolution.");
        builder.AppendLine();
        builder.AppendLine($"Endpoints: {endpoints}");
        builder.AppendLine($"DI Provider: {request.DiProvider ?? "Splat"}");
        builder.AppendLine($"Additional Features: {features}");
        builder.AppendLine($"Settings Store: {request.SettingsStore ?? "None"}");
        builder.AppendLine($"Application Features: {appFeatures}");
        builder.AppendLine($"Validation Mode: {request.ValidationMode ?? "None"}");
        builder.AppendLine($"Theme Colors: {request.PrimaryColors ?? "Default"}");
        builder.AppendLine();
        builder.AppendLine("The generated solution includes starter host files for WPF, MAUI, and Blazor when those endpoints are selected.");
        builder.AppendLine("Refine DI registrations, navigation, persistence, and domain logic after generation.");
        return builder.ToString();
    }

    private static string CreateAppBootstrapSource(CreateReactiveUiSolutionWizardRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"namespace {request.SolutionName}.Core.Services;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// High-level bootstrap notes for the generated solution.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public static class AppBootstrap");
        builder.AppendLine("{");
        builder.AppendLine($"    public const string DiProvider = \"{request.DiProvider ?? "Splat"}\";");
        builder.AppendLine($"    public const string SettingsStore = \"{request.SettingsStore ?? "None"}\";");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateThemeSource(CreateReactiveUiSolutionWizardRequest request)
    {
        var colors = (request.PrimaryColors ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(color => $"\"{color}\"");

        var builder = new StringBuilder();
        builder.AppendLine($"namespace {request.SolutionName}.Core.Themes;");
        builder.AppendLine();
        builder.AppendLine("public static class AppTheme");
        builder.AppendLine("{");
        builder.AppendLine($"    public static string[] PrimeColors {{ get; }} = new[] {{ {string.Join(", ", colors)} }};");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateSettingsStoreSource(CreateReactiveUiSolutionWizardRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"namespace {request.SolutionName}.Core.Settings;");
        builder.AppendLine();
        builder.AppendLine("public static class SettingsStoreOptions");
        builder.AppendLine("{");
        builder.AppendLine($"    public const string SelectedStore = \"{request.SettingsStore ?? "None"}\";");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateViewModelSource(ReactiveUiViewScaffold view, CreateReactiveUiSolutionWizardRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using ReactiveUI;");
        builder.AppendLine("using ReactiveUI.SourceGenerators;");
        builder.AppendLine();
        builder.AppendLine($"namespace {request.SolutionName}.Core.ViewModels;");
        builder.AppendLine();
        builder.AppendLine($"public partial class {view.ViewModelName} : ReactiveObject");
        builder.AppendLine("{");
        builder.AppendLine("    [Reactive]");
        builder.AppendLine($"    private string _title = \"{view.ViewName}\";");
        builder.AppendLine();
        builder.AppendLine(string.Equals(request.ValidationMode, "ReactiveUI.Validation", StringComparison.OrdinalIgnoreCase)
            ? "    // Add ReactiveUI.Validation rules here for user input scenarios."
            : "    // Add validation rules here if needed.");
        builder.AppendLine(request.AdditionalFeatures.Contains("ReactiveUI.Extensions.Async", StringComparer.OrdinalIgnoreCase)
            ? "    // Consider IObservableAsync-based workflows from ReactiveUI.Extensions.Async for async streaming operations."
            : "    // Add async workflows here using the selected ReactiveUI patterns.");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateSampleTestSource(CreateReactiveUiSolutionWizardRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using ReactiveUI.Testing;");
        builder.AppendLine();
        builder.AppendLine($"namespace {request.SolutionName}.Tests;");
        builder.AppendLine();
        builder.AppendLine("public class SampleReactiveTests");
        builder.AppendLine("{");
        builder.AppendLine("    [Test]");
        builder.AppendLine("    public async Task ReactiveUiTesting_Package_Is_Configured()");
        builder.AppendLine("    {");
        builder.AppendLine("        await Assert.That(typeof(TestSchedulerExtensions)).IsNotNull();");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateEndpointNotes(string endpoint, CreateReactiveUiSolutionWizardRequest request, IReadOnlyList<ReactiveUiViewScaffold> endpointViews) =>
        $"Endpoint: {endpoint}\n" +
        $"DI Provider: {request.DiProvider ?? "Splat"}\n" +
        $"Settings Store: {request.SettingsStore ?? "None"}\n" +
        $"Generated Views: {(endpointViews.Count == 0 ? "None" : string.Join(", ", endpointViews.Select(view => view.ViewName)))}\n";

    private static IReadOnlyList<(string Path, string Content)> CreateEndpointHostFiles(
        string endpointProjectDir,
        string endpointProjectName,
        string endpoint,
        CreateReactiveUiSolutionWizardRequest request,
        IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var results = new List<(string Path, string Content)>();

        switch (endpoint)
        {
            case "WPF":
                results.Add((Path.Combine(endpointProjectDir, "App.xaml"), CreateWpfAppXaml(endpointProjectName)));
                results.Add((Path.Combine(endpointProjectDir, "App.xaml.cs"), CreateWpfAppCodeBehind(endpointProjectName)));
                results.Add((Path.Combine(endpointProjectDir, "Views", "MainWindow.xaml"), CreateWpfMainWindowXaml(endpointProjectName, endpointViews)));
                results.Add((Path.Combine(endpointProjectDir, "Views", "MainWindow.xaml.cs"), CreateWpfMainWindowCodeBehind(endpointProjectName)));
                break;
            case "MAUI":
                results.Add((Path.Combine(endpointProjectDir, "MauiProgram.cs"), CreateMauiProgramSource(endpointProjectName, request, endpointViews)));
                results.Add((Path.Combine(endpointProjectDir, "App.xaml"), CreateMauiAppXaml(endpointProjectName)));
                results.Add((Path.Combine(endpointProjectDir, "App.xaml.cs"), CreateMauiAppCodeBehind(endpointProjectName)));
                results.Add((Path.Combine(endpointProjectDir, "AppShell.xaml"), CreateMauiAppShellXaml(endpointProjectName, endpointViews)));
                results.Add((Path.Combine(endpointProjectDir, "AppShell.xaml.cs"), CreateMauiAppShellCodeBehind(endpointProjectName)));
                break;
            case "Blazor":
                results.Add((Path.Combine(endpointProjectDir, "Program.cs"), CreateBlazorProgramSource(endpointProjectName, request, endpointViews)));
                results.Add((Path.Combine(endpointProjectDir, "Components", "App.razor"), "<Routes />\n"));
                results.Add((Path.Combine(endpointProjectDir, "Components", "Routes.razor"), CreateBlazorRoutesComponent()));
                results.Add((Path.Combine(endpointProjectDir, "Components", "_Imports.razor"), CreateBlazorImports(request, endpointProjectName)));
                results.Add((Path.Combine(endpointProjectDir, "Components", "Layout", "MainLayout.razor"), CreateBlazorMainLayout(request, endpointViews)));
                break;
        }

        return results;
    }

    private static IReadOnlyList<(string Path, string Content)> CreateEndpointViewFiles(
        string endpointProjectDir,
        string endpointProjectName,
        string endpoint,
        ReactiveUiViewScaffold view,
        CreateReactiveUiSolutionWizardRequest request)
    {
        var viewsDir = Path.Combine(endpointProjectDir, "Views");
        Directory.CreateDirectory(viewsDir);

        var endpointNamespace = endpointProjectName.Replace('-', '_');
        var viewModelNamespace = $"{request.SolutionName}.Core.ViewModels";
        var results = new List<(string Path, string Content)>();

        switch (endpoint)
        {
            case "WPF":
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}View.xaml"), CreateWpfViewXaml(endpointNamespace, request.SolutionName!, view)));
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}View.xaml.cs"), CreateWpfViewCodeBehind(endpointNamespace, request.SolutionName!, view)));
                break;
            case "MAUI":
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}Page.xaml"), CreateMauiPageXaml(endpointNamespace, request.SolutionName!, view)));
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}Page.xaml.cs"), CreateMauiPageCodeBehind(endpointNamespace, request.SolutionName!, view)));
                break;
            case "Blazor":
                results.Add((Path.Combine(endpointProjectDir, "Components", "Pages", $"{view.ViewName}.razor"), CreateBlazorPageComponent(viewModelNamespace, view)));
                break;
            case "WinForms":
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}Form.cs"),
                    "using System.Windows.Forms;\n\n" +
                    $"namespace {endpointNamespace}.Views;\n\n" +
                    $"public partial class {view.ViewName}Form : Form\n" +
                    "{\n" +
                    $"    public {view.ViewName}Form()\n" +
                    "    {\n" +
                    "        InitializeComponent();\n" +
                    "    }\n\n" +
                    "    private void InitializeComponent()\n" +
                    "    {\n" +
                    $"        Text = \"{view.ViewName}\";\n" +
                    "    }\n" +
                    "}\n"));
                break;
            case "AndroidX":
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}Activity.cs"),
                    $"namespace {endpointNamespace}.Views;\n\npublic class {view.ViewName}Activity\n{{\n    // Add ReactiveAppCompatActivity<{view.ViewModelName}> integration here.\n}}\n"));
                break;
            default:
                results.Add((Path.Combine(viewsDir, $"{view.ViewName}.txt"),
                    $"Generated {endpoint} view placeholder for {view.ViewName}.\nBind to {viewModelNamespace}.{view.ViewModelName}.\nAdd WhenActivated bindings and endpoint-specific base types.\n"));
                break;
        }

        return results;
    }

    private static bool IsSourceGeneratorPackage(string package) =>
        string.Equals(package, "ReactiveUI.SourceGenerators", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(package, "ReactiveUI.Binding", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(package, "ReactiveUI.Binding.Reactive", StringComparison.OrdinalIgnoreCase);

    private static string CreateWpfAppXaml(string endpointProjectName) =>
        $"<Application x:Class=\"{endpointProjectName}.App\"\n" +
        "             xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
        "             xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
        "             StartupUri=\"Views/MainWindow.xaml\">\n" +
        "    <Application.Resources />\n" +
        "</Application>\n";

    private static string CreateWpfAppCodeBehind(string endpointProjectName) =>
        "using System.Windows;\n\n" +
        $"namespace {endpointProjectName};\n\n" +
        "public partial class App : Application\n" +
        "{\n" +
        "}\n";

    private static string CreateWpfMainWindowXaml(string endpointProjectName, IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var lines = endpointViews.Count == 0
            ? "            <TextBlock Text=\"No views were selected for this endpoint.\" />\n"
            : string.Join(string.Empty, endpointViews.Select(view => $"            <TextBlock Text=\"• {view.ViewName}View\" Margin=\"0,0,0,6\" />\n"));

        return $"<Window x:Class=\"{endpointProjectName}.Views.MainWindow\"\n" +
               "        xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
               "        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
               $"        Title=\"{endpointProjectName}\" Height=\"450\" Width=\"800\">\n" +
               "    <ScrollViewer>\n" +
               "        <StackPanel Margin=\"24\">\n" +
               "            <TextBlock Text=\"ReactiveUI WPF Shell\" FontSize=\"24\" FontWeight=\"Bold\" Margin=\"0,0,0,12\" />\n" +
               "            <TextBlock Text=\"This shell is the generated startup window. Replace this with your navigation host or region manager.\" TextWrapping=\"Wrap\" Margin=\"0,0,0,20\" />\n" +
               lines +
               "        </StackPanel>\n" +
               "    </ScrollViewer>\n" +
               "</Window>\n";
    }

    private static string CreateWpfMainWindowCodeBehind(string endpointProjectName) =>
        "using System.Windows;\n\n" +
        $"namespace {endpointProjectName}.Views;\n\n" +
        "public partial class MainWindow : Window\n" +
        "{\n" +
        "    public MainWindow()\n" +
        "    {\n" +
        "        InitializeComponent();\n" +
        "    }\n" +
        "}\n";

    private static string CreateWpfViewXaml(string endpointNamespace, string solutionName, ReactiveUiViewScaffold view) =>
        $"<reactiveui:ReactiveUserControl x:Class=\"{endpointNamespace}.Views.{view.ViewName}View\"\n" +
        "                              xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
        "                              xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
        "                              xmlns:reactiveui=\"http://reactiveui.net\"\n" +
        $"                              xmlns:viewModels=\"clr-namespace:{solutionName}.Core.ViewModels;assembly={solutionName}.Core\"\n" +
        $"                              x:TypeArguments=\"viewModels:{view.ViewModelName}\">\n" +
        "    <Grid Margin=\"24\">\n" +
        "        <StackPanel>\n" +
        "            <TextBlock x:Name=\"TitleBlock\" FontSize=\"22\" FontWeight=\"SemiBold\" />\n" +
        "            <TextBlock Margin=\"0,12,0,0\" Text=\"Generated ReactiveUI WPF view. Add more bindings and services as needed.\" TextWrapping=\"Wrap\" />\n" +
        "        </StackPanel>\n" +
        "    </Grid>\n" +
        "</reactiveui:ReactiveUserControl>\n";

    private static string CreateWpfViewCodeBehind(string endpointNamespace, string solutionName, ReactiveUiViewScaffold view) =>
        "using ReactiveUI;\n" +
        "using System.Reactive.Disposables.Fluent;\n" +
        $"using {solutionName}.Core.ViewModels;\n\n" +
        $"namespace {endpointNamespace}.Views;\n\n" +
        $"public partial class {view.ViewName}View : ReactiveUserControl<{view.ViewModelName}>\n" +
        "{\n" +
        $"    public {view.ViewName}View()\n" +
        "    {\n" +
        "        InitializeComponent();\n" +
        "        ViewModel = new();\n" +
        "\n" +
        "        this.WhenActivated(disposables =>\n" +
        "        {\n" +
        "            this.OneWayBind(ViewModel, vm => vm.Title, v => v.TitleBlock.Text)\n" +
        "                .DisposeWith(disposables);\n" +
        "        });\n" +
        "    }\n" +
        "}\n";

    private static string CreateMauiProgramSource(string endpointProjectName, CreateReactiveUiSolutionWizardRequest request, IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var registrations = string.Join(string.Empty, endpointViews.Select(view => $"        builder.Services.AddTransient<Views.{view.ViewName}Page>();\n"));
        return "using Microsoft.Extensions.DependencyInjection;\n" +
               "using Microsoft.Maui;\n" +
               "using Microsoft.Maui.Controls.Hosting;\n" +
               "using Microsoft.Maui.Hosting;\n\n" +
               $"namespace {endpointProjectName};\n\n" +
               "public static class MauiProgram\n" +
               "{\n" +
               "    public static MauiApp CreateMauiApp()\n" +
               "    {\n" +
               "        var builder = MauiApp.CreateBuilder();\n" +
               "        builder\n" +
               "            .UseMauiApp<App>();\n\n" +
               registrations +
               "        return builder.Build();\n" +
               "    }\n" +
               "}\n";
    }

    private static string CreateMauiAppXaml(string endpointProjectName) =>
        $"<Application x:Class=\"{endpointProjectName}.App\"\n" +
        "             xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"\n" +
        "             xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\">\n" +
        "    <Application.Resources />\n" +
        "</Application>\n";

    private static string CreateMauiAppCodeBehind(string endpointProjectName) =>
        "using Microsoft.Maui.Controls;\n\n" +
        $"namespace {endpointProjectName};\n\n" +
        "public partial class App : Application\n" +
        "{\n" +
        "    public App()\n" +
        "    {\n" +
        "        InitializeComponent();\n" +
        "        MainPage = new AppShell();\n" +
        "    }\n" +
        "}\n";

    private static string CreateMauiAppShellXaml(string endpointProjectName, IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var items = endpointViews.Count == 0
            ? "        <ShellContent Title=\"Home\" ContentTemplate=\"{DataTemplate ContentPage}\" />\n"
            : string.Join(string.Empty, endpointViews.Select(view =>
                $"        <ShellContent Title=\"{view.ViewName}\" ContentTemplate=\"{{DataTemplate views:{view.ViewName}Page}}\" Route=\"{view.ViewName.ToLowerInvariant()}\" />\n"));

        return $"<Shell x:Class=\"{endpointProjectName}.AppShell\"\n" +
               "       xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"\n" +
               "       xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"\n" +
               $"       xmlns:views=\"clr-namespace:{endpointProjectName}.Views\">\n" +
               "    <TabBar>\n" +
               items +
               "    </TabBar>\n" +
               "</Shell>\n";
    }

    private static string CreateMauiAppShellCodeBehind(string endpointProjectName) =>
        "using Microsoft.Maui.Controls;\n\n" +
        $"namespace {endpointProjectName};\n\n" +
        "public partial class AppShell : Shell\n" +
        "{\n" +
        "    public AppShell()\n" +
        "    {\n" +
        "        InitializeComponent();\n" +
        "    }\n" +
        "}\n";

    private static string CreateMauiPageXaml(string endpointNamespace, string solutionName, ReactiveUiViewScaffold view) =>
        $"<reactive:ReactiveContentPage x:Class=\"{endpointNamespace}.Views.{view.ViewName}Page\"\n" +
        "                           xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"\n" +
        "                           xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"\n" +
        "                           xmlns:reactive=\"clr-namespace:ReactiveUI.Maui;assembly=ReactiveUI.Maui\"\n" +
        $"                           xmlns:viewModels=\"clr-namespace:{solutionName}.Core.ViewModels;assembly={solutionName}.Core\"\n" +
        $"                           x:TypeArguments=\"viewModels:{view.ViewModelName}\">\n" +
        "    <VerticalStackLayout Padding=\"24\" Spacing=\"12\">\n" +
        "        <Label x:Name=\"TitleLabel\" FontSize=\"28\" FontAttributes=\"Bold\" />\n" +
        "        <Label Text=\"Generated ReactiveUI MAUI page. Add commands, services, and navigation as needed.\" />\n" +
        "    </VerticalStackLayout>\n" +
        "</reactive:ReactiveContentPage>\n";

    private static string CreateMauiPageCodeBehind(string endpointNamespace, string solutionName, ReactiveUiViewScaffold view) =>
        "using ReactiveUI;\n" +
        "using ReactiveUI.Maui;\n" +
        "using System.Reactive.Disposables.Fluent;\n" +
        $"using {solutionName}.Core.ViewModels;\n\n" +
        $"namespace {endpointNamespace}.Views;\n\n" +
        $"public partial class {view.ViewName}Page : ReactiveContentPage<{view.ViewModelName}>\n" +
        "{\n" +
        $"    public {view.ViewName}Page()\n" +
        "    {\n" +
        "        InitializeComponent();\n" +
        "        ViewModel = new();\n" +
        "\n" +
        "        this.WhenActivated(disposables =>\n" +
        "        {\n" +
        "            this.OneWayBind(ViewModel, vm => vm.Title, v => v.TitleLabel.Text)\n" +
        "                .DisposeWith(disposables);\n" +
        "        });\n" +
        "    }\n" +
        "}\n";

    private static string CreateBlazorProgramSource(string endpointProjectName, CreateReactiveUiSolutionWizardRequest request, IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var registrations = string.Join(string.Empty, endpointViews.Select(view => $"builder.Services.AddTransient<{request.SolutionName}.Core.ViewModels.{view.ViewModelName}>();\n"));
        return "using Microsoft.AspNetCore.Components.Web;\n" +
               "using Microsoft.AspNetCore.Components.WebAssembly.Hosting;\n\n" +
               $"var builder = WebAssemblyHostBuilder.CreateDefault(args);\n" +
               "builder.RootComponents.Add<Routes>(\"#app\");\n" +
               registrations +
               "await builder.Build().RunAsync();\n";
    }

    private static string CreateBlazorRoutesComponent() =>
        "<Router AppAssembly=\"@typeof(Program).Assembly\">\n" +
        "    <Found Context=\"routeData\">\n" +
        "        <RouteView RouteData=\"@routeData\" DefaultLayout=\"@typeof(Layout.MainLayout)\" />\n" +
        "    </Found>\n" +
        "    <NotFound>\n" +
        "        <Layout.MainLayout>\n" +
        "            <p>Sorry, there's nothing at this address.</p>\n" +
        "        </Layout.MainLayout>\n" +
        "    </NotFound>\n" +
        "</Router>\n";

    private static string CreateBlazorImports(CreateReactiveUiSolutionWizardRequest request, string endpointProjectName) =>
        "@using Microsoft.AspNetCore.Components\n" +
        "@using Microsoft.AspNetCore.Components.Web\n" +
        $"@using {endpointProjectName}.Components\n" +
        $"@using {endpointProjectName}.Components.Layout\n" +
        $"@using {request.SolutionName}.Core.ViewModels\n";

    private static string CreateBlazorMainLayout(CreateReactiveUiSolutionWizardRequest request, IReadOnlyList<ReactiveUiViewScaffold> endpointViews)
    {
        var links = endpointViews.Count == 0
            ? "        <li>No generated pages yet</li>\n"
            : string.Join(string.Empty, endpointViews.Select(view => $"        <li><a href=\"/{view.ViewName.ToLowerInvariant()}\">{view.ViewName}</a></li>\n"));

        return "@inherits LayoutComponentBase\n\n" +
               "<div class=\"page\">\n" +
               "    <nav>\n" +
               "        <h3>Generated Pages</h3>\n" +
               "        <ul>\n" +
               links +
               "        </ul>\n" +
               "    </nav>\n" +
               "    <main>\n" +
               "        @Body\n" +
               "    </main>\n" +
               "</div>\n";
    }

    private static string CreateBlazorPageComponent(string viewModelNamespace, ReactiveUiViewScaffold view) =>
        $"@page \"/{view.ViewName.ToLowerInvariant()}\"\n" +
        $"@inject {viewModelNamespace}.{view.ViewModelName} ViewModel\n\n" +
        $"<PageTitle>{view.ViewName}</PageTitle>\n\n" +
        "<h1>@ViewModel.Title</h1>\n" +
        "<p>Generated ReactiveUI Blazor page. Extend this with forms, state, and commands as needed.</p>\n";

    private static void WriteFile(string path, string content, ICollection<string> createdFiles)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content, Encoding.UTF8);
        createdFiles.Add(path);
    }
}
