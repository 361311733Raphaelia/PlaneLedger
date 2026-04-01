using System;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Data.SO
{
    /// <summary>
    /// 成就配置数据。每个成就一个 SO 实例。
    /// 路径: Assets/_Project/ScriptableObjects/Achievements/
    /// </summary>
    [CreateAssetMenu(fileName = "Achievement_", menuName = "PlaneLedger/Achievement")]
    public class AchievementSO : ScriptableObject
    {
        [Header("基础信息")]
        public string Id;               // 如 A-001, B-002
        public string SteamApiName;     // Steam 成就 API 名称
        public string DisplayName;      // 成就名
        [TextArea(1, 3)]
        public string Description;      // 达成条件描述
        public Sprite Icon;

        [Header("分类")]
        public AchievementCategory Category;
        public AchievementCheckTiming CheckTiming;
        public bool IsHidden;           // 隐藏成就（未达成前显示 ???）

        [Header("达成条件")]
        public AchievementConditionType ConditionType;
        public int TargetValue = 1;     // 目标值（累计类/连续类）

        [Header("奖励")]
        public int TokenReward;
        public string DecorationRewardId; // 奖励装饰品ID，空=无

        [Header("元数据")]
        public string Version = "1.0";
    }

    public enum AchievementConditionType
    {
        // 登录类
        TotalLoginDays,             // 累计登录天数
        ConsecutiveLoginDays,       // 连续登录天数
        LoginAtTimeRange,           // 特定时间段登录
        LoginOnDate,                // 特定日期登录

        // 记账类
        TotalEntryCount,            // 累计记账笔数
        ConsecutiveAccountingDays,  // 连续记账天数
        UseAllModulesInOneDay,      // 同一天使用全部3个模块
        FirstUseModule,             // 首次使用某模块
        EntryAtTimeRange,           // 特定时间段记账
        EntryOnDate,                // 特定日期记账
        EntryInCategory,            // 录入特定分类支出
        ConsecutiveNoteDays,        // 连续N天每笔有备注
        CreateMultipleAccounts,     // 创建多个账户

        // 财务类
        SetBudget,                  // 设置预算
        BudgetMetMonths,            // 连续N月不超预算
        MonthlySavings,             // 月净存款达标
        TotalSavings,               // 账户总余额达标
        PassiveIncomeRatio,         // 被动收入占比达标
        CategoryReduction,          // 某分类支出比上月减少
        SurvivalDays,               // 生存天数达标
        FirstPassiveIncome,         // 首笔被动收入
        YearFirstSaving,            // 年度首存

        // 克制消费类
        ConsecutiveNoImpulseDays,   // 连续N天无即时物欲
        ConsecutiveNoHabitDays,     // 连续N天无习惯消耗
        MonthlyImpulseUnder,        // 当月即时物欲+习惯低于阈值
        OnlyFoodToday,              // 当日仅含饮食支出
        NoTransportToday,           // 当日无出行支出
        ZeroExpenseDay,             // 当日支出为零
        LowExpenseDay,              // 当日总支出低于阈值
        MonthlyZeroImpulseCount,    // 当月即时物欲笔数为0

        // 收集类
        TotalCardsCollected,        // 累计收集残章数
        FactionCardsCollected,      // 特定阵营收集数
        AllCardsCollected,          // 全部残章收集完
        TotalDecorationsUnlocked,   // 累计装饰解锁数
        StyleDecorationsComplete,   // 某风格全部解锁
        AllStylesComplete,          // 全部风格系列解锁
        SlotChangeCount,            // 更换装饰次数

        // 探索功能类
        CreateCustomCategory,       // 创建自定义分类
        UseDifferentCategories,     // 使用N个不同分类
        ViewReport,                 // 查看报告
        ConsecutiveViewStats,       // 连续查看统计
        AmortizationItems,          // 平摊项目数
        DifferentIncomeTypes,       // 不同收入类型数
        FirstDecorationChange,      // 首次更换装饰
        RefreshPool,                // 首次刷新掉落池

        // 隐藏/特殊
        PerfectDay,                 // 完美的一天（记账+无物欲+在预算内）
        AllAchievementsComplete,    // 全部成就达成
        ConsecutiveEarlySettlement, // 连续22:00前结算
    }
}
