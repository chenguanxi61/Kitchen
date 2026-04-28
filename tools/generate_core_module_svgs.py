from __future__ import annotations

from pathlib import Path
from xml.sax.saxutils import escape


OUT = Path(r"E:\UnityLearn\Kitchen\output\doc\核心模块设计图")


STYLE = """
<defs>
  <style>
    .title{font:700 36px "Microsoft YaHei","SimHei",Arial,sans-serif;fill:#111}
    .box-title{font:700 22px "Microsoft YaHei","SimHei",Arial,sans-serif;fill:#111}
    .text{font:20px "Microsoft YaHei","SimSun",Arial,sans-serif;fill:#222}
    .small{font:18px "Microsoft YaHei","SimSun",Arial,sans-serif;fill:#333}
    .box{fill:#fff;stroke:#222;stroke-width:1.6;rx:8}
    .soft{fill:#f8fafc;stroke:#94a3b8;stroke-width:1.4;rx:8}
    .decision{fill:#fff;stroke:#222;stroke-width:1.6}
    .line{stroke:#333;stroke-width:2.1;fill:none}
    .dash{stroke:#333;stroke-width:2.1;stroke-dasharray:7 5;fill:none}
  </style>
  <marker id="arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="8" markerHeight="8" orient="auto-start-reverse">
    <path d="M0 0 L10 5 L0 10 z" fill="#333"/>
  </marker>
</defs>
"""


def text_lines(lines: list[str], x: int, y: int, cls: str = "text", anchor: str = "middle", gap: int = 22) -> str:
    chunks = []
    for i, line in enumerate(lines):
        chunks.append(f'<text x="{x}" y="{y + i * gap}" text-anchor="{anchor}" class="{cls}">{escape(line)}</text>')
    return "\n".join(chunks)


def box(x: int, y: int, w: int, h: int, title: str, lines: list[str] | None = None, cls: str = "box") -> str:
    lines = lines or []
    return "\n".join(
        [
            f'<rect x="{x}" y="{y}" width="{w}" height="{h}" class="{cls}"/>',
            f'<text x="{x + w / 2:.1f}" y="{y + 36}" text-anchor="middle" class="box-title">{escape(title)}</text>',
            text_lines(lines, int(x + w / 2), y + 70, "small", "middle", 24),
        ]
    )


def pill(x: int, y: int, w: int, h: int, title: str) -> str:
    return "\n".join(
        [
            f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{h / 2:.1f}" fill="#eef6ff" stroke="#2563eb" stroke-width="1.6"/>',
            f'<text x="{x + w / 2:.1f}" y="{y + h / 2 + 6:.1f}" text-anchor="middle" class="box-title">{escape(title)}</text>',
        ]
    )


def diamond(cx: int, cy: int, w: int, h: int, title: str, lines: list[str] | None = None) -> str:
    lines = lines or []
    points = f"{cx},{cy - h//2} {cx + w//2},{cy} {cx},{cy + h//2} {cx - w//2},{cy}"
    return "\n".join(
        [
            f'<polygon points="{points}" class="decision"/>',
            f'<text x="{cx}" y="{cy - 4}" text-anchor="middle" class="box-title">{escape(title)}</text>',
            text_lines(lines, cx, cy + 18, "small", "middle", 18),
        ]
    )


