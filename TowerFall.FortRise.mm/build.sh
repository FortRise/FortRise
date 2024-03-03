#!/bin/bash
Path="$1"

FileCopy() {
	File="./$1"
	Destination="$Path/Installer-Framework/$1"
	echo "Copying $File to $Destination"
	cp $File $Destination

	# Destination="$Path/Installer/lib-framework/"
	# echo "Copying $File to $Destination"
	# cp $File $Destination
}


dotnet build -c Release

if [ $? -ne 0 ]; then
	# dotnet fails building the mod rules
	echo
	echo -e "Failed to build the mod rules!"
	echo
	read -p "Press enter to continue!"
	exit 1
fi
# Proceed if succeed

cd "bin/Release/net472"

FileCopy "TowerFall.FortRise.mm.dll"
FileCopy "TowerFall.FortRise.mm.pdb"
FileCopy "TowerFall.FortRise.mm.xml"

echo "Proceeding to backup the original TowerFall"
cd "$Path/fortOrig"
cp "TowerFall.exe" "../TowerFall.exe"
cd "../Installer-Framework"

echo "Patching TowerFall with installer"
./Installer.NoAnsi.bin.x86_64 --patch "../"

echo
echo "Success!"
echo

read -p "Press enter to continue!"