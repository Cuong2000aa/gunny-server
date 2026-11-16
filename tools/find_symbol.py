import subprocess, glob, os

for swf in glob.glob("Source Flash/FlashSV1/ui/**/*.swf", recursive=True) + glob.glob("Tank.Flash/ui/**/*.swf", recursive=True):
    try:
        out = subprocess.check_output(["ffdec", "-dumpSymbols", swf], text=True, errors="ignore")
        if "GoodsTipItemNameMc" in out:
            print(f"Found in: {swf}")
    except:
        pass
