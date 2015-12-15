.\setBuildVersion.ps1
dnu pack .\src\* --out packageoutput
cp packageoutput\Debug\*.nupkg C:\MicroserviceAnalyticPackageRepository

