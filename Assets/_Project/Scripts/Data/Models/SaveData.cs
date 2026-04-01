using System;
using System.Collections.Generic;

namespace PlaneLedger.Data.Models
{
    /// <summary>
    /// 存档根对象，包含所有需要持久化的玩家数据。
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>存档版本号，用于数据迁移</summary>
        public int Version = 1;

        /// <summary>上次保存时间</summary>
        public string LastSaveTime;

        public PlayerProfile Profile = new PlayerProfile();
        public AccountingData Accounting = new AccountingData();
        public CollectionData Collection = new CollectionData();
        public AchievementData Achievements = new AchievementData();
        public SettlementData Settlement = new SettlementData();
        public SettingsData Settings = new SettingsData();
    }

    [Serializable]
    public class PlayerProfile
    {
        /// <summary>首次注册日期（用于"生日快乐"成就）</summary>
        public string RegisterDate;

        /// <summary>总存款金额（用户手动录入的账户余额）</summary>
        public float TotalSavings;

        /// <summary>月度预算上限</summary>
        public float MonthlyBudget;

        /// <summary>各分类专项预算 (category -> amount)</summary>
        public Dictionary<string, float> CategoryBudgets = new Dictionary<string, float>();
    }

    #region 记账数据

    [Serializable]
    public class AccountingData
    {
        /// <summary>日常消费条目，按日期字符串索引 (yyyy-MM-dd -> entries)</summary>
        public Dictionary<string, List<DailyEntry>> DailyEntries = new Dictionary<string, List<DailyEntry>>();

        /// <summary>月度收入数据，按月份索引 (yyyy-MM -> income)</summary>
        public Dictionary<string, MonthlyIncome> MonthlyIncomes = new Dictionary<string, MonthlyIncome>();

        /// <summary>月度大额支出，按月份索引 (yyyy-MM -> expenses)</summary>
        public Dictionary<string, List<LargeExpense>> MonthlyLargeExpenses = new Dictionary<string, List<LargeExpense>>();

        /// <summary>年度平摊项目列表</summary>
        public List<AmortizedExpense> AmortizedExpenses = new List<AmortizedExpense>();

        /// <summary>自定义分类列表</summary>
        public List<string> CustomCategories = new List<string>();

        /// <summary>已使用过的分类（用于成就检测）</summary>
        public HashSet<string> UsedCategories = new HashSet<string>();
    }

    [Serializable]
    public class DailyEntry
    {
        public string Id;           // GUID
        public string Category;     // 大类：EnergyIntake, SpatialShift, ImpulseBuy, HabitConsume, RandomEntertainment
        public string SubCategory;  // 子类：Breakfast, Lunch, Dinner, Snack, Bus, Taxi...
        public float Amount;
        public string Note;
        public string Time;         // HH:mm 记录时间
    }

    [Serializable]
    public class MonthlyIncome
    {
        /// <summary>劳动收入（工资/兼职）</summary>
        public float ActiveIncome;
        /// <summary>资产收益（利息/分红/租金）</summary>
        public float PassiveIncome;
        /// <summary>投资收益（股票/基金/黄金）</summary>
        public float InvestmentIncome;

        /// <summary>各项收入明细</summary>
        public List<IncomeItem> IncomeItems = new List<IncomeItem>();
    }

    [Serializable]
    public class IncomeItem
    {
        public string Id;
        public string Type;        // Active, Passive, Investment
        public string Description;
        public float Amount;
    }

    [Serializable]
    public class LargeExpense
    {
        public string Id;
        public string Category;    // Rent, CarLoan, Utility, Insurance, Installment...
        public string Description;
        public float Amount;
    }

    [Serializable]
    public class AmortizedExpense
    {
        public string Id;
        public string Name;
        public float TotalAmount;
        public string StartMonth;  // yyyy-MM
        public int SpreadMonths;
        public float MonthlyAmount; // TotalAmount / SpreadMonths
    }

    #endregion

    #region 收集数据

    [Serializable]
    public class CollectionData
    {
        /// <summary>已解锁的装饰品ID集合</summary>
        public HashSet<string> UnlockedDecorationIds = new HashSet<string>();

        /// <summary>已解锁的残章卡片ID集合</summary>
        public HashSet<string> UnlockedCardIds = new HashSet<string>();

        /// <summary>当前装备的装饰品 (slotIndex -> decorationId)</summary>
        public Dictionary<int, string> EquippedDecorations = new Dictionary<int, string>();

        /// <summary>装饰品保底计数器（连续未出稀有+的次数）</summary>
        public int DecorationPityCounter;

        /// <summary>卡片保底计数器</summary>
        public int CardPityCounter;

        /// <summary>装饰更换次数（用于成就：换了三次账本）</summary>
        public Dictionary<int, int> SlotChangeCount = new Dictionary<int, int>();
    }

    #endregion

    #region 成就数据

    [Serializable]
    public class AchievementData
    {
        /// <summary>已解锁的成就ID集合</summary>
        public HashSet<string> UnlockedIds = new HashSet<string>();

        /// <summary>成就进度 (achievementId -> currentValue)</summary>
        public Dictionary<string, int> Progress = new Dictionary<string, int>();

        /// <summary>连续天数追踪 (trackerId -> consecutiveDays)</summary>
        public Dictionary<string, int> ConsecutiveStreaks = new Dictionary<string, int>();

        /// <summary>连续天数追踪的最后日期 (trackerId -> lastDate yyyy-MM-dd)</summary>
        public Dictionary<string, string> StreakLastDates = new Dictionary<string, string>();

        /// <summary>每月独立数据 (yyyy-MM -> monthly achievement data)</summary>
        public Dictionary<string, MonthlyAchievementData> MonthlyData = new Dictionary<string, MonthlyAchievementData>();
    }

    [Serializable]
    public class MonthlyAchievementData
    {
        public bool BudgetMet;              // 当月是否未超预算
        public float TotalImpulseBuy;       // 当月即时物欲总额
        public int ImpulseBuyCount;         // 当月即时物欲笔数
        public float TotalHabitExpense;     // 当月习惯消耗总额
    }

    #endregion

    #region 结算数据

    [Serializable]
    public class SettlementData
    {
        /// <summary>上次登录日期 (yyyy-MM-dd)</summary>
        public string LastLoginDate;

        /// <summary>上次结算日期 (yyyy-MM-dd)</summary>
        public string LastSettlementDate;

        /// <summary>连续登录天数</summary>
        public int ConsecutiveLoginDays;

        /// <summary>累计登录天数</summary>
        public int TotalLoginDays;

        /// <summary>连续记账天数</summary>
        public int ConsecutiveAccountingDays;

        /// <summary>累计记账笔数</summary>
        public int TotalEntryCount;

        /// <summary>代币余额</summary>
        public int TokenBalance;

        /// <summary>今日是否已领取登录奖励</summary>
        public bool TodayLoginRewardClaimed;

        /// <summary>今日是否已完成结算</summary>
        public bool TodaySettlementDone;

        /// <summary>连续天数无即时物欲支出</summary>
        public int ConsecutiveNoImpulseDays;

        /// <summary>连续天数无习惯消耗支出</summary>
        public int ConsecutiveNoHabitDays;

        /// <summary>连续天数无出行支出</summary>
        public int ConsecutiveNoTransportDays;

        /// <summary>连续天数22:00前完成结算</summary>
        public int ConsecutiveEarlySettlementDays;

        /// <summary>连续天数每笔有备注</summary>
        public int ConsecutiveNoteDays;

        /// <summary>当日是否所有支出仅含饮食</summary>
        public bool TodayOnlyFood;

        /// <summary>掉落池刷新次数</summary>
        public int PoolRefreshCount;
    }

    #endregion

    #region 设置数据

    [Serializable]
    public class SettingsData
    {
        public float MasterVolume = 1f;
        public float BGMVolume = 0.7f;
        public float SFXVolume = 1f;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public bool IsFullscreen = true;
    }

    #endregion
}
