from subprocess import run
from sys import exit


def dotnet_build(release: bool=False):
    command = ["dotnet", "build", "-c", "Release"] if release else ["dotnet", "build"]
    return run(command, check=False).returncode

def dotnet_publish(platform: str):
    command = ["dotnet", "publish", "-c", "Release", "-r", platform, "--self-contained"]
    result = run(command, check=False)
    if result.returncode != 0:
        print(f"Publish for {platform} failed")
        exit(result.returncode)
    return result.returncode
