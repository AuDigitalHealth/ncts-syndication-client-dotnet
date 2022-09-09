del *.nupkg

dotnet pack DigitalHealth.Ncts.Client/DigitalHealth.Ncts.Client.csproj --configuration Release --output .

pause

forfiles /m *.nupkg /c "cmd /c NuGet.exe push @FILE -Source https://www.nuget.org/api/v2/package"
