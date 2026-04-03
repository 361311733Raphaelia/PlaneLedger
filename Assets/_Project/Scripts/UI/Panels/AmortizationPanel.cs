using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Accounting;
using PlaneLedger.Data.Models;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 年度平摊中心面板
    /// 管理所有平摊项目（如年费、保险等），将大额支出分摊到每月
    /// </summary>
    public class AmortizationPanel : BasePanel
    {
        [Header("列表")]
        [SerializeField] private Transform _itemListContainer;
        [SerializeField] private GameObject _itemRowPrefab;

        [Header("按钮")]
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _closeButton;

        [Header("统计")]
        [SerializeField] private TextMeshProUGUI _totalMonthlyText;

        private AmortizationService _service;
        private MonthlyOverviewService _monthlyService;

        /// <summary>
        /// 初始化：获取服务引用，绑定按钮事件
        /// </summary>
        public override void OnInitialize()
        {
            _service = ServiceLocator.Get<AmortizationService>();
            _monthlyService = ServiceLocator.Get<MonthlyOverviewService>();

            _addButton.onClick.AddListener(OnAddClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// 面板显示时刷新列表
        /// </summary>
        public override void OnShow()
        {
            RefreshList();
        }

        /// <summary>
        /// 点击添加平摊项目：弹出金额输入对话框，确认后创建默认12个月的平摊项
        /// </summary>
        private void OnAddClicked()
        {
            var inputDialog = PanelManager.Instance.GetPanel<InputAmountDialog>("InputAmountDialog");
            PanelManager.Instance.OpenPanel("InputAmountDialog");

            inputDialog.SetOnConfirm((amount, note) =>
            {
                string name = string.IsNullOrEmpty(note) ? "平摊项目" : note;
                string startMonth = DateTime.Now.ToString("yyyy-MM");
                int spreadMonths = 12;

                _service.AddItem(name, amount, startMonth, spreadMonths);
                RefreshList();
            });
        }

        /// <summary>
        /// 刷新平摊项目列表：清空容器，重新生成所有行，更新月度合计
        /// </summary>
        private void RefreshList()
        {
            // 清空现有子物体
            for (int i = _itemListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemListContainer.GetChild(i).gameObject);
            }

            // 获取所有平摊项目并生成行
            List<AmortizedExpense> items = _service.GetAllItems();
            foreach (var item in items)
            {
                CreateItemRow(item);
            }

            // 更新本月平摊合计
            RefreshTotalMonthly();
        }

        /// <summary>
        /// 为单个平摊项目创建一行UI
        /// </summary>
        /// <param name="item">平摊项目数据</param>
        private void CreateItemRow(AmortizedExpense item)
        {
            GameObject row = Instantiate(_itemRowPrefab, _itemListContainer);

            TextMeshProUGUI nameText = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI monthlyText = row.transform.Find("MonthlyText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI detailText = row.transform.Find("DetailText").GetComponent<TextMeshProUGUI>();
            Button deleteButton = row.transform.Find("DeleteButton").GetComponent<Button>();

            nameText.text = item.Name;
            monthlyText.text = $"¥{item.MonthlyAmount:F2}/月";
            detailText.text = $"总额¥{item.TotalAmount:F2} / {item.SpreadMonths}个月";

            string itemId = item.Id;
            deleteButton.onClick.AddListener(() =>
            {
                _service.RemoveItem(itemId);
                RefreshList();
            });
        }

        /// <summary>
        /// 刷新底部本月平摊合计显示
        /// </summary>
        private void RefreshTotalMonthly()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            float total = _monthlyService.GetMonthlyAmortizedTotal(year, month);
            _totalMonthlyText.text = $"本月平摊合计: ¥{total:F2}";
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        private void OnCloseClicked()
        {
            PanelManager.Instance.CloseCurrentPanel();
        }
    }
}
