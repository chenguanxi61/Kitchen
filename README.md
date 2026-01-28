# 🍳 Kitchen

一款基于 **Unity** 开发的厨房协作类游戏 Demo，玩法灵感来源于《胡闹厨房（Overcooked）》。  
项目以 **订单生成、食材组合、送菜判定** 为核心玩法，并在设计之初即为 **多人联机扩展** 预留了完整的架构空间。

---

## 📖 项目简介

在 Kitchen 游戏中，玩家需要在限定时间内，根据随机生成的订单，正确组合食材并完成送菜任务。  
游戏强调 **时间管理、协作逻辑与系统设计**，核心系统采用模块化、事件驱动的方式实现。

---

## 🎮 核心玩法

- 🍽 **订单系统**
  - 随机生成菜谱订单
  - 控制等待中订单的最大数量
- 🥕 **食材组合**
  - 玩家将多个食材放入盘子
  - 盘子内容可动态变化
- ✅ **送菜判定**
  - 对比盘子中的食材与订单菜谱
  - 数量与类型完全匹配才算成功
- ⏱ **时间控制**
  - 使用协程定时生成订单
  - 支持后续扩展超时失败机制

---

## 🧠 技术实现要点

- **Unity 引擎**
- **C#**
- **ScriptableObject**
  - 用于定义菜谱（RecipeSO）与食材（KitchenObjSO）
  - 数据驱动，便于扩展新菜品
- **协程（Coroutine）**
  - 控制订单生成节奏
- **事件 / Action**
  - 解耦核心逻辑与 UI 显示
- **模块化 Manager 设计**
  - DeliverManager：订单生成与送菜判定
  - Plate / Player / UI 各自职责清晰

---

## 📂 项目结构
Kitchen/
├── Assets/
│ ├── Scenes/ # 游戏场景
│ ├── Scripts/ # 核心逻辑脚本
│ │ ├── DeliverManager.cs # 订单生成与送菜系统
│ │ ├── PlateIconUI.cs # 订单 UI 显示
│ │ └── ...
│ ├── ScriptableObjects/ # 菜谱 / 食材数据
│ └── Prefabs/ # 游戏预制体
├── Packages/
├── ProjectSettings/
└── README.md
## 🚀 运行方式

1. 克隆仓库：
   ```bash
   git clone https://github.com/chenguanxi61/Kitchen.git
   使用 Unity Hub 打开项目（建议 Unity 2021 及以上版本）
   打开主场景（Scenes 文件夹）运行即可（注意拖动相应SO到脚本里）
## 🙏 致谢

本项目主要参考了CODEMONKEY相关视频以及B站UP雪原疾行的视频及 Unity 社区相关教学内容，
用于学习与研究目的。  
