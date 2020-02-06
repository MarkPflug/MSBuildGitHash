using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

class MsBuildFixture : IDisposable
{
	public MsBuildFixture()
	{
		MSBuildLocator.RegisterDefaults();
	}

	public void Dispose()
	{
		MSBuildLocator.Unregister();
	}
}

[CollectionDefinition("MSBuild")]
public class MSBuildCollection : ICollectionFixture<MsBuildFixture>
{
}

[Collection("MSBuild")]
public class MSBuildGitHashTests
{
	readonly ILogger logger;
	ITestOutputHelper o;

	static readonly Dictionary<string, string> gp =
		new Dictionary<string, string>
		{
			["Configuration"] = "Debug",
			["Platform"] = "AnyCPU",
		};

	public MSBuildGitHashTests(ITestOutputHelper o)
	{
		this.o = o;
		this.logger = new XUnitTestLogger(o);
	}

	string GetOutput(string exePath, string args)
	{
		var psi = new ProcessStartInfo(exePath, args)
		{
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true,
		};
		var proc = Process.Start(psi);
		var text = proc.StandardOutput.ReadToEnd();
		proc.WaitForExit();
		return text;
	}

	void LogProps(Project proj)
	{
		foreach (var kvp in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().OrderBy(e => e.Key))
		{
			o.WriteLine(kvp.Key + ": " + kvp.Value);
		}

		foreach (var prop in proj.AllEvaluatedProperties.OrderBy(p => p.Name))
		{
			o.WriteLine(prop.Name + ": " + prop.EvaluatedValue + " (" + prop.UnevaluatedValue + ")");
		}
	}

	string BuildProject(string projFile)
	{
		var pc = new ProjectCollection(gp);
		var proj = pc.LoadProject(projFile);
		var restored = proj.Build("Restore", new[] { logger });
		if (!restored)
		{
			LogProps(proj);
		}
		Assert.True(restored, "Failed to restore packages");
		var result = proj.Build(logger);
		LogProps(proj);
		var outputPath = proj.GetPropertyValue("TargetPath");
		Assert.True(result, "Build failed");
		return outputPath;
	}

	const string InfoVersionPattern = @"^\d+.\d+.\d+\+[0-9a-f]{7}(-dirty)?$";
	const string InfoVersionShortPattern = @"^[0-9a-f]{7}(-dirty)?$";

	[Theory]
	[InlineData("Data/Sdk1/Proj.csproj")]
	[InlineData("Data/SdkVb/Proj.vbproj")]
	public void SdkProjectTest(string projectFile)
	{
		var exepath = BuildProject(projectFile);
		var v = FileVersionInfo.GetVersionInfo(exepath).ProductVersion;
		Assert.Matches(InfoVersionPattern, v);
		var asm = Assembly.LoadFile(exepath);
		var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
		var attr = attrs.FirstOrDefault(a => a.Key == "GitRepository");
		Assert.NotNull(attr);
		Assert.Equal("https://github.com/MarkPflug/MSBuildGitHash", attr.Value);
	}

	[Theory]
	[InlineData("Data/Legacy2/Proj.csproj")]
	[InlineData("Data/Sdk2/Proj.csproj")]
	public void ProjectInfoVersionTest(string projectFile)
	{
		var exepath = BuildProject(projectFile);
		var v = FileVersionInfo.GetVersionInfo(exepath).ProductVersion;
		Assert.Matches(InfoVersionShortPattern, v);
		var asm = Assembly.LoadFile(exepath);
		var attr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		Assert.NotNull(attr);
		Assert.NotNull(attr.InformationalVersion);
	}

	[Fact]
	public void LegacyProjectTest()
	{
		var exepath = BuildProject("Data/Legacy1/Proj.csproj");
		var v = FileVersionInfo.GetVersionInfo(exepath).ProductVersion;
		Assert.Matches(InfoVersionPattern, v);
	}
}