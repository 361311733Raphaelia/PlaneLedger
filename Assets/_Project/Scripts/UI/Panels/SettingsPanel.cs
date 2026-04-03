using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Core;
using PlaneLedger.UI.Common;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 设置面板，提供存款目标、月预算编辑，以及存档导入导出功能。
    /// </summary>
    public class SettingsPanel : BasePanel
    {
        /// <summary>存款目标输入框</summary>
        [SerializeField] private TMP_InputField _savingsInput;

        /// <summary>月预算输入框</summary>
        [SerializeField] private TMP_InputField _budgetInput;

        /// <summary>保存按钮</summary>
        [SerializeField] private Button _saveButton;

        /// <summary>关闭按钮</summary>
        [SerializeField] private Button _closeButton;

        /// <summary>导出存档按钮</summary>
        [SerializeField] private Button _exportButton;

        /// <summary>导入存档按钮</summary>
        [SerializeField] private Button _importButton;

        /// <summary>版本号文本</summary>
        [SerializeField] private TextMeshProUGUI _versionText;

        /// <summary>
        /// 初始化面板，绑定所有按钮事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _closeButton.onClick.AddListener(() => PanelManager.Instance.CloseCurrentPanel());

            _saveButton.onClick.AddListener(OnSaveClicked);
            _exportButton.onClick.AddListener(OnExportClicked);
            _importButton.onClick.AddListener(OnImportClicked);
        }

        /// <summary>
        /// 面板显示时从存档读取数据并填充输入框。
        /// </summary>
        protected override void OnShow()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            var saveData = saveSystem.Load();

            _savingsInput.text = saveData.Profile.TotalSavings.ToString();
            _budgetInput.text = saveData.Profile.MonthlyBudget.ToString();

            _versionText.text = "位面账簿 v1.0";
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        protected override void OnHide() { }

        /// <summary>
        /// 保存按钮点击：解析输入框数据，写回存档并发布保存事件。
        /// </summary>
        private void OnSaveClicked()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            var saveData = saveSystem.Load();

            // 解析存款目标
            if (decimal.TryParse(_savingsInput.text, out decimal savings))
            {
                saveData.Profile.TotalSavings = savings;
            }
            else
            {
                ToastNotification.Show("存款目标格式无效");
                return;
            }

            // 解析月预算
            if (decimal.TryParse(_budgetInput.text, out decimal budget))
            {
                saveData.Profile.MonthlyBudget = budget;
            }
            else
            {
                ToastNotification.Show("月预算格式无效");
                return;
            }

            // 触发保存事件
            EventBus.Publish(new SaveRequested());

            ToastNotification.Show("设置已保存");
        }

        /// <summary>
        /// 导出存档按钮点击：尝试调用导出功能。
        /// </summary>
        private void OnExportClicked()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();

            try
            {
                saveSystem.ExportToFile();
                ToastNotification.Show("存档导出成功");
            }
            catch (System.Exception)
            {
                ToastNotification.Show("功能开发中");
            }
        }

        /// <summary>
        /// 导入存档按钮点击：尝试调用导入功能。
        /// </summary>
        private void OnImportClicked()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();

            try
            {
                saveSystem.ImportFromFile();
                ToastNotification.Show("存档导入成功");
            }
            catch (System.Exception)
            {
                ToastNotification.Show("功能开发中");
            }
        }
    }
}
