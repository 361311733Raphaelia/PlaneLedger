using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Collection;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using PlaneLedger.UI.Common;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 掉落结果展示面板，以翻牌效果展示获得的物品详情。
    /// </summary>
    public class DropResultPanel : BasePanel
    {
        /// <summary>物品名称文本</summary>
        [SerializeField] private TextMeshProUGUI _itemNameText;

        /// <summary>稀有度文本</summary>
        [SerializeField] private TextMeshProUGUI _rarityText;

        /// <summary>物品描述文本</summary>
        [SerializeField] private TextMeshProUGUI _descriptionText;

        /// <summary>物品图标</summary>
        [SerializeField] private Image _itemIcon;

        /// <summary>确认按钮</summary>
        [SerializeField] private Button _confirmButton;

        /// <summary>重复物品信息容器</summary>
        [SerializeField] private GameObject _duplicateInfo;

        /// <summary>重复物品代币补偿文本</summary>
        [SerializeField] private TextMeshProUGUI _duplicateTokenText;

        /// <summary>掉落结果数据</summary>
        private DropResult _result;

        /// <summary>
        /// 初始化面板，绑定确认按钮事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _confirmButton.onClick.AddListener(() => PanelManager.Instance.CloseCurrentPanel());
        }

        /// <summary>
        /// 设置掉落结果数据，需在 OnShow 之前调用。
        /// </summary>
        /// <param name="result">掉落结果</param>
        public void Setup(DropResult result)
        {
            _result = result;
        }

        /// <summary>
        /// 面板显示时根据掉落结果填充UI。
        /// </summary>
        protected override void OnShow()
        {
            Rarity rarity = (Rarity)_result.Rarity;

            if (_result.IsDecoration)
            {
                // 装饰类物品：从 DecorationSystem 获取详情
                var decorationSystem = ServiceLocator.Get<DecorationSystem>();
                var deco = decorationSystem.GetDecoration(_result.ItemId);

                _itemNameText.text = deco.DisplayName;
                _descriptionText.text = deco.Description;

                if (deco.Icon != null)
                {
                    _itemIcon.sprite = deco.Icon;
                    _itemIcon.enabled = true;
                }
                else
                {
                    _itemIcon.enabled = false;
                }
            }
            else
            {
                // 卡牌类物品：从 WisdomCardSystem 获取详情
                var cardSystem = ServiceLocator.Get<WisdomCardSystem>();
                var card = cardSystem.GetCard(_result.ItemId);

                _itemNameText.text = card.DisplayName;
                _descriptionText.text = card.PlaneAnnotation;

                // 卡牌暂无图标
                _itemIcon.enabled = false;
            }

            // 设置稀有度文字和颜色
            _rarityText.text = GetRarityDisplayName(rarity);
            _rarityText.color = GetRarityColor(rarity);

            // 处理重复物品信息
            if (_result.IsDuplicate)
            {
                _duplicateInfo.SetActive(true);
                _duplicateTokenText.text = $"重复！转换为 {_result.TokenCompensation} 代币";
            }
            else
            {
                _duplicateInfo.SetActive(false);
            }
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        protected override void OnHide() { }

        /// <summary>
        /// 获取稀有度的中文显示名称。
        /// </summary>
        /// <param name="rarity">稀有度枚举值</param>
        /// <returns>中文稀有度名称</returns>
        private string GetRarityDisplayName(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return "普通";
                case Rarity.Rare: return "稀有";
                case Rarity.Epic: return "史诗";
                case Rarity.Legendary: return "传说";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取稀有度对应的显示颜色。
        /// </summary>
        /// <param name="rarity">稀有度枚举值</param>
        /// <returns>对应颜色</returns>
        private Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return Color.white;
                case Rarity.Rare: return new Color(0.3f, 0.5f, 1f);
                case Rarity.Epic: return new Color(0.7f, 0.3f, 1f);
                case Rarity.Legendary: return new Color(1f, 0.8f, 0f);
                default: return Color.white;
            }
        }
    }
}
