#!/usr/bin/python
from sys import argv
from os import path, chdir
from subprocess import run
from shutil import copyfile
import build_copy

def build(tfpath: str):
    process = run(["dotnet", "build", "-c", "Release"])
    if process.returncode != 0:
        print("Failed to build mod rules!")
        input("Press any key to continue.")
        exit(1)

    chdir(path.join("bin", "Release", "net472"))

    build_copy.copy_all(tfpath)

    print("Proceeding to backup original TowerFall")

    chdir(path.join(tfpath, "fortOrig"))
    copyfile("TowerFall.exe", "../TowerFall.exe")
    chdir(path.join("..", "Installer-Framework"))

    print("Patching TowerFall with Installer")
    run(["./Installer.bin.x86_64", "--patch", "../"])

    print("Success!")

    input("Press any key to continue.")

if __name__ == "__main__":
    chdir(path.join("..", "TowerFall.FortRise.mm"))

    tfpath = path.realpath(argv[1])
    build(tfpath)