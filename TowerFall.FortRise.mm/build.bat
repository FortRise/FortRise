dotnet build -c Release
cd bin/Release/net472
copy TowerFall.FortRise.mm.dll "../../../../../BepInEx/monomod/TowerFall.FortRise.mm.dll"
copy TeuJson.dll "../../../../../BepInEx/monomod/TeuJson.dll"


pause