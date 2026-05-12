from pathlib import Path
import math
import shutil

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(r"E:\UnityLearn\Kitchen")
OUT_DIR = ROOT / "output" / "doc"
DESKTOP_DIR = Path.home() / "Desktop" / "毕业设计" / "毕设图片"
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图5-4_厨房对象与柜台处理流程图_简化版.png"
DESKTOP_PNG = DESKTOP_DIR / "图5-4 厨房对象与柜台处理流程图_简化版.png"

W, H = 2200, 1720
BG = (255, 255, 255)
BLACK = (18, 18, 18)
LINE = (36, 36, 36)


def font(size: int, bold: bool = False):
    candidates = [
        r"C:\Windows\Fonts\msyhbd.ttc" if bold else r"C:\Windows\Fonts\msyh.ttc",
        r"C:\Windows\Fonts\simhei.ttf",
        r"C:\Windows\Fonts\simsun.ttc",
    ]
    for candidate in candidates:
        try:
            return ImageFont.truetype(candidate, size)
        except OSError:
            continue
    return ImageFont.load_default()


F_TITLE = font(60, True)
F_BOX = font(38, True)
F_HEAD = font(40, True)
F_LABEL = font(30, True)


def text_bbox(draw, text, fnt):
    box = draw.textbbox((0, 0), text, font=fnt)
    return box[2] - box[0], box[3] - box[1], box


def centered(draw, rect, text, fnt=F_BOX):
    x, y, w, h = rect
    tw, th, box = text_bbox(draw, text, fnt)
    draw.text(
        (x + (w - tw) / 2 - box[0], y + (h - th) / 2 - box[1] - 1),
        text,
        font=fnt,
        fill=BLACK,
    )


def node(draw, rect, text):
    x, y, w, h = rect
    draw.rounded_rectangle((x, y, x + w, y + h), radius=8, fill=BG, outline=BLACK, width=3)
    centered(draw, rect, text)


def diamond(draw, center, size, text):
    cx, cy = center
    w, h = size
    pts = [(cx, cy - h / 2), (cx + w / 2, cy), (cx, cy + h / 2), (cx - w / 2, cy)]
    draw.polygon(pts, fill=BG, outline=BLACK)
    for a, b in zip(pts, pts[1:] + pts[:1]):
        draw.line((*a, *b), fill=BLACK, width=3)
    centered(draw, (cx - w / 2 + 50, cy - h / 2 + 30, w - 100, h - 60), text)


def arrow_head(draw, end, angle):
    length = 20
    spread = math.radians(25)
    x, y = end
    p1 = (x - length * math.cos(angle - spread), y - length * math.sin(angle - spread))
    p2 = (x - length * math.cos(angle + spread), y - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)


def arrow(draw, start, end, width=3):
    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    arrow_head(draw, end, math.atan2(y2 - y1, x2 - x1))


def poly_arrow(draw, points, width=3):
    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    x1, y1 = points[-2]
    x2, y2 = points[-1]
    arrow_head(draw, points[-1], math.atan2(y2 - y1, x2 - x1))


def label(draw, pos, text):
    x, y = pos
    tw, th, box = text_bbox(draw, text, F_LABEL)
    draw.rectangle((x - tw / 2 - 12, y - th / 2 - 8, x + tw / 2 + 12, y + th / 2 + 8), fill=BG)
    draw.text((x - tw / 2 - box[0], y - th / 2 - box[1] - 1), text, font=F_LABEL, fill=BLACK)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 45, W, 75), "厨房对象与柜台处理流程图", F_TITLE)

main_w, main_h = 420, 105
main_x = (W - main_w) // 2
main_boxes = [
    (main_x, 165, main_w, main_h, "玩家与柜台交互"),
    (main_x, 315, main_w, main_h, "厨房对象转移"),
    (main_x, 465, main_w, main_h, "网络同步显示"),
]

for x, y, w, h, text in main_boxes:
    node(draw, (x, y, w, h), text)

for a, b in zip(main_boxes, main_boxes[1:]):
    arrow(draw, (a[0] + a[2] / 2, a[1] + a[3]), (b[0] + b[2] / 2, b[1]))

decision_center = (W // 2, 685)
diamond(draw, decision_center, (500, 165), "判断柜台类型")
arrow(draw, (main_x + main_w / 2, 570), (decision_center[0], decision_center[1] - 82))

col_w, box_h = 470, 100
col_xs = [220, 865, 1510]
head_y = 835
step_ys = [955, 1105, 1255]
headers = ["盘子对象", "切菜台", "炉灶"]
steps = [
    ["验证食材", "加入盘子列表", "更新盘子图标"],
    ["增加切割次数", "更新加工进度", "生成切配结果"],
    ["进入加热状态", "同步烹饪进度", "生成熟成或烧焦对象"],
]

branch_y = 785
for x, header, col_steps in zip(col_xs, headers, steps):
    centered(draw, (x, head_y, col_w, 60), header, F_HEAD)
    poly_arrow(draw, [(decision_center[0], decision_center[1] + 82), (decision_center[0], branch_y), (x + col_w / 2, branch_y), (x + col_w / 2, head_y + 115)])

    previous = None
    for y, text in zip(step_ys, col_steps):
        rect = (x, y, col_w, box_h)
        node(draw, rect, text)
        if previous:
            arrow(draw, (previous[0] + col_w / 2, previous[1] + box_h), (x + col_w / 2, y))
        previous = rect

end_rect = ((W - 420) // 2, 1460, 420, 105)
merge_y = 1410
for x in col_xs:
    poly_arrow(
        draw,
        [
            (x + col_w / 2, step_ys[-1] + box_h),
            (x + col_w / 2, merge_y),
            (W / 2, merge_y),
            (W / 2, end_rect[1]),
        ],
    )
node(draw, end_rect, "结束")

img.save(OUT_PNG)

try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(OUT_PNG, DESKTOP_PNG)
    print(DESKTOP_PNG)
except Exception as exc:
    print(f"desktop save failed: {exc}")

print(OUT_PNG)
