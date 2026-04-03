using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.Core;
using PlaneLedger.Collection;
using PlaneLedger.Accounting;
using PlaneLedger.Currency;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 每日结算面板 - 展示结算流程和结果。
    /// 分为两个阶段：结算前（今日概要 + 结算按钮）和结算后（结果展示 + 领取掉落按钮）。
    /// </summary>
    public class SettlementPanel : BasePanel
    {
        [Header("文本组件")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private TextMeshProUGUI _tokenRewardText;
        [SerializeField] private TextMeshProUGUI _streakText;

        [Header("按钮")]
        [SerializeField] private Button _settleButton;
        [SerializeField] private Button _claimDropButton;
        [SerializeField] private Button _closeButton;

        [Header("UI区域")]
        /// <summary>结算前阶段：显示今日概要和结算按钮。</summary>
        [SerializeField] private GameObject _settleSection;
        /// <summary>结算后阶段：显示结算结果和领取掉落按钮。</summary>
        [SerializeField] private GameObject _resultSection;

        /// <summary>掉落系统服务引用。</summary>
        private DropSystem _dropSystem;

        /// <summary>每日矩阵服务引用。</summary>
        private DailyMatrixService _dailyMatrixService;

        /// <summary>
        /// 初始化面板，绑定按钮事件。
        /// </summary>
        public override void OnInitialize()
        {
            _dropSystem = ServiceLocator.Get<DropSystem>();
            _dailyMatrixService = ServiceLocator.Get<DailyMatrixService>();

            _settleButton.onClick.AddListener(OnSettleButtonClicked);
            _claimDropButton.onClick.AddListener(OnClaimDropButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        /// <summary>
        /// 面板显示时重置UI状态，根据是否已结算决定显示哪个阶段。
        /// </summary>
        public override void OnShow()
        {
            var today = System.DateTime.Today;

            // 检查今日是否已结算
            var saveData = ServiceLocator.Get<SaveData>();
            if (saveData != null && saveData.HasSettledToday(today))
            {
                // 已结算，直接显示结果阶段
                ShowResultSection(today);
            }
            else
            {
                // 未结算，显示结算前阶段
                ShowSettleSection(today);
            }
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        public override void OnHide()
        {
        }

        /// <summary>
        /// 显示结算前阶段，展示今日记账概要。
        /// </summary>
        /// <param name="date">当前日期。</param>
        private void ShowSettleSection(System.DateTime date)
        {
            _settleSection.SetActive(true);
            _resultSection.SetActive(false);

            float dailyTotal = _dailyMatrixService.GetDailyTotal(date);
            int entryCount = _dailyMatrixService.GetEntries(date).Count;

            _titleText.text = "每日结算";
            _summaryText.text = $"今日支出：¥{dailyTotal:F2}\n记账笔数：{entryCount} 笔";
        }

        /// <summary>
        /// 显示结算后阶段，展示结算结果信息。
        /// </summary>
        /// <param name="date">当前日期。</param>
        private void ShowResultSection(System.DateTime date)
        {
            _settleSection.SetActive(false);
            _resultSection.SetActive(true);

            float dailyTotal = _dailyMatrixService.GetDailyTotal(date);
            int entryCount = _dailyMatrixService.GetEntries(date).Count;
            var tokenManager = ServiceLocator.Get<TokenManager>();

            _titleText.text = "结算完成";
            _summaryText.text = $"今日总支出：¥{dailyTotal:F2}\n记账笔数：{entryCount} 笔";
            _tokenRewardText.text = $"获得代币 +10（余额：{tokenManager.Balance}）";

            var saveData = ServiceLocator.Get<SaveData>();
            if (saveData != null)
            {
                int streak = saveData.ConsecutiveSettlementDays;
                _streakText.text = $"连续记账：{streak} 天";
            }
        }

        /// <summary>
        /// 结算按钮点击回调 - 执行每日结算。
        /// </summary>
        private void OnSettleButtonClicked()
        {
            GameManager.Instance.PerformDailySettlement();
            ShowResultSection(System.DateTime.Today);
            ToastNotification.Show("每日结算完成！");
        }

        /// <summary>
        /// 领取掉落按钮点击回调 - 执行结算掉落并展示结果。
        /// </summary>
        private void OnClaimDropButtonClicked()
        {
            DropResult result = _dropSystem.PerformSettlementDrop();

            // 通过 PanelManager 获取 DropResultPanel 并展示掉落结果
            var dropResultPanel = PanelManager.Instance.GetPanel<DropResultPanel>("DropResult");
            dropResultPanel.Setup(result);
            PanelManager.Instance.OpenPanel("DropResult");
        }

        /// <summary>
        /// 关闭按钮点击回调。
        /// </summary>
        private void OnCloseButtonClicked()
        {
            PanelManager.Instance.CloseCurrentPanel();
        }
    }
}
