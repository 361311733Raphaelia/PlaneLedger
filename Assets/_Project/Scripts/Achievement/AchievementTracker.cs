using System;
using PlaneLedger.Collection;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Achievement
{
    /// <summary>
    /// 成就追踪器。监听 EventBus 事件，自动判定成就条件并触发解锁。
    /// 在 GameManager 初始化时创建并注册。
    /// </summary>
    public class AchievementTracker
    {
        private readonly AchievementManager _manager;
        private readonly SaveData _saveData;
        private readonly DecorationSystem _decorationSystem;
        private readonly WisdomCardSystem _cardSystem;

        public AchievementTracker(AchievementManager manager, SaveData saveData,
            DecorationSystem decorationSystem, WisdomCardSystem cardSystem)
        {
            _manager = manager;
            _saveData = saveData;
            _decorationSystem = decorationSystem;
            _cardSystem = cardSystem;

            SubscribeEvents();
        }

        public void Dispose()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<DailyLoginTriggered>(OnDailyLogin);
            EventBus.Subscribe<DailyEntryAdded>(OnEntryAdded);
            EventBus.Subscribe<DailySettlementCompleted>(OnSettlement);
            EventBus.Subscribe<DecorationUnlocked>(OnDecorationUnlocked);
            EventBus.Subscribe<DecorationEquipped>(OnDecorationEquipped);
            EventBus.Subscribe<WisdomCardUnlocked>(OnCardUnlocked);
            EventBus.Subscribe<AchievementUnlocked>(OnAchievementUnlocked);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<DailyLoginTriggered>(OnDailyLogin);
            EventBus.Unsubscribe<DailyEntryAdded>(OnEntryAdded);
            EventBus.Unsubscribe<DailySettlementCompleted>(OnSettlement);
            EventBus.Unsubscribe<DecorationUnlocked>(OnDecorationUnlocked);
            EventBus.Unsubscribe<DecorationEquipped>(OnDecorationEquipped);
            EventBus.Unsubscribe<WisdomCardUnlocked>(OnCardUnlocked);
            EventBus.Unsubscribe<AchievementUnlocked>(OnAchievementUnlocked);
        }

        #region 登录成就

        private void OnDailyLogin(DailyLoginTriggered e)
        {
            // 累计登录天数
            CheckThreshold("A-001", e.TotalLoginDays, 1);    // 位面初醒
            CheckThreshold("A-013", e.TotalLoginDays, 2);    // 二度归来
            CheckThreshold("A-014", e.TotalLoginDays, 5);    // 第五天
            CheckThreshold("A-002", e.TotalLoginDays, 7);    // 归来者
            CheckThreshold("A-015", e.TotalLoginDays, 15);   // 半月常客
            CheckThreshold("A-003", e.TotalLoginDays, 30);   // 常驻居民
            CheckThreshold("A-016", e.TotalLoginDays, 60);   // 两月驻守
            CheckThreshold("A-004", e.TotalLoginDays, 100);  // 位面支柱
            CheckThreshold("A-017", e.TotalLoginDays, 180);  // 半年印记
            CheckThreshold("A-005", e.TotalLoginDays, 365);  // 永恒驻守

            // 连续登录天数
            CheckThreshold("A-006", e.ConsecutiveLoginDays, 3);   // 晨光仪式
            CheckThreshold("A-018", e.ConsecutiveLoginDays, 5);   // 连续五日
            CheckThreshold("A-007", e.ConsecutiveLoginDays, 7);   // 七日之约
            CheckThreshold("A-019", e.ConsecutiveLoginDays, 14);  // 连续半月
            CheckThreshold("A-008", e.ConsecutiveLoginDays, 30);  // 月之守护
            CheckThreshold("A-020", e.ConsecutiveLoginDays, 60);  // 连续两月
            CheckThreshold("A-009", e.ConsecutiveLoginDays, 100); // 黎明使者

            // 时间段登录
            int hour = DateTime.Now.Hour;
            if (hour >= 23) _manager.TryUnlock("A-010");          // 深夜启示
            if (hour < 6) _manager.TryUnlock("A-011");            // 晨型生物

            // 特殊日期
            var now = DateTime.Now;
            if (now.Month == 12 && now.Day == 31 && hour >= 23)
                _manager.TryUnlock("A-012"); // 跨年守夜

            // 生日快乐（隐藏）
            if (!string.IsNullOrEmpty(_saveData.Profile.RegisterDate))
            {
                var regDate = DateTime.Parse(_saveData.Profile.RegisterDate);
                if (now.Month == regDate.Month && now.Day == regDate.Day && now.Year > regDate.Year)
                    _manager.TryUnlock("G-003");
            }
        }

        #endregion

        #region 记账成就

        private void OnEntryAdded(DailyEntryAdded e)
        {
            var settlement = _saveData.Settlement;
            int totalEntries = settlement.TotalEntryCount + 1; // +1因为Settlement尚未更新

            // 累计记账笔数
            CheckThreshold("B-001", totalEntries, 1);     // 最初的记录
            CheckThreshold("B-021", totalEntries, 3);
            CheckThreshold("B-002", totalEntries, 10);
            CheckThreshold("B-022", totalEntries, 20);
            CheckThreshold("B-023", totalEntries, 50);
            CheckThreshold("B-003", totalEntries, 100);
            CheckThreshold("B-024", totalEntries, 200);
            CheckThreshold("B-004", totalEntries, 500);
            CheckThreshold("B-025", totalEntries, 1000);
            CheckThreshold("B-005", totalEntries, 1000);
            CheckThreshold("B-026", totalEntries, 2000);

            // 特定分类
            if (e.SubCategory == "Breakfast") _manager.TryUnlock("B-033");    // 今天记了早饭
            if (e.SubCategory == "Taxi") _manager.TryUnlock("B-034");         // 打车了
            if (e.SubCategory == "OnlineShopping") _manager.TryUnlock("B-035"); // 网购到了
            if (e.Category == "RandomEntertainment") _manager.TryUnlock("B-036"); // 今天看电影了

            // 时间段记账
            int hour = DateTime.Now.Hour;
            if (hour < 7) _manager.TryUnlock("B-016");    // 早起记账
            if (hour >= 12 && hour < 13) _manager.TryUnlock("B-017"); // 午间盘点
            if (hour >= 1 && hour < 3) _manager.TryUnlock("B-018");   // 深夜核算

            // 特殊日期
            if (DateTime.Now.Month == 1 && DateTime.Now.Day == 1)
                _manager.TryUnlock("B-019"); // 元旦启账

            // 凌晨记账（隐藏）
            if (hour >= 0 && hour < 3)
            {
                _manager.TryUnlock("G-001"); // 凌晨的账簿
                if (e.Category == "HabitConsume")
                    _manager.TryUnlock("G-002"); // 午夜嗜好者
            }

            // 收入类
            if (e.Category == "Active")
            {
                _manager.TryUnlock("G-006"); // 第一笔工资
                _manager.TryUnlock("B-037"); // 加班费入账
            }
        }

        #endregion

        #region 结算成就

        private void OnSettlement(DailySettlementCompleted e)
        {
            var settlement = _saveData.Settlement;

            // 连续记账天数
            int consecutive = settlement.ConsecutiveAccountingDays;
            CheckThreshold("B-027", consecutive, 2);
            CheckThreshold("B-006", consecutive, 3);
            CheckThreshold("B-028", consecutive, 5);
            CheckThreshold("B-007", consecutive, 7);
            CheckThreshold("B-029", consecutive, 10);
            CheckThreshold("B-008", consecutive, 14);
            CheckThreshold("B-030", consecutive, 20);
            CheckThreshold("B-009", consecutive, 30);
            CheckThreshold("B-031", consecutive, 60);
            CheckThreshold("B-010", consecutive, 100);
            CheckThreshold("B-032", consecutive, 180);
            CheckThreshold("B-011", consecutive, 365);

            // 克制消费
            if (!e.HasImpulseBuy)
            {
                _manager.TryUnlock("D-001");  // 理性闪现
                _manager.TryUnlock("D-017");  // 今天少买一件
                CheckThreshold("D-002", settlement.ConsecutiveNoImpulseDays, 3);
                CheckThreshold("D-013", settlement.ConsecutiveNoImpulseDays, 5);
                CheckThreshold("D-003", settlement.ConsecutiveNoImpulseDays, 7);
                CheckThreshold("D-014", settlement.ConsecutiveNoImpulseDays, 14);
                CheckThreshold("D-004", settlement.ConsecutiveNoImpulseDays, 30);
            }

            if (!e.HasHabitExpense)
            {
                _manager.TryUnlock("D-005");  // 禁欲一日
                CheckThreshold("D-006", settlement.ConsecutiveNoHabitDays, 3);
                CheckThreshold("D-015", settlement.ConsecutiveNoHabitDays, 5);
                CheckThreshold("D-007", settlement.ConsecutiveNoHabitDays, 7);
                CheckThreshold("D-016", settlement.ConsecutiveNoHabitDays, 14);
            }

            if (!e.HasTransportExpense)
            {
                _manager.TryUnlock("D-011");  // 步行者
            }

            // 低消费日
            if (e.TotalExpenseToday < 50f) _manager.TryUnlock("D-018");
            if (e.TotalExpenseToday < 20f) _manager.TryUnlock("D-019");
            if (e.TotalExpenseToday == 0f && e.TotalEntriesCount > 0)
                _manager.TryUnlock("D-012"); // 无为之日

            // 极限操作（隐藏）
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute >= 59)
                _manager.TryUnlock("G-008");

            // 22:00前结算
            CheckThreshold("G-017", settlement.ConsecutiveEarlySettlementDays, 14); // 治好了拖延症

            // 周末宅家（隐藏）
            if (!e.HasTransportExpense &&
                (e.Date.DayOfWeek == DayOfWeek.Saturday || e.Date.DayOfWeek == DayOfWeek.Sunday))
                _manager.TryUnlock("G-014");

            // 5月1日记账（隐藏）
            if (e.Date.Month == 5 && e.Date.Day == 1)
                _manager.TryUnlock("G-005");

            // 11月11日无即时物欲（隐藏）
            if (e.Date.Month == 11 && e.Date.Day == 11 && !e.HasImpulseBuy)
                _manager.TryUnlock("G-004"); // 双十一幸存者
        }

        #endregion

        #region 收集成就

        private void OnDecorationUnlocked(DecorationUnlocked e)
        {
            int count = _decorationSystem.UnlockedCount;
            CheckThreshold("E-008", count, 1);
            CheckThreshold("E-022", count, 5);
            CheckThreshold("E-009", count, 10);
            CheckThreshold("E-023", count, 20);
            CheckThreshold("E-010", count, 30);
            CheckThreshold("E-024", count, 50);
            CheckThreshold("E-011", count, 60);

            // 风格完成检查
            if (_decorationSystem.IsStyleComplete(DecorationStyle.Cthulhu))
                _manager.TryUnlock("E-012");
            if (_decorationSystem.IsStyleComplete(DecorationStyle.ChineseClassical))
                _manager.TryUnlock("E-013");
            if (_decorationSystem.IsStyleComplete(DecorationStyle.Futuristic))
                _manager.TryUnlock("E-014");
            if (_decorationSystem.AreAllStylesComplete())
                _manager.TryUnlock("E-015");

            // 克苏鲁入梦（隐藏）
            var deco = _decorationSystem.GetDecoration(e.DecorationId);
            if (deco != null && deco.Style == DecorationStyle.Cthulhu)
                _manager.TryUnlock("G-009");
        }

        private void OnDecorationEquipped(DecorationEquipped e)
        {
            _manager.TryUnlock("F-009"); // 装饰师出道
            if (e.SlotIndex == 0) _manager.TryUnlock("F-010"); // 账本启用

            // 换了三次账本
            int bookChanges = _decorationSystem.GetSlotChangeCount(0);
            CheckThreshold("E-025", bookChanges, 3);
        }

        private void OnCardUnlocked(WisdomCardUnlocked e)
        {
            int total = _cardSystem.UnlockedCount;
            CheckThreshold("E-001", total, 1);
            CheckThreshold("E-016", total, 5);
            CheckThreshold("E-002", total, 10);
            CheckThreshold("E-017", total, 20);
            CheckThreshold("E-006", total, 30);

            // 阵营收集
            int wealthCount = _cardSystem.GetFactionUnlockedCount(CardFaction.Wealth);
            int healthCount = _cardSystem.GetFactionUnlockedCount(CardFaction.Health);

            CheckThreshold("E-018", wealthCount, 3);
            CheckThreshold("E-003", wealthCount, 10);
            CheckThreshold("E-020", wealthCount, 20);

            CheckThreshold("E-019", healthCount, 3);
            CheckThreshold("E-004", healthCount, 10);
            CheckThreshold("E-021", healthCount, 20);

            // 双修之道
            if (wealthCount >= 15 && healthCount >= 15)
                _manager.TryUnlock("E-005");

            // 残章博士
            if (_cardSystem.IsAllCollected())
                _manager.TryUnlock("E-007");
        }

        #endregion

        #region 元成就

        private void OnAchievementUnlocked(AchievementUnlocked e)
        {
            // 检查是否全部达成
            if (_manager.AreAllUnlocked())
            {
                _manager.TryUnlock("G-012"); // 镜中镜
                _manager.TryUnlock("G-018"); // 全部达成
            }
        }

        #endregion

        /// <summary>
        /// 检查阈值：当 currentValue >= threshold 时尝试解锁成就。
        /// </summary>
        private void CheckThreshold(string achievementId, int currentValue, int threshold)
        {
            if (currentValue >= threshold)
            {
                _manager.TryUnlock(achievementId);
            }
        }
    }
}
