from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


MEDIA = Path(r"E:\UnityLearn\Kitchen\tmp\slides\thesis_defense_assets\docx_media")
OUT = Path(r"E:\UnityLearn\Kitchen\tmp\slides\thesis_defense_assets\media_contact_sheet.png")


def font(size):
    for f in [r"C:\Windows\Fonts\msyh.ttc", r"C:\Windows\Fonts\simhei.ttf"]:
        try:
            return ImageFont.truetype(f, size)
        except OSError:
            pass
    return ImageFont.load_default()


items = []
for path in sorted(MEDIA.glob("*.png")):
    im = Image.open(path).convert("RGB")
    items.append((path.name, im))

thumb_w, thumb_h = 220, 140
cell_w, cell_h = 260, 190
cols = 5
rows = (len(items) + cols - 1) // cols
sheet = Image.new("RGB", (cols * cell_w, rows * cell_h), "white")
draw = ImageDraw.Draw(sheet)
fnt = font(20)

for idx, (name, im) in enumerate(items):
    row, col = divmod(idx, cols)
    x, y = col * cell_w, row * cell_h
    im.thumbnail((thumb_w, thumb_h))
    sheet.paste(im, (x + (cell_w - im.width) // 2, y + 10))
    draw.text((x + 12, y + 155), name, fill=(0, 0, 0), font=fnt)

OUT.parent.mkdir(parents=True, exist_ok=True)
sheet.save(OUT)
print(OUT)
