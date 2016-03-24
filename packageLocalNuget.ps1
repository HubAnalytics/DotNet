.\setBuildVersion.ps1

dnu pack .\src\MicroserviceAnalytics.Ado\project.json --configuration Debug 
dnu pack .\src\MicroserviceAnalytics.AspNet4\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.Core\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.EF6\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.GenericDotNet\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.MVC5\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.OWIN\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.Serilog\project.json --configuration Debug
dnu pack .\src\MicroserviceAnalytics.WebAPI2\project.json --configuration Debug

cp .\src\MicroserviceAnalytics.Ado\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.AspNet4\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.Core\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.EF6\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.GenericDotNet\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.MVC5\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.OWIN\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.Serilog\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
cp .\src\MicroserviceAnalytics.WebAPI2\bin\Debug\*.nupkg d:\MicroserviceAnalyticPackageRepository
