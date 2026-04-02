---
name: 位面账簿项目概况
description: Unity游戏化记账工具"位面账簿(The Plane Ledger)"的项目状态、架构和进度
type: project
---

# 位面账簿 (The Plane Ledger)

Steam平台游戏化记账工具，买断制6元。通过装饰收集、卡牌掉落、成就系统驱动用户养成记账习惯。

## 技术栈
- Unity 2021.3.x LTS, 2D项目, 内置渲染管线
- uGUI + TextMeshPro + Newtonsoft.Json
- JSON文件存档 + Steam Auto-Cloud
- Steamworks.NET（待导入）

## GitHub仓库
https://github.com/361311733Raphaelia/PlaneLedger

## 项目结构
代码骨架已全部完成（41个文件: 30个C#脚本 + 11个asmdef），位于 `D:\PlaneLedger\Assets\_Project\Scripts\`

核心模块：
- Core: ServiceLocator, EventBus, Events, SaveSystem, GameManager
- Data: SaveData模型, Enums, CategoryConfig, DecorationSO/WisdomCardSO/AchievementSO
- Currency: TokenManager
- Accounting: DailyMatrixService, MonthlyOverviewService, AmortizationService
- Collection: DecorationSystem, WisdomCardSystem, DropSystem
- Achievement: AchievementManager, AchievementTracker (覆盖164个成就)
- PlaneSpace: PlaneSpaceController, DecorationSlotView, SceneInteractionPoint
- UI: PanelManager, BasePanel, HUDController, ToastNotification
- Steam: SteamInitializer, SteamAchievementBridge
- Utils: DateUtils

## 当前进度
- ✅ 代码骨架全部完成
- ✅ 搭建指南已写好（D:\PlaneLedger\搭建指南_新手版.md）
- ✅ GitHub仓库已创建并推送
- ⬜ 用户尚未在Unity中创建项目和搭建场景
- ⬜ 记账面板UI（日常消耗矩阵）未开始
- ⬜ 装饰商店/成就列表等面板UI未开始
- ⬜ 美术资源未开始
- ⬜ Steamworks.NET未导入

## 策划文档位置
- 策划案: C:\Users\chenchen_f\Downloads\位面账簿_策划案_v0.1.md
- 装饰品规范: C:\Users\chenchen_f\Downloads\位面账簿_装饰品系统规范.md
- 残章卡片列表: C:\Users\chenchen_f\Downloads\位面账簿_残章卡片列表.md
