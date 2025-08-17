#!/usr/bin/bash
npath=$(pwd)

echo $npath

# This specific script will build MonoMod and FortRise. MonoMod submodule must be present with full dependencies on it.
cd "lib-ext/MonoMod"

echo "Building MonoMod.."
dotnet build -c Release
dotnet pack 

echo "Some packages might failed, but having the nupkg present on 'artifacts/packages/release' should be enough"

cd $npath

# Build FortRise
dotnet build



