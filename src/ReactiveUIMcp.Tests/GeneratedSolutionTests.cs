using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using ReactiveUIMcp.Knowledge.Services;
using ReactiveUIMcp.Server.Services;

namespace ReactiveUIMcp.Tests;

/// <summary>
/// Tests for on-disk solution generation.
/// </summary>
public class GeneratedSolutionTests
{
    /// <summary>
    /// Verifies that the scaffolder creates a multi-project solution skeleton on disk.
    /// </summary>
    [Test]
    public async Task Scaffolder_Generates_Solution_Skeleton_On_Disk()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "ReactiveUiMcpTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            IReactiveUiSolutionScaffolder scaffolder = new ReactiveUiSolutionScaffolder();
            var result = scaffolder.Generate(new CreateReactiveUiSolutionWizardRequest(
                "9",
                "Contoso.GeneratedApp",
                ["WPF", "Blazor"],
                "Splat.Microsoft.Extensions.DependencyInjection",
                ["ReactiveUI.SourceGenerators", "ReactiveUI.Extensions.Async", "ReactiveUI.Testing"],
                "Akavache SQLite",
                ["Authentication", "Settings Page", "Theming"],
                "#1D4ED8,#0F172A",
                "ReactiveUI.Validation",
                "WPF:Login,Dashboard,Settings|Blazor:Login,Home,Settings",
                tempRoot,
                true));

            await Assert.That(Directory.Exists(result.OutputPath)).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.slnx"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "Contoso.GeneratedApp.Core.csproj"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Tests", "Contoso.GeneratedApp.Tests.csproj"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.WPF", "App.xaml"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.WPF", "Views", "LoginView.xaml"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Blazor", "Program.cs"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Blazor", "Components", "Pages", "Home.razor"))).IsTrue();
            await Assert.That(result.CreatedFiles.Any(file => file.EndsWith("LoginViewModel.cs", StringComparison.Ordinal))).IsTrue();
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
