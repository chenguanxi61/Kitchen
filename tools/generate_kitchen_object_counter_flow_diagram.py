from pathlib import Path
import math
import shutil
import textwrap

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(r"E:\UnityLearn\Kitchen")
OUT_DIR = ROOT / "output" / "doc"
DESKTOP_DIR = Path.home() / "Desktop" / "毕业设计" / "毕设图片"
OUT_DIR.mkdir(parents=True, exist_ok=True)

OUT_PNG = OUT_DIR / "图5-4_厨房对象与柜台处理流程图.png"
DESKTOP_PNG = DESKTOP_DIR / "图5-4 厨房对象与柜台处理流程图.png"

W, H = 2800, 2200
BG = (255, 255, 255)
BLACK = (20, 20, 20)
LINE = (35, 35, 35)


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


F_TITLE = font(66, True)
F_LANE = font(43, True)
F_BOX = font(38, True)
F_SUB = font(31)
F_LABEL = font(30, True)


def bbox(draw, text, fnt):
    box = draw.textbbox((0, 0), text, font=fnt)
    return box[2] - box[0], box[3] - box[1], box


def centered_line(draw, rect, text, fnt, fill=BLACK):
    x, y, w, h = rect
    tw, th, tb = bbox(draw, text, fnt)
    draw.text(
        (x + (w - tw) / 2 - tb[0], y + (h - th) / 2 - tb[1] - 1),
        text,
        font=fnt,
        fill=fill,
    )


def draw_multiline_center(draw, rect, lines, fonts):
    x, y, w, h = rect
    line_sizes = [bbox(draw, line, fnt)[:2] for line, fnt in zip(lines, fonts)]
    line_gap = 9
    total_h = sum(size[1] for size in line_sizes) + line_gap * (len(lines) - 1)
    cy = y + (h - total_h) / 2
    for (line, fnt), (tw, th) in zip(zip(lines, fonts), line_sizes):
        tb = draw.textbbox((0, 0), line, font=fnt)
        draw.text((x + (w - tw) / 2 - tb[0], cy - tb[1]), line, font=fnt, fill=BLACK)
        cy += th + line_gap


def wrap_label(draw, text, fnt, max_width):
    if "\n" in text:
        return text.splitlines()
    chars = list(text)
    lines = []
    current = ""
    for ch in chars:
        candidate = current + ch
        if bbox(draw, candidate, fnt)[0] <= max_width or not current:
            current = candidate
        else:
            lines.append(current)
            current = ch
    if current:
        lines.append(current)
    return lines


def node(draw, rect, title, subtitle=None):
    x, y, w, h = rect
    draw.rounded_rectangle((x, y, x + w, y + h), radius=8, fill=BG, outline=BLACK, width=3)
    title_lines = wrap_label(draw, title, F_BOX, w - 38)
    lines = title_lines[:2]
    fonts = [F_BOX for _ in lines]
    if subtitle:
        sub_lines = wrap_label(draw, subtitle, F_SUB, w - 42)
        lines.extend(sub_lines[:2])
        fonts.extend([F_SUB for _ in sub_lines[:2]])
    draw_multiline_center(draw, (x + 18, y + 12, w - 36, h - 24), lines, fonts)


def lane(draw, rect, title):
    x, y, w, h = rect
    draw.rectangle((x, y, x + w, y + h), fill=BG, outline=BLACK, width=3)
    draw.line((x, y + 90, x + w, y + 90), fill=BLACK, width=3)
    centered_line(draw, (x, y + 12, w, 66), title, F_LANE)


def decision(draw, center, size, text):
    cx, cy = center
    w, h = size
    points = [(cx, cy - h / 2), (cx + w / 2, cy), (cx, cy + h / 2), (cx - w / 2, cy)]
    draw.polygon(points, fill=BG, outline=BLACK)
    for a, b in zip(points, points[1:] + points[:1]):
        draw.line((*a, *b), fill=BLACK, width=3)
    lines = wrap_label(draw, text, F_BOX, w - 100)
    draw_multiline_center(draw, (cx - w / 2 + 55, cy - h / 2 + 32, w - 110, h - 64), lines, [F_BOX] * len(lines))


def arrow_head(draw, end, angle):
    length = 22
    spread = math.radians(25)
    x, y = end
    p1 = (x - length * math.cos(angle - spread), y - length * math.sin(angle - spread))
    p2 = (x - length * math.cos(angle + spread), y - length * math.sin(angle + spread))
    draw.polygon((end, p1, p2), fill=LINE)


def label(draw, pos, text):
    x, y = pos
    tw, th, tb = bbox(draw, text, F_LABEL)
    draw.rounded_rectangle(
        (x - tw / 2 - 12, y - th / 2 - 8, x + tw / 2 + 12, y + th / 2 + 8),
        radius=6,
        fill=BG,
        outline=None,
    )
    draw.text((x - tw / 2 - tb[0], y - th / 2 - tb[1] - 1), text, font=F_LABEL, fill=BLACK)


def arrow(draw, start, end, text=None, text_pos=None, width=3):
    x1, y1 = start
    x2, y2 = end
    draw.line((x1, y1, x2, y2), fill=LINE, width=width)
    arrow_head(draw, end, math.atan2(y2 - y1, x2 - x1))
    if text:
        label(draw, text_pos or ((x1 + x2) / 2, (y1 + y2) / 2), text)


