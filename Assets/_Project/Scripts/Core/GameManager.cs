using System;
using PlaneLedger.Data.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlaneLedger.Core
{
    /// <summary>
    /// 全局游戏管理器。Boot 场景唯一入口，DontDestroyOnLoad。
    /// 负责：初始化所有服务 → 加载存档 → 日登录/跨日检测 → 自动保存。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("配置")]
        [SerializeField] private float _autoSaveInterval = 60f; // 自动保存间隔（秒）
        [SerializeField] private string _mainSceneName = "MainPlane";

        private SaveSystem _saveSystem;
        private SaveData _saveData;
        private float _autoSaveTimer;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // 1. 初始化存档系统
            _saveSystem = new SaveSystem();
            ServiceLocator.Register(_saveSystem);

            // 2. 加载存档
            _saveData = _saveSystem.Load();

            // 3. 注册所有服务（各模块在此注册，后续实现时取消注释）
            // ServiceLocator.Register(new TokenManager(_saveData));
            // ServiceLocator.Register(new DailyMatrixService(_saveData));
            // ServiceLocator.Register(new MonthlyOverviewService(_saveData));
            // ServiceLocator.Register(new AmortizationService(_saveData));
            // ServiceLocator.Register(new DecorationSystem(_saveData));
            // ServiceLocator.Register(new WisdomCardSystem(_saveData));
            // ServiceLocator.Register(new DropSystem(_saveData));
            // ServiceLocator.Register(new AchievementManager(_saveData));

            // 4. 处理每日登录
            HandleDailyLogin();

            // 5. 订阅存档请求事件
            EventBus.Subscribe<SaveRequested>(OnSaveRequested);

            // 6. 加载主场景
            Debug.Log("[GameManager] 初始化完成，加载主场景...");
            SceneManager.LoadScene(_mainSceneName);
        }

        private void Update()
        {
            // 自动保存
            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= _autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                _saveSystem.SaveCached();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _saveData != null)
            {
                // 重新获得焦点时检查是否跨日
                CheckDayChange();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 应用暂停时自动保存
                _saveSystem?.SaveCached();
            }
        }

        private void OnApplicationQuit()
        {
            // 退出前保存
            _saveSystem?.SaveCached();
            ServiceLocator.Clear();
            EventBus.Clear();
        }

        /// <summary>
        /// 处理每日登录逻辑：更新登录天数、触发登录奖励。
        /// </summary>
        private void HandleDailyLogin()
        {
            var today = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var settlement = _saveData.Settlement;

            if (settlement.LastLoginDate == today)
            {
                // 今日已登录过
                return;
            }

            // 检查连续登录
            if (!string.IsNullOrEmpty(settlement.LastLoginDate))
            {
                var lastLogin = DateTime.Parse(settlement.LastLoginDate);
                var daysDiff = (DateTime.Now.Date - lastLogin.Date).Days;

                if (daysDiff == 1)
                {
                    // 连续登录
                    settlement.ConsecutiveLoginDays++;
                }
                else
                {
                    // 断连
                    settlement.ConsecutiveLoginDays = 1;
                }
            }
            else
            {
                // 首次登录
                settlement.ConsecutiveLoginDays = 1;
            }

            settlement.TotalLoginDays++;
            settlement.LastLoginDate = today;
            settlement.TodayLoginRewardClaimed = false;
            settlement.TodaySettlementDone = false;

            // 重置每日状态
            ResetDailyState();

            // 发布登录事件
            EventBus.Publish(new DailyLoginTriggered
            {
                Date = DateTime.Now.Date,
                ConsecutiveLoginDays = settlement.ConsecutiveLoginDays,
                TotalLoginDays = settlement.TotalLoginDays
            });

            _saveSystem.SaveCached();
            Debug.Log($"[GameManager] 每日登录: 连续{settlement.ConsecutiveLoginDays}天, 累计{settlement.TotalLoginDays}天");
        }

        /// <summary>
        /// 检查是否跨日（应用重新获得焦点时调用）。
        /// </summary>
        private void CheckDayChange()
        {
            var today = DateTime.Now.Date.ToString("yyyy-MM-dd");
            if (_saveData.Settlement.LastLoginDate != today)
            {
                var previousDate = DateTime.Parse(_saveData.Settlement.LastLoginDate);
                HandleDailyLogin();

                EventBus.Publish(new DayChanged
                {
                    PreviousDate = previousDate,
                    NewDate = DateTime.Now.Date
                });
            }
        }

        /// <summary>
        /// 重置每日状态标记。
        /// </summary>
        private void ResetDailyState()
        {
            _saveData.Settlement.TodayLoginRewardClaimed = false;
            _saveData.Settlement.TodaySettlementDone = false;
            _saveData.Settlement.TodayOnlyFood = true; // 默认为true，有非饮食支出时改为false
        }

        /// <summary>
        /// 执行每日结算（由UI层的结算按钮调用）。
        /// </summary>
        public void PerformDailySettlement()
        {
            if (_saveData.Settlement.TodaySettlementDone)
            {
                Debug.Log("[GameManager] 今日已结算");
                return;
            }

            var today = DateTime.Now.Date;
            var todayKey = today.ToString("yyyy-MM-dd");

            // 统计今日数据
            float totalExpense = 0f;
            int entryCount = 0;
            bool hasImpulse = false;
            bool hasHabit = false;
            bool hasTransport = false;

            if (_saveData.Accounting.DailyEntries.TryGetValue(todayKey, out var entries))
            {
                foreach (var entry in entries)
                {
                    totalExpense += entry.Amount;
                    entryCount++;

                    if (entry.Category == "ImpulseBuy") hasImpulse = true;
                    if (entry.Category == "HabitConsume") hasHabit = true;
                    if (entry.Category == "SpatialShift") hasTransport = true;
                }
            }

            // 更新连续记账天数
            var settlement = _saveData.Settlement;
            if (!string.IsNullOrEmpty(settlement.LastSettlementDate))
            {
                var lastSettlement = DateTime.Parse(settlement.LastSettlementDate);
                var daysDiff = (today - lastSettlement.Date).Days;

                if (daysDiff == 1)
                    settlement.ConsecutiveAccountingDays++;
                else if (daysDiff > 1)
                    settlement.ConsecutiveAccountingDays = 1;
                // daysDiff == 0 不应该发生（已检查 TodaySettlementDone）
            }
            else
            {
                settlement.ConsecutiveAccountingDays = 1;
            }

            // 更新克制消费连续天数
            if (!hasImpulse)
                settlement.ConsecutiveNoImpulseDays++;
            else
                settlement.ConsecutiveNoImpulseDays = 0;

            if (!hasHabit)
                settlement.ConsecutiveNoHabitDays++;
            else
                settlement.ConsecutiveNoHabitDays = 0;

            if (!hasTransport)
                settlement.ConsecutiveNoTransportDays++;
            else
                settlement.ConsecutiveNoTransportDays = 0;

            // 检查是否22:00前结算
            if (DateTime.Now.Hour < 22)
                settlement.ConsecutiveEarlySettlementDays++;
            else
                settlement.ConsecutiveEarlySettlementDays = 0;

            settlement.LastSettlementDate = todayKey;
            settlement.TodaySettlementDone = true;
            settlement.TotalEntryCount += entryCount;

            // 发布结算事件（触发掉落、成就检测等）
            EventBus.Publish(new DailySettlementCompleted
            {
                Date = today,
                TotalExpenseToday = totalExpense,
                TotalEntriesCount = entryCount,
                HasImpulseBuy = hasImpulse,
                HasHabitExpense = hasHabit,
                HasTransportExpense = hasTransport
            });

            _saveSystem.SaveCached();
            Debug.Log($"[GameManager] 每日结算完成: {entryCount}笔, 总额{totalExpense:F2}元, 连续记账{settlement.ConsecutiveAccountingDays}天");
        }

        private void OnSaveRequested(SaveRequested _)
        {
            _saveSystem.SaveCached();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SaveRequested>(OnSaveRequested);
        }
    }
}
