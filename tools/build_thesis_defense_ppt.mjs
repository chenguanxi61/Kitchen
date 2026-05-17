import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "@oai/artifact-tool";

const ROOT = "E:/UnityLearn/Kitchen";
const OUT = path.join(ROOT, "output/doc/基于Unity的多人协作烹饪游戏设计与实现_答辩PPT_内容丰富版.pptx");
const MEDIA = path.join(ROOT, "tmp/slides/thesis_defense_assets/docx_media");
const DOC_IMG = (name) => path.join(MEDIA, name);
const OUT_IMG = (name) => path.join(ROOT, "output/doc", name);

const W = 1280;
const H = 720;
const C = {
  navy: "#15324B",
  blue: "#2F6F9F",
  lightBlue: "#EAF4FB",
  pale: "#F7FAFC",
  orange: "#F2A03D",
  green: "#4A9B72",
  gray: "#5B6770",
  dark: "#1E2933",
  white: "#FFFFFF",
  line: "#D6E2EA",
};
const FONT = "Microsoft YaHei";

async function readImageBlob(imagePath) {
  const bytes = await fs.readFile(imagePath);
  return bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength);
}

function setText(shape, text, opts = {}) {
  shape.text = text;
  shape.text.typeface = opts.typeface ?? FONT;
  shape.text.fontSize = opts.fontSize ?? 28;
  shape.text.color = opts.color ?? C.dark;
  shape.text.bold = opts.bold ?? false;
  shape.text.alignment = opts.align ?? "left";
  shape.text.verticalAlignment = opts.valign ?? "middle";
  shape.text.insets = opts.insets ?? { left: 12, right: 12, top: 8, bottom: 8 };
  shape.text.autoFit = "shrinkText";
  return shape;
}

function addBox(slide, left, top, width, height, fill = C.white, line = C.line, radius = 8) {
  return slide.shapes.add({
    geometry: "roundRect",
    position: { left, top, width, height },
    fill,
    line: { style: "solid", fill: line, width: 1.4 },
    adjustmentList: [{ name: "adj", formula: `val ${radius * 1200}` }],
  });
}

function addText(slide, text, left, top, width, height, opts = {}) {
  const shape = slide.shapes.add({
    geometry: "rect",
    position: { left, top, width, height },
    fill: opts.fill ?? "#FFFFFF00",
    line: { style: "solid", fill: "#FFFFFF00", width: 0 },
  });
  return setText(shape, text, opts);
}

