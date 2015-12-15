Remove-Item .\packageoutput\ -Recurse -ErrorAction Ignore

.\setBuildVersion.ps1
$nugetCmd = ".\nuget.exe push .\packageoutput\Release\*-" + $env:DNX_BUILD_VERSION + ".nupkg"

dnu pack .\src\* --out packageoutput --configuration Release
Invoke-Expression $nugetCmd
