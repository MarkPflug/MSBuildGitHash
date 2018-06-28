# MSBuildGitHash
Includes the Git repository hash in your compiled .NET assemblies. 
This allows you to easily correlate an assembly to exact version of the code that produced it.

## Nuget

This project is available as a nuget package: https://www.nuget.org/packages/MSBuildGitHash.

## Usage
By default, including the nuget package (MSBuildGitHash) will automatically add the git repository hash to your assembly as a `System.Reflection.AssemblyMetadataAttribute` with they key "GitHash". As of 0.3.0, you can optionally include the remote repository URL as well, typically this would be "origin". The repository URL will be attached with the key "GitRepository". To include the repository URL, you must specify the name of the git remote in your project file:

```xml
<PropertyGroup>
...
<MSBuildGitHashRemote>origin</MSBuildGitHashRemote>
</PropertyGroup>
```

Alternately, you can hard-code the repository URL by defining the `MSBuildGitRepository` property in your project file.

Basic validation is performed on the generated hash version to ensure that a git command error doesn't result in a bad value being attached. If the validation causes problems for some reason, it can be disabled by defining the `<MSBuildGitHashValidate>False</MSBuildGitHashValidate>` in your project.

## Customization

By default, the package will include the output of the command `git descibe --long --always --dirty`. This produces a truncation (first 8 hex characters) of the full repository hash. You can customize the command that is executed by defining the `MSBuildGitHashCommand` property in your .csproj file. For example, if you want to include the full hash, you can add the following:

```xml
<PropertyGroup>
  <MSBuildGitHashCommand>git rev-parse HEAD</MSBuildGitHashCommand>
</PropertyGroup>
```

## Version History

_0.3.0_
- Adds option to include git remote repository url as well.

_0.2.0_
- Adds optional validation that the hash value looks correct.
- Improved build error message when git commands fail.

_0.1.0_
- Base functionality.
