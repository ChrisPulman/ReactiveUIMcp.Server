using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using CP.BuildTools;
using Microsoft.Build.Construction;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace ReactiveUIMcp.Build;

sealed class Build : NukeBuild
{
    private const string PackageId = "CP.ReactiveUI.Mcp.Server";
    private const string SkillPackagePath = "skills/reactiveui-mcp";
    private const string ToolPayloadPath = "tools/net10.0/any";

    public static int Main() => Execute<Build>(x => x.Compile);

    private static AbsolutePath SolutionFile => RootDirectory / "src" / "ReactiveUIMcpServer.slnx";

    private static AbsolutePath NukeBuildProjectFile => RootDirectory / "build" / "_build.csproj";

    private static AbsolutePath ServerProjectFile => RootDirectory / "src" / "ReactiveUIMcp.Server" / "CP.ReactiveUIMcp.Server.csproj";

    private static AbsolutePath McpManifestFile => RootDirectory / ".mcp" / "server.json";

    private static AbsolutePath SkillFile => RootDirectory / "skills" / "reactiveui-mcp" / "SKILL.md";

    private static AbsolutePath SkillMetadataFile => RootDirectory / "skills" / "reactiveui-mcp" / "agents" / "openai.yaml";

    private static AbsolutePath PackagesDirectory => RootDirectory / "packages";

    readonly Solution Solution = SolutionFile.ReadSolution();

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    string _minVerVersion = string.Empty;
    string _packageVersion = string.Empty;

    IEnumerable<Project> ProductProjects => Solution.AllProjects.Where(
        project => !string.Equals(project.Path, NukeBuildProjectFile, StringComparison.OrdinalIgnoreCase));

