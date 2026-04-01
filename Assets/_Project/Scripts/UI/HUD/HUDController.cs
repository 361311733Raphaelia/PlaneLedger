using PlaneLedger.Accounting;
using PlaneLedger.Core;
using PlaneLedger.Currency;
using TMPro;
using UnityEngine;

namespace PlaneLedger.UI
{
    /// <summary>
    /// HUD 控制器。常驻显示生存天数和代币余额。
    /// 挂载到 Canvas_HUD 上。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TextMeshProUGUI _survivalDaysText;
        [SerializeField] private TextMeshProUGUI _tokenText;

        [Header("颜色")]
        [SerializeField] private Color _survivalColor = new Color(0.2f, 0.8f, 0.3f); // 绿色
        [SerializeField] private Color _tokenColor = new Color(0.9f, 0.7f, 0.2f);    // 琥珀色

        private TokenManager _tokenManager;
        private MonthlyOverviewService _monthlyService;

        private void Start()
        {
            _tokenManager = ServiceLocator.TryGet<TokenManager>();
            _monthlyService = ServiceLocator.TryGet<MonthlyOverviewService>();

            if (_survivalDaysText != null) _survivalDaysText.color = _survivalColor;
            if (_tokenText != null) _tokenText.color = _tokenColor;

            // 订阅代币变化
            if (_tokenManager != null)
            {
                _tokenManager.OnBalanceChanged += OnTokenChanged;
            }

            // 订阅结算事件（刷新生存天数）
            EventBus.Subscribe<DailySettlementCompleted>(OnSettlement);
            EventBus.Subscribe<MonthlyDataUpdated>(OnMonthlyUpdate);

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_tokenManager != null)
            {
                _tokenManager.OnBalanceChanged -= OnTokenChanged;
            }
            EventBus.Unsubscribe<DailySettlementCompleted>(OnSettlement);
            EventBus.Unsubscribe<MonthlyDataUpdated>(OnMonthlyUpdate);
        }

        /// <summary>刷新所有HUD显示。</summary>
        public void RefreshAll()
        {
            RefreshSurvivalDays();
            RefreshToken();
        }

        private void RefreshSurvivalDays()
        {
            if (_survivalDaysText == null || _monthlyService == null) return;

            float days = _monthlyService.GetSurvivalDays();
            if (days < 0)
                _survivalDaysText.text = "生存天数: ∞";
            else
                _survivalDaysText.text = $"生存天数: {days:F0}天";
        }

        private void RefreshToken()
        {
            if (_tokenText == null || _tokenManager == null) return;
            _tokenText.text = $"代币: {_tokenManager.Balance}";
        }

        private void OnTokenChanged(int newBalance)
        {
            RefreshToken();
        }

        private void OnSettlement(DailySettlementCompleted _)
        {
            RefreshSurvivalDays();
        }

        private void OnMonthlyUpdate(MonthlyDataUpdated _)
        {
            RefreshSurvivalDays();
        }
    }
}
