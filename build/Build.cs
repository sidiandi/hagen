using System.Threading.Tasks;
using System;
using System.IO;
using Amg.Build;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Linq;

public partial class BuildTargets
{
    static int Main(string[] args) => Runner.Run<BuildTargets>(args);

    public BuildTargets(string configuration)
    {
        Configuration = configuration;
    }

    public BuildTargets()
    {
    }

    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    string productName => name;
    string year => DateTime.UtcNow.ToString("yyyy");
    string copyright => $"Copyright (c) {company} {year}";

    [Description("Release|Debug")]
    public string Configuration { get; set; } = "Release";

    string Root { get; set; } = Runner.RootDirectory();

    string OutDir => Root.Combine("out", Configuration);
    string PackagesDir => OutDir.Combine("packages");
    string SrcDir => Root;
    string CommonAssemblyInfoFile => OutDir.Combine("generated", "CommonAssemblyInfo.cs");
    string VersionPropsFile => OutDir.Combine("generated", "Version.props");

    string SlnFile => SrcDir.Combine($"{name}.sln");
    string LibDir => SrcDir.Combine(name);

    [Once]
    protected virtual Dotnet Dotnet => Once.Create<Dotnet>();

    [Once]
    protected virtual Git Git => Git.Create(Runner.RootDirectory());

    static string FindMsbuild()
    {
        return new[]
        {
            @"Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            @"Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
        }.Select(_ => @"C:\Program Files (x86)".Combine(_)).First(_ => _.IsFile());
    }

    [Once]
    protected virtual ITool Msbuild => Tools.Default
        .WithFileName(FindMsbuild())
        .WithArguments("-verbosity:minimal");

    [Once]
    [Description("Nuget restore")]
    public virtual async Task Restore()
    {
        await Nuget.Run("restore", SlnFile);
    }

    [Once]
    protected virtual async Task GenerateCode()
    {
        await WriteAssemblyInformationFile();
        await WriteVersionPropsFile();
    }

    [Once]
    protected virtual Task TerminateRunningApplication() => Task.Factory.StartNew(() =>
    {
        var p = Process.GetProcessesByName("hagen");
        foreach (var i in p)
        {
            Logger.Information("Kill {process}", i.ProcessName);
            i.Kill();
        }
    }, TaskCreationOptions.LongRunning);

    [Once]
    [Description("Build")]
    public virtual async Task Build()
    {
        await Task.WhenAll(Restore(), GenerateCode(), TerminateRunningApplication());
        await Msbuild.Run(SlnFile, "/p:Configuration=" + Configuration);
    }

