from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图3-1_系统用例图.png"

W, H = 2600, 1700
BG = "white"
BLACK = (28, 28, 28)
GRAY = (82, 82, 82)
LINE = (65, 65, 65)
BLUE = (235, 244, 255)
GREEN = (237, 252, 242)
YELLOW = (255, 250, 235)
PURPLE = (245, 243, 255)
PINK = (255, 241, 242)
CYAN = (236, 254, 255)
ORANGE = (255, 247, 237)
SECTION = (248, 250, 252)


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


F_TITLE = font(62, True)
F_BOUNDARY = font(36, True)
F_SECTION = font(30, True)
F_ACTOR = font(32, True)
F_CASE = font(30, True)
F_NOTE = font(24)


def text_bbox(draw, text, fnt):
    return draw.textbbox((0, 0), text, font=fnt)


def centered(draw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    bbox = text_bbox(draw, text, fnt)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text(
        (x + (w - tw) / 2 - bbox[0], y + (h - th) / 2 - bbox[1] - 1),
        text,
        font=fnt,
        fill=fill,
    )


def actor(draw, cx, cy, name):
    head_r = 34
    draw.ellipse((cx - head_r, cy - 130, cx + head_r, cy - 62), outline=BLACK, width=3)
    draw.line((cx, cy - 62, cx, cy + 54), fill=BLACK, width=3)
    draw.line((cx - 68, cy - 14, cx + 68, cy - 14), fill=BLACK, width=3)
    draw.line((cx, cy + 54, cx - 58, cy + 142), fill=BLACK, width=3)
    draw.line((cx, cy + 54, cx + 58, cy + 142), fill=BLACK, width=3)
    bbox = text_bbox(draw, name, F_ACTOR)
    tw = bbox[2] - bbox[0]
    draw.text((cx - tw / 2 - bbox[0], cy + 162 - bbox[1]), name, font=F_ACTOR, fill=BLACK)


def use_case(draw, xy, title, fill):
    x, y, w, h = xy[:4]
    draw.ellipse((x, y, x + w, y + h), fill=fill, outline=BLACK, width=2)
    centered(draw, (x + 20, y + 8, w - 40, h - 16), title, F_CASE)


def center_of(xy):
    x, y, w, h = xy[:4]
    return x + w / 2, y + h / 2


def edge_point(actor_pos, target_xy):
    ax, ay = actor_pos
    tx, ty = center_of(target_xy)
    return (ax, ay - 10), (tx, ty)


def line(draw, start, end, width=2):
    draw.line((*start, *end), fill=LINE, width=width)


def dashed_arrow(draw, start, end, label=None, label_pos=None):
    import math

    x1, y1 = start
    x2, y2 = end
    dx, dy = x2 - x1, y2 - y1
    dist = math.hypot(dx, dy)
    if dist == 0:
        return
    ux, uy = dx / dist, dy / dist
    dash, gap = 18, 12
    cur = 0
    while cur < dist:
        a = cur
        b = min(cur + dash, dist)
        draw.line((x1 + ux * a, y1 + uy * a, x1 + ux * b, y1 + uy * b), fill=LINE, width=2)
        cur += dash + gap
    if label and label_pos:
        lx, ly = label_pos
        bbox = text_bbox(draw, label, F_NOTE)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((lx - tw / 2 - 8, ly - th / 2 - 5, lx + tw / 2 + 8, ly + th / 2 + 5), radius=6, fill=BG)
        draw.text((lx - tw / 2 - bbox[0], ly - th / 2 - bbox[1]), label, font=F_NOTE, fill=GRAY)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 46, W, 80), "系统用例图", F_TITLE)

boundary = (430, 180, 1740, 1350)
bx, by, bw, bh = boundary
draw.rounded_rectangle((bx, by, bx + bw, by + bh), radius=20, outline=BLACK, width=3)
centered(draw, (bx, by + 18, bw, 45), "厨房协作游戏系统", F_BOUNDARY)

sections = [
    (520, 290, 1560, 350, "联机准备"),
    (520, 720, 1560, 350, "厨房交互"),
    (520, 1150, 1560, 300, "订单交付"),
]
for sx, sy, sw, sh, title in sections:
    draw.rounded_rectangle((sx, sy, sx + sw, sy + sh), radius=12, fill=SECTION, outline=(215, 220, 228), width=2)
    centered(draw, (sx + 20, sy + 18, sw - 40, 36), title, F_SECTION, GRAY)

host = (240, 420)
client = (2350, 420)
player = (240, 1030)
actor(draw, *host, "房主玩家")
actor(draw, *client, "客户端玩家")
actor(draw, *player, "玩家")

cases = {
    "create": (620, 390, 380, 120, "创建房间", GREEN),
    "join": (1580, 390, 380, 120, "加入房间", GREEN),
    "ready": (1100, 390, 380, 120, "角色选择与准备", YELLOW),
    "scene": (1100, 535, 380, 100, "进入游戏场景", YELLOW),
    "move": (620, 825, 380, 120, "移动角色", CYAN),
    "operate": (1100, 825, 380, 120, "操作柜台", CYAN),
    "process": (1580, 825, 380, 120, "处理食材", ORANGE),
    "order": (620, 1245, 380, 120, "查看订单", PURPLE),
    "deliver": (1100, 1245, 380, 120, "提交菜品", PURPLE),
    "feedback": (1580, 1245, 380, 120, "交付结果反馈", PINK),
}

for key, xy in cases.items():
    titles = {
        "create": "创建房间",
        "join": "加入房间",
        "ready": "角色选择与准备",
        "scene": "进入游戏场景",
        "move": "移动角色",
        "operate": "操作柜台",
        "process": "处理食材",
        "order": "查看订单",
        "deliver": "提交菜品",
        "feedback": "交付结果反馈",
    }
    fill = {
        "create": GREEN,
        "join": GREEN,
        "ready": YELLOW,
        "scene": YELLOW,
        "move": CYAN,
        "operate": CYAN,
        "process": ORANGE,
        "order": PURPLE,
        "deliver": PURPLE,
        "feedback": PINK,
    }[key]
    use_case(draw, xy, titles[key], fill)

# Actor associations, routed to avoid crossing the middle of the diagram.
line(draw, (host[0], host[1] - 15), (620, 450), width=2)
line(draw, (client[0], client[1] - 15), (1960, 450), width=2)
line(draw, (player[0], player[1] - 15), (620, 885), width=2)
line(draw, (player[0], player[1] + 5), (620, 1305), width=2)

# Internal use-case relationships inside each section only.
dashed_arrow(draw, center_of(cases["create"]), center_of(cases["ready"]), "<<include>>", (1050, 345))
dashed_arrow(draw, center_of(cases["join"]), center_of(cases["ready"]), "<<include>>", (1550, 345))
dashed_arrow(draw, center_of(cases["ready"]), center_of(cases["scene"]), "<<include>>", (1370, 520))

dashed_arrow(draw, center_of(cases["move"]), center_of(cases["operate"]), "<<include>>", (1050, 780))
dashed_arrow(draw, center_of(cases["operate"]), center_of(cases["process"]), "<<include>>", (1535, 780))

dashed_arrow(draw, center_of(cases["order"]), center_of(cases["deliver"]), "<<include>>", (1050, 1200))
dashed_arrow(draw, center_of(cases["deliver"]), center_of(cases["feedback"]), "<<include>>", (1535, 1200))

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "图3-1 系统用例图.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
