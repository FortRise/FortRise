dotnet build FortRise.Generator/FortRise.Generator.csproj -c Release 
dotnet build -c Release

dotnet pack FortRise.Configuration/FortRise.Configuration.csproj

API_KEY=$1
VERSION=$2

dotnet nuget push FortRise.Configuration/bin/Release/FortRise.Configuration.$VERSION.nupkg --source https://api.nuget.org/v3/index.json --skip-duplicate --api-key $API_KEY
