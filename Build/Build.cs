using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities.Collections;

using static System.IO.Directory;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.SonarScanner.SonarScannerTasks;

[GitHubActions
(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { "*" },
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(Test), nameof(SonarEnd) },
    FetchDepth = 0,
    ImportSecrets = new[] { nameof(SonarProjectKey), nameof(SonarToken), nameof(SonarHostUrl), nameof(SonarOrganization) }
)]
[GitHubActions
(
    "publish",
    GitHubActionsImage.UbuntuLatest,
    OnPushTags = new[] { "*" },
    InvokedTargets = new[] { nameof(PushToNuGet) },
    FetchDepth = 0,
    ImportSecrets = new[] { nameof(NugetApiUrl), nameof(NugetApiKey) }
)]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("NuGet feed to use")] readonly string NugetApiUrl = default!;
    [Parameter("NuGet API key")] readonly string NugetApiKey = default!;

    [Parameter("Sonar project key")] readonly string SonarProjectKey = default!;
    [Parameter("Sonar token")] readonly string SonarToken = default!;
    [Parameter("Sonar host URL")] readonly string SonarHostUrl = default!;
    [Parameter("Sonar organization")] readonly string SonarOrganization = default!;

    [Solution(GenerateProjects = true)] readonly Solution Solution = default!;
    [GitVersion] readonly GitVersion GitVersion = default!;

    AbsolutePath SourceDirectory => RootDirectory / "Sources";
    AbsolutePath TestsDirectory => RootDirectory / "Tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "Artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
            DotNetRestore(s => s
                .SetProjectFile(Solution)));

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableNoRestore()));

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableCollectCoverage()
                .SetCoverletOutputFormat(CoverletOutputFormat.opencover)));

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
            DotNetPack(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetPackageReleaseNotes(File.ReadAllText(Solution.Directory / "CHANGELOG.md"))));

    Target MoveArtifacts => _ => _
        .DependsOn(Pack)
        .Executes(() =>
            Solution
                .AllProjects
                .Where(p => p != Solution.Build)
                .SelectMany(p => EnumerateFiles(p.Directory, "*.nupkg", SearchOption.AllDirectories))
                .Where(n => !n.EndsWith("symbols.nupkg"))
                .ForEach(x => CopyFileToDirectory(x, ArtifactsDirectory, FileExistsPolicy.Overwrite)));

    Target PushToNuGet => _ => _
        .DependsOn(MoveArtifacts)
        .Requires(() => NugetApiUrl, () => NugetApiKey)
        .Executes(() =>
            EnumerateFiles(ArtifactsDirectory, "*.nupkg")
                .ForEach(x =>
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NugetApiKey))));

    Target SonarBegin => _ => _
        .Before(Restore)
        .Requires(() => SonarProjectKey)
        .Requires(() => SonarToken)
        .Requires(() => SonarHostUrl)
        .Requires(() => SonarOrganization)
        .Executes(() => SonarScannerBegin(s => s
            .SetFramework("net5.0")
            .SetProjectKey(SonarProjectKey)
            .SetLogin(SonarToken)
            .SetServer(SonarHostUrl)
            .SetOrganization(SonarOrganization)
            .SetOpenCoverPaths("**/*.opencover.xml")
            .SetVersion(GitVersion.NuGetVersionV2)));

    Target SonarEnd => _ => _
        .DependsOn(SonarBegin)
        .After(Test)
        .Executes(() => SonarScannerEnd(s => s
            .SetFramework("net5.0")
            .SetLogin(SonarToken)));
}
