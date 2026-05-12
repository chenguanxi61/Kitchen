from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUT_DIR = Path(r"E:\UnityLearn\Kitchen\output\doc")
DESKTOP_DIR = Path(r"C:\Users\Administrator\Desktop\毕业设计\毕设图片")
OUT_DIR.mkdir(parents=True, exist_ok=True)

GENERATION_PNG = OUT_DIR / "图5-3_订单生成实现流程图.png"
DELIVERY_PNG = OUT_DIR / "图5-4_订单交付判定实现流程图.png"

W = 1900
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


F_TITLE = font(62, True)
F_BOX = font(39, True)
F_SUB = font(30)
F_LABEL = font(29)


def text_size(draw, text, fnt):
    bbox = draw.textbbox((0, 0), text, font=fnt)
    return bbox[2] - bbox[0], bbox[3] - bbox[1]


def centered(draw, xy, text, fnt, fill=BLACK):
    x, y, w, h = xy
    bbox = draw.textbbox((0, 0), text, font=fnt)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text(
        (x + (w - tw) / 2 - bbox[0], y + (h - th) / 2 - bbox[1] - 1),
        text,
        font=fnt,
        fill=fill,
    )


def node(draw, xy, title, lines, fill):
    x, y, w, h = xy
    draw.rounded_rectangle((x, y, x + w, y + h), radius=14, fill=fill, outline=BLACK, width=2)
    centered(draw, (x, y + 26, w, 52), title, F_BOX)
    start_y = y + 102
    line_h = 44
    for i, line in enumerate(lines):
        centered(draw, (x + 20, start_y + i * line_h, w - 40, line_h), line, F_SUB, GRAY)


def decision(draw, center, size, title, lines, fill):
    cx, cy = center
    w, h = size
    points = [(cx, cy - h / 2), (cx + w / 2, cy), (cx, cy + h / 2), (cx - w / 2, cy)]
    draw.polygon(points, fill=fill, outline=BLACK)
    centered(draw, (cx - w / 2 + 48, cy - 72, w - 96, 46), title, F_BOX)
    for i, line in enumerate(lines):
        centered(draw, (cx - w / 2 + 70, cy - 14 + i * 42, w - 140, 42), line, F_SUB, GRAY)


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
    bbox = draw.textbbox((0, 0), text, font=F_LABEL)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.rounded_rectangle(
        (x - tw / 2 - 10, y - th / 2 - 6, x + tw / 2 + 10, y + th / 2 + 6),
        radius=7,
        fill=BG,
    )
    draw.text((x - tw / 2 - bbox[0], y - th / 2 - bbox[1] - 1), text, font=F_LABEL, fill=GRAY)


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


def generation_diagram():
    h = 3250
    img = Image.new("RGB", (W, h), BG)
    draw = ImageDraw.Draw(img)
    centered(draw, (0, 55, W, 80), "订单生成实现流程图", F_TITLE)

    x, bw = 570, 760
    on_spawn = (x, 220, bw, 230)
    server_center = (950, 650)
    first = (x, 890, bw, 230)
    coroutine = (x, 1210, bw, 230)
    limit_center = (950, 1650)
    wait = (x, 1930, bw, 230)
    spawn = (x, 2250, bw, 280)
    rpc = (x, 2620, bw, 320)

    node(draw, on_spawn, "网络对象生成", ["DeliverManager.OnNetworkSpawn", "服务器端负责生成订单"], BLUE)
    decision(draw, server_center, (560, 300), "是否为服务器？", ["IsServer"], PINK)
    node(draw, first, "生成初始订单", ["SpawnNewRecipe", "立即创建首个订单"], GREEN)
    node(draw, coroutine, "启动刷新协程", ["StartCoroutine", "SpawnRecipeLoop"], PURPLE)
    decision(draw, limit_center, (620, 330), "订单未达上限？", ["waitingRecipeSOList.Count", "waitingRecipeMax"], PINK)
    node(draw, wait, "等待刷新间隔", ["WaitForSeconds(4f)", "控制订单生成频率"], YELLOW)
    node(draw, spawn, "随机选择配方", ["SpawnNewRecipe", "Random.Range", "RecipeSO"], ORANGE)
    node(draw, rpc, "同步订单列表", ["SpawnNewRecipeClientRpc", "waitingRecipeSOList.Add", "OnRecipeSpawned"], CYAN)

    arrow(draw, (950, 450), (950, 500), None)
    arrow(draw, (950, 800), (950, 890), "是", (1110, 845))
    arrow(draw, (950, 1120), (950, 1210), None)
    arrow(draw, (950, 1440), (950, 1485), None)
    arrow(draw, (950, 1815), (950, 1930), "是", (1110, 1870))
    arrow(draw, (950, 2160), (950, 2250), None)
    arrow(draw, (950, 2530), (950, 2620), None)

    poly_arrow(draw, [(1230, 650), (1550, 650), (1550, 300), (1330, 300)], "否，结束", (1550, 500))
    poly_arrow(draw, [(1260, 1650), (1580, 1650), (1580, 1360), (1330, 1360)], "否，继续等待", (1580, 1510))
    poly_arrow(draw, [(950, 2940), (950, 3110), (360, 3110), (360, 1370), (570, 1370)], "进入下一轮刷新", (610, 3110))

    return img


