using UnityEngine;

namespace PlaneLedger.UI
{
    /// <summary>
    /// 面板基类。所有功能面板继承此类。
    /// 提供统一的显示/隐藏/初始化接口。
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("面板配置")]
        [SerializeField] private string _panelId;

        public string PanelId => _panelId;

        /// <summary>是否已初始化（延迟初始化，首次显示时调用）。</summary>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// 显示面板。首次显示时自动调用 OnInitialize。
        /// </summary>
        public void Show()
        {
            if (!IsInitialized)
            {
                OnInitialize();
                IsInitialized = true;
            }

            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>隐藏面板。</summary>
        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        /// <summary>首次显示时调用（延迟初始化），子类重写以绑定组件。</summary>
        protected virtual void OnInitialize() { }

        /// <summary>每次显示时调用，子类重写以刷新数据。</summary>
        protected virtual void OnShow() { }

        /// <summary>每次隐藏时调用，子类重写以清理状态。</summary>
        protected virtual void OnHide() { }
    }
}
