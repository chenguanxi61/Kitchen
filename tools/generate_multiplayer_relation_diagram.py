from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图4-3_联机管理模块关系图.png"

W, H = 3000, 1800
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
ORANGE = (255, 247, 237)


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
    start_y = y + 118
    line_h = 48
    for i, line in enumerate(lines):
        centered(draw, (x + 18, start_y + i * line_h, w - 36, line_h), line, F_SUB, GRAY)


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

centered(draw, (0, 55, W, 80), "联机管理模块关系图", F_TITLE)

lobby_ui = (150, 250, 560, 300)
character_ui = (150, 740, 560, 320)
network_manager = (150, 1240, 560, 320)
manager = (1060, 520, 880, 430)
data_node = (1060, 1160, 880, 380)
ready_node = (2260, 220, 600, 340)
scene_node = (2260, 690, 600, 340)
spawn_node = (2260, 1190, 600, 340)

node(draw, lobby_ui, "大厅界面模块", ["启动 Host", "连接 Client", "进入角色选择场景"], BLUE)
node(draw, character_ui, "角色选择界面", ["玩家点击准备", "调用 SetPlayerReady", "显示准备状态"], YELLOW)
node(draw, network_manager, "NetworkManager", ["StartHost / StartClient", "连接与断开回调", "场景加载回调"], PURPLE)
node(draw, manager, "KitchGameMultiPlayer", ["联机管理核心", "维护连接玩家", "处理准备请求", "统一推进开局"], GREEN)
node(draw, data_node, "联机数据维护", ["playerReadyDictionary", "spawnedPlayerDictionary", "playerDataNetworkList", "playerColorCacheDictionary"], CYAN)
node(draw, ready_node, "准备状态管理", ["SetPlayerReadyServerRpc", "记录客户端准备状态", "CheckAllPlayersReady"], PINK)
node(draw, scene_node, "场景切换控制", ["全员准备后", "Loader.LoadNetwork", "切换 GameScene"], ORANGE)
node(draw, spawn_node, "玩家对象生成", ["OnLoadEventCompleted", "SpawnPlayersForAllClients", "SpawnAsPlayerObject"], BLUE)

arrow(draw, (710, 400), (1060, 620), "建房 / 加入", label_offset=(0, -58))
arrow(draw, (710, 900), (1060, 740), "发送准备请求", label_offset=(0, 58))
arrow(draw, (710, 1400), (1060, 840), "连接回调", label_offset=(-80, 10))

arrow(draw, (1500, 950), (1500, 1160), "更新联机数据", label_offset=(125, 0))
arrow(draw, (1940, 630), (2260, 390), "维护准备状态", label_offset=(0, -58))
arrow(draw, (1940, 740), (2260, 860), "全员准备", label_offset=(0, 58))
arrow(draw, (1940, 880), (2260, 1360), "加载完成后生成", label_offset=(20, -18))

poly_arrow(
    draw,
    [(2560, 1030), (2560, 1110), (2560, 1190)],
    "GameScene 加载回调",
    (2680, 1110),
)

poly_arrow(
    draw,
    [(2260, 1380), (2110, 1380), (2110, 1350), (1940, 1350)],
    "记录玩家对象",
    (2110, 1320),
)

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "图4-3 联机管理模块关系图.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
