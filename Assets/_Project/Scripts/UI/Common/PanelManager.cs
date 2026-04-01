using System.Collections.Generic;
using UnityEngine;

namespace PlaneLedger.UI
{
    /// <summary>
    /// UI 面板管理器。管理所有功能面板的显示/隐藏，支持面板堆栈（返回上一面板）。
    /// 挂载到 Canvas_Panels 对象上。
    /// </summary>
    public class PanelManager : MonoBehaviour
    {
        public static PanelManager Instance { get; private set; }

        [Header("所有面板")]
        [SerializeField] private BasePanel[] _allPanels;

        private readonly Stack<BasePanel> _panelStack = new Stack<BasePanel>();
        private readonly Dictionary<string, BasePanel> _panelMap = new Dictionary<string, BasePanel>();

        private void Awake()
        {
            Instance = this;

            foreach (var panel in _allPanels)
            {
                if (panel != null)
                {
                    _panelMap[panel.PanelId] = panel;
                    panel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>打开一个面板（隐藏当前面板，压入堆栈）。</summary>
        public void OpenPanel(string panelId)
        {
            if (!_panelMap.TryGetValue(panelId, out var panel))
            {
                Debug.LogWarning($"[PanelManager] 未找到面板: {panelId}");
                return;
            }

            // 隐藏当前面板
            if (_panelStack.Count > 0)
            {
                _panelStack.Peek().Hide();
            }

            _panelStack.Push(panel);
            panel.Show();
        }

        /// <summary>关闭当前面板，返回上一个面板。</summary>
        public void CloseCurrentPanel()
        {
            if (_panelStack.Count == 0) return;

            var current = _panelStack.Pop();
            current.Hide();

            if (_panelStack.Count > 0)
            {
                _panelStack.Peek().Show();
            }
        }

        /// <summary>关闭所有面板。</summary>
        public void CloseAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                _panelStack.Pop().Hide();
            }
        }

        /// <summary>获取指定面板。</summary>
        public T GetPanel<T>(string panelId) where T : BasePanel
        {
            return _panelMap.TryGetValue(panelId, out var panel) ? panel as T : null;
        }

        /// <summary>是否有面板正在显示。</summary>
        public bool HasOpenPanel => _panelStack.Count > 0;

        private void Update()
        {
            // ESC 关闭当前面板
            if (Input.GetKeyDown(KeyCode.Escape) && _panelStack.Count > 0)
            {
                CloseCurrentPanel();
            }
        }
    }
}
