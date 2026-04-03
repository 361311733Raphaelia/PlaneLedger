using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Achievement;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 成就列表面板，按分类Tab展示所有成就及其解锁状态和进度。
    /// </summary>
    public class AchievementListPanel : BasePanel
    {
        /// <summary>7个分类Tab按钮，对应7个AchievementCategory</summary>
        [SerializeField] private Button[] _categoryTabs;

        /// <summary>成就列表容器</summary>
        [SerializeField] private Transform _achievementListContainer;

        /// <summary>关闭按钮</summary>
        [SerializeField] private Button _closeButton;

        /// <summary>成就行预制体（含 TMP名称 + TMP描述 + TMP进度 + Image状态图标）</summary>
        [SerializeField] private GameObject _achievementRowPrefab;

        /// <summary>总进度文本（X/164）</summary>
        [SerializeField] private TextMeshProUGUI _progressText;

        /// <summary>当前选中的成就分类</summary>
        private AchievementCategory _currentCategory = AchievementCategory.Login;

        /// <summary>
        /// 初始化面板，绑定关闭按钮和分类Tab事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _closeButton.onClick.AddListener(() => PanelManager.Instance.CloseCurrentPanel());

            // 绑定每个Tab按钮到对应的成就分类
            for (int i = 0; i < _categoryTabs.Length && i < 7; i++)
            {
                int categoryIndex = i;
                _categoryTabs[i].onClick.AddListener(() =>
                {
                    _currentCategory = (AchievementCategory)categoryIndex;
                    RefreshList();
                });
            }
        }

        /// <summary>
        /// 面板显示时刷新成就列表。
        /// </summary>
        protected override void OnShow()
        {
            RefreshList();
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        protected override void OnHide() { }

        /// <summary>
        /// 刷新当前分类下的成就列表。
        /// </summary>
        private void RefreshList()
        {
            var achievementManager = ServiceLocator.Get<AchievementManager>();

            // 清空容器中的旧项
            for (int i = _achievementListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_achievementListContainer.GetChild(i).gameObject);
            }

            // 获取当前分类的成就列表
            var achievements = achievementManager.GetByCategory(_currentCategory);

            foreach (var achievement in achievements)
            {
                CreateAchievementRow(achievement, achievementManager);
            }

            // 更新总进度
            _progressText.text = $"{achievementManager.UnlockedCount}/{achievementManager.TotalCount}";
        }

        /// <summary>
        /// 创建单个成就行UI元素。
        /// </summary>
        /// <param name="achievement">成就SO数据</param>
        /// <param name="achievementManager">成就管理器</param>
        private void CreateAchievementRow(AchievementSO achievement, AchievementManager achievementManager)
        {
            var row = Instantiate(_achievementRowPrefab, _achievementListContainer);

            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            var icons = row.GetComponentsInChildren<Image>();

            bool isUnlocked = achievementManager.IsUnlocked(achievement.Id);
            int progress = achievementManager.GetProgress(achievement.Id);

            if (isUnlocked)
            {
                // 已解锁：显示 ✓ + 名称 + 描述
                if (texts.Length >= 1) texts[0].text = achievement.DisplayName;
                if (texts.Length >= 2) texts[1].text = achievement.Description;
                if (texts.Length >= 3) texts[2].text = "✓";
            }
            else if (achievement.IsHidden)
            {
                // 未解锁的隐藏成就：显示 ???
                if (texts.Length >= 1) texts[0].text = "???";
                if (texts.Length >= 2) texts[1].text = "隐藏成就";
                if (texts.Length >= 3) texts[2].text = "";
            }
            else
            {
                // 未解锁的普通成就：显示名称 + 描述 + 进度
                if (texts.Length >= 1) texts[0].text = achievement.DisplayName;
                if (texts.Length >= 2) texts[1].text = achievement.Description;
                if (texts.Length >= 3) texts[2].text = $"{progress}/{achievement.TargetValue}";
            }
        }
    }
}
