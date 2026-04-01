namespace PlaneLedger.Data.Models
{
    /// <summary>装饰品/卡片稀有度</summary>
    public enum Rarity
    {
        Common = 0,     // 普通
        Rare = 1,       // 稀有
        Epic = 2,       // 史诗
        Legendary = 3   // 传说
    }

    /// <summary>装饰品风格</summary>
    public enum DecorationStyle
    {
        Default = 0,        // 默认
        Cthulhu = 1,        // 克苏鲁
        ChineseClassical = 2, // 古风
        Modern = 3,         // 现代
        Futuristic = 4,     // 未来
        Fantasy = 5         // 奇幻
    }

    /// <summary>装饰品槽位</summary>
    public enum DecorationSlot
    {
        Book = 0,           // 账本 SLOT-01
        Curtain = 1,        // 窗帘 SLOT-02
        WindowView = 2,     // 窗外景色 SLOT-03
        Carpet = 3,         // 地毯 SLOT-04
        Lamp = 4,           // 灯具 SLOT-05
        Furniture = 5,      // 桌椅家具 SLOT-06
        Ambient = 6         // 氛围特效 SLOT-07
    }

    /// <summary>思想残章阵营</summary>
    public enum CardFaction
    {
        Wealth = 0,         // 财富增长流派（搞钱位面）
        Health = 1          // 生命进化流派（健康位面）
    }

    /// <summary>日常消费大类</summary>
    public enum ExpenseCategory
    {
        EnergyIntake,       // 能量摄入（饮食）
        SpatialShift,       // 空间位移（短途出行）
        ImpulseBuy,         // 即时物欲（小额随机）
        HabitConsume,       // 习惯消耗（嗜好品）
        RandomEntertainment // 随机娱乐
    }

    /// <summary>成就分类</summary>
    public enum AchievementCategory
    {
        Login,              // 登录/启动
        Accounting,         // 记账行为
        Financial,          // 财务目标
        Restraint,          // 克制消费
        Collection,         // 收集行为
        Exploration,        // 探索功能
        Hidden              // 隐藏成就
    }

    /// <summary>成就检测时机</summary>
    public enum AchievementCheckTiming
    {
        Realtime,           // 实时检测（操作完成时）
        OnSettlement,       // 结算时检测
        OnMonthEnd          // 月末检测
    }

    /// <summary>收入类型</summary>
    public enum IncomeType
    {
        Active,             // 劳动收入（工资/兼职）
        Passive,            // 资产收益（利息/分红/租金）
        Investment          // 投资收益（股票/基金/黄金）
    }
}
