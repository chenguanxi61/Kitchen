from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
OUT_DIR.mkdir(parents=True, exist_ok=True)
OUT_PNG = OUT_DIR / "图3-3_数据驱动配置关系图_修改版.png"


W, H = 1280, 820
BG = "white"
BLACK = (30, 30, 30)
GRAY = (90, 90, 90)
BLUE = (235, 244, 255)
GREEN = (237, 252, 242)
YELLOW = (255, 250, 235)
LINE = (75, 75, 75)


def font(size: int, bold: bool = False):
    candidates = [
        r"C:\Windows\Fonts\msyhbd.ttc" if bold else r"C:\Windows\Fonts\msyh.ttc",
        r"C:\Windows\Fonts\simhei.ttf",
        r"C:\Windows\Fonts\simsun.ttc",
    ]
    for c in candidates:
        try:
            return ImageFont.truetype(c, size)
        except OSError:
            continue
    return ImageFont.load_default()


F_TITLE = font(34, True)
F_BOX = font(27, True)
F_SUB = font(20)
F_LABEL = font(18)


def centered(draw: ImageDraw.ImageDraw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    bbox = draw.textbbox((0, 0), text, font=fnt)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text((x + (w - tw) / 2, y + (h - th) / 2 - 2), text, font=fnt, fill=fill)


def box(draw, xy, title, subtitle, fill):
    x, y, w, h = xy
    draw.rounded_rectangle((x, y, x + w, y + h), radius=10, fill=fill, outline=BLACK, width=2)
    draw.line((x, y + 52, x + w, y + 52), fill=BLACK, width=1)
    centered(draw, (x, y + 8, w, 38), title, F_BOX)
    centered(draw, (x, y + 54, w, h - 54), subtitle, F_SUB, GRAY)


def arrow(draw, start, end, label=None, label_offset=(0, -22), width=3):
    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    # arrow head
    import math

    angle = math.atan2(y2 - y1, x2 - x1)
    length = 18
    spread = math.radians(25)
    p1 = (x2 - length * math.cos(angle - spread), y2 - length * math.sin(angle - spread))
    p2 = (x2 - length * math.cos(angle + spread), y2 - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)
    if label:
        lx = (x1 + x2) / 2 + label_offset[0]
        ly = (y1 + y2) / 2 + label_offset[1]
        bbox = draw.textbbox((0, 0), label, font=F_LABEL)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((lx - tw / 2 - 6, ly - th / 2 - 4, lx + tw / 2 + 6, ly + th / 2 + 4), radius=5, fill=BG)
        draw.text((lx - tw / 2, ly - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


def polyline_arrow(draw, points, label=None, label_pos=None, width=3):
    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    arrow(draw, points[-2], points[-1], None, width=width)
    if label and label_pos:
        x, y = label_pos
        bbox = draw.textbbox((0, 0), label, font=F_LABEL)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((x - tw / 2 - 6, y - th / 2 - 4, x + tw / 2 + 6, y + th / 2 + 4), radius=5, fill=BG)
        draw.text((x - tw / 2, y - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 20, W, 50), "数据驱动配置关系图", F_TITLE)

left_x, left_w, left_h = 170, 310, 92
left_nodes = {
    "recipe": (left_x, 120, left_w, left_h),
    "cut": (left_x, 270, left_w, left_h),
    "fry": (left_x, 420, left_w, left_h),
    "burn": (left_x, 570, left_w, left_h),
}

box(draw, left_nodes["recipe"], "RecipeSO", "菜谱配置", BLUE)
box(draw, left_nodes["cut"], "CuttingRecipeSO", "切配配方配置", GREEN)
box(draw, left_nodes["fry"], "FryRecipeSO", "加热配方配置", GREEN)
box(draw, left_nodes["burn"], "BurningRecipeSO", "烧焦配方配置", GREEN)

kitchen = (485, 690, 330, 96)
box(draw, kitchen, "KitchenObjSO", "厨房对象/食材配置", YELLOW)

right_x, right_w, rel_h = 850, 330, 92
relations = {
    "recipe": (right_x, 120, right_w, rel_h),
    "cut": (right_x, 270, right_w, rel_h),
    "fry": (right_x, 420, right_w, rel_h),
    "burn": (right_x, 570, right_w, rel_h),
}
box(draw, relations["recipe"], "菜谱食材列表", "订单所需食材组合", BLUE)
box(draw, relations["cut"], "切配输入输出关系", "原始食材 → 切配结果", GREEN)
box(draw, relations["fry"], "加热输入输出关系", "未熟食材 → 熟成结果", GREEN)
box(draw, relations["burn"], "烧焦输入输出关系", "熟成食材 → 烧焦结果", GREEN)

# All configuration objects reference KitchenObjSO through a separate left-side bus.
# This avoids crossing the horizontal rule arrows.
bus_x = 105
for _, y, _, h in left_nodes.values():
    cy = y + h // 2
    draw.line((left_x, cy, bus_x, cy), fill=LINE, width=2)
draw.line((bus_x, 166, bus_x, 738), fill=LINE, width=2)
polyline_arrow(draw, [(bus_x, 738), (485, 738)], "共同引用食材/物体配置", (300, 716), width=2)

# Each ScriptableObject also owns a corresponding rule relation.
arrow(draw, (480, 166), (850, 166), "生成订单依据", (0, -28))
arrow(draw, (480, 316), (850, 316), "定义切配规则", (0, -28))
arrow(draw, (480, 466), (850, 466), "定义加热规则", (0, -28))
arrow(draw, (480, 616), (850, 616), "定义烧焦规则", (0, -28))

draw.text((90, 792), "说明：各类 ScriptableObject 保存菜谱、食材和加工规则，运行时逻辑根据配置读取数据并生成对应订单或加工结果。", font=F_LABEL, fill=GRAY)

img.save(OUT_PNG)
print(OUT_PNG)
