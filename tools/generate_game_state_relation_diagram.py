from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图4-2_游戏状态管理模块关系图.png"

W, H = 2700, 1700
BG = "white"
BLACK = (28, 28, 28)
GRAY = (82, 82, 82)
LINE = (70, 70, 70)
BLUE = (235, 244, 255)
GREEN = (237, 252, 242)
YELLOW = (255, 250, 235)
PURPLE = (245, 243, 255)
PINK = (255, 241, 242)
CYAN = (236, 254, 255)


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


F_TITLE = font(64, True)
F_BOX = font(42, True)
F_SUB = font(32)
F_SMALL = font(30)
F_LABEL = font(30)


def text_size(draw, text, fnt):
    bbox = draw.textbbox((0, 0), text, font=fnt)
    return bbox[2] - bbox[0], bbox[3] - bbox[1]


def centered(draw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    tw, th = text_size(draw, text, fnt)
    draw.text((x + (w - tw) / 2, y + (h - th) / 2 - 1), text, font=fnt, fill=fill)


def node(draw, xy, title, lines, fill):
    x, y, w, h = xy
    draw.rounded_rectangle((x, y, x + w, y + h), radius=14, fill=fill, outline=BLACK, width=2)
    centered(draw, (x, y + 34, w, 56), title, F_BOX)
    if lines:
        start_y = y + 118
        line_h = 48
        for i, line in enumerate(lines):
            centered(draw, (x + 14, start_y + i * line_h, w - 28, line_h), line, F_SUB, GRAY)


def pill(draw, xy, text, fill):
    x, y, w, h = xy
    draw.rounded_rectangle((x, y, x + w, y + h), radius=18, fill=fill, outline=BLACK, width=2)
    centered(draw, (x, y, w, h), text, F_SUB)


def arrow_head(draw, end, angle):
    import math

    length = 18
    spread = math.radians(25)
    x, y = end
    p1 = (x - length * math.cos(angle - spread), y - length * math.sin(angle - spread))
    p2 = (x - length * math.cos(angle + spread), y - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)


def arrow(draw, start, end, label=None, label_offset=(0, 0), width=3):
    import math

    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    angle = math.atan2(y2 - y1, x2 - x1)
    arrow_head(draw, end, angle)
    if label:
        mx = (x1 + x2) / 2 + label_offset[0]
        my = (y1 + y2) / 2 + label_offset[1]
        tw, th = text_size(draw, label, F_LABEL)
        draw.rounded_rectangle(
            (mx - tw / 2 - 10, my - th / 2 - 6, mx + tw / 2 + 10, my + th / 2 + 6),
            radius=7,
            fill=BG,
        )
        draw.text((mx - tw / 2, my - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


def poly_arrow(draw, points, label=None, label_pos=None, width=3):
    import math

    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    x1, y1 = points[-2]
    x2, y2 = points[-1]
    arrow_head(draw, points[-1], math.atan2(y2 - y1, x2 - x1))
    if label and label_pos:
        x, y = label_pos
        tw, th = text_size(draw, label, F_LABEL)
        draw.rounded_rectangle(
            (x - tw / 2 - 10, y - th / 2 - 6, x + tw / 2 + 10, y + th / 2 + 6),
            radius=7,
            fill=BG,
        )
        draw.text((x - tw / 2, y - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 55, W, 80), "游戏状态管理模块关系图", F_TITLE)

input_node = (150, 250, 500, 260)
flow_node = (150, 760, 500, 260)
manager = (910, 470, 780, 360)
state_node = (640, 1080, 560, 380)
timer_node = (1280, 1110, 560, 330)
event_node = (2010, 540, 510, 330)
ui_node = (2100, 1110, 510, 330)

node(draw, input_node, "玩家输入模块", ["暂停按键", "交互输入"], BLUE)
node(draw, flow_node, "流程控制模块", ["开始游戏", "结束游戏"], YELLOW)
node(draw, manager, "GameManager", ["游戏状态管理核心", "控制生命周期", "处理暂停与恢复"], GREEN)
node(draw, state_node, "GameState 枚举", ["WaitingToStart", "CountdownToStart", "GamePlaying", "GameOver"], PURPLE)
node(draw, timer_node, "时间进度管理", ["记录游戏时间", "计算剩余进度"], CYAN)
node(draw, event_node, "状态事件通知", ["OnStateChanged", "OnGamePaused", "OnGameUnpaused"], PINK)
node(draw, ui_node, "UI 模块", ["倒计时 UI", "进度条 UI", "暂停 / 结束 UI"], BLUE)

arrow(draw, (650, 380), (910, 560), "暂停 / 恢复输入", label_offset=(0, -54))
arrow(draw, (650, 890), (910, 720), "推进流程", label_offset=(0, 54))

arrow(draw, (1130, 830), (910, 1080), "维护状态", label_offset=(-58, 0))
arrow(draw, (1510, 830), (1560, 1110), "更新时间", label_offset=(78, 0))
arrow(draw, (1690, 635), (2010, 635), "发布事件", label_offset=(0, -54))
arrow(draw, (2355, 870), (2355, 1110), "通知界面更新", label_offset=(150, 0))
arrow(draw, (1840, 1275), (2100, 1275), "提供进度", label_offset=(0, -62))

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "图4-2 游戏状态管理模块关系图.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