    Target Print => _ => _
        .DependsOn(SynchronizeVersion)
        .Executes(() =>
        {
            Log.Information("Configuration = {Configuration}", Configuration);
            Log.Information("MinVerVersionOverride = {Value}", _minVerVersion);
            Log.Information("PackageVersion = {Value}", _packageVersion);
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            if (!IsLocalBuild)
            {
                PackagesDirectory.CreateOrCleanDirectory();
            }
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() => DotNetRestore(s => s.SetProjectFile(Solution)));

    Target ResolveVersion => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var arguments =
                $"msbuild \"{ServerProjectFile}\" " +
                "-target:MinVer " +
                "-property:Restore=false " +
                "-getProperty:MinVerVersion,PackageVersion " +
                "-nologo -verbosity:quiet";
            var process = ProcessTasks.StartProcess(DotNetPath, arguments, RootDirectory);
            process.AssertWaitForExit();

            var output = string.Join(Environment.NewLine, process.Output.Select(line => line.Text));
            var jsonStart = output.IndexOf('{', StringComparison.Ordinal);
            if (jsonStart < 0)
            {
                throw new InvalidOperationException("MinVer did not return its calculated MSBuild properties.");
            }

            using var result = JsonDocument.Parse(output[jsonStart..]);
            var properties = result.RootElement.GetProperty("Properties");
            _minVerVersion = properties.GetProperty("MinVerVersion").GetString() ?? string.Empty;
            _packageVersion = properties.GetProperty("PackageVersion").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_minVerVersion) || string.IsNullOrWhiteSpace(_packageVersion))
            {
                throw new InvalidOperationException("MinVer returned an empty version.");
            }

            Environment.SetEnvironmentVariable("MinVerVersionOverride", _minVerVersion);
        });

    Target SynchronizeVersion => _ => _
        .DependsOn(ResolveVersion)
        .Executes(() =>
        {
            SynchronizeMcpManifest();
            Log.Information("Synchronized MCP metadata to package version {PackageVersion}", _packageVersion);
        });

    Target Compile => _ => _
        .DependsOn(Print)
        .Executes(() =>
        {
            foreach (var project in ProductProjects)
            {
                DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetNoRestore(true));
            }
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(ServerProjectFile)
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetOutputDirectory(PackagesDirectory));
            VerifyPackedPackage();
        });

    void SynchronizeMcpManifest()
    {
        var source = File.ReadAllText(McpManifestFile);
        var manifest = JsonNode.Parse(source)?.AsObject()
            ?? throw new InvalidOperationException("The MCP server manifest is not a JSON object.");
        manifest["version"] = _packageVersion;

        var packages = manifest["packages"]?.AsArray()
            ?? throw new InvalidOperationException("The MCP server manifest does not contain a packages array.");
        var package = packages
            .OfType<JsonObject>()
            .SingleOrDefault(candidate => string.Equals(
                candidate["identifier"]?.GetValue<string>(),
                PackageId,
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"The MCP server manifest does not contain package '{PackageId}'.");

        package["version"] = _packageVersion;
        var updated = manifest
            .ToJsonString(new JsonSerializerOptions { WriteIndented = true })
            .ReplaceLineEndings("\n")
            + "\n";
        WriteAllTextIfChanged(McpManifestFile, source, updated);
    }

    void VerifyPackedPackage()
    {
        var packageFile = PackagesDirectory / $"{PackageId}.{_packageVersion}.nupkg";
        if (!File.Exists(packageFile))
        {
            throw new InvalidOperationException($"The expected package was not created: {packageFile}");
        }

        using var archive = ZipFile.OpenRead(packageFile);
        var manifest = JsonNode.Parse(ReadPackageEntry(archive, ".mcp/server.json"))?.AsObject()
            ?? throw new InvalidOperationException("The packaged MCP server manifest is not a JSON object.");
        VerifyExpectedVersion("packaged MCP manifest", manifest["version"]?.GetValue<string>());

        var package = manifest["packages"]?.AsArray()
            .OfType<JsonObject>()
            .SingleOrDefault(candidate => string.Equals(
                candidate["identifier"]?.GetValue<string>(),
                PackageId,
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"The packaged MCP manifest does not contain package '{PackageId}'.");
        VerifyExpectedVersion("packaged MCP package metadata", package["version"]?.GetValue<string>());

        var nuspecEntries = archive.Entries
            .Where(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (nuspecEntries.Count != 1)
        {
            throw new InvalidOperationException(
                $"The package must contain exactly one nuspec file; found {nuspecEntries.Count}.");
        }

        var nuspec = XDocument.Parse(ReadPackageEntry(nuspecEntries[0]));
        var metadata = nuspec.Descendants().Single(element => string.Equals(
            element.Name.LocalName,
            "metadata",
            StringComparison.Ordinal));
        var nuspecId = metadata.Elements().Single(element => string.Equals(
            element.Name.LocalName,
            "id",
            StringComparison.Ordinal)).Value;
        if (!string.Equals(nuspecId, PackageId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"NuGet package id '{nuspecId}' does not match expected id '{PackageId}'.");
        }

        var nuspecVersion = metadata.Elements().Single(element => string.Equals(
            element.Name.LocalName,
            "version",
            StringComparison.Ordinal)).Value;
        VerifyExpectedVersion("NuGet package", nuspecVersion);

        VerifyPackageEntryMatchesSource(archive, $"{SkillPackagePath}/SKILL.md", SkillFile);
        VerifyPackageEntryMatchesSource(archive, $"{SkillPackagePath}/agents/openai.yaml", SkillMetadataFile);
        VerifyPackageEntryMatchesSource(archive, $"{ToolPayloadPath}/{SkillPackagePath}/SKILL.md", SkillFile);
        VerifyPackageEntryMatchesSource(
            archive,
            $"{ToolPayloadPath}/{SkillPackagePath}/agents/openai.yaml",
            SkillMetadataFile);

        Log.Information(
            "Verified package id, manifest, nuspec, and agent skill for MinVer package version {PackageVersion}",
            _packageVersion);
    }

    void VerifyExpectedVersion(string source, string? actualVersion)
    {
        if (!string.Equals(actualVersion, _packageVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{source} version '{actualVersion}' does not match MinVer package version '{_packageVersion}'.");
        }
    }

    static string ReadPackageEntry(ZipArchive archive, string entryName)
    {
        var entry = archive.GetEntry(entryName)
            ?? throw new InvalidOperationException($"The package does not contain {entryName}.");
        return ReadPackageEntry(entry);
    }

    static string ReadPackageEntry(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    static void VerifyPackageEntryMatchesSource(
        ZipArchive archive,
        string entryName,
        AbsolutePath sourceFile)
    {
        var packageEntries = archive.Entries
            .Where(entry => string.Equals(entry.FullName, entryName, StringComparison.Ordinal))
            .ToList();
        if (packageEntries.Count != 1)
        {
            throw new InvalidOperationException(
                $"The package must contain exactly one '{entryName}' entry; found {packageEntries.Count}.");
        }

        var expected = File.ReadAllText(sourceFile).ReplaceLineEndings("\n");
        var actual = ReadPackageEntry(packageEntries[0]).ReplaceLineEndings("\n");
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The packaged entry '{entryName}' does not match its source file '{sourceFile}'.");
        }
    }

    static void WriteAllTextIfChanged(AbsolutePath path, string source, string updated)
    {
        if (string.Equals(source, updated, StringComparison.Ordinal))
        {
            return;
        }

        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, updated);
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }
}
