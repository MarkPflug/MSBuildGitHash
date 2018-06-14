using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class MSBuildGitHashTests
{
    ILogger logger;
    ITestOutputHelper o;

    static Dictionary<string, string> gp =
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
        var outputPath = proj.GetPropertyValue("TargetPath");
        Assert.True(result, "Build failed");
        return outputPath;
    }

    [Fact]
    public void BuildTest()
    {
        var exepath = BuildProject("Data/Proj1/Proj.csproj");
    }
}