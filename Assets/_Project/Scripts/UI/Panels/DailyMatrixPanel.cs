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
    /// 日常消耗矩阵面板 - 核心记账界面
    /// 显示当日所有消费记录，支持添加/删除条目
    /// </summary>
    public class DailyMatrixPanel : BasePanel
    {
        [Header("列表")]
        [SerializeField] private Transform _entryListContainer;
        [SerializeField] private GameObject _entryRowPrefab;

        [Header("按钮")]
        [SerializeField] private Button _addEntryButton;
        [SerializeField] private Button _closeButton;

        [Header("文本")]
        [SerializeField] private TextMeshProUGUI _dailyTotalText;
        [SerializeField] private TextMeshProUGUI _dateText;

        private DailyMatrixService _service;
        private DateTime _currentDate = DateTime.Now.Date;

        /// <summary>
        /// 初始化：获取服务引用，绑定按钮事件
        /// </summary>
        public override void OnInitialize()
        {
            _service = ServiceLocator.Get<DailyMatrixService>();

            _addEntryButton.onClick.AddListener(OnAddEntryClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// 面板显示时刷新数据
        /// </summary>
        public override void OnShow()
        {
            _currentDate = DateTime.Now.Date;
            RefreshEntryList();
        }

        /// <summary>
        /// 点击添加记录：先选分类，再输金额，最后写入服务
        /// 整个流程通过回调串联实现异步交互
        /// </summary>
        private void OnAddEntryClicked()
        {
            // 第一步：打开分类选择面板
            var categorySelector = PanelManager.Instance.GetPanel<CategorySelectorPanel>("CategorySelector");
            PanelManager.Instance.OpenPanel("CategorySelector");

            categorySelector.SetOnCategorySelected((category, subCategory) =>
            {
                // 第二步：分类选完后，打开金额输入对话框
                var inputDialog = PanelManager.Instance.GetPanel<InputAmountDialog>("InputAmountDialog");
                PanelManager.Instance.OpenPanel("InputAmountDialog");

                inputDialog.SetOnConfirm((amount, note) =>
                {
                    // 第三步：金额输入完成，添加记录并刷新
                    _service.AddEntry(_currentDate, category, subCategory, amount, note);
                    RefreshEntryList();
                });
            });
        }

        /// <summary>
        /// 刷新条目列表：清空容器，重新生成所有行
        /// </summary>
        private void RefreshEntryList()
        {
            // 清空现有子物体
            for (int i = _entryListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_entryListContainer.GetChild(i).gameObject);
            }

            // 更新日期显示
            _dateText.text = _currentDate.ToString("yyyy年MM月dd日");

            // 获取当日所有条目并生成行
            List<DailyEntry> entries = _service.GetEntries(_currentDate);
            foreach (var entry in entries)
            {
                CreateEntryRow(entry);
            }

            RefreshDailyTotal();
        }

        /// <summary>
        /// 为单条记录创建一行UI
        /// </summary>
        /// <param name="entry">日常消费条目</param>
        private void CreateEntryRow(DailyEntry entry)
        {
            GameObject row = Instantiate(_entryRowPrefab, _entryListContainer);

            // 获取行内各组件（按预制体中的子物体顺序）
            TextMeshProUGUI categoryText = row.transform.Find("CategoryText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountText = row.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI noteText = row.transform.Find("NoteText").GetComponent<TextMeshProUGUI>();
            Button deleteButton = row.transform.Find("DeleteButton").GetComponent<Button>();

            // 设置显示内容
            string categoryDisplay = CategoryConfig.CategoryDisplayNames.ContainsKey(entry.Category)
                ? CategoryConfig.CategoryDisplayNames[entry.Category]
                : entry.Category;
            string subCategoryDisplay = CategoryConfig.SubCategoryDisplayNames.ContainsKey(entry.SubCategory)
                ? CategoryConfig.SubCategoryDisplayNames[entry.SubCategory]
                : entry.SubCategory;

            categoryText.text = $"{categoryDisplay} - {subCategoryDisplay}";
            amountText.text = $"¥{entry.Amount:F2}";
            noteText.text = string.IsNullOrEmpty(entry.Note) ? "" : entry.Note;

            // 绑定删除按钮（捕获entryId避免闭包问题）
            string entryId = entry.Id;
            deleteButton.onClick.AddListener(() => OnDeleteEntry(entryId));
        }

        /// <summary>
        /// 删除指定条目并刷新列表
        /// </summary>
        /// <param name="entryId">要删除的条目ID</param>
        private void OnDeleteEntry(string entryId)
        {
            _service.RemoveEntry(_currentDate, entryId);
            RefreshEntryList();
        }

        /// <summary>
        /// 刷新当日消费总额显示
        /// </summary>
        private void RefreshDailyTotal()
        {
            float total = _service.GetDailyTotal(_currentDate);
            _dailyTotalText.text = $"今日合计: ¥{total:F2}";
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
