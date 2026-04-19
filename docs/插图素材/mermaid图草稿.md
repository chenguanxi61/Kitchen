# Mermaid 图草稿

下面这些 Mermaid 草稿可以直接复制到支持 Mermaid 的网站或编辑器中继续调整。

---

## 图2-1 系统业务流程图

```mermaid
flowchart LR
    A[进入主菜单] --> B[创建房间或加入房间]
    B --> C[玩家准备完成]
    C --> D[加载游戏场景]
    D --> E[系统生成订单]
    E --> F[玩家取材与加工]
    F --> G[装盘提交]
    G --> H{订单匹配判定}
    H -- 成功 --> I[移除订单并记录成功数量]
    I --> E
    H -- 失败 --> J[反馈失败并继续加工]
    J --> F
    E --> K[倒计时结束]
    K --> L[游戏结束]
```

---

## 图3-1 系统总体架构图

```mermaid
flowchart TD
    subgraph A[表现层]
        A1[主菜单UI]
        A2[订单UI]
        A3[进度条UI]
        A4[倒计时UI]
        A5[暂停界面]
        A6[结束界面]
        A7[动画与音效反馈]
    end

    subgraph B[逻辑控制层]
        B1[GameManager]
        B2[DeliverManager]
        B3[Player]
        B4[KitchenObj]
        B5[PlateKitchObj]
        B6[Counter系列]
    end

    subgraph C[网络通信层]
        C1[Netcode for GameObjects]
        C2[ServerRpc]
        C3[ClientRpc]
        C4[NetworkVariable]
        C5[NetworkList]
        C6[NetworkManager]
    end

    A --> B
    B --> C
```

---

## 图3-2 系统功能模块图

```mermaid
flowchart LR
    S[多人协作烹饪游戏系统] --> M1[游戏状态管理模块]
    S --> M2[联机管理模块]
    S --> M3[玩家交互模块]
    S --> M4[柜台加工模块]
    S --> M5[订单与交付模块]
    S --> M6[UI显示模块]

    M1 --> M1A[等待开始]
    M1 --> M1B[倒计时]
    M1 --> M1C[游戏进行]
    M1 --> M1D[游戏结束]

    M2 --> M2A[建房]
    M2 --> M2B[加入]
    M2 --> M2C[准备同步]
    M2 --> M2D[场景切换]

    M3 --> M3A[移动]
    M3 --> M3B[目标选中]
    M3 --> M3C[主交互]
    M3 --> M3D[辅助交互]

    M4 --> M4A[取材]
    M4 --> M4B[切菜]
    M4 --> M4C[烹饪]
    M4 --> M4D[装盘]

    M5 --> M5A[生成订单]
    M5 --> M5B[维护订单列表]
    M5 --> M5C[匹配判定]
    M5 --> M5D[结果反馈]

    M6 --> M6A[订单显示]
    M6 --> M6B[进度显示]
    M6 --> M6C[倒计时显示]
    M6 --> M6D[暂停与结束]
```

---

## 图3-3 数据驱动配置关系图

```mermaid
flowchart LR
    R[RecipeSO]
    K[KitchenObjSO]
    C[CuttingRecipeSO]
    F[FryRecipeSO]
    B[BurningRecipeSO]

    R --> K
    C --> K
    F --> K
    B --> K
    C --> C2[切配输入输出关系]
    F --> F2[加热输入输出关系]
    B --> B2[烧焦输入输出关系]
```

---

## 图3-4 玩家准备与场景切换时序图

```mermaid
sequenceDiagram
    participant Client
    participant Host
    participant NetworkManager

    Client->>Host: 发送准备请求
    Host->>Host: 记录准备状态
    Host->>Host: 检查所有玩家是否准备完成
    Host->>NetworkManager: 触发网络场景加载
    NetworkManager-->>Client: 同步切换到游戏场景
    Host->>NetworkManager: 生成玩家对象
    NetworkManager-->>Client: 同步玩家对象
```

---

## 图4-1 系统场景流转图

```mermaid
flowchart LR
    A[MainMenu] --> B[LobbyScene]
    B --> C[CharacterSelectScene]
    A --> D[LoadingScene]
    D --> B
    C --> E[GameScene]
    B -. Host建房 .-> C
    C -. 所有玩家准备完成 .-> E
```

---

## 图4-2 食材加工流程图

```mermaid
flowchart LR
    A[容器柜台取材] --> B[切菜台加工]
    B --> C[炉灶加热]
    C --> D[装入盘子]
    D --> E[提交到交付柜台]
    E --> F[订单匹配判定]
```

---

## 图4-3 订单生成与交付判定流程图

```mermaid
flowchart LR
    A[系统生成订单] --> B[加入待完成订单列表]
    B --> C[玩家处理食材]
    C --> D[盘子提交]
    D --> E[系统遍历订单匹配]
    E --> F{是否匹配成功}
    F -- 是 --> G[移除订单]
    G --> H[更新成功数量]
    H --> B
    F -- 否 --> I[反馈失败]
    I --> C
```