def line(x1: int, y1: int, x2: int, y2: int, label: str = "", dashed: bool = False) -> str:
    cls = "dash" if dashed else "line"
    midx, midy = (x1 + x2) // 2, (y1 + y2) // 2
    label_svg = f'<text x="{midx}" y="{midy - 8}" text-anchor="middle" class="small">{escape(label)}</text>' if label else ""
    return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" class="{cls}" marker-end="url(#arrow)"/>{label_svg}'


def path(d: str, label: str = "", lx: int = 0, ly: int = 0, dashed: bool = False) -> str:
    cls = "dash" if dashed else "line"
    label_svg = f'<text x="{lx}" y="{ly}" text-anchor="middle" class="small">{escape(label)}</text>' if label else ""
    return f'<path d="{d}" class="{cls}" marker-end="url(#arrow)"/>{label_svg}'


def svg(width: int, height: int, title: str, body: str) -> str:
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">
{STYLE}
<rect x="0" y="0" width="{width}" height="{height}" fill="#fff"/>
<text x="{width/2:.1f}" y="44" text-anchor="middle" class="title">{escape(title)}</text>
{body}
</svg>
'''


def module_relation() -> str:
    body = "\n".join(
        [
            box(60, 96, 230, 108, "联机管理模块", ["KitchGameMultiPlayer", "建房、加入、准备、玩家生成"], "soft"),
            box(390, 96, 230, 108, "游戏状态管理模块", ["GameManager", "状态切换、计时、暂停恢复"], "soft"),
            box(720, 96, 230, 108, "UI 显示模块", ["订单、进度、倒计时", "暂停和结束界面"], "soft"),
            box(60, 330, 230, 108, "玩家交互模块", ["Player", "移动、选中、交互分发"], "soft"),
            box(390, 330, 230, 108, "柜台加工模块", ["BaseCounter 及子类", "取材、切菜、烹饪、装盘"], "soft"),
            box(720, 330, 230, 108, "订单与交付模块", ["DeliverManager", "订单生成、匹配判定、反馈"], "soft"),
            line(290, 150, 390, 150, "统一进入游戏"),
            line(620, 150, 720, 150, "状态变化事件"),
            line(175, 204, 175, 330, "生成玩家对象"),
            line(290, 384, 390, 384, "主/辅助交互"),
            line(620, 384, 720, 384, "盘子与菜品"),
            line(835, 330, 835, 204, "订单刷新/结果反馈"),
            path("M505 330 C505 270 690 260 780 204", "加工进度事件", 620, 268),
            path("M175 330 C250 260 390 245 505 204", "网络同步支持", 330, 260, True),
        ]
    )
    return svg(1010, 510, "核心模块关系图", body)


def state_flow() -> str:
    body = "\n".join(
        [
            pill(400, 82, 220, 56, "进入游戏场景"),
            box(400, 172, 220, 78, "WaitingToStart", ["等待开始"]),
            box(400, 288, 220, 78, "CountdownToStart", ["开始倒计时"]),
            box(400, 404, 220, 78, "GamePlaying", ["游戏进行"]),
            box(400, 520, 220, 78, "GameOver", ["游戏结束"]),
            box(720, 404, 220, 78, "暂停状态", ["Time.timeScale = 0"]),
            box(400, 636, 220, 78, "结束界面", ["显示交付结果"]),
            line(510, 138, 510, 172),
            line(510, 250, 510, 288, "等待时间结束"),
            line(510, 366, 510, 404, "倒计时结束"),
            line(510, 482, 510, 520, "游戏时间归零"),
            line(510, 598, 510, 636),
            line(620, 443, 720, 443, "触发暂停"),
            path("M720 463 C660 515 610 515 560 482", "再次触发", 648, 535),
        ]
    )
    return svg(1040, 760, "游戏状态管理流程图", body)


def network_flow() -> str:
    body = "\n".join(
        [
            pill(440, 82, 220, 56, "进入房间流程"),
            diamond(550, 190, 220, 94, "玩家身份"),
            box(220, 280, 220, 74, "启动 Host 会话"),
            box(660, 280, 220, 74, "客户端加入房间"),
            box(440, 400, 220, 74, "记录已连接玩家"),
            box(440, 510, 220, 74, "提交准备状态"),
            box(440, 620, 220, 74, "服务器更新准备字典"),
            diamond(550, 780, 250, 106, "所有玩家", ["均已准备？"]),
            box(440, 920, 220, 74, "统一加载游戏场景"),
            box(440, 1030, 220, 74, "生成玩家网络对象"),
            pill(440, 1140, 220, 56, "进入多人协作游戏"),
            line(550, 138, 550, 143),
            path("M495 210 C380 230 330 250 330 280", "Host", 382, 238),
            path("M605 210 C720 230 770 250 770 280", "Client", 720, 238),
            path("M330 354 C330 385 440 385 500 400"),
            path("M770 354 C770 385 660 385 600 400"),
            line(550, 474, 550, 510),
            line(550, 584, 550, 620),
            line(550, 694, 550, 727),
            path("M425 780 C300 780 300 525 440 545", "否，继续等待", 300, 660),
            line(550, 833, 550, 920, "是"),
            line(550, 994, 550, 1030),
            line(550, 1104, 550, 1140),
        ]
    )
    return svg(1100, 1240, "联机准备与开局流程图", body)


def player_counter_flow() -> str:
    body = "\n".join(
        [
            pill(440, 82, 220, 56, "玩家输入"),
            box(440, 170, 220, 70, "读取移动方向"),
            box(440, 270, 220, 70, "碰撞检测"),
            diamond(550, 400, 240, 104, "是否可以移动？"),
            box(250, 525, 220, 70, "尝试单轴移动"),
            box(630, 525, 220, 70, "更新角色位置"),
            box(440, 650, 220, 70, "记录有效朝向"),
            box(440, 750, 220, 70, "发射交互射线"),
            diamond(550, 900, 240, 104, "是否命中柜台？"),
            box(230, 1040, 260, 78, "清空选中对象"),
            box(610, 1040, 260, 78, "设置选中柜台", ["显示高亮反馈"]),
            diamond(740, 1210, 250, 104, "交互类型"),
            box(430, 1360, 240, 78, "主交互", ["拾取、放置、提交、转移"]),
            box(820, 1360, 240, 78, "辅助交互", ["切菜等连续加工"]),
            pill(625, 1510, 220, 56, "交互结束"),
            line(550, 138, 550, 170),
            line(550, 240, 550, 270),
            line(550, 340, 550, 348),
            path("M490 420 C390 455 360 490 360 525", "否", 405, 468),
            path("M610 420 C710 455 740 490 740 525", "是", 696, 468),
            path("M360 595 C390 635 440 635 500 650"),
            line(740, 595, 600, 650),
            line(550, 720, 550, 750),
            line(550, 820, 550, 848),
            path("M490 920 C370 950 360 1000 360 1040", "否", 395, 990),
            path("M610 920 C730 950 740 1000 740 1040", "是", 700, 990),
            line(740, 1118, 740, 1158),
            path("M680 1230 C585 1280 550 1320 550 1360", "主交互", 585, 1300),
            path("M800 1230 C895 1280 940 1320 940 1360", "辅助交互", 900, 1300),
            path("M550 1438 C570 1480 625 1488 700 1510"),
            path("M940 1438 C910 1480 845 1488 770 1510"),
        ]
    )
    return svg(1160, 1610, "玩家与柜台交互流程图", body)


def order_flow() -> str:
    body = "\n".join(
        [
            pill(440, 82, 220, 56, "游戏进行阶段"),
            diamond(550, 210, 270, 110, "待完成订单", ["是否达到上限？"]),
            box(170, 320, 220, 74, "等待下一次检查"),
            box(470, 350, 300, 86, "服务器生成订单", ["从 RecipeSO 随机选择菜谱"]),
            box(470, 470, 300, 74, "广播到各客户端"),
            box(470, 580, 300, 74, "刷新订单列表 UI"),
            box(470, 690, 300, 74, "玩家取材、加工、装盘"),
            box(470, 800, 300, 74, "提交盘子到交付柜台"),
            box(470, 910, 300, 74, "读取盘中食材列表"),
            diamond(620, 1060, 300, 120, "是否匹配订单？", ["数量和种类均一致"]),
            box(260, 1220, 260, 84, "交付失败", ["触发失败反馈"]),
            box(720, 1220, 260, 84, "交付成功", ["移除订单、增加成功数量"]),
            box(490, 1390, 260, 74, "UI 和音效反馈"),
            line(550, 138, 550, 155),
            path("M435 210 C310 230 280 280 280 320", "是", 338, 265),
            path("M280 394 C280 455 390 455 470 395", "返回检查", 350, 470),
            line(590, 265, 590, 350, "否"),
            line(620, 436, 620, 470),
            line(620, 544, 620, 580),
            line(620, 654, 620, 690),
            line(620, 764, 620, 800),
            line(620, 874, 620, 910),
            line(620, 984, 620, 1000),
            path("M540 1095 C430 1135 390 1175 390 1220", "否", 448, 1170),
            path("M700 1095 C815 1135 850 1175 850 1220", "是", 790, 1170),
            path("M390 1304 C430 1360 500 1365 590 1390"),
            path("M850 1304 C810 1360 740 1365 620 1390"),
            path("M620 1464 C620 1515 160 1515 160 210 C160 160 410 160 500 170", "继续下一轮", 300, 1538),
        ]
    )
    return svg(1120, 1570, "订单生成与交付判定流程图", body)


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    files = {
        "图4-2_核心模块关系图.svg": module_relation(),
        "图4-3_游戏状态管理流程图.svg": state_flow(),
        "图4-4_联机准备与开局流程图.svg": network_flow(),
        "图4-5_玩家与柜台交互流程图.svg": player_counter_flow(),
        "图4-6_订单生成与交付判定流程图.svg": order_flow(),
    }
    for name, content in files.items():
        (OUT / name).write_text(content, encoding="utf-8")
        print(OUT / name)


if __name__ == "__main__":
    main()
