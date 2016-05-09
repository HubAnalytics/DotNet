Remove-Item .\packageoutput\ -Recurse -ErrorAction Ignore

#.\setBuildVersion.ps1
#$nugetCmd = ".\nuget.exe push .\packageoutput\Release\*-" + $env:DNX_BUILD_VERSION + ".nupkg"
$packageVersion = "1.3.1"

dnu pack .\src\HubAnalytics.Ado\project.json --configuration Release
dnu pack .\src\HubAnalytics.AspNet4\project.json --configuration Release
dnu pack .\src\HubAnalytics.Core\project.json --configuration Release
dnu pack .\src\HubAnalytics.EF6\project.json --configuration Release
dnu pack .\src\HubAnalytics.GenericDotNet\project.json --configuration Release
dnu pack .\src\HubAnalytics.MVC5\project.json --configuration Release
dnu pack .\src\HubAnalytics.OWIN\project.json --configuration Release
dnu pack .\src\HubAnalytics.Serilog\project.json --configuration Release
dnu pack .\src\HubAnalytics.WebAPI2\project.json --configuration Release
dnu pack .\src\HubAnalytics.AzureSqlDatabase\project.json --configuration Release

#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Ado\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.AspNet4\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Core\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.EF6\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.GenericDotNet\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.MVC5\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.OWIN\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Serilog\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"
#Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.WebAPI2\bin\Release\*-$env:DNX_BUILD_VERSION.nupkg"

Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Ado\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.AspNet4\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Core\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.EF6\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.GenericDotNet\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.MVC5\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.OWIN\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.Serilog\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.WebAPI2\bin\Release\*.$packageVersion.nupkg"
Invoke-Expression ".\nuget.exe push .\src\HubAnalytics.AzureSqlDatabase\bin\Release\*.$packageVersion.nupkg"