from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "玩家控制与交互活动图.png"

W, H = 1180, 1580
BG = "white"
BLACK = (30, 30, 30)
GRAY = (80, 80, 80)
BLUE = (235, 244, 255)
GREEN = (237, 252, 242)
YELLOW = (255, 250, 235)
LINE = (60, 60, 60)


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


F_TITLE = font(36, True)
F_NODE = font(23, True)
F_SUB = font(19)
F_LABEL = font(18)


def centered(draw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    bbox = draw.textbbox((0, 0), text, font=fnt)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text((x + (w - tw) / 2, y + (h - th) / 2 - 1), text, font=fnt, fill=fill)


def rect(draw, x, y, w, h, title, sub=None, fill=BLUE):
    draw.rounded_rectangle((x, y, x + w, y + h), radius=12, fill=fill, outline=BLACK, width=2)
    if sub:
        centered(draw, (x, y + 13, w, 34), title, F_NODE)
        centered(draw, (x, y + 50, w, h - 52), sub, F_SUB, GRAY)
    else:
        centered(draw, (x, y, w, h), title, F_NODE)


def pill(draw, x, y, w, h, title):
    draw.rounded_rectangle((x, y, x + w, y + h), radius=h // 2, fill="#eef6ff", outline="#2563eb", width=2)
    centered(draw, (x, y, w, h), title, F_NODE)


def diamond(draw, cx, cy, w, h, title, sub=None):
    pts = [(cx, cy - h // 2), (cx + w // 2, cy), (cx, cy + h // 2), (cx - w // 2, cy)]
    draw.polygon(pts, fill="white", outline=BLACK)
    draw.line((pts[0], pts[1], pts[2], pts[3], pts[0]), fill=BLACK, width=2)
    centered(draw, (cx - w // 2, cy - 25, w, 34), title, F_NODE)
    if sub:
        centered(draw, (cx - w // 2, cy + 5, w, 28), sub, F_SUB, GRAY)


def arrow(draw, start, end, label=None, offset=(0, -24), width=3):
    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    import math

    angle = math.atan2(y2 - y1, x2 - x1)
    length = 18
    spread = math.radians(25)
    p1 = (x2 - length * math.cos(angle - spread), y2 - length * math.sin(angle - spread))
    p2 = (x2 - length * math.cos(angle + spread), y2 - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)
    if label:
        mx, my = (x1 + x2) / 2 + offset[0], (y1 + y2) / 2 + offset[1]
        bbox = draw.textbbox((0, 0), label, font=F_LABEL)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((mx - tw / 2 - 6, my - th / 2 - 4, mx + tw / 2 + 6, my + th / 2 + 4), radius=5, fill=BG)
        draw.text((mx - tw / 2, my - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


def path_arrow(draw, points, label=None, label_pos=None, width=3):
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

centered(draw, (0, 28, W, 60), "玩家控制与交互活动图", F_TITLE)

cx = W // 2

pill(draw, cx - 150, 115, 300, 66, "开始")
rect(draw, cx - 180, 225, 360, 86, "读取玩家输入", "获取移动方向和交互按键", BLUE)
rect(draw, cx - 180, 355, 360, 86, "计算移动方向", "二维输入转换为三维方向", BLUE)
rect(draw, cx - 180, 485, 360, 86, "执行胶囊体检测", "判断前方是否存在阻挡物", BLUE)
diamond(draw, cx, 655, 360, 150, "当前方向", "是否可以移动？")

rect(draw, 135, 780, 330, 86, "尝试单轴移动", "分别检测 X 轴或 Z 轴方向", YELLOW)
rect(draw, 715, 780, 330, 86, "更新角色位置", "按照可移动方向移动角色", GREEN)

rect(draw, cx - 180, 925, 360, 86, "记录有效朝向", "保存最近一次有效移动方向", BLUE)
rect(draw, cx - 180, 1055, 360, 86, "发射交互射线", "检测前方可交互柜台", BLUE)
diamond(draw, cx, 1225, 360, 150, "是否命中", "可交互柜台？")

rect(draw, 145, 1320, 320, 82, "清空选中对象", fill=YELLOW)
rect(draw, 715, 1320, 320, 82, "设置选中柜台", "更新高亮反馈", GREEN)
rect(draw, cx - 180, 1430, 360, 86, "分发交互请求", "主交互或辅助交互", GREEN)

arrow(draw, (cx, 181), (cx, 225))
arrow(draw, (cx, 311), (cx, 355))
arrow(draw, (cx, 441), (cx, 485))
arrow(draw, (cx, 571), (cx, 580))

path_arrow(draw, [(cx - 120, 695), (300, 730), (300, 780)], "否", (360, 725))
path_arrow(draw, [(cx + 120, 695), (880, 730), (880, 780)], "是", (820, 725))
path_arrow(draw, [(300, 866), (300, 895), (cx, 895), (cx, 925)])
path_arrow(draw, [(880, 866), (880, 895), (cx, 895), (cx, 925)])

arrow(draw, (cx, 1011), (cx, 1055))
arrow(draw, (cx, 1141), (cx, 1150))

path_arrow(draw, [(cx - 130, 1260), (305, 1290), (305, 1320)], "否", (350, 1288))
path_arrow(draw, [(cx + 130, 1260), (875, 1290), (875, 1320)], "是", (830, 1288))
arrow(draw, (875, 1402), (cx, 1430), "触发输入", (0, -18))

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "玩家控制与交互活动图.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
