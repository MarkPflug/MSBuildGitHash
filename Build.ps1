if((Test-Path .\nuget.exe) -ne $True) {
    Invoke-WebRequest -uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -outfile Nuget.exe
}
.\Nuget.exe pack Package.nuspec