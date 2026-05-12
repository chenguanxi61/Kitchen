from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图4-1_系统场景流转图_修改版.png"

W, H = 2000, 720
BG = "white"
BLACK = (30, 30, 30)
GRAY = (80, 80, 80)
BLUE = (235, 244, 255)
GREEN = (237, 252, 242)
YELLOW = (255, 250, 235)
PURPLE = (245, 243, 255)
LINE = (70, 70, 70)


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


F_TITLE = font(40, True)
F_BOX = font(29, True)
F_SUB = font(23)
F_LABEL = font(22)


def centered(draw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    bbox = draw.textbbox((0, 0), text, font=fnt)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text((x + (w - tw) / 2, y + (h - th) / 2 - 1), text, font=fnt, fill=fill)


def box(draw, xy, title, subtitle, fill, title_size=None):
    x, y, w, h = xy
    title_font = title_size or F_BOX
    header_h = 78
    draw.rounded_rectangle((x, y, x + w, y + h), radius=12, fill=fill, outline=BLACK, width=2)
    draw.line((x, y + header_h, x + w, y + header_h), fill=BLACK, width=1)
    if isinstance(title, (list, tuple)):
        centered(draw, (x, y + 8, w, 34), title[0], title_font)
        centered(draw, (x, y + 42, w, 32), title[1], title_font)
    else:
        centered(draw, (x, y + 14, w, 56), title, title_font)
    centered(draw, (x, y + header_h + 2, w, h - header_h - 2), subtitle, F_SUB, GRAY)


def arrow(draw, start, end, label=None, label_y_offset=-28, width=3):
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
        mx, my = (x1 + x2) / 2, (y1 + y2) / 2 + label_y_offset
        bbox = draw.textbbox((0, 0), label, font=F_LABEL)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((mx - tw / 2 - 7, my - th / 2 - 4, mx + tw / 2 + 7, my + th / 2 + 4), radius=5, fill=BG)
        draw.text((mx - tw / 2, my - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


def poly_arrow(draw, points, label=None, label_pos=None, width=3):
    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    arrow(draw, points[-2], points[-1], None, width=width)
    if label and label_pos:
        x, y = label_pos
        bbox = draw.textbbox((0, 0), label, font=F_LABEL)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        draw.rounded_rectangle((x - tw / 2 - 7, y - th / 2 - 4, x + tw / 2 + 7, y + th / 2 + 4), radius=5, fill=BG)
        draw.text((x - tw / 2, y - th / 2 - 1), label, font=F_LABEL, fill=GRAY)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 28, W, 48), "系统场景流转图", F_TITLE)

main = (90, 270, 250, 140)
loading = (455, 270, 280, 140)
lobby = (850, 270, 260, 140)
character = (1240, 270, 330, 140)
game = (1710, 270, 240, 140)

box(draw, main, "MainMenu", "主菜单场景", BLUE)
box(draw, loading, "LoadingScene", "加载过渡场景", YELLOW)
box(draw, lobby, "LobbyScene", "房间场景", GREEN)
box(draw, character, ["Character", "SelectScene"], "角色选择场景", PURPLE, font(27, True))
box(draw, game, "GameScene", "游戏主场景", BLUE)

arrow(draw, (340, 340), (455, 340), "开始游戏", label_y_offset=-100)
arrow(draw, (735, 340), (850, 340), "加载完成", label_y_offset=-100)
arrow(draw, (1110, 340), (1240, 340), "Host 建房", label_y_offset=-100)
arrow(draw, (1570, 340), (1710, 340), None)
bbox = draw.textbbox((0, 0), "全部玩家准备完成", font=F_LABEL)
tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
lx, ly = 1640, 225
draw.rounded_rectangle((lx - tw / 2 - 7, ly - th / 2 - 4, lx + tw / 2 + 7, ly + th / 2 + 4), radius=5, fill=BG)
draw.text((lx - tw / 2, ly - th / 2 - 1), "全部玩家准备完成", font=F_LABEL, fill=GRAY)

# Optional direct return / scene management notes kept outside the main path.
poly_arrow(draw, [(1830, 410), (1830, 540), (215, 540), (215, 410)], "返回主菜单", (1010, 515), width=2)

draw.text((90, 610), "说明：主流程按照“主菜单 - 加载过渡 - 房间 - 角色选择 - 游戏主场景”推进；多人模式下由 Host 创建房间，并在所有玩家准备完成后统一进入游戏场景。", font=F_LABEL, fill=GRAY)

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "图4-1 系统场景流转图_修改版.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
