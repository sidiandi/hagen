#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0014"

using System.IO;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var outDir = Directory($"./out/{configuration}");
var IntermediateOutputPath = outDir + Directory("temp");

var CompanyName = "Andreas Grimme";
var ProductName = "hagen";
var Platform = "AnyCPU";

var UpgradeCode = "57CAC978-D1D8-4375-A93F-E906E4E66D60";

string version = null;
string informationalVersion = null;

static string Quote(string x)
{
    return "\"" + x + "\"";
}

Task("Clean")
  .Does(() =>
{
  CleanDirectory(outDir);
});

Task("GetVersion")
.Does(() =>
{
  var gitVersion = GitVersion();
  version = gitVersion.SemVer;
  informationalVersion = gitVersion.InformationalVersion;
});

Task("WriteCommonAssemblyInfo")
.IsDependentOn("GetVersion")
.Does(() =>
{
  var Output = $"{outDir}/CommonAssemblyInfo.cs";
  System.IO.File.WriteAllText(Output, $@"// Generated. Changes will be lost.
[assembly: System.Reflection.AssemblyCopyright({Quote($"Copyright (c) {CompanyName} {DateTime.Now.Year}")})]
[assembly: System.Reflection.AssemblyCompany({Quote(CompanyName)})]
[assembly: System.Reflection.AssemblyProduct({Quote(ProductName)})]
[assembly: System.Reflection.AssemblyFileVersion({Quote(version)})]
[assembly: System.Reflection.AssemblyVersion({Quote(version)})]
[assembly: System.Reflection.AssemblyInformationalVersion({Quote(informationalVersion)})]
");

});

Task("Build")
  .IsDependentOn("WriteCommonAssemblyInfo")
  .Does(() =>
{
  MSBuild("hagen.sln", new MSBuildSettings()
  .WithProperty("Configuration", configuration)
  .WithTarget("Restore;Build"));
});

Task("UnitTest")
  .IsDependentOn("Build")
  .Does(() =>
{
    NUnit3($"{outDir}/**/bin/*.Test.dll", new NUnit3Settings
    {
        NoResults = true
    });
});

Task("ZipProductFiles")
.IsDependentOn("Build")
.Does(()=>{
  Zip($"{outDir}/{ProductName}/bin", $"{outDir}/{ProductName}-{version}.zip");
});

Task("Setup")
.IsDependentOn("Build")
.Does(() =>
{
  MSBuild("wix/wix.wixproj", new MSBuildSettings()
  .WithProperty("CompanyName", CompanyName)
  .WithProperty("ProductName", ProductName)
  .WithProperty("Version", version)
  .WithProperty("Description", informationalVersion)
  .WithProperty("Configuration", configuration)
  .WithProperty("Platform", Platform)
  .WithProperty("OutputPath", outDir)
  .WithProperty("BinDir", outDir)
  .WithProperty("UpgradeCode", UpgradeCode)
  .WithProperty("IntermediateOutputPath", $"{IntermediateOutputPath}/setup/")
  );
});

Task("Install")
.IsDependentOn("Setup")
.Does(() =>
{
  var MsiFile = MakeAbsolute(outDir + File($"{ProductName}-{version}.msi")).FullPath.Replace("/", "\\");
  StartProcess("msiexec", $"/quiet /x {Quote(MsiFile)}");
  StartProcess("msiexec", $"/package {Quote(MsiFile)}");
});

/*
 todo:

  <Target Name="Tag" DependsOnTargets="CallGetVersion">
    <PropertyGroup>
      <ReleaseTag>v$(GfvFullSemVer)</ReleaseTag>
    </PropertyGroup>
    <Exec WorkingDirectory="$(SourceDir)" Command="$(Git) tag --force $(ReleaseTag)" />
  </Target>

  <Target Name="Pack" DependsOnTargets="CallGetVersion">
    <PropertyGroup>
		<PackageDir>$(BuildDir)\package</PackageDir>
	</PropertyGroup>
	<RemoveDir Directories="$(PackageDir)"/>
	<MakeDir Directories="$(PackageDir)" />
	<Exec WorkingDirectory="$(ReleaseDir)" Command="&quot;$(Nuget)&quot; pack &quot;$(SourceDir)\%(Package.Identity)\%(Package.Identity).csproj&quot; -OutputDirectory &quot;$(PackageDir)&quot; -Version $(Version)" />
	<ItemGroup>
		<PackageFiles Include="$(PackageDir)\%(Package.Identity).$(Version).nupkg" />
	</ItemGroup>
  </Target>

  <Target Name="Push" DependsOnTargets="Test;Pack">
    <Exec WorkingDirectory="$(ReleaseDir)" Command="&quot;$(Nuget)&quot; push &quot;%(PackageFiles.Identity)&quot; -ApiKey $(NugetApiKey) -Source $(NugetFeed)" />
  </Target>

  <Target Name="Install" DependsOnTargets="Setup">
    <Exec Command="&quot;@(MsiFiles, ' ')&quot;" />
  </Target>
  
*/

Task("Run")
  .IsDependentOn("Build")
  .Does(() =>
  {
    var exe = $"{outDir}/{ProductName}/bin/{ProductName}.exe";
    Information(exe);
    StartAndReturnProcess(exe, new ProcessSettings{Arguments = ProcessArgumentBuilder.FromString("--popup") });
  });

Task("Default")
  .IsDependentOn("UnitTest")
  .IsDependentOn("Setup")
;

RunTarget(target);