function addTitle(slide, title, subtitle = "") {
  addText(slide, title, 58, 36, 880, 54, {
    fontSize: 34,
    bold: true,
    color: C.navy,
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
  slide.shapes.add({
    geometry: "rect",
    position: { left: 58, top: 96, width: 82, height: 5 },
    fill: C.orange,
    line: { fill: C.orange, width: 0 },
  });
  if (subtitle) {
    addText(slide, subtitle, 155, 82, 760, 32, {
      fontSize: 17,
      color: C.gray,
      insets: { left: 0, right: 0, top: 0, bottom: 0 },
    });
  }
}

function addFooter(slide, page) {
  addText(slide, `基于 Unity 的多人协作烹饪游戏设计与实现  |  ${page}`, 58, 686, 480, 24, {
    fontSize: 12,
    color: "#7A8791",
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
}

async function addImage(slide, imagePath, left, top, width, height, fit = "cover", alt = "") {
  const img = slide.images.add({ blob: await readImageBlob(imagePath), fit, alt });
  img.position = { left, top, width, height };
  return img;
}

async function addFramedImage(slide, imagePath, left, top, width, height, fit = "contain", alt = "") {
  addBox(slide, left - 8, top - 8, width + 16, height + 16, C.white, C.line);
  return addImage(slide, imagePath, left, top, width, height, fit, alt);
}

function addBulletCard(slide, title, bullets, left, top, width, height, accent = C.blue) {
  const box = addBox(slide, left, top, width, height, C.white, C.line);
  addText(slide, title, left + 22, top + 18, width - 44, 34, {
    fontSize: 24,
    bold: true,
    color: accent,
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
  addText(slide, bullets.map((b) => `• ${b}`).join("\n"), left + 24, top + 62, width - 48, height - 76, {
    fontSize: 16,
    color: C.dark,
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
  return box;
}

function addStep(slide, i, label, left, top, width = 155, height = 74) {
  addBox(slide, left, top, width, height, C.lightBlue, C.blue);
  addText(slide, String(i), left + 12, top + 16, 34, 36, {
    fontSize: 22,
    bold: true,
    align: "center",
    color: C.white,
    fill: C.blue,
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
  addText(slide, label, left + 52, top + 10, width - 64, height - 20, {
    fontSize: 20,
    bold: true,
    color: C.navy,
    align: "center",
  });
}

function addArrow(slide, x1, y1, x2, y2) {
  const left = Math.min(x1, x2);
  const top = y1 - 8;
  const width = Math.max(24, Math.abs(x2 - x1));
  slide.shapes.add({
    geometry: "rightArrow",
    position: { left, top, width, height: 16 },
    fill: C.blue,
    line: { style: "solid", fill: C.blue, width: 0 },
  });
}

const presentation = Presentation.create({ slideSize: { width: W, height: H } });
presentation.theme.colorScheme = {
  name: "DefenseTheme",
  themeColors: {
    accent1: C.blue,
    accent2: C.orange,
    bg1: C.white,
    bg2: C.pale,
    tx1: C.dark,
    tx2: C.gray,
  },
};

// 1. Title
{
  const slide = presentation.slides.add();
  slide.background.fill = C.white;
  await addImage(slide, DOC_IMG("image33.png"), 0, 0, W, H, "cover", "厨房协作游戏运行场景");
  slide.shapes.add({ geometry: "rect", position: { left: 0, top: 0, width: W, height: H }, fill: "#FFFFFFD9", line: { fill: "#FFFFFF00", width: 0 } });
  addText(slide, "基于 Unity 的多人协作烹饪游戏\n设计与实现", 72, 150, 720, 130, {
    fontSize: 42,
    bold: true,
    color: C.navy,
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
  addText(slide, "本科毕业论文答辩", 76, 302, 300, 36, { fontSize: 24, bold: true, color: C.orange, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  addText(slide, "研究内容：多人协作流程、厨房对象交互、订单交付判定与基础联机同步", 76, 362, 650, 46, { fontSize: 20, color: C.gray, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  addBox(slide, 830, 160, 330, 310, C.white, C.line);
  await addImage(slide, DOC_IMG("image17.png"), 850, 185, 290, 190, "contain", "游戏主菜单界面");
  addText(slide, "Unity 2022.3\nNetcode for GameObjects\nScriptableObject 数据配置", 860, 390, 270, 74, {
    fontSize: 19,
    color: C.navy,
    align: "center",
  });
}

// 2. Background
{
  const slide = presentation.slides.add();
  addTitle(slide, "研究背景与意义", "从单机交互到多人协作，系统复杂度显著提升");
  addBulletCard(slide, "研究背景", [
    "Unity 具备场景编辑、组件化脚本、资源管理和跨平台部署能力，适合完成中小型游戏系统原型开发。",
    "多人协作游戏不仅要实现单机交互，还要处理玩家连接、共享对象状态同步和关键事件一致性。",
    "协作烹饪玩法将取材、加工、装盘、交付和反馈集中在同一流程中，能较完整体现系统工程能力。",
  ], 70, 140, 520, 330, C.blue);
  addBulletCard(slide, "研究意义", [
    "工程实现层面：系统覆盖玩家控制、厨房对象交互、订单任务管理、UI 反馈和基础联机同步等功能。",
    "软件设计层面：通过状态管理、模块拆分和数据配置降低耦合，使后续扩展菜谱和交互规则更方便。",
    "实践应用层面：最终形成可运行、可演示的多人协作游戏闭环，便于展示毕业设计成果。",
  ], 690, 140, 520, 330, C.green);
  await addFramedImage(slide, DOC_IMG("image33.png"), 385, 515, 510, 120, "cover", "多人厨房场景");
  addFooter(slide, 2);
}

// 3. Objectives
{
  const slide = presentation.slides.add();
  addTitle(slide, "课题目标与研究内容", "目标是实现一套可运行的多人协作烹饪游戏原型");
  const goals = [
    ["完整玩法闭环", "实现从订单生成、食材处理、装盘交付到结果结算的连续流程，使玩家目标清晰、反馈完整。"],
    ["多人协作支持", "支持 Host 创建房间、客户端加入、玩家准备状态维护，并由主机统一切换到游戏场景。"],
    ["核心对象同步", "围绕玩家对象、厨房对象、炉灶状态和订单列表进行同步，保证多人场景下主要结果一致。"],
    ["可扩展设计", "通过 ScriptableObject 管理菜谱、食材、切配和烹饪规则，减少新增内容时对核心代码的修改。"],
  ];
  goals.forEach((g, idx) => {
    const left = 80 + (idx % 2) * 560;
    const top = 155 + Math.floor(idx / 2) * 185;
    addBox(slide, left, top, 500, 135, idx % 2 ? "#FFF8EA" : C.lightBlue, idx % 2 ? C.orange : C.blue);
    addText(slide, g[0], left + 24, top + 20, 450, 32, { fontSize: 25, bold: true, color: idx % 2 ? "#A86112" : C.navy, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
    addText(slide, g[1], left + 24, top + 60, 450, 62, { fontSize: 16, color: C.dark, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  });
  addText(slide, "研究内容覆盖：需求分析、系统设计、核心模块实现、功能展示、联机测试与结果分析。", 90, 570, 1100, 42, {
    fontSize: 22,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.pale,
  });
  addFooter(slide, 3);
}

// 4. Overall flow
{
  const slide = presentation.slides.add();
  addTitle(slide, "游戏整体流程", "从进入系统到完成订单结算形成完整闭环");
  const labels = ["主菜单", "联机准备", "进入厨房", "订单生成", "协作烹饪", "出菜交付", "结果结算"];
  labels.forEach((label, idx) => {
    const left = 70 + idx * 170;
    addStep(slide, idx + 1, label, left, 150, 140, 70);
    if (idx < labels.length - 1) addArrow(slide, left + 140, 185, left + 165, 185);
  });
  await addFramedImage(slide, DOC_IMG("image17.png"), 70, 285, 180, 105, "cover", "主菜单");
  await addFramedImage(slide, DOC_IMG("image18.png"), 270, 285, 180, 105, "cover", "准备界面");
  await addFramedImage(slide, DOC_IMG("image33.png"), 470, 285, 180, 105, "cover", "主场景");
  await addFramedImage(slide, DOC_IMG("image28.png"), 670, 285, 95, 220, "contain", "订单显示");
  await addFramedImage(slide, DOC_IMG("image29.png"), 805, 285, 180, 105, "cover", "装盘");
  await addFramedImage(slide, DOC_IMG("image31.png"), 1005, 285, 205, 115, "cover", "结束界面");
  addText(slide, "该流程体现了“准备 - 执行 - 判定 - 反馈”的完整游戏生命周期。", 145, 590, 990, 42, {
    fontSize: 22,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.lightBlue,
  });
  addFooter(slide, 4);
}

// 5. Tech route
{
  const slide = presentation.slides.add();
  addTitle(slide, "技术路线", "以 Unity 为基础，结合联机框架和数据配置实现核心玩法");
  const tech = [
    ["Unity 2022.3", "负责厨房场景搭建、预制体组织、碰撞检测、动画表现和 UI 界面显示，是系统运行基础。"],
    ["C# 脚本", "实现玩家移动、交互分发、柜台加工、状态切换、订单判定和界面事件响应等主要逻辑。"],
    ["Netcode", "通过 ServerRpc、ClientRpc、NetworkVariable 和 NetworkList 完成连接、对象和关键状态同步。"],
    ["ScriptableObject", "将菜谱、食材、切配、烹饪和烧焦规则配置化，提升内容维护和扩展效率。"],
    ["Host 架构", "由一名玩家同时承担服务器和客户端角色，统一处理准备、场景切换和关键业务判断。"],
  ];
  tech.forEach((t, i) => {
    const top = 135 + i * 92;
    addBox(slide, 95, top, 1030, 68, i % 2 ? C.pale : C.lightBlue, C.line);
    addText(slide, t[0], 125, top + 14, 220, 34, { fontSize: 24, bold: true, color: C.navy, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
    addText(slide, t[1], 365, top + 10, 720, 48, { fontSize: 16, color: C.dark, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  });
  addFooter(slide, 5);
}

// 6. Requirements
{
  const slide = presentation.slides.add();
  addTitle(slide, "需求分析", "围绕完整玩法闭环和多人协作体验展开");
  addBulletCard(slide, "功能需求", [
    "场景流转：系统需要覆盖主菜单、房间、角色准备、加载过渡和游戏主场景等完整进入流程。",
    "玩家操作：玩家能够完成角色移动、柜台选中、拾取放置、切菜烹饪、装盘提交等核心行为。",
    "厨房对象：食材、盘子和柜台之间需要支持归属转移，并能表现切配、加热、烧焦等状态变化。",
    "订单交付：系统根据菜谱生成订单，玩家提交盘子后进行配方比对，并给出成功或失败反馈。",
  ], 70, 130, 500, 440, C.blue);
  addBulletCard(slide, "非功能需求", [
    "易用性：订单、进度、倒计时和交互高亮应保持直观，使玩家能够快速理解当前目标和操作对象。",
    "实时性：加工进度、订单刷新、交付判定和倒计时更新需要及时响应，避免影响协作节奏。",
    "稳定性：空手交互、错误提交、重复装盘等异常情况应被条件判断拦截，避免流程中断。",
    "可扩展性：新增食材、菜谱和加工规则时尽量通过配置资源完成，减少对核心逻辑的侵入。",
  ], 705, 130, 500, 440, C.green);
  addFooter(slide, 6);
}

// 7. Architecture
{
  const slide = presentation.slides.add();
  addTitle(slide, "系统总体架构", "采用表现层、逻辑控制层、网络通信层的分层设计");
  await addFramedImage(slide, DOC_IMG("image4.png"), 80, 145, 710, 400, "contain", "系统架构图");
  addBulletCard(slide, "架构特点", [
    "表现层负责主菜单、订单界面、进度条、倒计时、角色视觉和交互反馈，直接面向玩家体验。",
    "逻辑控制层负责游戏状态推进、玩家输入分发、柜台差异化处理、厨房对象管理和订单判定。",
    "网络通信层负责 Host / Client 连接、准备状态维护、网络场景切换、对象生成销毁和事件广播。",
    "分层后各模块职责更清晰，后续扩展新菜谱、新柜台或新界面时不需要大范围改动原有流程。",
  ], 860, 160, 330, 370, C.blue);
  addFooter(slide, 7);
}

// 8. Modules
{
  const slide = presentation.slides.add();
  addTitle(slide, "核心模块设计", "围绕主流程和核心玩法拆分职责");
  const modules = [
    ["GameManager", "维护等待、倒计时、游戏中和结束状态，并处理时间进度、暂停恢复和界面通知。"],
    ["KitchGameMultiPlayer", "负责 Host / Client 连接、玩家准备状态、统一开局和网络玩家对象生成。"],
    ["Player", "处理移动输入、交互方向记录、柜台检测，并把主交互和辅助交互请求分发出去。"],
    ["BaseCounter 子类", "通过不同柜台子类实现取材、切菜、烹饪、装盘、丢弃和交付等差异化行为。"],
    ["DeliverManager", "维护待完成订单列表，控制订单生成上限，并完成盘中食材与菜谱需求的匹配。"],
    ["UI 模块", "监听逻辑事件并刷新订单、进度条、倒计时、暂停界面和游戏结束结果。"],
  ];
  modules.forEach((m, i) => {
    const left = 75 + (i % 3) * 390;
    const top = 150 + Math.floor(i / 3) * 185;
    addBox(slide, left, top, 340, 130, C.white, i % 2 ? C.orange : C.blue);
    addText(slide, m[0], left + 20, top + 20, 300, 32, { fontSize: 24, bold: true, color: C.navy, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
    addText(slide, m[1], left + 20, top + 56, 300, 62, { fontSize: 14, color: C.dark, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  });
  addText(slide, "模块之间通过事件、接口和网络同步机制协作，避免单一脚本承担过多职责。", 125, 565, 1030, 40, {
    fontSize: 21,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.pale,
  });
  addFooter(slide, 8);
}

// 9. Scene and state
{
  const slide = presentation.slides.add();
  addTitle(slide, "场景流转与游戏状态", "通过统一场景加载和状态机推进游戏生命周期");
  await addFramedImage(slide, DOC_IMG("image11.png"), 65, 145, 565, 250, "contain", "场景流转图");
  await addFramedImage(slide, DOC_IMG("image12.png"), 665, 145, 555, 250, "contain", "状态管理关系图");
  addBulletCard(slide, "实现要点", [
    "Loader 统一封装场景枚举与加载入口，避免不同脚本中重复编写场景名称和加载逻辑。",
    "GameManager 定义等待、倒计时、游戏中和游戏结束四类状态，并在 Update 中持续推进计时流程。",
    "状态变化事件通知 UI 模块，使倒计时、进度条、暂停界面和结束界面能够跟随主流程自动刷新。",
  ], 150, 465, 980, 150, C.blue);
  addFooter(slide, 9);
}

// 10. Player interaction
{
  const slide = presentation.slides.add();
  addTitle(slide, "玩家交互与柜台加工", "玩家只负责请求分发，具体逻辑由柜台子类处理");
  await addFramedImage(slide, DOC_IMG("image13.png"), 55, 140, 600, 250, "contain", "玩家交互机制图");
  await addFramedImage(slide, OUT_IMG("图5-4_厨房对象与柜台处理流程图_简化版.png"), 705, 140, 500, 250, "contain", "厨房对象与柜台处理流程图");
  addBulletCard(slide, "关键实现", [
    "移动输入被转换为三维方向，并通过胶囊体检测判断是否存在障碍；斜向受阻时尝试单轴移动。",
    "系统记录玩家最后一次有效朝向，并基于该方向检测可交互柜台，保证静止时也能继续交互。",
    "切菜台按加工次数推进，炉灶按状态和时间推进，盘子通过食材列表维护最终菜品组合。",
  ], 105, 455, 1040, 150, C.green);
  addFooter(slide, 10);
}

// 11. Delivery
{
  const slide = presentation.slides.add();
  addTitle(slide, "订单生成与出菜结算", "订单驱动玩家完成取材、加工、装盘和交付");
  await addFramedImage(slide, DOC_IMG("image28.png"), 70, 130, 145, 310, "contain", "订单界面");
  await addFramedImage(slide, DOC_IMG("image29.png"), 245, 130, 280, 170, "cover", "装盘界面");
  await addFramedImage(slide, DOC_IMG("image31.png"), 245, 330, 280, 160, "cover", "结束界面");
  await addFramedImage(slide, DOC_IMG("image14.png"), 590, 130, 590, 360, "contain", "订单交付判定流程");
  addText(slide, "闭环：订单生成 → 玩家加工 → 提交盘子 → 配方匹配 → 成功/失败反馈 → 结算显示", 95, 565, 1080, 44, {
    fontSize: 22,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.lightBlue,
  });
  addFooter(slide, 11);
}

// 12. Multiplayer
{
  const slide = presentation.slides.add();
  addTitle(slide, "多人联机与同步", "以 Host 架构和服务器权威保证关键流程一致");
  await addFramedImage(slide, DOC_IMG("image5.png"), 70, 135, 610, 360, "contain", "网络同步设计图");
  addBulletCard(slide, "同步策略", [
    "玩家准备状态由服务器集中记录，只有全部玩家准备完成后，Host 才统一切换到游戏场景。",
    "厨房对象的生成、销毁和归属变化由服务器统一处理，减少客户端本地判断造成的状态分歧。",
    "炉灶当前状态和加工进度使用 NetworkVariable 同步，使各客户端看到一致的烹饪过程。",
    "盘中食材使用 NetworkList 记录，订单生成和交付结果通过 ClientRpc 广播给所有客户端。",
  ], 750, 145, 390, 345, C.blue);
  await addFramedImage(slide, DOC_IMG("image32.png"), 750, 525, 390, 95, "cover", "联机准备界面");
  addFooter(slide, 12);
}

// 13. Running effect
{
  const slide = presentation.slides.add();
  addTitle(slide, "系统运行效果", "界面能够体现从准备、协作加工到结算的完整过程");
  const imgs = [
    ["image17.png", "主菜单"],
    ["image18.png", "角色选择"],
    ["image33.png", "协作场景"],
    ["image35.png", "加工进度"],
    ["image29.png", "装盘效果"],
    ["image31.png", "结果结算"],
  ];
  for (let i = 0; i < imgs.length; i++) {
    const [img, label] = imgs[i];
    const left = 70 + (i % 3) * 390;
    const top = 135 + Math.floor(i / 3) * 230;
    await addFramedImage(slide, DOC_IMG(img), left, top, 330, 150, "cover", label);
    addText(slide, label, left, top + 158, 330, 30, { fontSize: 18, bold: true, color: C.navy, align: "center" });
  }
  addFooter(slide, 13);
}

// 14. Testing
{
  const slide = presentation.slides.add();
  addTitle(slide, "测试结果与分析", "通过功能测试、联机测试和边界测试验证系统可行性");
  const tests = [
    ["场景切换", "验证主菜单、加载过渡、房间准备和游戏主场景之间能按设计流程正常跳转。"],
    ["玩家交互", "验证角色移动、柜台选中、主交互和辅助交互均能响应，并能触发对应柜台逻辑。"],
    ["对象处理", "验证取材、切菜、烹饪、装盘、丢弃等流程可以连续执行，食材状态变化正确。"],
    ["订单交付", "验证订单能持续生成，正确菜品可移除订单并计数，错误提交不会误删订单。"],
    ["联机流程", "验证 Host 创建、Client 加入、准备状态同步、统一开局和关键对象同步可用。"],
    ["边界情况", "验证空手交互、重复装盘、错误交付和游戏结束等边界不会导致流程异常。"],
  ];
  tests.forEach((t, i) => {
    const left = 80 + (i % 2) * 565;
    const top = 135 + Math.floor(i / 2) * 135;
    addBox(slide, left, top, 500, 92, i % 2 ? "#FFF8EA" : C.lightBlue, C.line);
    addText(slide, t[0], left + 20, top + 16, 130, 28, { fontSize: 22, bold: true, color: i % 2 ? "#A86112" : C.navy, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
    addText(slide, t[1], left + 155, top + 10, 315, 62, { fontSize: 14, color: C.dark, insets: { left: 0, right: 0, top: 0, bottom: 0 } });
  });
  addText(slide, "结论：系统能够完成核心玩法闭环和基础多人协作流程，满足本科毕业设计可运行、可演示要求。", 100, 575, 1080, 46, {
    fontSize: 21,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.pale,
  });
  addFooter(slide, 14);
}

// 15. Summary
{
  const slide = presentation.slides.add();
  addTitle(slide, "总结与展望", "完成原型实现，并为后续扩展保留空间");
  addBulletCard(slide, "主要成果", [
    "完成基于 Unity 的多人协作烹饪游戏原型设计与实现，系统能够在本地环境中完整运行。",
    "实现从进入房间、角色准备、协作加工、出菜交付到游戏结算的完整业务流程。",
    "使用 ScriptableObject 管理菜谱、食材和加工规则，使内容配置与逻辑代码相对分离。",
    "使用 Netcode 实现玩家准备、网络对象、炉灶状态、盘子内容和订单结果等关键同步。",
  ], 70, 135, 520, 360, C.blue);
  addBulletCard(slide, "不足与展望", [
    "当前联机实现主要面向小规模基础原型，对高延迟、断线恢复和复杂并发冲突验证不足。",
    "测试方式以人工功能验证为主，后续可以增加自动化测试和更系统的回归测试流程。",
    "玩法内容仍可继续扩展，例如增加更多菜谱、加工设备、地图机制、角色外观和积分统计。",
  ], 690, 135, 520, 360, C.green);
  addText(slide, "未来优化方向：网络一致性提升、测试体系完善、玩法内容扩展。", 130, 565, 1020, 42, {
    fontSize: 23,
    bold: true,
    color: C.navy,
    align: "center",
    fill: C.lightBlue,
  });
  addFooter(slide, 15);
}

// 16. Thanks
{
  const slide = presentation.slides.add();
  slide.background.fill = C.pale;
  addText(slide, "谢谢各位老师", 0, 230, W, 70, { fontSize: 48, bold: true, color: C.navy, align: "center" });
  addText(slide, "恳请批评指正", 0, 315, W, 44, { fontSize: 28, color: C.orange, align: "center", bold: true });
  await addFramedImage(slide, DOC_IMG("image33.png"), 390, 410, 500, 135, "cover", "游戏运行场景");
}

const pptx = await PresentationFile.exportPptx(presentation);
await pptx.save(OUT);

console.log(OUT);
