dotnet build -c Release
cd bin/Release/net472
copy TowerFall.FortRise.mm.dll "../../../../../../TowerFall.FortRise.mm.dll"
copy TowerFall.FortRise.mm.pdb "../../../../../../TowerFall.FortRise.mm.pdb"
copy TowerFall.FortRise.mm.xml "../../../../../../TowerFall.FortRise.mm.xml"
copy TowerFall.FortRise.mm.dll "../../../../Installer/lib/TowerFall.FortRise.mm.dll"
copy TowerFall.FortRise.mm.pdb "../../../../Installer/lib/TowerFall.FortRise.mm.pdb"
copy TowerFall.FortRise.mm.xml "../../../../Installer/lib/TowerFall.FortRise.mm.xml"


copy TeuJson.dll "../../../../../../TeuJson.dll"
cd ../../../../../../orig
copy "TowerFall.exe" "../TowerFall.exe"
cd ../
MonoMod.exe TowerFall.exe
copy MONOMODDED_TowerFall.exe TowerFall.exe
cd MonoMod/TowerFallMM

pause