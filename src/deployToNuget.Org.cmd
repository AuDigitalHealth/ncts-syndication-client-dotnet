del *.nupkg

nuget restore

msbuild DigitalHealth.Ncts.Client.sln /p:Configuration=Release

NuGet.exe pack DigitalHealth.Ncts.Client/DigitalHealth.Ncts.Client.csproj 
REM  -Properties Configuration=Release

pause

forfiles /m *.nupkg /c "cmd /c NuGet.exe push @FILE -Source https://www.nuget.org/api/v2/package"
