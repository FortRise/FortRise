dotnet build -c Release
cd bin/Release/net472
copy TowerFall.FortRise.mm.dll "../../../../../../Installer-Framework/TowerFall.FortRise.mm.dll"
copy TowerFall.FortRise.mm.pdb "../../../../../../Installer-Framework/TowerFall.FortRise.mm.pdb"
copy TowerFall.FortRise.mm.xml "../../../../../../Installer-Framework/TowerFall.FortRise.mm.xml"
copy TowerFall.FortRise.mm.dll "../../../../Installer/lib-framework/TowerFall.FortRise.mm.dll"
copy TowerFall.FortRise.mm.pdb "../../../../Installer/lib-framework/TowerFall.FortRise.mm.pdb"
copy TowerFall.FortRise.mm.xml "../../../../Installer/lib-framework/TowerFall.FortRise.mm.xml"


cd ../../../../../../orig
copy "TowerFall.exe" "../TowerFall.exe"
cd ../Installer-Framework
Installer.NoAnsi.exe --patch "../"
cd ../MonoMod/TowerFallMM

pause