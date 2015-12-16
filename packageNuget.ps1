Remove-Item .\packageoutput\ -Recurse -ErrorAction Ignore

.\setBuildVersion.ps1
$nugetCmd = ".\nuget.exe push .\packageoutput\Release\*-" + $env:DNX_BUILD_VERSION + ".nupkg"

dnu pack .\src\MicroserviceAnalytics.Ado\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.AspNet4\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.Core\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.EF6\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.GenericDotNet\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.MVC5\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.OWIN\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.Serilog\project.json --configuration Release
dnu pack .\src\MicroserviceAnalytics.WebAPI2\project.json --configuration Release

Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.Ado\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.AspNet4\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.Core\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.EF6\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.GenericDotNet\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.MVC5\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.OWIN\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.Serilog\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
Invoke-Expression ".\nuget.exe push .\src\MicroserviceAnalytics.WebAPI2\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"