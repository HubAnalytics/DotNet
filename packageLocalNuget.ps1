# .\setBuildVersion.ps1

dnu pack .\src\HubAnalytics.Ado\project.json --configuration Debug 
dnu pack .\src\HubAnalytics.AspNet4\project.json --configuration Debug
dnu pack .\src\HubAnalytics.Core\project.json --configuration Debug
dnu pack .\src\HubAnalytics.EF6\project.json --configuration Debug
dnu pack .\src\HubAnalytics.GenericDotNet\project.json --configuration Debug
dnu pack .\src\HubAnalytics.MVC5\project.json --configuration Debug
dnu pack .\src\HubAnalytics.OWIN\project.json --configuration Debug
dnu pack .\src\HubAnalytics.Serilog\project.json --configuration Debug
dnu pack .\src\HubAnalytics.WebAPI2\project.json --configuration Debug
dnu pack .\src\HubAnalytics.AzureSqlDatabase\project.json --configuration Debug

cp .\src\HubAnalytics.Ado\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.AspNet4\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.Core\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.EF6\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.GenericDotNet\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.MVC5\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.OWIN\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.Serilog\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.WebAPI2\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\HubAnalytics.AzureSqlDatabase\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