    [Once]
    protected virtual async Task<string> WriteAssemblyInformationFile()
    {
        var v = await Git.GetVersion();
        return await CommonAssemblyInfoFile.WriteAllTextIfChangedAsync(
$@"// Generated. Changes will be lost.
[assembly: System.Reflection.AssemblyCopyright({copyright.Quote()})]
[assembly: System.Reflection.AssemblyCompany({company.Quote()})]
[assembly: System.Reflection.AssemblyProduct({productName.Quote()})]
[assembly: System.Reflection.AssemblyVersion({v.AssemblySemVer.Quote()})]
[assembly: System.Reflection.AssemblyFileVersion({v.AssemblySemFileVer.Quote()})]
[assembly: System.Reflection.AssemblyInformationalVersion({v.InformationalVersion.Quote()})]
");
    }

    [Once]
    protected virtual async Task<string> WriteVersionPropsFile()
    {
        var v = await Git.GetVersion();
        return await VersionPropsFile.WriteAllTextIfChangedAsync(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <PropertyGroup>
        <VersionPrefix>{v.MajorMinorPatch}</VersionPrefix>
        <VersionSuffix>{v.NuGetPreReleaseTagV2}</VersionSuffix>
    </PropertyGroup>
</Project>

");
    }

    static async Task Start(string command)
    {
        Process.Start(command);
        await Task.CompletedTask;
    }

    [Once]
    protected virtual Task<ITool> NunitConsole() => NugetTool("nunit.consolerunner", "nunit3-console.exe");

    [Once]
    [Description("run unit tests")]
    public virtual async Task Test()
    {
        await Task.WhenAll(NunitConsole(), Build());
        var tests = OutDir.Glob().Include("**/bin/*.Test.dll");
        Console.WriteLine(tests.Join());
        await (await NunitConsole())
            .WithArguments(
                "--output", OutDir.Combine("test-result.xml"),
                "--noresult"
            )
            .Run(tests.EnumerateFiles().ToArray());
    }

    ITool Nuget => Tools.Default.WithFileName("Nuget.exe");

    [Once]
    protected virtual async Task<ITool> NugetTool(string name, string exe)
    {
        var r = await Nuget.Run("install", name, "-ForceEnglishOutput");

        var m = Regex.Match(r.Output, @"Installing package '([^']+)' to '([^']+)'.");
        var packageId = m.Groups[1].Value;
        var dir = m.Groups[2].Value;
        string version = null;

        m = Regex.Match(r.Output, @"Successfully installed '([^ ]+) ([^ ]+)' to");
        if (m.Success)
        {
            version = m.Groups[2].Value;
        }
        else
        {
            m = Regex.Match(r.Output, $@"Package ""{packageId.ToLower()}\.([^""]+)"" is already installed.", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
            }
        }

        var installDir = dir.Combine(new[] { packageId, version }.Join("."));

        var fileName = installDir.Combine("tools", exe);
        Logger.Information("nuget tool {name} uses {fileName}", name, fileName);
        return Tools.Default.WithFileName(fileName);
    }

    [Once]
    [Description("run")]
    public virtual async Task Run()
    {
        var hagen = (OutDir.Combine(name, "bin", $"{name}.exe"));
        Logger.Information(hagen);
        if (hagen.IsOutOfDate(SrcDir.Glob("**")
            .Include("hagen/**")
            .Exclude("out")
            .EnumerateFiles()))
        {
            await Build();
        }
        await Start(hagen);
    }

    [Once]
    [Description("Delete all build results")]
    public virtual async Task Clean()
    {
        await TerminateRunningApplication();
        await EnsureNotExists(OutDir);
    }

    static async Task<string> EnsureNotExists(string path)
    {
        await Task.Factory.StartNew(() =>
        {
            if (path.IsFile())
            {
                path.EnsureFileNotExists();
            }
            else if (path.IsDirectory())
            {
                Directory.Delete(path, true);
            }
        }, TaskCreationOptions.LongRunning);
        return path;
    }

    [Once]
    [Default]
    public virtual async Task Default()
    {
        await Test();
    }

    [Once]
    [Description("Open in Visual Studio")]
    public virtual async Task OpenInVisualStudio()
    {
        foreach (var configuration in new[] { "Release", "Debug" })
        {
            var build = Once.Create<BuildTargets>(configuration);
            await build.GenerateCode();
        }
        await Start(SlnFile);
    }

    [Once][Description("Install to c:\bin")]
    public virtual async Task Install()
    {
        await TerminateRunningApplication();
        await Build();
        var installDir = @"C:\bin\hagen";
        foreach (var assemblyOutput in OutDir.Glob("hagen/bin").EnumerateFileSystemInfos().Where(_ => _ is DirectoryInfo))
        {
            await assemblyOutput.FullName.CopyTree(installDir);
        }

        var plugins = OutDir.Glob("hagen.plugin.*/bin").EnumerateFileSystemInfos()
            .Where(_ => _ is DirectoryInfo)
            .Select(_ => _.FullName.Parent().FileName())
            .Where(_ => !_.EndsWith(".Test"))
            .ToList();

        foreach (var plugin in plugins)
        {
            var assemblyOutput = OutDir.Combine(plugin, "bin");
            await assemblyOutput.CopyTree(installDir.Combine("plugin", plugin));
        }
        await Start(installDir.Combine(name + ".exe"));
    }
}

