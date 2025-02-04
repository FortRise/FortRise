#!/usr/bin/python
from sys import argv
from os import path, chdir, makedirs
from shutil import copyfile

dependencies = [
    "DiscordGameSdk.dll",
    "DiscordGameSdk.pdb",
    "TowerFall.FortRise.mm.dll",
    "TowerFall.FortRise.mm.dll.config",
    "TowerFall.FortRise.mm.pdb",
    "TowerFall.FortRise.mm.xml",
    "Mono.Cecil.dll",
    "Mono.Cecil.Mdb.dll",
    "Mono.Cecil.Pdb.dll",
    "Mono.Cecil.Rocks.dll",
    "MonoMod.Patcher.exe",
    "MonoMod.RuntimeDetour.dll",
    "MonoMod.RuntimeDetour.HookGen.exe",
    "MonoMod.Utils.dll",
    "MonoMod.Backports.dll",
    "MonoMod.Iced.dll",
    "MonoMod.ILHelpers.dll",
    "MonoMod.Core.dll",
    "System.ValueTuple.dll"
]

library_dependencies = [
    "Microsoft.Bcl.AsyncInterfaces.dll",
    "System.Buffers.dll",
    "System.IO.Compression.dll",
    "System.IO.Compression.FileSystem.dll",
    "System.IO.Pipelines.dll",
    "System.Memory.dll",
    "System.Numerics.Vectors.dll",
    "System.Runtime.CompilerServices.Unsafe.dll",
    "System.Text.Encodings.Web.dll",
    "System.Text.Json.dll",
    "System.Threading.Tasks.Extensions.dll"
]

def file_copy(tfpath: str, fpath: str):
    file = path.join(".", fpath)

    dir = path.join(tfpath, "Installer-Framework")    
    if not path.exists(dir):
        makedirs(dir)
    destination = path.join(dir, fpath)
    print(f"Copying {file} to {destination}")
    copyfile(file, destination)

    dir = path.join("..", "..", "..", "..", "Installer", "lib-framework")
    if not path.exists(dir):
        makedirs(dir)

    destination = path.join(dir, fpath)
    print(f"Copying {file} to {destination}")
    copyfile(file, destination)

def file_copy_library(tfpath:str, fpath: str):
    file = path.join(".", fpath)

    dir = path.join(tfpath, "Installer-Framework", "Libraries")    
    if not path.exists(dir):
        makedirs(dir)
    destination = path.join(dir, fpath)
    print(f"Copying {file} to {destination}")
    copyfile(file, destination)

    dir = path.join("..", "..", "..", "..", "Installer", "lib-framework", "Libraries")
    if not path.exists(dir):
        makedirs(dir)

    destination = path.join(dir, fpath)
    print(f"Copying {file} to {destination}")
    copyfile(file, destination)

def copy_all(tfpath: str):
    i = 0
    while i < len(dependencies):
        file_copy(tfpath, dependencies[i])
        i += 1

    i = 0
    while i < len(library_dependencies):
        file_copy_library(tfpath, library_dependencies[i])
        i += 1

if __name__ == "__main__":
    chdir(path.join("..", "TowerFall.FortRise.mm"))
    tfpath = path.realpath(argv[1])
    chdir(path.join("bin", "Release", "net472"))
    copy_all(tfpath)