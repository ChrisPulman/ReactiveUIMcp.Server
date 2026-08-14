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
                ["WPF", "Blazor", "MAUI"],
                "Splat.Microsoft.Extensions.DependencyInjection",
                ["ReactiveUI.SourceGenerators", "ReactiveUI.Primitives.Async", "ReactiveUI.Testing"],
                "Akavache SQLite",
                ["Authentication", "Settings Page", "Theming"],
                "#1D4ED8,#0F172A",
                "ReactiveUI.Validation",
                "WPF:Login,Dashboard,Settings|Blazor:Login,Home,Settings|MAUI:Login,Home,Settings",
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
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "Services", "ServiceCollectionExtensions.cs"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "Settings", "AkavacheSetup.cs"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "ViewModels", "ThemeDesignerViewModel.cs"))).IsTrue();
            await Assert.That(result.CreatedFiles.Any(file => file.EndsWith("LoginViewModel.cs", StringComparison.Ordinal))).IsTrue();
            var generatedServiceRegistrations = File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "Services", "ServiceCollectionExtensions.cs"));
            await Assert.That(generatedServiceRegistrations).Contains("AddGeneratedApplicationServices");
            await Assert.That(generatedServiceRegistrations).Contains("AddSingleton<IAkavacheBootstrap, global::Contoso.GeneratedApp.Core.Settings.AkavacheBootstrap>()");
            await Assert.That(File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Core", "Settings", "AkavacheSetup.cs"))).Contains("Registrations.Start");
            var packages = File.ReadAllText(Path.Combine(result.OutputPath, "Directory.Packages.props"));
            var generatedWpfView = File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.WPF", "Views", "LoginView.xaml.cs"));
            var generatedMauiView = File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.MAUI", "Views", "LoginPage.xaml.cs"));
            var generatedMauiProgram = File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.MAUI", "MauiProgram.cs"));
            var generatedBlazorProgram = File.ReadAllText(Path.Combine(result.OutputPath, "src", "Contoso.GeneratedApp.Blazor", "Program.cs"));
            await Assert.That(packages).Contains("ReactiveUI.Primitives");
            await Assert.That(packages).Contains("ReactiveUI.SourceGenerators\" Version=\"3.2.0");
            await Assert.That(generatedWpfView).Contains("ReactiveUI.Primitives.Disposables");
            await Assert.That(generatedWpfView).DoesNotContain("System.Reactive");
            await Assert.That(generatedMauiView).Contains("ReactiveUI.Primitives.Disposables");
            await Assert.That(generatedMauiView).DoesNotContain("System.Reactive");
            await Assert.That(generatedMauiProgram).Contains("var app = builder.Build();\n        app.Services.GetRequiredService<IAkavacheBootstrap>().Initialize();\n        return app;");
            await Assert.That(generatedMauiProgram).DoesNotContain("builder.Services.GetRequiredService<IAkavacheBootstrap>()");
            await Assert.That(generatedBlazorProgram).Contains("var host = builder.Build();\nhost.Services.GetRequiredService<IAkavacheBootstrap>().Initialize();\nawait host.RunAsync();");
            await Assert.That(generatedBlazorProgram).DoesNotContain("builder.Services.GetRequiredService<IAkavacheBootstrap>()");
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
