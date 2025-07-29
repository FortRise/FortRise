#!/usr/bin/bash
FR_VERSION=$(cat version.txt)
echo "FortRise Version: $FR_VERSION"
MAIN=$(pwd)

INTERNALS_DIR="FortLauncher/bin/Debug/net9.0/Internals"
INTERNALS_DEST_DIR="artifacts/Internals"

if [ -d "$INTERNALS_DIR" ]; then
    echo "Cleaning up Internals"
    rm -r "$INTERNALS_DIR"
fi

echo "Cleaning all binaries of Internal mods"
rm -r InternalMods/FortRise.Content/bin
rm -r InternalMods/FortRise.ImGui/bin

echo "Rebuilding all projects for Internals"
dotnet build -c Release

echo "Copying all Internals to artifacts"
cp -r "$INTERNALS_DIR" "artifacts"

echo "Publishing Artifacts for Linux"
dotnet publish -c Release -r linux-x64 --self-contained

echo "Publishing Artifacts for Windows"
dotnet publish -c Release -r win-x64 --self-contained

# todo, macos

echo "Copying all Internals to its respective artifacts"
cp -r "$INTERNALS_DEST_DIR" "artifacts/FortRise.v$FR_VERSION-win-x64/FortRise"
cp -r "$INTERNALS_DEST_DIR" "artifacts/FortRise.v$FR_VERSION-linux-x64/FortRise"
# todo macos

echo "Zipping all artifacts"

cd "artifacts/FortRise.v$FR_VERSION-win-x64/"
zip -r "../FortRise.v$FR_VERSION-win-x64.zip" "FortRise"
cd "$MAIN"

cd "artifacts/FortRise.v$FR_VERSION-linux-x64/"
zip -r "../FortRise.v$FR_VERSION-linux-x64.zip" "FortRise"
cd "$MAIN"
# todo macos

echo "Cleaning up"
rm -r "$INTERNALS_DEST_DIR"
rm -r "$INTERNALS_DIR"

# rerun the build script so I can still run the modloader afterwards
dotnet build -c Release