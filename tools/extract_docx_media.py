from pathlib import Path
from zipfile import ZipFile


DOC = Path(r"E:\UnityLearn\Kitchen\output\doc\基于Unity的多人协作烹饪游戏设计与实现.docx")
OUT = Path(r"E:\UnityLearn\Kitchen\tmp\slides\thesis_defense_assets\docx_media")
OUT.mkdir(parents=True, exist_ok=True)

with ZipFile(DOC) as zf:
    media = [name for name in zf.namelist() if name.startswith("word/media/")]
    for name in media:
        target = OUT / Path(name).name
        target.write_bytes(zf.read(name))
        print(target)
