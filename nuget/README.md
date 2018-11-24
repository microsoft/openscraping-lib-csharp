Reference - [How to Create a NuGet Package with Cross Platform Tools](https://github.com/dotnet/docs/blob/master/docs/core/deploying/creating-nuget-packages.md)

Reference - [dotnet nuget push](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push)

To generate the Nuget package:
- Right click on OpenScraping project > Properties > Package. Increment Package version, Assembly version, Assembly file version
- Go to the root folder openscraping-lib-csharp
- dotnet pack --configuration release
- dotnet nuget push /path/to/OpenScraping.X.X.X.nupkg -s https://www.nuget.org/api/v2/package -k <API_KEY_HERE>
