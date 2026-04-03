using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlaneLedger.UI
{
    /// <summary>
    /// 金额输入对话框面板。
    /// 提供一个数字输入框，用户输入金额后回调返回解析后的浮点值。
    /// </summary>
    public class InputAmountDialog : BasePanel
    {
        [SerializeField]
        [Tooltip("标题文本")]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        [Tooltip("金额输入框")]
        private TMP_InputField _inputField;

        [SerializeField]
        [Tooltip("确认按钮")]
        private Button _confirmButton;

        [SerializeField]
        [Tooltip("取消按钮")]
        private Button _cancelButton;

        /// <summary>
        /// 确认回调，参数为解析后的金额。
        /// </summary>
        private Action<float> _onConfirm;

        /// <summary>
        /// 初始化时设置输入框类型并绑定按钮事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// 配置对话框的标题、回调及占位符文本。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="onConfirm">确认回调，参数为输入的金额</param>
        /// <param name="placeholder">输入框占位符文本，默认"输入金额"</param>
        public void Setup(string title, Action<float> onConfirm, string placeholder = "输入金额")
        {
            _titleText.text = title;
            _onConfirm = onConfirm;

            if (_inputField.placeholder is TextMeshProUGUI placeholderText)
            {
                placeholderText.text = placeholder;
            }
        }

        /// <summary>
        /// 面板显示时清空输入框并聚焦。
        /// </summary>
        protected override void OnShow()
        {
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        /// <summary>
        /// 确认按钮点击处理：解析输入值，成功则回调并关闭面板。
        /// </summary>
        private void OnConfirmClicked()
        {
            if (float.TryParse(_inputField.text, out float amount))
            {
                _onConfirm?.Invoke(amount);
                PanelManager.Instance.CloseCurrentPanel();
            }
        }

        /// <summary>
        /// 取消按钮点击处理：关闭面板。
        /// </summary>
        private void OnCancelClicked()
        {
            PanelManager.Instance.CloseCurrentPanel();
        }

        /// <summary>
        /// 静态便捷方法：显示金额输入对话框。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="onConfirm">确认回调，参数为输入的金额</param>
        public static void Show(string title, Action<float> onConfirm)
        {
            var dialog = PanelManager.Instance.GetPanel<InputAmountDialog>("InputAmountDialog");
            dialog.Setup(title, onConfirm);
            PanelManager.Instance.OpenPanel("InputAmountDialog");
        }
    }
}
