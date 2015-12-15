$env:DNX_BUILD_VERSION=2
dnu pack .\src\* --out packageoutput
cp packageoutput\Debug\*.nupkg C:\MicroserviceAnalyticPackageRepository