def delivery_diagram():
    h = 4300
    img = Image.new("RGB", (W, h), BG)
    draw = ImageDraw.Draw(img)
    centered(draw, (0, 55, W, 80), "订单交付判定实现流程图", F_TITLE)

    x, bw = 570, 760
    interact = (x, 220, bw, 230)
    has_obj = (950, 620)
    plate = (950, 980)
    deliver = (x, 1240, bw, 230)
    list_node = (x, 1560, bw, 230)
    count_match = (950, 1960)
    ingredient_match = (950, 2360)
    server_rpc = (x, 2670, bw, 230)
    client_rpc = (x, 2990, bw, 330)
    destroy_plate = (x, 3410, bw, 230)
    fail_rpc = (x, 3790, bw, 310)

    node(draw, interact, "交付柜台交互", ["DeliveryCounter.Interact", "玩家提交手中物品"], BLUE)
    decision(draw, has_obj, (560, 300), "玩家持有物品？", ["player.HasKitchenObj"], PINK)
    decision(draw, plate, (560, 300), "是否为盘子？", ["TryGetPlate", "PlateKitchObj"], PINK)
    node(draw, deliver, "执行交付判定", ["DeliverManager.DeliverRecipe", "获取盘中食材列表"], GREEN)
    node(draw, list_node, "遍历待完成订单", ["waitingRecipeSOList", "逐个取出 RecipeSO"], CYAN)
    decision(draw, count_match, (620, 330), "数量是否一致？", ["recipeList.Count", "plateList.Count"], PINK)
    decision(draw, ingredient_match, (660, 350), "食材完全匹配？", ["逐项查找 KitchenObjSO", "全部找到才成功"], PINK)
    node(draw, server_rpc, "发送成功请求", ["DeliverRecipeServerRpc", "服务器校验 recipeIndex"], ORANGE)
    node(draw, client_rpc, "广播成功结果", ["DeliverCorrectRecipeClientRpc", "RemoveAt(recipeIndex)", "successfulDeliveries++", "OnRecipeCompleted / OnDeliverySuccess"], GREEN)
    node(draw, destroy_plate, "销毁已交付盘子", ["KitchenObj.DestoryKitchenObj", "完成本次成功交付"], YELLOW)
    node(draw, fail_rpc, "广播失败反馈", ["DeliverIncorrectRecipeServerRpc", "DeliverIncorrectRecipeClientRpc", "OnDeliveryFail"], PURPLE)

    arrow(draw, (950, 450), (950, 470), None)
    arrow(draw, (950, 770), (950, 830), "是", (1110, 800))
    arrow(draw, (950, 1130), (950, 1240), "是", (1110, 1180))
    arrow(draw, (950, 1470), (950, 1560), None)
    arrow(draw, (950, 1790), (950, 1795), None)
    arrow(draw, (950, 2125), (950, 2185), "是", (1110, 2160))
    arrow(draw, (950, 2535), (950, 2670), "是", (1110, 2600))
    arrow(draw, (950, 2900), (950, 2990), None)
    arrow(draw, (950, 3320), (950, 3410), None)

    poly_arrow(draw, [(1230, 620), (1580, 620), (1580, 340), (1330, 340)], "否，结束", (1580, 500))
    poly_arrow(draw, [(1230, 980), (1580, 980), (1580, 340), (1330, 340)], "否，结束", (1580, 840))
    poly_arrow(draw, [(1260, 1960), (1580, 1960), (1580, 1660), (1330, 1660)], "否，检查下一个", (1580, 1810))
    poly_arrow(draw, [(1280, 2360), (1600, 2360), (1600, 1660), (1330, 1660)], "否，检查下一个", (1600, 2130))
    poly_arrow(draw, [(570, 1680), (320, 1680), (320, 3945), (570, 3945)], "全部订单未匹配", (320, 2780))

    return img


def save_image(img, out_path, desktop_name):
    img.save(out_path)
    desktop_path = DESKTOP_DIR / desktop_name
    try:
        DESKTOP_DIR.mkdir(parents=True, exist_ok=True)
        img.save(desktop_path)
        print(desktop_path)
    except Exception as exc:
        print(f"desktop save failed: {exc}")
    print(out_path)


save_image(generation_diagram(), GENERATION_PNG, "图5-3 订单生成实现流程图.png")
save_image(delivery_diagram(), DELIVERY_PNG, "图5-4 订单交付判定实现流程图.png")
