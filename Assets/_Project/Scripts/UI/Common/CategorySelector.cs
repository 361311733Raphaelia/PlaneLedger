using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.Data.Models;

namespace PlaneLedger.UI
{
    /// <summary>
    /// 分类选择器面板（两级联动）。
    /// 第一级显示所有大类，选择后第二级显示对应子分类，
    /// 选择子分类后通过回调返回大类和子类的键值。
    /// </summary>
    public class CategorySelector : BasePanel
    {
        [SerializeField]
        [Tooltip("大类按钮容器")]
        private Transform _categoryContainer;

        [SerializeField]
        [Tooltip("子分类按钮容器")]
        private Transform _subCategoryContainer;

        [SerializeField]
        [Tooltip("大类按钮预制体")]
        private Button _categoryButtonPrefab;

        [SerializeField]
        [Tooltip("子分类按钮预制体")]
        private Button _subCategoryButtonPrefab;

        /// <summary>
        /// 选择回调，参数为 (大类键, 子分类键)。
        /// </summary>
        private Action<string, string> _onSelected;

        /// <summary>
        /// 当前选中的大类键。
        /// </summary>
        private string _selectedCategory;

        /// <summary>
        /// 配置分类选择器的回调。
        /// </summary>
        /// <param name="onSelected">选择回调，参数为 (大类键, 子分类键)</param>
        public void Setup(Action<string, string> onSelected)
        {
            _onSelected = onSelected;
        }

        /// <summary>
        /// 面板显示时构建大类按钮列表。
        /// </summary>
        protected override void OnShow()
        {
            BuildCategoryButtons();
        }

        /// <summary>
        /// 构建大类按钮列表。
        /// 清空容器后遍历 CategoryConfig.Categories，为每个大类创建一个按钮。
        /// </summary>
        private void BuildCategoryButtons()
        {
            ClearContainer(_categoryContainer);
            ClearContainer(_subCategoryContainer);

            foreach (var kvp in CategoryConfig.Categories)
            {
                string categoryKey = kvp.Key;
                Button button = Instantiate(_categoryButtonPrefab, _categoryContainer);

                // 设置按钮显示文本
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = CategoryConfig.CategoryDisplayNames[categoryKey];
                }

                // 绑定点击事件
                button.onClick.AddListener(() =>
                {
                    _selectedCategory = categoryKey;
                    BuildSubCategoryButtons(categoryKey);
                });
            }
        }

        /// <summary>
        /// 构建指定大类下的子分类按钮列表。
        /// 清空子分类容器后遍历该大类的所有子分类，为每个子类创建一个按钮。
        /// </summary>
        /// <param name="category">大类键</param>
        private void BuildSubCategoryButtons(string category)
        {
            ClearContainer(_subCategoryContainer);

            if (!CategoryConfig.Categories.ContainsKey(category))
                return;

            foreach (var subCategory in CategoryConfig.Categories[category])
            {
                string subKey = subCategory.Key;
                Button button = Instantiate(_subCategoryButtonPrefab, _subCategoryContainer);

                // 设置按钮显示文本
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = subCategory.Value;
                }

                // 绑定点击事件：回调并关闭面板
                button.onClick.AddListener(() =>
                {
                    _onSelected?.Invoke(_selectedCategory, subKey);
                    PanelManager.Instance.CloseCurrentPanel();
                });
            }
        }

        /// <summary>
        /// 清空指定容器下的所有子对象。
        /// </summary>
        /// <param name="container">要清空的容器</param>
        private void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 静态便捷方法：显示分类选择器。
        /// </summary>
        /// <param name="onSelected">选择回调，参数为 (大类键, 子分类键)</param>
        public static void Show(Action<string, string> onSelected)
        {
            var selector = PanelManager.Instance.GetPanel<CategorySelector>("CategorySelector");
            selector.Setup(onSelected);
            PanelManager.Instance.OpenPanel("CategorySelector");
        }
    }
}
