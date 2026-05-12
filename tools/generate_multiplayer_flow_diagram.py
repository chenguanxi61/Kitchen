from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图4-3_联机管理模块流程图.png"

W, H = 1900, 3950
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
F_BOX = font(40, True)
F_SUB = font(31)
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
    centered(draw, (x, y + 28, w, 54), title, F_BOX)
    start_y = y + 106
    line_h = 46
    for i, line in enumerate(lines):
        centered(draw, (x + 18, start_y + i * line_h, w - 36, line_h), line, F_SUB, GRAY)


def decision(draw, center, size, title, lines, fill):
    cx, cy = center
    w, h = size
    points = [(cx, cy - h / 2), (cx + w / 2, cy), (cx, cy + h / 2), (cx - w / 2, cy)]
    draw.polygon(points, fill=fill, outline=BLACK)
    centered(draw, (cx - w / 2 + 34, cy - 62, w - 68, 46), title, F_BOX)
    for i, line in enumerate(lines):
        centered(draw, (cx - w / 2 + 60, cy - 4 + i * 42, w - 120, 42), line, F_SUB, GRAY)


def arrow_head(draw, end, angle):
    import math

    length = 20
    spread = math.radians(25)
    x, y = end
    p1 = (x - length * math.cos(angle - spread), y - length * math.sin(angle - spread))
    p2 = (x - length * math.cos(angle + spread), y - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)


def label(draw, pos, text):
    x, y = pos
    tw, th = text_size(draw, text, F_LABEL)
    draw.rounded_rectangle(
        (x - tw / 2 - 10, y - th / 2 - 6, x + tw / 2 + 10, y + th / 2 + 6),
        radius=7,
        fill=BG,
    )
    draw.text((x - tw / 2, y - th / 2 - 1), text, font=F_LABEL, fill=GRAY)


def arrow(draw, start, end, text=None, text_pos=None, width=3):
    import math

    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    arrow_head(draw, end, math.atan2(y2 - y1, x2 - x1))
    if text and text_pos:
        label(draw, text_pos, text)


def poly_arrow(draw, points, text=None, text_pos=None, width=3):
    import math

    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    x1, y1 = points[-2]
    x2, y2 = points[-1]
    arrow_head(draw, points[-1], math.atan2(y2 - y1, x2 - x1))
    if text and text_pos:
        label(draw, text_pos, text)


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered(draw, (0, 55, W, 80), "联机管理模块流程图", F_TITLE)

start = (570, 220, 760, 230)
connect = (570, 560, 760, 260)
select = (570, 930, 760, 260)
ready = (570, 1300, 760, 260)

data = (570, 1670, 760, 260)
check_center = (950, 2140)
scene = (570, 2430, 760, 260)
loaded = (570, 2800, 760, 240)

spawn = (570, 3130, 760, 260)
record = (570, 3500, 760, 310)

node(draw, start, "开始联机", ["大厅界面发起操作", "Host 建房或 Client 加入"], BLUE)
node(draw, connect, "建立网络连接", ["NetworkManager", "StartHost / StartClient", "注册连接回调"], PURPLE)
node(draw, select, "进入角色选择", ["Loader.LoadNetwork", "CharacterSelectScene", "同步玩家数据"], YELLOW)
node(draw, ready, "玩家点击准备", ["SetPlayerReady", "发送准备请求", "等待服务器处理"], ORANGE)

node(draw, data, "维护准备数据", ["SetPlayerReadyServerRpc", "playerReadyDictionary", "playerDataNetworkList"], CYAN)
decision(draw, check_center, (520, 330), "全员准备？", ["CheckAllPlayersReady"], PINK)
node(draw, scene, "切换游戏场景", ["全部玩家准备完成", "Loader.LoadNetwork", "GameScene"], GREEN)
node(draw, loaded, "场景加载完成", ["OnLoadEventCompleted", "服务器接收加载回调"], BLUE)

node(draw, spawn, "生成玩家对象", ["SpawnPlayersForAllClients", "遍历连接客户端", "SpawnAsPlayerObject"], GREEN)
node(draw, record, "记录生成结果", ["spawnedPlayerDictionary", "保存客户端与对象关系", "进入游戏主流程"], CYAN)

arrow(draw, (950, 450), (950, 560), "启动连接", (1160, 505))
arrow(draw, (950, 820), (950, 930), "连接成功", (1160, 875))
arrow(draw, (950, 1190), (950, 1300), "选择完成", (1160, 1245))
arrow(draw, (950, 1560), (950, 1670), "准备请求", (1160, 1615))
arrow(draw, (950, 1930), (950, 1975), "更新状态", (1160, 1950))
arrow(draw, (950, 2305), (950, 2430), "是", (1110, 2360))
arrow(draw, (950, 2690), (950, 2800), "加载 GameScene", (1190, 2745))
arrow(draw, (950, 3040), (950, 3130), "触发生成", (1160, 3085))
arrow(draw, (950, 3390), (950, 3500), "记录玩家对象", (1190, 3445))

poly_arrow(
    draw,
    [(1300, 2140), (1620, 2140), (1620, 1430), (1330, 1430)],
    "否，继续等待",
    (1630, 1785),
)

img.save(OUT_PNG)
desktop_path = DESKTOP_DIR / "图4-3 联机管理模块流程图.png"
try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    img.save(desktop_path)
    print(desktop_path)
except Exception as exc:
    print(f"desktop save failed: {exc}")
print(OUT_PNG)
