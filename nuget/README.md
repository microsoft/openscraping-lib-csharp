To generate the Nuget package:
- Build **both** Release-Net40 and Release-Net45
- Delete the obj folder from the openscraping-lib-csharp\OpenScraping\ folder
- cd to the openscraping-lib-csharp\nuget\ folder
- NuGet.exe Pack OpenScraping.nuspec -Symbols
- NuGet.exe SetApiKey <API_KEY_HERE>
- NuGet.exe Push OpenScraping.X.X.X.nupkg