def poly_arrow(draw, points, text=None, text_pos=None, width=3):
    for a, b in zip(points, points[1:]):
        draw.line((*a, *b), fill=LINE, width=width)
    x1, y1 = points[-2]
    x2, y2 = points[-1]
    arrow_head(draw, points[-1], math.atan2(y2 - y1, x2 - x1))
    if text:
        label(draw, text_pos or points[len(points) // 2], text)


def vertical_flow(draw, boxes):
    for upper, lower in zip(boxes, boxes[1:]):
        x1, y1, w1, h1 = upper
        x2, y2, w2, _ = lower
        arrow(draw, (x1 + w1 / 2, y1 + h1), (x2 + w2 / 2, y2))


img = Image.new("RGB", (W, H), BG)
draw = ImageDraw.Draw(img)

centered_line(draw, (0, 48, W, 86), "厨房对象与柜台处理流程图", F_TITLE)

main_y = 170
main_w = 390
main_h = 145
gap = 55
main_xs = [115 + i * (main_w + gap) for i in range(6)]
main_nodes = [
    (main_xs[0], main_y, main_w, main_h, "玩家与柜台交互", "BaseCounter.Interact"),
    (main_xs[1], main_y, main_w, main_h, "KitchenObj归属变化", "玩家 / 柜台之间转移"),
    (main_xs[2], main_y, main_w, main_h, "发送服务器请求", "SetKitchenObjParentServerRpc"),
    (main_xs[3], main_y, main_w, main_h, "客户端广播同步", "SetKitchenObjParentClientRpc"),
    (main_xs[4], main_y, main_w, main_h, "更新父对象引用", "IKitchObjParent"),
    (main_xs[5], main_y, main_w, main_h, "同步显示位置", "FollowTransfrom"),
]

for x, y, w, h, title, sub in main_nodes:
    node(draw, (x, y, w, h), title, sub)
for current, nxt in zip(main_nodes, main_nodes[1:]):
    arrow(draw, (current[0] + current[2], current[1] + current[3] / 2), (nxt[0], nxt[1] + nxt[3] / 2))

decision_center = (W / 2, 490)
decision(draw, decision_center, (520, 180), "判断柜台处理类型")
last = main_nodes[-1]
poly_arrow(
    draw,
    [
        (last[0] + last[2] / 2, last[1] + last[3]),
        (last[0] + last[2] / 2, 390),
        (decision_center[0], 390),
        (decision_center[0], decision_center[1] - 90),
    ],
)

lane_y = 660
lane_w = 780
lane_h = 1430
lane_xs = [110, 1010, 1910]
lane_titles = ["盘子对象 PlateKitchObj", "切菜台 CuttingCounter", "炉灶 StoveCounter"]
for x, title in zip(lane_xs, lane_titles):
    lane(draw, (x, lane_y, lane_w, lane_h), title)

box_w = 620
box_h = 116
box_gap = 76
start_y = lane_y + 145

plate_boxes = []
for i, (title, sub) in enumerate(
    [
        ("尝试添加食材", "TryAddSomething"),
        ("验证食材范围", "validKitchenObjSOList"),
        ("检查是否重复", "ContainsKitchenObj"),
        ("服务器写入索引", "AddKitchenObjServerRpc"),
        ("网络列表同步", "NetworkList<int>"),
        ("触发图标更新", "OnAddSomething"),
    ]
):
    rect = (lane_xs[0] + 80, start_y + i * (box_h + box_gap), box_w, box_h)
    plate_boxes.append(rect)
    node(draw, rect, title, sub)
vertical_flow(draw, plate_boxes)

cut_boxes = []
for i, (title, sub) in enumerate(
    [
        ("判断是否可切割", "HasRecipeWithInput"),
        ("执行辅助交互", "InteractAlternate"),
        ("增加加工次数", "cuttingProgress++"),
        ("广播进度事件", "OnProgressChanged"),
        ("达到切割阈值", "cuttingProgressMax"),
        ("生成切配结果", "SpawnKitchenObj"),
    ]
):
    rect = (lane_xs[1] + 80, start_y + i * (box_h + box_gap), box_w, box_h)
    cut_boxes.append(rect)
    node(draw, rect, title, sub)
vertical_flow(draw, cut_boxes)

stove_boxes = []
for i, (title, sub) in enumerate(
    [
        ("判断是否可加热", "GetFryRecipeSOWithInput"),
        ("进入加热状态", "State.Frying"),
        ("同步状态和进度", "NetworkVariable"),
        ("达到熟成时间", "fryTimerMax"),
        ("生成熟成对象", "State.Fryed"),
        ("超时生成烧焦对象", "State.Burned"),
    ]
):
    rect = (lane_xs[2] + 80, start_y + i * (box_h + box_gap), box_w, box_h)
    stove_boxes.append(rect)
    node(draw, rect, title, sub)
vertical_flow(draw, stove_boxes)

source_x, source_y = decision_center[0], decision_center[1] + 90
branch_y = 610
targets = [
    (plate_boxes[0][0] + box_w / 2, plate_boxes[0][1]),
    (cut_boxes[0][0] + box_w / 2, cut_boxes[0][1]),
    (stove_boxes[0][0] + box_w / 2, stove_boxes[0][1]),
]
labels = ["盘子组合", "切菜加工", "炉灶加热"]
label_positions = [(515, 612), (1400, 612), (2288, 612)]
for target, text, label_pos in zip(targets, labels, label_positions):
    poly_arrow(draw, [(source_x, source_y), (source_x, branch_y), (target[0], branch_y), target], text, label_pos)

img.save(OUT_PNG)

try:
    DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(OUT_PNG, DESKTOP_PNG)
    print(DESKTOP_PNG)
except Exception as exc:
    print(f"desktop save failed: {exc}")

print(OUT_PNG)
