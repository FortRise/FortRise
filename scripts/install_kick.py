#!/usr/bin/python
from subprocess import run
from os import path, chdir, listdir
from shutil import copyfile

def is_file_and_not_debuggable(f: str) -> bool:
    return path.isfile(f) and not f.endswith("debug")

def install(install_path: str):
    chdir(install_path)
    if not path.exists("MonoKickstart"):
        run(["git", "clone", "https://github.com/flibitijibibo/MonoKickstart.git"])

    chdir(path.join("MonoKickstart", "precompiled"))
    files = [f for f in listdir() if is_file_and_not_debuggable(f)]

    i = 0
    while i < len(files):
        file = files[i]
        destination = path.join("..", "..", file)
        copyfile(file, destination)        
        i += 1

if __name__ == "__main__":
    chdir(path.join("..", "Installer"))
    install("lib-kick")