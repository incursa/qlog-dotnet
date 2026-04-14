using System.Diagnostics;
using System.Text;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class ReleaseVersioningScriptTests
{
    [Fact]
    public async Task ValidatePublicApiVersioning_AcceptsTheInitialReleaseVersion()
    {
        using TemporaryDirectory temp = new();
        await InitializeReleaseRepoAsync(temp.Path);

        string output = await InvokePowerShellAsync(
            temp.Path,
            """
            & ./scripts/release/validate-public-api-versioning.ps1 -Tag v1.0.6
            """);

        Assert.Contains("No prior release tags found. Skipping incremental semver enforcement.", output, StringComparison.Ordinal);
        Assert.Contains("Unshipped API baselines: no changes since prior release.", output, StringComparison.Ordinal);

        string summaryPath = Path.Combine(temp.Path, "artifacts", "release", "api-versioning-summary.md");
        Assert.True(File.Exists(summaryPath));

        string summary = await File.ReadAllTextAsync(summaryPath);
        Assert.Contains("Tag: v1.0.6", summary, StringComparison.Ordinal);
        Assert.Contains("Prior tag: none", summary, StringComparison.Ordinal);
        Assert.Contains("Required bump: None", summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeReleaseVersioning_CalculateOnlyReportsTheInitialReleaseVersion_WithoutMutatingFiles()
    {
        using TemporaryDirectory temp = new();
        await InitializeReleaseRepoAsync(temp.Path);

        string propsPath = Path.Combine(temp.Path, "Directory.Build.props");
        string originalProps = await File.ReadAllTextAsync(propsPath);

        string output = await InvokePowerShellAsync(
            temp.Path,
            """
            & ./scripts/release/Invoke-ReleaseVersioning.ps1 -CalculateOnly
            """);

        Assert.Contains("Current props version: 1.0.6", output, StringComparison.Ordinal);
        Assert.Contains("Prior release tag: none", output, StringComparison.Ordinal);
        Assert.Contains("Required bump: Patch", output, StringComparison.Ordinal);
        Assert.Contains("Target release version: 1.0.7", output, StringComparison.Ordinal);
        Assert.Contains("Calculation only; no files will be modified or release actions performed.", output, StringComparison.Ordinal);
        Assert.Equal(originalProps, await File.ReadAllTextAsync(propsPath));
    }

    private static async Task InitializeReleaseRepoAsync(string path)
    {
        Directory.CreateDirectory(Path.Combine(path, "scripts", "release"));
        Directory.CreateDirectory(Path.Combine(path, "src", "Test.Library"));

        await File.WriteAllTextAsync(
            Path.Combine(path, "Directory.Build.props"),
            """
            <Project>
              <PropertyGroup>
                <Version>1.0.6</Version>
              </PropertyGroup>
            </Project>
            """,
            Encoding.UTF8);

        await File.WriteAllTextAsync(
            Path.Combine(path, "src", "Test.Library", "Test.Library.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """,
            Encoding.UTF8);

        await File.WriteAllTextAsync(
            Path.Combine(path, "src", "Test.Library", "PublicAPI.Shipped.txt"),
            """
            #nullable enable
            Test.Library.Example
            Test.Library.Example.Example() -> void
            """,
            Encoding.UTF8);

        await File.WriteAllTextAsync(
            Path.Combine(path, "src", "Test.Library", "PublicAPI.Unshipped.txt"),
            string.Empty,
            Encoding.UTF8);

        await File.WriteAllTextAsync(
            Path.Combine(path, "README.md"),
            "initial" + Environment.NewLine,
            Encoding.UTF8);

        string repoScriptsRoot = Path.Combine(path, "scripts", "release");
        string sourceScriptsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "release"));
        await File.WriteAllTextAsync(
            Path.Combine(repoScriptsRoot, "Invoke-ReleaseVersioning.ps1"),
            await File.ReadAllTextAsync(Path.Combine(sourceScriptsRoot, "Invoke-ReleaseVersioning.ps1"), Encoding.UTF8),
            Encoding.UTF8);
        await File.WriteAllTextAsync(
            Path.Combine(repoScriptsRoot, "validate-public-api-versioning.ps1"),
            await File.ReadAllTextAsync(Path.Combine(sourceScriptsRoot, "validate-public-api-versioning.ps1"), Encoding.UTF8),
            Encoding.UTF8);

        await RunProcessAsync(path, "git", "init");
        await RunProcessAsync(path, "git", "config user.email \"qlog-tests@example.com\"");
        await RunProcessAsync(path, "git", "config user.name \"Qlog Tests\"");
        await RunProcessAsync(path, "git", "add -A");
        await RunProcessAsync(path, "git", "commit -m \"initial release\"");
    }

    private static async Task<string> InvokePowerShellAsync(string workingDirectory, string script)
    {
        using TemporaryDirectory temp = new();
        string scriptPath = Path.Combine(temp.Path, "test.ps1");
        await File.WriteAllTextAsync(
            scriptPath,
            """
            $ErrorActionPreference = 'Stop'
            Set-StrictMode -Version Latest
            """ + Environment.NewLine + script,
            Encoding.UTF8);

        ProcessStartInfo startInfo = new()
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -File \"{scriptPath}\"",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start pwsh.");

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Xunit.Sdk.XunitException($"pwsh exited with code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");
        }

        return stdout.Trim();
    }

    private static async Task RunProcessAsync(string workingDirectory, string fileName, string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Xunit.Sdk.XunitException($"{fileName} {arguments} exited with code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "qlog-dotnet-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                ResetAttributes(Path);

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        Directory.Delete(Path, recursive: true);
                        return;
                    }
                    catch (UnauthorizedAccessException) when (attempt < 4)
                    {
                        global::System.Threading.Thread.Sleep(100);
                    }
                    catch (IOException) when (attempt < 4)
                    {
                        global::System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }

        private static void ResetAttributes(string root)
        {
            foreach (string filePath in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }
        }
    }
}
