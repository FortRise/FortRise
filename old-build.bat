dotnet build -c Release
cd bin/Release/net472
copy TowerFall.FortRise.mm.dll "../../../../../TowerFall.FortRise.mm.dll"
copy TeuJson.dll "../../../../../TeuJson.dll"
cd ../../../../../orig
copy "TowerFall.exe" "../TowerFall.exe"
cd ../MonoMod
MonoMod.exe ../TowerFall.exe
cd ../
copy MONOMODDED_TowerFall.exe STowerFall.exe
cd MonoMod/TowerFallMM

pause