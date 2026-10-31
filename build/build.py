#!/usr/bin/python

import shutil
import sys
from pathlib import Path
from shutil import rmtree
from time import sleep

from dotnet import dotnet_build, dotnet_publish
from helper import read_file

DOTNET_VERSION = "net10.0"
FR_VERSION = read_file("version.txt").strip()
print("FortRise Version:", FR_VERSION)

RIDS = ["linux-x64", "win-x64", "osx-x64"]
ARTIFACTS = Path("artifacts")
INTERNALS_DIR = Path(f".bin/{DOTNET_VERSION}/Internals")
INTERNALS_DEST_DIR = ARTIFACTS / "Internals"
IMGUI_RUNTIMES = Path(f"InternalMods/FortRise.ImGui/bin/Release/{DOTNET_VERSION}/runtimes")
MAC_APP = ARTIFACTS / "FortRise.app"


def artifact(rid: str) -> Path:
    return ARTIFACTS / f"FortRise.v{FR_VERSION}-{rid}"


def remove(path: Path):
    if path.is_dir():
        rmtree(path)
    elif path.exists():
        path.unlink()


def copy_contents(src: Path, dest: Path):
    dest.mkdir(parents=True, exist_ok=True)
    for child in src.iterdir():
        if child.is_dir():
            shutil.copytree(child, dest / child.name, dirs_exist_ok=True)
        else:
            shutil.copy2(child, dest / child.name)


def clean_internal_bins():
    print("Cleaning all binaries of Internal mods")
    for bin_dir in Path("InternalMods").glob("*/bin"):
        rmtree(bin_dir)


if INTERNALS_DIR.exists():
    print("Cleaning up Internals")
    rmtree(INTERNALS_DIR)

clean_internal_bins()

print("Rebuilding all projects for Internals")
if dotnet_build(release=True) != 0:
    print("Build failed")
    sys.exit(1)

sleep(0.5)

print("Copying all Internals to artifacts")
shutil.copytree(INTERNALS_DIR, INTERNALS_DEST_DIR, dirs_exist_ok=True)

if artifact("win-x64").exists():
    print("Removing all similar artifacts")
    for rid in RIDS:
        remove(artifact(rid))
        remove(artifact(rid).with_name(artifact(rid).name + ".zip"))

for name, rid in [("Linux", "linux-x64"), ("Windows", "win-x64"), ("Mac", "osx-x64")]:
    print(f"Publishing artifacts for {name}")
    dotnet_publish(rid)

print("Copying all Internals to its respective artifacts")
for rid in RIDS:
    shutil.copytree(INTERNALS_DEST_DIR, artifact(rid) / "FortRise" / "Internals", dirs_exist_ok=True)

# things that we don't care about yet
rmtree(IMGUI_RUNTIMES / "win-arm64", ignore_errors=True)
rmtree(IMGUI_RUNTIMES / "win-x86", ignore_errors=True)

for rid in RIDS:
    unmanaged = artifact(rid) / "FortRise" / "Internals" / "FortRise.ImGui" / "Unmanaged"
    copy_contents(IMGUI_RUNTIMES, unmanaged)
    osx = unmanaged / "osx"
    if osx.exists():
        remove(unmanaged / "osx-x64")
        osx.rename(unmanaged / "osx-x64")

print("Zipping all artifacts")
for rid in ["win-x64", "linux-x64"]:
    shutil.make_archive(str(artifact(rid)), "zip", root_dir=artifact(rid), base_dir="FortRise")

remove(MAC_APP)
copy_contents(Path("MacOS/Contents"), MAC_APP / "Contents")
copy_contents(artifact("osx-x64") / "FortRise", MAC_APP / "Contents" / "MacOS")
shutil.make_archive(str(artifact("osx-x64")), "zip", root_dir=ARTIFACTS, base_dir="FortRise.app")

print("Cleaning up")
remove(INTERNALS_DEST_DIR)
remove(INTERNALS_DIR)

dotnet_build(release=True)

clean_internal_bins()
