using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlaneLedger.UI
{
    /// <summary>
    /// 确认对话框面板。
    /// 提供标题、消息文本以及确认/取消按钮，支持回调。
    /// </summary>
    public class ConfirmDialog : BasePanel
    {
        [SerializeField]
        [Tooltip("标题文本")]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        [Tooltip("消息正文文本")]
        private TextMeshProUGUI _messageText;

        [SerializeField]
        [Tooltip("确认按钮")]
        private Button _confirmButton;

        [SerializeField]
        [Tooltip("取消按钮")]
        private Button _cancelButton;

        /// <summary>
        /// 确认回调。
        /// </summary>
        private Action _onConfirm;

        /// <summary>
        /// 取消回调。
        /// </summary>
        private Action _onCancel;

        /// <summary>
        /// 初始化时绑定按钮点击事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// 配置对话框的标题、消息及回调。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">对话框消息内容</param>
        /// <param name="onConfirm">确认回调</param>
        /// <param name="onCancel">取消回调（可选）</param>
        public void Setup(string title, string message, Action onConfirm, Action onCancel = null)
        {
            _titleText.text = title;
            _messageText.text = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        /// <summary>
        /// 确认按钮点击处理：执行确认回调并关闭面板。
        /// </summary>
        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            PanelManager.Instance.CloseCurrentPanel();
        }

        /// <summary>
        /// 取消按钮点击处理：执行取消回调并关闭面板。
        /// </summary>
        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            PanelManager.Instance.CloseCurrentPanel();
        }

        /// <summary>
        /// 静态便捷方法：显示确认对话框。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">对话框消息内容</param>
        /// <param name="onConfirm">确认回调</param>
        /// <param name="onCancel">取消回调（可选）</param>
        public static void Show(string title, string message, Action onConfirm, Action onCancel = null)
        {
            var dialog = PanelManager.Instance.GetPanel<ConfirmDialog>("ConfirmDialog");
            dialog.Setup(title, message, onConfirm, onCancel);
            PanelManager.Instance.OpenPanel("ConfirmDialog");
        }
    }
}
