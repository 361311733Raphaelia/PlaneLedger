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
    /// 月度总览面板
    /// 展示当月收支概况、收入列表、大额支出列表以及关键财务指标
    /// </summary>
    public class MonthlyOverviewPanel : BasePanel
    {
        [Header("统计文本")]
        [SerializeField] private TextMeshProUGUI _monthText;
        [SerializeField] private TextMeshProUGUI _totalIncomeText;
        [SerializeField] private TextMeshProUGUI _totalExpenseText;
        [SerializeField] private TextMeshProUGUI _savingsText;
        [SerializeField] private TextMeshProUGUI _survivalDaysText;
        [SerializeField] private TextMeshProUGUI _passiveRatioText;

        [Header("收入列表")]
        [SerializeField] private Transform _incomeListContainer;
        [SerializeField] private GameObject _incomeRowPrefab;

        [Header("大额支出列表")]
        [SerializeField] private Transform _largeExpenseListContainer;
        [SerializeField] private GameObject _largeExpenseRowPrefab;

        [Header("按钮")]
        [SerializeField] private Button _addIncomeButton;
        [SerializeField] private Button _addLargeExpenseButton;
        [SerializeField] private Button _closeButton;

        private MonthlyOverviewService _service;
        private int _year;
        private int _month;

        /// <summary>
        /// 初始化：获取服务引用，设置当前年月，绑定按钮事件
        /// </summary>
        public override void OnInitialize()
        {
            _service = ServiceLocator.Get<MonthlyOverviewService>();

            _year = DateTime.Now.Year;
            _month = DateTime.Now.Month;

            _addIncomeButton.onClick.AddListener(OnAddIncomeClicked);
            _addLargeExpenseButton.onClick.AddListener(OnAddLargeExpenseClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// 面板显示时刷新所有数据
        /// </summary>
        public override void OnShow()
        {
            RefreshAll();
        }

        /// <summary>
        /// 点击添加收入：弹出金额输入对话框，确认后写入服务
        /// </summary>
        private void OnAddIncomeClicked()
        {
            var inputDialog = PanelManager.Instance.GetPanel<InputAmountDialog>("InputAmountDialog");
            PanelManager.Instance.OpenPanel("InputAmountDialog");

            inputDialog.SetOnConfirm((amount, note) =>
            {
                string description = string.IsNullOrEmpty(note) ? "工资" : note;
                _service.AddIncomeItem(_year, _month, "Active", description, amount);
                RefreshAll();
            });
        }

        /// <summary>
        /// 点击添加大额支出：弹出金额输入对话框，确认后写入服务
        /// </summary>
        private void OnAddLargeExpenseClicked()
        {
            var inputDialog = PanelManager.Instance.GetPanel<InputAmountDialog>("InputAmountDialog");
            PanelManager.Instance.OpenPanel("InputAmountDialog");

            inputDialog.SetOnConfirm((amount, note) =>
            {
                string description = string.IsNullOrEmpty(note) ? "大额支出" : note;
                _service.AddLargeExpense(_year, _month, "Other", description, amount);
                RefreshAll();
            });
        }

        /// <summary>
        /// 刷新所有数据：统计指标 + 收入列表 + 大额支出列表
        /// </summary>
        private void RefreshAll()
        {
            RefreshStatistics();
            RefreshIncomeList();
            RefreshLargeExpenseList();
        }

        /// <summary>
        /// 刷新顶部统计指标显示
        /// </summary>
        private void RefreshStatistics()
        {
            _monthText.text = $"{_year}年{_month}月";

            float totalIncome = _service.GetMonthlyTotalIncome(_year, _month);
            float totalExpense = _service.GetMonthlyTotalExpense(_year, _month);
            float savings = _service.GetMonthlySavings(_year, _month);
            float survivalDays = _service.GetSurvivalDays();
            float passiveRatio = _service.GetPassiveIncomeRatio(_year, _month);

            _totalIncomeText.text = $"总收入: ¥{totalIncome:F2}";
            _totalExpenseText.text = $"总支出: ¥{totalExpense:F2}";
            _savingsText.text = $"结余: ¥{savings:F2}";
            _survivalDaysText.text = $"生存天数: {survivalDays:F0}天";
            _passiveRatioText.text = $"被动收入比: {passiveRatio:P1}";
        }

        /// <summary>
        /// 刷新收入列表：清空容器，重新生成所有收入行
        /// </summary>
        private void RefreshIncomeList()
        {
            ClearContainer(_incomeListContainer);

            MonthlyIncome monthlyIncome = _service.GetOrCreateMonthlyIncome(_year, _month);
            if (monthlyIncome == null || monthlyIncome.Items == null) return;

            foreach (var item in monthlyIncome.Items)
            {
                CreateIncomeRow(item);
            }
        }

        /// <summary>
        /// 为单条收入创建一行UI
        /// </summary>
        /// <param name="item">收入条目</param>
        private void CreateIncomeRow(IncomeItem item)
        {
            GameObject row = Instantiate(_incomeRowPrefab, _incomeListContainer);

            TextMeshProUGUI descriptionText = row.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountText = row.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            Button deleteButton = row.transform.Find("DeleteButton").GetComponent<Button>();

            descriptionText.text = $"[{item.Type}] {item.Description}";
            amountText.text = $"¥{item.Amount:F2}";

            string itemId = item.Id;
            deleteButton.onClick.AddListener(() =>
            {
                _service.RemoveIncomeItem(_year, _month, itemId);
                RefreshAll();
            });
        }

        /// <summary>
        /// 刷新大额支出列表：清空容器，重新生成所有大额支出行
        /// </summary>
        private void RefreshLargeExpenseList()
        {
            ClearContainer(_largeExpenseListContainer);

            // 从服务获取大额支出总额用于底部显示（列表通过MonthlyIncome获取）
            MonthlyIncome monthlyIncome = _service.GetOrCreateMonthlyIncome(_year, _month);
            if (monthlyIncome == null || monthlyIncome.LargeExpenses == null) return;

            foreach (var expense in monthlyIncome.LargeExpenses)
            {
                CreateLargeExpenseRow(expense);
            }
        }

        /// <summary>
        /// 为单条大额支出创建一行UI
        /// </summary>
        /// <param name="expense">大额支出条目</param>
        private void CreateLargeExpenseRow(LargeExpense expense)
        {
            GameObject row = Instantiate(_largeExpenseRowPrefab, _largeExpenseListContainer);

            TextMeshProUGUI descriptionText = row.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountText = row.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            Button deleteButton = row.transform.Find("DeleteButton").GetComponent<Button>();

            string categoryDisplay = CategoryConfig.LargeExpenseCategories.ContainsKey(expense.Category)
                ? CategoryConfig.LargeExpenseCategories[expense.Category]
                : expense.Category;

            descriptionText.text = $"[{categoryDisplay}] {expense.Description}";
            amountText.text = $"¥{expense.Amount:F2}";

            string expenseId = expense.Id;
            deleteButton.onClick.AddListener(() =>
            {
                _service.RemoveLargeExpense(_year, _month, expenseId);
                RefreshAll();
            });
        }

        /// <summary>
        /// 清空指定容器内的所有子物体
        /// </summary>
        /// <param name="container">要清空的容器Transform</param>
        private void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
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
