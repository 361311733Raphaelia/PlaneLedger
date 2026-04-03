using UnityEngine;
using UnityEngine.UI;
using PlaneLedger.Achievement;
using PlaneLedger.Core;
using PlaneLedger.Collection;
using PlaneLedger.Currency;
using PlaneLedger.Data;
using PlaneLedger.Data.Models;
using PlaneLedger.UI.Panels;

namespace PlaneLedger.UI
{
    /// <summary>
    /// 主场景启动引导脚本。
    /// 挂载到 MainPlane 场景的根对象上，负责绑定 HUD 按钮事件和处理每日登录逻辑。
    /// </summary>
    public class MainSceneBootstrap : MonoBehaviour
    {
        [Header("HUD 入口按钮")]
        /// <summary>每日矩阵面板入口按钮。</summary>
        [SerializeField] private Button _dailyMatrixButton;

        /// <summary>月度总览面板入口按钮。</summary>
        [SerializeField] private Button _monthlyOverviewButton;

        /// <summary>分期管理面板入口按钮。</summary>
        [SerializeField] private Button _amortizationButton;

        /// <summary>装饰商店面板入口按钮。</summary>
        [SerializeField] private Button _decorationShopButton;

        /// <summary>卡牌收藏面板入口按钮。</summary>
        [SerializeField] private Button _cardCollectionButton;

        /// <summary>成就列表面板入口按钮。</summary>
        [SerializeField] private Button _achievementButton;

        /// <summary>设置面板入口按钮。</summary>
        [SerializeField] private Button _settingsButton;

        /// <summary>每日结算面板入口按钮。</summary>
        [SerializeField] private Button _settlementButton;

        /// <summary>掉落系统服务引用。</summary>
        private DropSystem _dropSystem;

        /// <summary>
        /// 场景启动时初始化收集系统、绑定按钮事件、订阅登录事件。
        /// </summary>
        private void Start()
        {
            // 初始化收集和成就系统（需要 DatabaseManager 已在 Awake 中注册）
            InitializeCollectionSystems();

            _dropSystem = ServiceLocator.Get<DropSystem>();

            // 绑定各面板入口按钮
            _dailyMatrixButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("DailyMatrix"));
            _monthlyOverviewButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("MonthlyOverview"));
            _amortizationButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("Amortization"));
            _decorationShopButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("DecorationShop"));
            _cardCollectionButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("CardCollection"));
            _achievementButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("AchievementList"));
            _settingsButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("Settings"));
            _settlementButton.onClick.AddListener(() => PanelManager.Instance.OpenPanel("Settlement"));

            // 订阅每日登录事件
            EventBus.Subscribe<DailyLoginTriggered>(OnDailyLogin);
        }

        /// <summary>
        /// 销毁时取消事件订阅，防止内存泄漏。
        /// </summary>
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DailyLoginTriggered>(OnDailyLogin);
        }

        /// <summary>
        /// 每日登录事件回调 - 执行登录掉落并展示结果。
        /// </summary>
        /// <param name="e">每日登录事件数据，包含连续登录天数等信息。</param>
        private void OnDailyLogin(DailyLoginTriggered e)
        {
            DropResult result = _dropSystem.PerformLoginDrop();

            // 如果掉落结果有效，打开 DropResultPanel 展示
            if (!string.IsNullOrEmpty(result.ItemId))
            {
                var dropResultPanel = PanelManager.Instance.GetPanel<DropResultPanel>("DropResult");
                dropResultPanel.Setup(result);
                PanelManager.Instance.OpenPanel("DropResult");
            }

            ToastNotification.Show($"欢迎回来！连续登录第{e.ConsecutiveLoginDays}天");
        }

        /// <summary>
        /// 初始化收集和成就系统。
        /// DatabaseManager 在 Awake 中已注册到 ServiceLocator，
        /// 这里利用其 SO 数据创建业务服务。
        /// </summary>
        private void InitializeCollectionSystems()
        {
            var db = ServiceLocator.TryGet<DatabaseManager>();
            if (db == null)
            {
                Debug.LogError("[MainSceneBootstrap] DatabaseManager 未找到，无法初始化收集系统");
                return;
            }

            var saveData = GameManager.Instance.GetSaveData();
            var tokenManager = ServiceLocator.Get<TokenManager>();

            var decoSystem = new DecorationSystem(saveData, db.AllDecorations);
            ServiceLocator.Register(decoSystem);

            var cardSystem = new WisdomCardSystem(saveData, db.AllCards);
            ServiceLocator.Register(cardSystem);

            var dropSystem = new DropSystem(saveData, decoSystem, cardSystem, tokenManager);
            ServiceLocator.Register(dropSystem);

            var achievementManager = new AchievementManager(saveData, tokenManager, db.AllAchievements);
            ServiceLocator.Register(achievementManager);

            var tracker = new AchievementTracker(achievementManager, saveData, decoSystem, cardSystem);
            ServiceLocator.Register(tracker);

            Debug.Log("[MainSceneBootstrap] 收集和成就系统初始化完成");
        }
    }
}
