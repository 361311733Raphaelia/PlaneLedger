using System;

namespace PlaneLedger.Core
{
    // ============================================================
    // 所有 EventBus 事件定义
    // 使用 struct 避免 GC，每个事件携带必要的上下文数据
    // ============================================================

    #region 记账相关事件

    /// <summary>新增一笔日常消费记录</summary>
    public struct DailyEntryAdded
    {
        public DateTime Date;
        public string Category;
        public string SubCategory;
        public float Amount;
    }

    /// <summary>删除一笔日常消费记录</summary>
    public struct DailyEntryRemoved
    {
        public DateTime Date;
        public string EntryId;
    }

    /// <summary>每日结算完成（触发掉落、成就检测等）</summary>
    public struct DailySettlementCompleted
    {
        public DateTime Date;
        public float TotalExpenseToday;
        public int TotalEntriesCount;
        public bool HasImpulseBuy;       // 是否有即时物欲支出
        public bool HasHabitExpense;      // 是否有习惯消耗支出
        public bool HasTransportExpense;  // 是否有出行支出
    }

    /// <summary>月度收入/大额支出更新</summary>
    public struct MonthlyDataUpdated
    {
        public int Year;
        public int Month;
        public float TotalActiveIncome;
        public float TotalPassiveIncome;
        public float TotalLargeExpense;
    }

    /// <summary>年度平摊项变更</summary>
    public struct AmortizationChanged
    {
        public string ItemId;
        public float MonthlyAmount;
    }

    #endregion

    #region 收集相关事件

    /// <summary>解锁一件装饰品</summary>
    public struct DecorationUnlocked
    {
        public string DecorationId;
        public int Rarity; // 0=普通, 1=稀有, 2=史诗, 3=传说
    }

    /// <summary>装备一件装饰品到槽位</summary>
    public struct DecorationEquipped
    {
        public int SlotIndex;    // 0~6
        public string DecorationId;
        public string PreviousDecorationId;
    }

    /// <summary>解锁一张思想残章卡片</summary>
    public struct WisdomCardUnlocked
    {
        public string CardId;
        public int Rarity;
    }

    /// <summary>掉落结果（登录掉落或结算掉落）</summary>
    public struct DropResult
    {
        public string ItemId;        // 装饰ID 或 卡片ID
        public bool IsDecoration;    // true=装饰, false=卡片
        public int Rarity;
        public bool IsDuplicate;     // 是否重复（转为代币）
        public int TokenCompensation; // 重复时转换的代币数量
    }

    #endregion

    #region 成就相关事件

    /// <summary>成就解锁</summary>
    public struct AchievementUnlocked
    {
        public string AchievementId;
        public string AchievementName;
        public int TokenReward;
        public string DecorationRewardId; // 可为 null
    }

    /// <summary>成就进度更新</summary>
    public struct AchievementProgressUpdated
    {
        public string AchievementId;
        public int CurrentValue;
        public int TargetValue;
    }

    #endregion

    #region 货币相关事件

    /// <summary>代币获得</summary>
    public struct TokensEarned
    {
        public int Amount;
        public string Source; // "settlement", "achievement", "duplicate" 等
    }

    /// <summary>代币消费</summary>
    public struct TokensSpent
    {
        public int Amount;
        public string Purpose; // "buy_decoration", "buy_card", "refresh_pool" 等
    }

    #endregion

    #region 系统事件

    /// <summary>日期变更（跨日检测触发）</summary>
    public struct DayChanged
    {
        public DateTime PreviousDate;
        public DateTime NewDate;
    }

    /// <summary>请求存档</summary>
    public struct SaveRequested { }

    /// <summary>用户登录（每日首次打开）</summary>
    public struct DailyLoginTriggered
    {
        public DateTime Date;
        public int ConsecutiveLoginDays;
        public int TotalLoginDays;
    }

    #endregion
}
