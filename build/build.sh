#!/usr/bin/bash

DOTNET_VERSION="net9.0"
FR_VERSION=$(cat version.txt)
echo "FortRise Version: $FR_VERSION"
MAIN=$(pwd)

INTERNALS_DIR="FortLauncher/bin/Debug/$DOTNET_VERSION/Internals"
INTERNALS_DEST_DIR="artifacts/Internals"

if [ -d "$INTERNALS_DIR" ]; then
    echo "Cleaning up Internals"
    rm -r "$INTERNALS_DIR"
fi

echo "Cleaning all binaries of Internal mods"
if [ -d "InternalMods/FortRise.Content/bin" ]; then
    rm -r InternalMods/FortRise.Content/bin
    rm -r InternalMods/FortRise.ImGui/bin
fi

echo "Rebuilding all projects for Internals"
dotnet build -c Release

if [ $? -eq 1 ]; then
    echo "Build failed"
    exit 1
fi

sleep 0.5s

echo "Copying all Internals to artifacts"
cp -r "$INTERNALS_DIR" "artifacts"

if [ -d "artifacts/FortRise.v$FR_VERSION-win-x64" ]; then
    echo "Removing all similar artifacts"
    rm -r artifacts/FortRise.v$FR_VERSION-win-x64
    rm -r artifacts/FortRise.v$FR_VERSION-linux-x64
    rm -r artifacts/FortRise.v$FR_VERSION-osx-x64

    rm -r artifacts/FortRise.v$FR_VERSION-win-x64.zip
    rm -r artifacts/FortRise.v$FR_VERSION-linux-x64.zip
    rm -r artifacts/FortRise.v$FR_VERSION-osx-x64.zip
fi

echo "Publishing Artifacts for Linux"
dotnet publish -c Release -r linux-x64 --self-contained

echo "Publishing Artifacts for Windows"
dotnet publish -c Release -r win-x64 --self-contained

echo "Publishing Artifacts for Mac"
dotnet publish -c Release -r osx-x64 --self-contained
# Mac is such a special case and I hate it
cd FortLauncher
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 # we need to bundle the app as well
cd $MAIN


echo "Copying all Internals to its respective artifacts"
cp -r "$INTERNALS_DEST_DIR" "artifacts/FortRise.v$FR_VERSION-win-x64/FortRise"
cp -r "$INTERNALS_DEST_DIR" "artifacts/FortRise.v$FR_VERSION-linux-x64/FortRise"
cp -r "$INTERNALS_DEST_DIR" "artifacts/FortRise.v$FR_VERSION-osx-x64/FortRise/FortRise.app/Contents/MacOS"

mkdir artifacts/FortRise.v$FR_VERSION-linux-x64/FortRise/Internals/FortRise.ImGui/Unmanaged
mkdir artifacts/FortRise.v$FR_VERSION-win-x64/FortRise/Internals/FortRise.ImGui/Unmanaged
mkdir artifacts/FortRise.v$FR_VERSION-osx-x64/FortRise/FortRise.app/Contents/MacOS/Internals/FortRise.ImGui/Unmanaged

# things that we don't care about yet
rm -r InternalMods/FortRise.ImGui/bin/Release/$DOTNET_VERSION/runtimes/win-arm64
rm -r InternalMods/FortRise.ImGui/bin/Release/$DOTNET_VERSION/runtimes/win-x86

cp -r InternalMods/FortRise.ImGui/bin/Release/$DOTNET_VERSION/runtimes/* artifacts/FortRise.v$FR_VERSION-linux-x64/FortRise/Internals/FortRise.ImGui/Unmanaged
cp -r InternalMods/FortRise.ImGui/bin/Release/$DOTNET_VERSION/runtimes/* artifacts/FortRise.v$FR_VERSION-win-x64/FortRise/Internals/FortRise.ImGui/Unmanaged
cp -r InternalMods/FortRise.ImGui/bin/Release/$DOTNET_VERSION/runtimes/* artifacts/FortRise.v$FR_VERSION-osx-x64/FortRise/FortRise.app/Contents/MacOS/Internals/FortRise.ImGui/Unmanaged
echo "Zipping all artifacts"

cd "artifacts/FortRise.v$FR_VERSION-win-x64/"
zip -r "../FortRise.v$FR_VERSION-win-x64.zip" "FortRise"
cd "$MAIN"

cd "artifacts/FortRise.v$FR_VERSION-linux-x64/"
zip -r "../FortRise.v$FR_VERSION-linux-x64.zip" "FortRise"
cd "$MAIN"

cd "artifacts/FortRise.v$FR_VERSION-osx-x64/FortRise"
zip -r "../../FortRise.v$FR_VERSION-osx-x64.zip" "FortRise.app"
cd "$MAIN"

echo "Cleaning up"
rm -r "$INTERNALS_DEST_DIR"
rm -r "$INTERNALS_DIR"

dotnet build -c Release
