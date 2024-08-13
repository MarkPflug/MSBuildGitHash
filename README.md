# MSBuildGitHash
Includes the Git repository hash in your compiled .NET assemblies. 
This allows you to easily correlate an assembly to exact version of the code that produced it.

## Nuget

This project is available as a nuget package: https://www.nuget.org/packages/MSBuildGitHash.

## Usage
By default, including the nuget package (MSBuildGitHash) will automatically add the git repository hash to your assembly as a `System.Reflection.AssemblyMetadataAttribute` with they key "GitHash". As of 0.4.0, it will include the git repository URL as well. This value is taken from the `RepositoryUrl` MSBuild property which is also used by nuget. This is only used if the RepositoryType is `git`. The repository URL will be attached with the key "GitRepository". 

Basic validation is performed on the generated hash version to ensure that a git command error doesn't result in a bad value being attached. If the validation causes problems for some reason, it can be disabled by defining the `<MSBuildGitHashValidate>False</MSBuildGitHashValidate>` in your project.

## Customization

### Git hash format

By default, the package will include the output of the command `git describe --long --always --dirty`. This produces a truncation (first 7 hex characters) of the full repository hash. You can customize the command that is executed by defining the `MSBuildGitHashCommand` property in your .csproj file. For example, if you want to include the full hash, you can add the following:

```xml
<PropertyGroup>
  <MSBuildGitHashCommand>git rev-parse HEAD</MSBuildGitHashCommand>
</PropertyGroup>
```

### Git hash length

Some IDEs and also DevOps platforms show first 8 hex characters. To change the default length from 7 hex characters to 8 hex characters you can add the following:

```xml
<PropertyGroup>
  <MSBuildGitHashCommand>git describe --long --always --dirty --exclude=* --abbrev=8</MSBuildGitHashCommand>
</PropertyGroup>
```

### Informational Version (aka Product Version) format

By default, the git hash is appended to the Informational Version attribute value.

Alternatively, the git hash can replace the Informational Version attribute value by setting `MSBuildGitHashReplaceInfoVersion` to `True` in your .csproj file:

```xml
<PropertyGroup>
  <MSBuildGitHashReplaceInfoVersion>True</MSBuildGitHashReplaceInfoVersion>
</PropertyGroup>
```
### Command Output to Assembly Metadata

If there are additional commands and output that are needed to be included in the meta data, you can do so by adding them using `AssemblyEmbed`. Examples:

```xml
<AssemblyMetadataCommand Include="KeyNameForOutput">
  <Command>your command</Command>
  <ValidationRegex>optional regex to validate output</ValidationRegex>
</AssemblyMetadataCommand>
```

```xml
<ItemGroup>
  <AssemblyMetadataCommand Include="GitDescription">
    <Command>git describe</Command>
  </AssemblyMetadataCommand>
  <AssemblyMetadataCommand Include="GitTag">
    <Command>git describe --tag --abbrev=0</Command>
  </AssemblyMetadataCommand>
</ItemGroup>
```

### Extract Assembly Metadata

```csharp
using System.Reflection;

string gitHash = Assembly
	.GetEntryAssembly()
	.GetCustomAttributes<AssemblyMetadataAttribute>()
	.FirstOrDefault(attr => attr.Key == "GitHash")?.Value;
```

## Version History
_2.0.2_
- Fix issues introduced by changes from the Microsoft.NET.Sdk.

_2.0.1_
- Restores the ability to use `MSBuildGitHashSuppressDirtyWarning` to suppress warnings that was lost during the 2.0 refactor.

_2.0.0_
- Now includes the most recent git tag as metadata as well, if the repository has any tags.
- Significant refactoring to allow more easily adding additional assembly metadata. Thanks [@zhilbug](https://github.com/zhilbug) for the bulk of the work on this.
- This is a major version, because it has more potential to affect existing builds. Projects that don't include signification build customizations should continue to work without modification.

_1.0.2_
- MSBuildGitHash is now properly marked as a development dependency, and so won't show up as a transitive dependency in your package.

_1.0.1_
- Fixes an issue where git tags would appear in hash version, causing validation failure.

_1.0.0_
- Adds the ability to disable error when running outside a git repository via `<MSBuildGitHashValidateSuccess>false`. Will emit a warning instead of error in such case.
    Can be specified on MSBuild command line as `/p:MSBuildGitHashValidateSuccess=false` or by setting an environment variable of the same name/value.

_0.5.3_ 
- Emits a warning when the repository is dirty. Can be suppressed with `<MSBuildGitHashSuppressDirtyWarning>True`.

_0.5.2_ 
- Adds ability to replace assembly InformationalVersion via `MSBuildGitHashReplaceInfoVersion`. Thanks @Br3nt

_0.5.1_ 
- Fix for #15, to allow supression of AssemblyInformationalVersionAttribute.

_0.5.0_
- Removed framework dependency from nupkg.

_0.4.1_
- No functional change.
- Minor build script cleanup.

_0.4.0_
- Adds an AssemblyInformationalVersion attribute containing the git version. This value shows up in the standard Windows properties dialog.
- No longer uses temp files in "obj" folder to operate.
- Replace remote repository to use standard "RepositoryUrl" used by nuget.
- Working unit tests.

_0.3.0_
- Adds option to include git remote repository url as well.

_0.2.0_
- Adds optional validation that the hash value looks correct.
- Improved build error message when git commands fail.

_0.1.0_
- Base functionality.
