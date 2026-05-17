from pathlib import Path

from PIL import Image


MEDIA = Path(r"E:\UnityLearn\Kitchen\tmp\slides\thesis_defense_assets\docx_media")

for path in sorted(MEDIA.glob("*")):
    try:
        image = Image.open(path)
        print(path.name, image.size)
    except Exception as exc:
        print(path.name, exc)
