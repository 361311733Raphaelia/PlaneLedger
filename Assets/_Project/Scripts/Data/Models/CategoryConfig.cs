using System.Collections.Generic;

namespace PlaneLedger.Data.Models
{
    /// <summary>
    /// 日常消费分类的静态配置。
    /// 按策划案定义的大类-子类体系。
    /// </summary>
    public static class CategoryConfig
    {
        public static readonly Dictionary<string, string[]> Categories = new Dictionary<string, string[]>
        {
            {
                "EnergyIntake", new[] { "Breakfast", "Lunch", "Dinner", "Snack" }
            },
            {
                "SpatialShift", new[] { "PublicTransport", "Taxi" }
            },
            {
                "ImpulseBuy", new[] { "OnlineShopping", "OfflineShopping" }
            },
            {
                "HabitConsume", new[] { "HabitItem" }
            },
            {
                "RandomEntertainment", new[] { "TicketOrGaming", "PersonalCare" }
            }
        };

        public static readonly Dictionary<string, string> CategoryDisplayNames = new Dictionary<string, string>
        {
            { "EnergyIntake", "能量摄入（饮食）" },
            { "SpatialShift", "空间位移（出行）" },
            { "ImpulseBuy", "即时物欲" },
            { "HabitConsume", "习惯消耗" },
            { "RandomEntertainment", "随机娱乐" }
        };

        public static readonly Dictionary<string, string> SubCategoryDisplayNames = new Dictionary<string, string>
        {
            { "Breakfast", "早餐" },
            { "Lunch", "午餐" },
            { "Dinner", "晚餐" },
            { "Snack", "零食/饮料/下午茶" },
            { "PublicTransport", "公交/地铁/单车" },
            { "Taxi", "打车/网约车" },
            { "OnlineShopping", "线上购物" },
            { "OfflineShopping", "线下购物" },
            { "HabitItem", "烟/酒/咖啡等" },
            { "TicketOrGaming", "电影/门票/氪金" },
            { "PersonalCare", "理发/护理/按摩" }
        };

        public static readonly Dictionary<string, string> LargeExpenseCategories = new Dictionary<string, string>
        {
            { "Rent", "房租/房贷" },
            { "CarLoan", "车贷" },
            { "PropertyFee", "物业取暖费" },
            { "Fuel", "燃油" },
            { "Insurance", "保险" },
            { "Installment", "分期还款" },
            { "Utility", "水电网费" },
            { "Cleaning", "保洁" }
        };
    }
}
